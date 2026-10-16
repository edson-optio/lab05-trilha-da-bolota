using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TrilhaDaBolota.Editor
{
    // Lê o mapa de texto da fase (Assets/Fases/faseN.txt) e monta os Tilemaps e os objetos.
    public class LevelBuilder
    {
        // Objetos de decoração do tileset: (coluna, linha, largura, altura) em blocos de 16 px,
        // com a linha contada de cima. O canto de baixo à esquerda fica no caractere do mapa.
        static readonly Dictionary<char, RectInt> Decorations = new Dictionary<char, RectInt>
        {
            { 'T', new RectInt(13, 1, 6, 7) },  // árvore
            { 'F', new RectInt(2, 0, 6, 2) },   // cerca
            { 'W', new RectInt(2, 4, 2, 2) },   // girassóis
            { 'R', new RectInt(2, 2, 2, 2) },   // pedra
            { 'G', new RectInt(12, 8, 2, 4) },  // poste
            { 'C', new RectInt(2, 7, 2, 1) },   // baú
            { 'H', new RectInt(0, 1, 2, 7) },   // escada
            { 'S', new RectInt(6, 4, 2, 2) },   // placa (ponto de retorno)
        };

        public string[] Rows { get; private set; }
        public int Width => Rows[0].Length;
        public int Height => Rows.Length;
        public Vector2 PlayerStart { get; private set; }

        readonly Prefabs prefabs;

        public struct Prefabs
        {
            public GameObject acorn, strawberry, heart, snail, bee;
        }

        public LevelBuilder(string mapPath, Prefabs prefabs)
        {
            this.prefabs = prefabs;
            var rows = new List<string>();
            foreach (string line in File.ReadAllLines(mapPath))
                if (line.Length > 0 && !line.StartsWith("# ")) rows.Add(line);
            Rows = rows.ToArray();
            foreach (string row in Rows)
                if (row.Length != Width) throw new Exception(mapPath + ": todas as linhas precisam ter " + Width + " caracteres");
        }

        // Caractere na posição (x, y), com y = 0 na linha de baixo do mapa.
        char At(int x, int y) => Rows[Height - 1 - y][x];

        // Fora do mapa pelos lados e por baixo conta como chão, para não desenhar bordas na moldura.
        bool Solid(int x, int y)
        {
            if (y >= Height) return false;
            if (x < 0 || x >= Width || y < 0) return true;
            return At(x, y) == '#';
        }

        public void Build(Transform parent)
        {
            var grid = new GameObject("Grid", typeof(Grid)).transform;
            grid.SetParent(parent, false);
            Tilemap decoration = CreateTilemap("Decoracao", grid, 0);
            Tilemap ground = CreateTilemap("Chao", grid, 5);

            var objects = new GameObject("Objetos").transform;
            objects.SetParent(parent, false);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    char c = At(x, y);
                    if (c == '#') ground.SetTile(new Vector3Int(x, y, 0), GroundTileFor(x, y));
                    else PlaceObject(c, x, y, decoration, objects);
                }
            }
            // A colisão entra depois de pintar os blocos, para o Composite já nascer com a forma do chão.
            AddGroundCollision(ground.gameObject);
            AddSideWalls(grid);
        }

        static Tilemap CreateTilemap(string name, Transform grid, int order)
        {
            var go = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
            go.transform.SetParent(grid, false);
            go.GetComponent<TilemapRenderer>().sortingOrder = order;
            return go.GetComponent<Tilemap>();
        }

        // Os três componentes dos slides: Tilemap Collider 2D + Composite Collider 2D + Rigidbody 2D estático.
        static void AddGroundCollision(GameObject ground)
        {
            ground.layer = ProjectBuilder.GroundLayer;
            var body = ground.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;
            var tilemapCollider = ground.AddComponent<TilemapCollider2D>();
            tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
            var composite = ground.AddComponent<CompositeCollider2D>();
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
            tilemapCollider.ProcessTilemapChanges();
            composite.GenerateGeometry();
        }

        // Escolhe o bloco certo do chao.png olhando os vizinhos (topo, terra, borda, plataforma fina).
        Tile GroundTileFor(int x, int y)
        {
            bool up = Solid(x, y + 1), down = Solid(x, y - 1);
            bool left = Solid(x - 1, y), right = Solid(x + 1, y);
            int row;
            if (!up && !down) row = 3;            // plataforma de um bloco só
            else if (!up) row = 0;                // grama do topo
            else if (!down) row = 4;              // base de bloco flutuante
            else if (!Solid(x, y + 2)) row = 1;   // terra com pedras logo abaixo da grama
            else row = 2;                         // terra funda
            int column = !left ? 0 : !right ? 3 : 1 + x % 2;
            return ArtImporter.GroundTile(row, column);
        }

        void PlaceObject(char c, int x, int y, Tilemap decoration, Transform objects)
        {
            Vector2 cellCenter = new Vector2(x + .5f, y + .5f);
            if (Decorations.TryGetValue(c, out RectInt area))
            {
                PaintDecoration(decoration, area, x, y);
                if (c == 'S') CreateCheckpoint(objects, new Vector2(x + 1f, y));
                return;
            }
            switch (c)
            {
                case 'P': PlayerStart = new Vector2(x + .5f, y); break;
                case 'a': Spawn(prefabs.acorn, cellCenter, objects); break;
                case 's': Spawn(prefabs.strawberry, cellCenter, objects); break;
                case 'h': Spawn(prefabs.heart, cellCenter, objects); break;
                case 'L': Spawn(prefabs.snail, new Vector2(x + .5f, y), objects); break;
                case 'B': Spawn(prefabs.bee, cellCenter, objects); break;
                case '.': break;
                default: throw new Exception("Caractere desconhecido no mapa: '" + c + "' em " + x + "," + y);
            }
        }

        static void PaintDecoration(Tilemap tilemap, RectInt area, int x, int y)
        {
            for (int dy = 0; dy < area.height; dy++)
            {
                for (int dx = 0; dx < area.width; dx++)
                {
                    int sourceRow = area.y + area.height - 1 - dy;
                    var tile = ArtImporter.TilesetTile(sourceRow * 20 + area.x + dx);
                    if (tile != null) tilemap.SetTile(new Vector3Int(x + dx, y + dy, 0), tile);
                }
            }
        }

        static void Spawn(GameObject prefab, Vector2 position, Transform parent)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.position = position;
        }

        static void CreateCheckpoint(Transform parent, Vector2 position)
        {
            var checkpoint = new GameObject("Placa (ponto de retorno)", typeof(BoxCollider2D), typeof(Checkpoint));
            checkpoint.transform.SetParent(parent, false);
            checkpoint.transform.position = position;
            var box = checkpoint.GetComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(1f, 4f);
            box.offset = new Vector2(0f, 2f);
        }

        // Paredes invisíveis nas pontas do mapa para o esquilo não sair pela lateral.
        void AddSideWalls(Transform grid)
        {
            foreach (float x in new[] { -.5f, Width + .5f })
            {
                var wall = new GameObject("Limite", typeof(BoxCollider2D));
                wall.layer = ProjectBuilder.GroundLayer;
                wall.transform.SetParent(grid, false);
                wall.transform.position = new Vector3(x, Height, 0f);
                wall.GetComponent<BoxCollider2D>().size = new Vector2(1f, Height * 2f + 10f);
            }
        }
    }
}
