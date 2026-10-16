using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TrilhaDaBolota
{
    // Executado somente com -selfTest. Joga sozinho as cenas reais do aplicativo compilado
    // (menu, duas fases, derrota, reinício e vitória) e salva um relatório JSON com capturas de tela.
    public class GameVerification : MonoBehaviour
    {
        [Serializable] class Report { public bool passed; public List<string> checks = new List<string>(); public List<string> failures = new List<string>(); }
        readonly Report report = new Report();
        static bool running;
        bool finished;
        string output;

        void Awake()
        {
            if (running || Array.IndexOf(Environment.GetCommandLineArgs(), "-selfTest") < 0) { Destroy(gameObject); return; }
            running = true;
            DontDestroyOnLoad(gameObject);
        }

        IEnumerator Start()
        {
            output = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../Verificacao"));
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-testOutput") output = args[i + 1];
            Directory.CreateDirectory(output);
            Application.logMessageReceived += OnLog;
            yield return new WaitForSeconds(1f);

            yield return TestMenu();
            yield return TestPhaseOne();
            yield return TestPhaseTwoAndDefeat();
            yield return TestRestartAndVictory();
            yield return TestWindow();

            Finish();
        }

        // Se algum passo travar (ex.: exceção no meio de uma corrotina), encerra com falha em vez de ficar aberto.
        void Update()
        {
            if (!finished && Time.realtimeSinceStartup > 240f)
            {
                report.failures.Add("Tempo esgotado: o teste não terminou em 4 minutos");
                Finish();
            }
        }

        void Finish()
        {
            finished = true;
            report.passed = report.failures.Count == 0;
            File.WriteAllText(Path.Combine(output, "resultado-testes.json"), JsonUtility.ToJson(report, true));
            Debug.Log("TRILHA_TESTS: " + (report.passed ? "PASSED" : "FAILED") + " / " + report.checks.Count + " checks, " + report.failures.Count + " falhas");
            Application.logMessageReceived -= OnLog;
            Application.Quit(report.passed ? 0 : 1);
        }

        // ------------------------------------------------------------------ Menu

        IEnumerator TestMenu()
        {
            var menu = FindAnyObjectByType<MenuScreen>();
            Check(SceneManager.GetActiveScene().name == GameRules.MenuScene && menu != null, "Jogo abre na tela inicial");
            Check(menu.TitleText == "A TRILHA DA BOLOTA" && menu.InstructionsText.Contains("COMO JOGAR")
                  && menu.InstructionsText.Contains("pular") && menu.InstructionsText.Contains("bolotas"),
                "Tela inicial mostra o título e as instruções de como jogar");
            Check(FindObjectsByType<Parallax>(FindObjectsSortMode.None).Length == 3, "Tela inicial tem o fundo em parallax");
            yield return Capture("01-menu");
            menu.PlayButton.onClick.Invoke();
            yield return WaitScene("Fase1");
            Check(SceneManager.GetActiveScene().name == "Fase1", "Botão JOGAR abre a fase 1");
        }

        // ------------------------------------------------------------------ Fase 1

        IEnumerator TestPhaseOne()
        {
            var level = LevelManager.Instance;
            var player = level.player;
            var hud = level.hud;
            yield return new WaitForSeconds(.8f);

            Check(GameSession.Score == 0 && GameSession.Lives == GameRules.InitialLives && level.AcornsTotal == 8 && hud.AcornText == "0 / 8"
                  && hud.HeartsShown == 3, "Fase 1 começa com 0 pontos, 3 vidas e 8 bolotas para coletar");
            Check(hud.ScoreText == "PONTOS  000000", "Pontuação aparece na tela");

            // Colisão com o chão (Tilemap Collider 2D + Composite Collider 2D)
            Check(player.Grounded && Mathf.Abs(player.transform.position.y - 3f) < .06f,
                "Esquilo cai e fica parado em cima do chão do Tilemap" + Pos(player));
            yield return Capture("02-fase1");

            // Andar
            float x0 = player.transform.position.x;
            var sprites = new HashSet<string>();
            player.useManualInput = true;
            player.manualMove = 1f;
            for (float t = 0; t < .7f; t += Time.deltaTime)
            {
                sprites.Add(player.GetComponent<SpriteRenderer>().sprite.name);
                yield return null;
            }
            player.manualMove = 0f;
            Check(player.transform.position.x - x0 > 3f && Mathf.Abs(player.transform.position.y - 3f) < .06f, "Esquilo anda para a direita sobre o chão");
            Check(sprites.Count >= 3, "Esquilo animado ao correr (" + sprites.Count + " quadros vistos)");
            yield return new WaitForSeconds(.3f);

            // Pular e cair de volta no chão
            float maxY = 0f;
            bool jumpSprite = false;
            player.manualJump = true;
            yield return new WaitForFixedUpdate();
            for (float t = 0; t < 1.6f; t += Time.deltaTime)
            {
                maxY = Mathf.Max(maxY, player.transform.position.y);
                jumpSprite |= player.GetComponent<SpriteRenderer>().sprite.name == "esquilo-pulo";
                yield return null;
            }
            Check(maxY > 5.4f && player.Grounded && Mathf.Abs(player.transform.position.y - 3f) < .06f,
                "Pulo sobe mais de 2 blocos e o esquilo volta a pousar no chão (altura máx. " + maxY.ToString("F2") + ")");
            Check(jumpSprite, "Sprite de pulo usado no ar");

            // Parede: o degrau em x = 16 bloqueia quem anda sem pular
            // (invulnerável para a lesma que patrulha o degrau não interferir na medida)
            player.MakeInvulnerable(4f);
            player.TeleportTo(new Vector2(13.5f, 3f));
            player.manualMove = 1f;
            yield return new WaitForSeconds(1f);
            player.manualMove = 0f;
            float wallX = player.transform.position.x;
            Check(wallX > 15.5f && wallX < 15.75f, "Colisão lateral: o degrau do chão funciona como parede" + Pos(player));

            // Subir no degrau com um pulo
            player.manualMove = 1f;
            player.manualJump = true;
            yield return new WaitForSeconds(1f);
            player.manualMove = 0f;
            yield return new WaitForSeconds(.4f);
            Check(player.Grounded && Mathf.Abs(player.transform.position.y - 5f) < .06f, "Pulo sobe no degrau de 2 blocos" + Pos(player));

            yield return TestParallax(player);
            yield return TestAnimations();
            yield return TestCollectibles(level);
            yield return TestEnemies(level, player);
            yield return TestFall(level, player);

            // Pausa
            level.TogglePause();
            float pausedX = player.transform.position.x;
            yield return new WaitForSecondsRealtime(.3f);
            Check(level.Paused && Time.timeScale == 0f && hud.PauseVisible && Mathf.Approximately(pausedX, player.transform.position.x), "P / Esc pausa o jogo");
            yield return Capture("03-pausa");
            level.TogglePause();
            Check(!level.Paused && Time.timeScale == 1f && !hud.PauseVisible, "Pausa: o jogo continua");

            // Juntar as bolotas restantes conclui a fase
            yield return CollectAll(player, CollectibleType.Bolota);
            Check(level.AcornsCollected == 8 && level.IsEnding && hud.BannerTitle == "FASE CONCLUÍDA!", "Coletar as 8 bolotas conclui a fase 1");
            yield return Capture("04-fase-concluida");
            yield return WaitScene("Fase2");
            Check(SceneManager.GetActiveScene().name == "Fase2", "Fase concluída leva à fase 2");
        }

        IEnumerator TestParallax(PlayerController player)
        {
            var cam = Camera.main.transform;
            var layers = FindObjectsByType<Parallax>(FindObjectsSortMode.None);
            Check(layers.Length == 3, "Três camadas de fundo com parallax (nuvens, colinas, bosque)");
            var start = new Dictionary<Parallax, float>();
            foreach (var layer in layers) start[layer] = layer.transform.position.x;
            float camStart = cam.position.x;

            player.TeleportTo(new Vector2(64f, 4f));
            yield return new WaitForSeconds(1.2f);
            float camMove = cam.position.x - camStart;
            Check(camMove > 40f, "Câmera acompanha o esquilo (" + camMove.ToString("F1") + " blocos)");

            bool ratios = true, covered = true;
            string detail = "";
            float halfView = GameRules.CameraSize * GameRules.TargetAspect;
            foreach (var layer in layers)
            {
                float moved = layer.transform.position.x - start[layer];
                // A camada anda parallaxEffect x câmera, a menos dos "pulos" de uma largura do fundo infinito.
                float error = Mathf.Abs(Mathf.Repeat(moved - camMove * layer.parallaxEffect + layer.Length / 2f, layer.Length) - layer.Length / 2f);
                ratios &= error < .05f;
                var bounds = layer.GetComponent<SpriteRenderer>().bounds;
                covered &= bounds.min.x <= cam.position.x - halfView && bounds.max.x >= cam.position.x + halfView;
                detail += " " + layer.name + "=" + layer.parallaxEffect;
            }
            Check(ratios, "Cada camada acompanha uma fração diferente da câmera:" + detail);
            Check(layers[0].parallaxEffect != layers[1].parallaxEffect && layers[1].parallaxEffect != layers[2].parallaxEffect,
                "Camadas em velocidades diferentes (as mais distantes andam menos em relação à tela)");
            Check(covered, "Fundo infinito: as camadas sempre cobrem a largura da tela");
        }

        IEnumerator TestAnimations()
        {
            var snail = FindAnyObjectByType<Snail>();
            var acorn = FindAnyObjectByType<Collectible>();
            string snailSprite = snail.GetComponent<SpriteRenderer>().sprite.name;
            string acornSprite = acorn.GetComponent<SpriteRenderer>().sprite.name;
            bool snailChanged = false, acornChanged = false;
            for (float t = 0; t < 1f; t += Time.deltaTime)
            {
                snailChanged |= snail.GetComponent<SpriteRenderer>().sprite.name != snailSprite;
                acornChanged |= acorn.GetComponent<SpriteRenderer>().sprite.name != acornSprite;
                yield return null;
            }
            Check(snailChanged, "Inimigo (lesma) com sprite animado");
            Check(acornChanged, "Item (bolota) com sprite animado");
        }

        IEnumerator TestCollectibles(LevelManager level)
        {
            var player = level.player;
            int score = GameSession.Score;
            int acorns = level.AcornsCollected; // o pulo no degrau já pode ter pegado a bolota de cima
            yield return Grab(player, Find(CollectibleType.Bolota));
            Check(level.AcornsCollected == acorns + 1 && GameSession.Score == score + GameRules.AcornPoints
                  && level.hud.AcornText == (acorns + 1) + " / 8", "Bolota coletada: +10 pontos e o contador sobe");
            Check(level.hud.ScoreText == "PONTOS  " + GameSession.Score.ToString("D6"), "HUD atualiza a pontuação");

            score = GameSession.Score;
            yield return Grab(player, Find(CollectibleType.Morango));
            Check(GameSession.Score == score + GameRules.StrawberryPoints, "Morango coletado: +50 pontos");

            int lives = GameSession.Lives;
            yield return Grab(player, Find(CollectibleType.Coracao));
            Check(GameSession.Lives == lives + 1 && level.hud.HeartsShown == lives + 1, "Coração coletado: +1 vida");
        }

        IEnumerator TestEnemies(LevelManager level, PlayerController player)
        {
            // Encostar de lado machuca
            var snail = FindAnyObjectByType<Snail>();
            int lives = GameSession.Lives;
            player.ClearInvulnerability();
            player.TeleportTo((Vector2)snail.transform.position + new Vector2(-.5f, 0f));
            for (float t = 0; t < .5f && GameSession.Lives == lives; t += Time.deltaTime) yield return null;
            Check(GameSession.Lives == lives - 1 && player.Invulnerable && level.hud.HeartsShown == lives - 1,
                "Lesma machuca o esquilo: -1 vida e invulnerabilidade curta");
            player.TeleportTo(new Vector2(2.5f, 3f)); // longe da lesma enquanto a invulnerabilidade acaba
            yield return new WaitForSeconds(GameRules.InvulnerableTime + .2f);

            // Pular em cima derrota
            int enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;
            int score = GameSession.Score;
            lives = GameSession.Lives;
            snail = FindAnyObjectByType<Snail>();
            player.TeleportTo((Vector2)snail.transform.position + new Vector2(0f, .8f));
            player.Body.linearVelocity = new Vector2(0f, -4f);
            bool bounced = false;
            for (float t = 0; t < .5f; t += Time.deltaTime)
            {
                bounced |= player.Body.linearVelocity.y > 5f;
                yield return null;
            }
            Check(FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length == enemies - 1 && GameSession.Score == score + GameRules.StompPoints
                  && GameSession.Lives == lives && bounced, "Pular em cima da lesma derrota o inimigo (+100) e o esquilo quica");
        }

        IEnumerator TestFall(LevelManager level, PlayerController player)
        {
            yield return new WaitForSeconds(.5f);
            int lives = GameSession.Lives;
            player.TeleportTo(new Vector2(31.5f, 1f)); // buraco entre x = 30 e 32
            for (float t = 0; t < 2f && GameSession.Lives == lives; t += Time.deltaTime) yield return null;
            yield return new WaitForSeconds(.3f);
            Check(GameSession.Lives == lives - 1 && player.transform.position.y > 2.5f && Vector2.Distance(player.transform.position, level.Checkpoint) < 1f,
                "Cair no buraco tira uma vida e o esquilo volta ao último ponto de retorno" + Pos(player));
        }

        // ------------------------------------------------------------------ Fase 2 e derrota

        IEnumerator TestPhaseTwoAndDefeat()
        {
            yield return new WaitForSeconds(.8f);
            var level = LevelManager.Instance;
            var player = level.player;
            Check(level.phaseIndex == 1 && level.AcornsTotal == 10 && level.hud.AcornText == "0 / 10", "Fase 2 tem 10 bolotas");
            Check(GameSession.Score > 0 && GameSession.AcornsCollected == 8, "Pontos e bolotas da fase 1 continuam na fase 2");
            Check(player.Grounded, "Esquilo pousa no chão da fase 2");

            var bee = FindAnyObjectByType<Bee>();
            Vector2 p0 = bee.transform.position;
            yield return new WaitForSeconds(.5f);
            Vector2 p1 = bee.transform.position;
            Check(Mathf.Abs(p1.x - p0.x) > .1f && Mathf.Abs(p1.y - p0.y) > .01f, "Abelha voa patrulhando");

            player.TeleportTo(new Vector2(28f, 3f));
            yield return new WaitForSeconds(.6f);
            yield return Capture("05-fase2");

            // Abelha machuca ao encostar
            int lives = GameSession.Lives;
            player.ClearInvulnerability();
            for (float t = 0; t < 1f && GameSession.Lives == lives; t += Time.deltaTime)
            {
                player.TeleportTo((Vector2)bee.transform.position + new Vector2(0f, -.5f));
                yield return new WaitForFixedUpdate();
            }
            Check(GameSession.Lives == lives - 1, "Abelha machuca o esquilo");

            // Dano até as vidas acabarem
            while (GameSession.Lives > 0)
            {
                player.ClearInvulnerability();
                player.Hurt(player.transform.position + Vector3.right);
                yield return null;
            }
            Check(level.IsEnding && !player.gameObject.activeSelf, "Sem vidas: o esquilo sai de cena");
            yield return WaitScene(GameRules.EndScene);
            var end = FindAnyObjectByType<EndScreen>();
            Check(end != null && end.TitleText == "FIM DE JOGO" && end.SummaryText.Contains(GameSession.Score.ToString("D6")),
                "Derrota: cena final informa FIM DE JOGO com a pontuação");
            Check(end.RestartButton != null && end.RestartButton.interactable, "Cena final tem botão para reiniciar");
            yield return new WaitForSeconds(.5f);
            yield return Capture("06-derrota");
        }

        // ------------------------------------------------------------------ Reinício e vitória

        IEnumerator TestRestartAndVictory()
        {
            FindAnyObjectByType<EndScreen>().RestartButton.onClick.Invoke();
            yield return WaitScene("Fase1");
            yield return new WaitForSeconds(.6f);
            Check(GameSession.Score == 0 && GameSession.Lives == GameRules.InitialLives && GameSession.AcornsCollected == 0
                  && LevelManager.Instance.AcornsCollected == 0, "Reiniciar começa uma partida nova na fase 1");

            yield return CollectAll(LevelManager.Instance.player, CollectibleType.Bolota);
            yield return WaitScene("Fase2");
            yield return new WaitForSeconds(.6f);
            yield return CollectAll(LevelManager.Instance.player, CollectibleType.Bolota);
            yield return WaitScene(GameRules.EndScene);
            var end = FindAnyObjectByType<EndScreen>();
            Check(GameSession.Victory && end.TitleText == "VITÓRIA!" && GameSession.AcornsCollected == 18 && end.SummaryText.Contains("18 de 18"),
                "Vitória: coletar as 18 bolotas das duas fases mostra VITÓRIA!");
            yield return new WaitForSeconds(.5f);
            yield return Capture("07-vitoria");

            end.MenuButton.onClick.Invoke();
            yield return WaitScene(GameRules.MenuScene);
            Check(SceneManager.GetActiveScene().name == GameRules.MenuScene, "Botão MENU volta para a tela inicial");
        }

        IEnumerator TestWindow()
        {
            if (Application.isBatchMode) yield break; // sem janela (-batchmode) não há o que medir
            Screen.SetResolution(1024, 768, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(.8f);
            var cam = Camera.main;
            var canvas = (RectTransform)FindAnyObjectByType<Canvas>().transform;
            Check(cam.rect.height < .99f && Mathf.Abs(cam.pixelRect.width / cam.pixelRect.height - GameRules.TargetAspect) < .02f
                  && Mathf.Abs(canvas.rect.width / canvas.rect.height - GameRules.TargetAspect) < .02f,
                "Cena e interface mantêm 16:9 numa janela 4:3");
            yield return Capture("08-janela-4x3");
            Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(.5f);
        }

        // ------------------------------------------------------------------ Auxiliares

        static Collectible Find(CollectibleType type)
        {
            Collectible best = null;
            foreach (var item in FindObjectsByType<Collectible>(FindObjectsSortMode.None))
                if (item.type == type && (best == null || item.transform.position.x < best.transform.position.x)) best = item;
            return best;
        }

        // Teleporta o esquilo para cima do item e espera a coleta pela colisão real (trigger).
        static IEnumerator Grab(PlayerController player, Collectible item)
        {
            if (item == null) yield break;
            for (float t = 0; item != null && t < 1f; t += Time.deltaTime)
            {
                player.TeleportTo((Vector2)item.transform.position + new Vector2(0f, -.4f));
                yield return new WaitForFixedUpdate();
            }
            yield return null;
        }

        static IEnumerator CollectAll(PlayerController player, CollectibleType type)
        {
            for (Collectible item = Find(type); item != null; item = Find(type))
            {
                player.MakeInvulnerable(2f); // o teste só quer juntar itens, sem levar dano no caminho
                yield return Grab(player, item);
                if (LevelManager.Instance.IsEnding) yield break;
            }
        }

        IEnumerator WaitScene(string name)
        {
            for (float t = 0; t < 6f && SceneManager.GetActiveScene().name != name; t += Time.unscaledDeltaTime) yield return null;
            yield return null;
            yield return null;
        }

        static string Pos(PlayerController player) => " [x " + player.transform.position.x.ToString("F2") + ", y " + player.transform.position.y.ToString("F2") + "]";

        IEnumerator Capture(string name)
        {
            yield return null;
            if (Application.isBatchMode) yield break;
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(output, name + ".png"));
            yield return new WaitForSecondsRealtime(.15f);
        }

        void Check(bool condition, string description)
        {
            if (condition) { report.checks.Add(description); Debug.Log("PASS: " + description); }
            else { report.failures.Add(description); Debug.LogWarning("FAIL: " + description); }
        }

        void OnLog(string text, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                report.failures.Add(text + "\n" + stack);
        }
    }
}
