using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TrilhaDaBolota.Editor
{
    // Conferência estática do projeto montado (sem rodar o jogo).
    public static class ProjectChecks
    {
        [MenuItem("Trilha da Bolota/3. Verificar laboratório")]
        public static void Verify()
        {
            var failures = new List<string>();
            void Check(bool ok, string description)
            {
                if (ok) Debug.Log("OK: " + description);
                else { Debug.LogError("FALHOU: " + description); failures.Add(description); }
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(ArtImporter.ArtPath + ArtImporter.TilesetName + ".png");
            Check(importer.spriteImportMode == SpriteImportMode.Multiple && Mathf.Approximately(importer.spritePixelsPerUnit, 16f),
                "Tileset em Sprite Mode Multiple com 16 Pixels Per Unit");
            int slices = 0;
            foreach (var asset in AssetDatabase.LoadAllAssetRepresentationsAtPath(ArtImporter.ArtPath + ArtImporter.TilesetName + ".png"))
                if (asset is Sprite sprite && sprite.rect.width == 16 && sprite.rect.height == 16) slices++;
            Check(slices > 50, "Tileset recortado em blocos de 16x16 (" + slices + " sprites)");
            Check(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Tiles/Paleta/Floresta.prefab") != null, "Tile Palette 'Floresta' criada");
            foreach (string prefab in new[] { "Bolota", "Morango", "Coracao", "Lesma", "Abelha" })
                Check(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/" + prefab + ".prefab") != null, "Prefab " + prefab);

            foreach (string sceneName in GameRules.PhaseScenes)
            {
                EditorSceneManager.OpenScene("Assets/Scenes/" + sceneName + ".unity");
                var ground = GameObject.Find("Chao");
                Check(ground != null && ground.GetComponent<TilemapCollider2D>() != null && ground.GetComponent<CompositeCollider2D>() != null
                      && ground.GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Static
                      && ground.GetComponent<CompositeCollider2D>().pathCount > 0,
                    sceneName + ": chão com Tilemap Collider 2D + Composite Collider 2D + Rigidbody 2D estático");
                Check(Object.FindObjectsByType<Parallax>(FindObjectsSortMode.None).Length == 3, sceneName + ": três camadas de parallax");
                Check(Object.FindAnyObjectByType<PlayerController>() != null, sceneName + ": esquilo na cena");
                Check(Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length >= 4, sceneName + ": inimigos na cena");
            }
            EditorSceneManager.OpenScene("Assets/Scenes/" + GameRules.MenuScene + ".unity");

            if (failures.Count > 0) throw new System.Exception("Verificação falhou: " + string.Join("; ", failures));
            Debug.Log("TRILHA_CHECKS_PASSED");
        }
    }
}
