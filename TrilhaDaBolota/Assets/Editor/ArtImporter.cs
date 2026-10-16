using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TrilhaDaBolota.Editor
{
    // Faz por código o mesmo passo a passo dos slides:
    //   Sprite Mode = Multiple, Pixels Per Unit = 16, Mesh Type = Full Rect,
    //   Sprite Editor > Slice > Grid By Cell Size (16 x 16), cria os Tiles e a Tile Palette.
    public static class ArtImporter
    {
        public const string ArtPath = "Assets/Art/";
        public const string GeneratedPath = "Assets/Art/Gerados/";
        public const string TilesetName = "tilesetOpenGame";
        public const int TileSize = 16;
        const string TilesPath = "Assets/Tiles/";

        public static void ImportAll()
        {
            Directory.CreateDirectory(TilesPath + "Tileset");
            Directory.CreateDirectory(TilesPath + "Chao");
            Directory.CreateDirectory(TilesPath + "Paleta");
            AssetDatabase.Refresh();

            SliceGrid(ArtPath + TilesetName + ".png");
            SliceGrid(GeneratedPath + "chao.png");
            foreach (var layer in new[] { ArtPath + "tilesetOpenGameBackground.png", GeneratedPath + "colinas.png", GeneratedPath + "bosque.png" })
                ImportBackground(layer);
            foreach (string path in Directory.GetFiles(GeneratedPath, "*.png"))
            {
                string name = Path.GetFileNameWithoutExtension(path);
                if (name == "chao" || name == "colinas" || name == "bosque") continue;
                // Personagens que andam no chão têm o pivô nos pés; o resto, no centro.
                bool feet = name.StartsWith("esquilo") || name.StartsWith("lesma");
                ImportSingle(path.Replace('\\', '/'), feet ? new Vector2(.5f, 0f) : new Vector2(.5f, .5f));
            }
            CreateTiles(ArtPath + TilesetName + ".png", "Tileset", Tile.ColliderType.None);
            CreateTiles(GeneratedPath + "chao.png", "Chao", Tile.ColliderType.Grid);
            CreatePalette();
        }

        static TextureImporter PixelArtImporter(string path)
        {
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = TileSize;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            return importer;
        }

        static void ImportSingle(string path, Vector2 pivot)
        {
            var importer = PixelArtImporter(path);
            importer.spriteImportMode = SpriteImportMode.Single;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        // Camadas do parallax: repetidas lado a lado (Draw Mode = Tiled), por isso Wrap = Repeat.
        static void ImportBackground(string path)
        {
            var importer = PixelArtImporter(path);
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.SaveAndReimport();
        }

        // Equivalente a "Slice > Grid By Cell Size 16x16" (blocos vazios são ignorados).
        static void SliceGrid(string path)
        {
            var importer = PixelArtImporter(path);
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.SaveAndReimport();

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var pixels = ReadPixels(path, texture.width, texture.height);
            string baseName = Path.GetFileNameWithoutExtension(path);
            int columns = texture.width / TileSize, rows = texture.height / TileSize;
            var rects = new List<SpriteRect>();
            var ids = new List<SpriteNameFileIdPair>();
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    // Linha contada de cima para baixo (como no editor); o rect do Unity começa embaixo.
                    var rect = new Rect(column * TileSize, texture.height - (row + 1) * TileSize, TileSize, TileSize);
                    if (IsEmpty(pixels, texture.width, rect)) continue;
                    var spriteRect = new SpriteRect
                    {
                        name = baseName + "_" + (row * columns + column),
                        rect = rect,
                        alignment = SpriteAlignment.Center,
                        pivot = new Vector2(.5f, .5f),
                        spriteID = GUID.Generate()
                    };
                    rects.Add(spriteRect);
                    ids.Add(new SpriteNameFileIdPair(spriteRect.name, spriteRect.spriteID));
                }
            }

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            provider.SetSpriteRects(rects.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(ids);
            provider.Apply();
            importer.SaveAndReimport();
        }

        static Color32[] ReadPixels(string path, int width, int height)
        {
            var copy = new Texture2D(width, height, TextureFormat.RGBA32, false);
            copy.LoadImage(File.ReadAllBytes(path));
            var pixels = copy.GetPixels32();
            Object.DestroyImmediate(copy);
            return pixels;
        }

        static bool IsEmpty(Color32[] pixels, int width, Rect rect)
        {
            for (int y = (int)rect.y; y < rect.yMax; y++)
                for (int x = (int)rect.x; x < rect.xMax; x++)
                    if (pixels[y * width + x].a > 0) return false;
            return true;
        }

        // Um Tile asset por sprite recortado. Chão: colisão em bloco cheio (Grid). Decoração: sem colisão.
        static void CreateTiles(string texturePath, string folder, Tile.ColliderType collider)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetRepresentationsAtPath(texturePath))
            {
                if (asset is not Sprite sprite) continue;
                string tilePath = TilesPath + folder + "/" + sprite.name + ".asset";
                var tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
                if (tile == null)
                {
                    tile = ScriptableObject.CreateInstance<Tile>();
                    AssetDatabase.CreateAsset(tile, tilePath);
                }
                tile.sprite = sprite;
                tile.colliderType = collider;
                EditorUtility.SetDirty(tile);
            }
            AssetDatabase.SaveAssets();
        }

        public static Tile TilesetTile(int index) => AssetDatabase.LoadAssetAtPath<Tile>(TilesPath + "Tileset/" + TilesetName + "_" + index + ".asset");
        public static Tile GroundTile(int row, int column) => AssetDatabase.LoadAssetAtPath<Tile>(TilesPath + "Chao/chao_" + (row * 4 + column) + ".asset");

        // Tile Palette "Floresta" com os blocos de chão e todo o tileset, para editar as fases à mão.
        static void CreatePalette()
        {
            const string palettePath = TilesPath + "Paleta/Floresta.prefab";
            if (File.Exists(palettePath)) AssetDatabase.DeleteAsset(palettePath);
            var palette = GridPaletteUtility.CreateNewPalette(TilesPath + "Paleta", "Floresta", GridLayout.CellLayout.Rectangle,
                GridPalette.CellSizing.Automatic, Vector3.one, GridLayout.CellSwizzle.XYZ);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(palette);
            var tilemap = instance.GetComponentInChildren<Tilemap>();
            for (int row = 0; row < 5; row++)
                for (int column = 0; column < 4; column++)
                    tilemap.SetTile(new Vector3Int(column, -row, 0), GroundTile(row, column));
            for (int index = 0; index < 20 * 12; index++)
            {
                var tile = TilesetTile(index);
                if (tile != null) tilemap.SetTile(new Vector3Int(6 + index % 20, -(index / 20), 0), tile);
            }
            PrefabUtility.SaveAsPrefabAsset(instance, palettePath);
            Object.DestroyImmediate(instance);
        }

        public static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(GeneratedPath + name + ".png");
        public static Sprite Background(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);

        public static Sprite[] Frames(string prefix, int count)
        {
            var frames = new Sprite[count];
            for (int i = 0; i < count; i++) frames[i] = Sprite(prefix + "-" + (i + 1));
            return frames;
        }
    }
}
