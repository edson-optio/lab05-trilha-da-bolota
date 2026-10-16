using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TrilhaDaBolota.Editor
{
    // Monta o projeto inteiro a partir dos arquivos de arte e dos mapas de texto:
    // importa sprites, cria tiles e paleta, prefabs e as quatro cenas (Menu, Fase1, Fase2, Fim).
    public static class ProjectBuilder
    {
        public const int GroundLayer = 6;
        const string ScenesPath = "Assets/Scenes/";
        const string PrefabsPath = "Assets/Prefabs/";
        const string AppPath = "../Entrega/A Trilha da Bolota.app";

        static readonly string[] SceneOrder = { GameRules.MenuScene, "Fase1", "Fase2", GameRules.EndScene };

        [InitializeOnLoadMethod]
        static void FirstOpen()
        {
            if (Application.isBatchMode || File.Exists(ScenesPath + "Menu.unity")) return;
            EditorApplication.delayCall += () =>
            {
                if (!File.Exists(ScenesPath + "Menu.unity")) CreateProject();
            };
        }

        [MenuItem("Trilha da Bolota/1. Montar fases e assets")]
        public static void CreateProject()
        {
            Directory.CreateDirectory(ScenesPath);
            Directory.CreateDirectory(PrefabsPath);
            ConfigureSettings();
            ArtImporter.ImportAll();
            AssetDatabase.Refresh();

            var prefabs = CreatePrefabs();
            int totalAcorns = 0;
            for (int phase = 0; phase < GameRules.PhaseScenes.Length; phase++)
                totalAcorns += CreatePhaseScene(phase, prefabs);
            CreateMenuScene();
            CreateEndScene(totalAcorns);

            var scenes = new EditorBuildSettingsScene[SceneOrder.Length];
            for (int i = 0; i < SceneOrder.Length; i++)
                scenes[i] = new EditorBuildSettingsScene(ScenesPath + SceneOrder[i] + ".unity", true);
            EditorBuildSettings.scenes = scenes;
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenesPath + GameRules.MenuScene + ".unity");
            Debug.Log("TRILHA_PROJECT_READY: " + totalAcorns + " bolotas");
        }

        // ------------------------------------------------------------------ Prefabs

        static LevelBuilder.Prefabs CreatePrefabs()
        {
            return new LevelBuilder.Prefabs
            {
                acorn = CreateCollectible("Bolota", CollectibleType.Bolota, ArtImporter.Frames("bolota", 4), 8f),
                strawberry = CreateCollectible("Morango", CollectibleType.Morango, ArtImporter.Frames("morango", 2), 3f),
                heart = CreateCollectible("Coracao", CollectibleType.Coracao, ArtImporter.Frames("coracao", 2), 4f),
                snail = CreateEnemy<Snail>("Lesma", ArtImporter.Frames("lesma", 2), 3f, new Vector2(.8f, .62f), new Vector2(0f, .31f)),
                bee = CreateEnemy<Bee>("Abelha", ArtImporter.Frames("abelha", 2), 12f, new Vector2(.72f, .6f), new Vector2(0f, -.05f)),
            };
        }

        static GameObject Animated(string name, Sprite[] frames, float fps, int order)
        {
            var go = new GameObject(name, typeof(SpriteRenderer));
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = frames[0];
            renderer.sortingOrder = order;
            var animator = go.AddComponent<SpriteAnimator>();
            animator.frames = frames;
            animator.framesPerSecond = fps;
            return go;
        }

        static GameObject CreateCollectible(string name, CollectibleType type, Sprite[] frames, float fps)
        {
            var go = Animated(name, frames, fps, 10);
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            var collider = go.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = .4f;
            go.AddComponent<Collectible>().type = type;
            return SavePrefab(go, name);
        }

        static GameObject CreateEnemy<T>(string name, Sprite[] frames, float fps, Vector2 size, Vector2 offset) where T : Enemy
        {
            var go = Animated(name, frames, fps, 15);
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = size;
            collider.offset = offset;
            go.AddComponent<T>();
            return SavePrefab(go, name);
        }

        static GameObject SavePrefab(GameObject go, string name)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabsPath + name + ".prefab");
            Object.DestroyImmediate(go);
            return prefab;
        }

        // ------------------------------------------------------------------ Cenas

        static int CreatePhaseScene(int phase, LevelBuilder.Prefabs prefabs)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var level = new LevelBuilder("Assets/Fases/fase" + (phase + 1) + ".txt", prefabs);
            var world = new GameObject("Fase " + (phase + 1)).transform;
            level.Build(world);

            PlayerController player = CreatePlayer(level.PlayerStart);
            Camera camera = CreateCamera();
            var follow = camera.gameObject.AddComponent<CameraFollow>();
            follow.target = player.transform;
            float halfWidth = GameRules.CameraSize * GameRules.TargetAspect;
            follow.min = new Vector2(halfWidth, GameRules.CameraSize);
            follow.max = new Vector2(level.Width - halfWidth, Mathf.Max(GameRules.CameraSize, level.Height - GameRules.CameraSize));
            camera.transform.position = new Vector3(Mathf.Clamp(level.PlayerStart.x, follow.min.x, follow.max.x), follow.min.y, -10f);
            CreateBackground(camera.transform.position);

            var manager = new GameObject("Gerenciador da fase").AddComponent<LevelManager>();
            manager.phaseIndex = phase;
            manager.player = player;
            manager.fallLimitY = -GameRules.FallMarginBelowMap;
            manager.sound = CreateAudio();
            manager.sparkleFrames = ArtImporter.Frames("brilho", 3);
            manager.hud = new GameObject("HUD").AddComponent<GameHUD>();
            manager.hud.heartIcon = ArtImporter.Sprite("coracao-1");
            manager.hud.acornIcon = ArtImporter.Sprite("bolota-1");

            EditorSceneManager.SaveScene(scene, ScenesPath + GameRules.PhaseScenes[phase] + ".unity");
            return CountAcorns(level);
        }

        static int CountAcorns(LevelBuilder level)
        {
            int count = 0;
            foreach (string row in level.Rows) foreach (char c in row) if (c == 'a') count++;
            return count;
        }

        static PlayerController CreatePlayer(Vector2 position)
        {
            var go = new GameObject("Esquilo (jogador)", typeof(SpriteRenderer));
            go.transform.position = position;
            go.GetComponent<SpriteRenderer>().sortingOrder = 20;
            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 4f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(.62f, .8f);
            collider.offset = new Vector2(0f, .42f);
            collider.edgeRadius = .02f;
            // Sem atrito: o esquilo não "gruda" ao pular encostado numa parede.
            collider.sharedMaterial = NoFriction();
            go.AddComponent<SpriteAnimator>();
            var player = go.AddComponent<PlayerController>();
            player.idleFrames = ArtImporter.Frames("esquilo-parado", 2);
            player.runFrames = ArtImporter.Frames("esquilo-corre", 4);
            player.jumpFrames = new[] { ArtImporter.Sprite("esquilo-pulo") };
            player.fallFrames = new[] { ArtImporter.Sprite("esquilo-queda") };
            go.GetComponent<SpriteRenderer>().sprite = player.idleFrames[0];
            return player;
        }

        static PhysicsMaterial2D NoFriction()
        {
            const string path = "Assets/Prefabs/SemAtrito.physicsMaterial2D";
            var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
            if (material != null) return material;
            material = new PhysicsMaterial2D("SemAtrito") { friction = 0f, bounciness = 0f };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        static Camera CreateCamera()
        {
            // Câmera de fundo pinta as barras pretas quando a janela não é 16:9.
            var bars = new GameObject("Barras", typeof(Camera)).GetComponent<Camera>();
            bars.clearFlags = CameraClearFlags.SolidColor;
            bars.backgroundColor = Color.black;
            bars.cullingMask = 0;
            bars.depth = -100;

            var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(FixedViewport));
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0f, GameRules.CameraSize, -10f);
            var camera = go.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = GameRules.CameraSize;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = GameRules.Sky;
            camera.nearClipPlane = .1f;
            camera.farClipPlane = 100f;
            return camera;
        }

        // Três camadas de parallax, da mais distante (nuvens) para a mais próxima (bosque).
        static void CreateBackground(Vector3 cameraPosition)
        {
            var background = new GameObject("Fundo (parallax)").transform;
            float viewBottom = cameraPosition.y - GameRules.CameraSize;
            CreateLayer(background, "Nuvens", ArtImporter.Background(ArtImporter.ArtPath + "tilesetOpenGameBackground.png"),
                cameraPosition.x, viewBottom + 10.5f, .92f, .9f, -30);
            CreateLayer(background, "Colinas", ArtImporter.Background(ArtImporter.GeneratedPath + "colinas.png"),
                cameraPosition.x, viewBottom + 7f, .75f, .75f, -20);
            CreateLayer(background, "Bosque", ArtImporter.Background(ArtImporter.GeneratedPath + "bosque.png"),
                cameraPosition.x, viewBottom + 6.5f, .5f, .5f, -10);
        }

        static void CreateLayer(Transform parent, string name, Sprite sprite, float x, float y, float effect, float vertical, int order)
        {
            var go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(x, y, 0f);
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = new Vector2(sprite.bounds.size.x * 3f, sprite.bounds.size.y);
            renderer.sortingOrder = order;
            var parallax = go.AddComponent<Parallax>();
            parallax.parallaxEffect = effect;
            parallax.verticalEffect = vertical;
        }

        static GameAudio CreateAudio()
        {
            var go = new GameObject("Áudio", typeof(AudioSource));
            return go.AddComponent<GameAudio>();
        }

        // Menu e tela final: só o fundo em parallax deslizando atrás da interface.
        static Camera CreateScrollingBackdrop()
        {
            Camera camera = CreateCamera();
            camera.gameObject.AddComponent<CameraFollow>().autoScrollSpeed = 2f;
            CreateBackground(camera.transform.position);
            return camera;
        }

        static void CreateMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateScrollingBackdrop();
            var menu = new GameObject("Menu").AddComponent<MenuScreen>();
            menu.squirrel = ArtImporter.Sprite("esquilo-parado-1");
            menu.acorn = ArtImporter.Sprite("bolota-1");
            menu.strawberry = ArtImporter.Sprite("morango-1");
            menu.heart = ArtImporter.Sprite("coracao-1");
            menu.snail = ArtImporter.Sprite("lesma-1");
            menu.bee = ArtImporter.Sprite("abelha-1");
            new GameObject("Verificação automática").AddComponent<GameVerification>();
            EditorSceneManager.SaveScene(scene, ScenesPath + GameRules.MenuScene + ".unity");
        }

        static void CreateEndScene(int totalAcorns)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateScrollingBackdrop();
            var end = new GameObject("Tela final").AddComponent<EndScreen>();
            end.victoryIcon = ArtImporter.Sprite("esquilo-pulo");
            end.defeatIcon = ArtImporter.Sprite("lesma-1");
            end.totalAcorns = totalAcorns;
            EditorSceneManager.SaveScene(scene, ScenesPath + GameRules.EndScene + ".unity");
        }

        // ------------------------------------------------------------------ Configuração e build

        static void ConfigureSettings()
        {
            PlayerSettings.companyName = "Laboratório de Jogos";
            PlayerSettings.productName = "A Trilha da Bolota";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
            PlayerSettings.colorSpace = ColorSpace.Gamma;
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Standalone, "com.laboratorio.trilhadabolota");
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetArchitecture(UnityEditor.Build.NamedBuildTarget.Standalone, 2);
            SetProperty("ProjectSettings/ProjectSettings.asset", "activeInputHandler", 0); // Input Manager clássico
            SetGroundLayer();
            QualitySettings.vSyncCount = 1;
            QualitySettings.antiAliasing = 0;
            Physics2D.gravity = new Vector2(0f, -9.81f);
            Physics2D.queriesHitTriggers = false;
            EditorSettings.serializationMode = SerializationMode.ForceText;
        }

        static void SetProperty(string asset, string property, int value)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(asset);
            if (assets.Length == 0) return;
            var settings = new SerializedObject(assets[0]);
            var field = settings.FindProperty(property);
            if (field == null) return;
            field.intValue = value;
            settings.ApplyModifiedPropertiesWithoutUndo();
        }

        // Camada "Chao" (6): usada para saber se o esquilo está pisando em algo e pelos sensores da lesma.
        static void SetGroundLayer()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            layers.GetArrayElementAtIndex(GroundLayer).stringValue = "Chao";
            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void BuildAll()
        {
            CreateProject();
            ProjectChecks.Verify();
            BuildMac();
        }

        [MenuItem("Trilha da Bolota/2. Gerar aplicativo macOS")]
        public static void BuildMac()
        {
            if (!File.Exists(ScenesPath + "Menu.unity")) CreateProject();
            Directory.CreateDirectory("../Entrega");
            var scenes = new string[SceneOrder.Length];
            for (int i = 0; i < scenes.Length; i++) scenes[i] = ScenesPath + SceneOrder[i] + ".unity";
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = AppPath,
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new System.Exception("Falha no build: " + report.summary.result);
            Debug.Log("TRILHA_BUILD_READY: " + Path.GetFullPath(AppPath));
        }
    }
}
