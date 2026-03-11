using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// In-game runtime map editor using Unity Tilemap.
    /// Supports pencil, eraser and flood-fill tools with undo/redo.
    /// Maps are saved as JSON in <see cref="Application.persistentDataPath"/>.
    /// </summary>
    [RequireComponent(typeof(Grid))]
    public class RuntimeMapEditor : MonoBehaviour
    {
        // ── Enums ─────────────────────────────────────────────────────────────
        public enum EditTool    { Pencil, Eraser, FloodFill, Select }
        public enum TilemapLayer { Ground, Collision, Overlay }

        // ── Inspector ─────────────────────────────────────────────────────────
        [SerializeField] private Tilemap  groundLayer;
        [SerializeField] private Tilemap  collisionLayer;
        [SerializeField] private Tilemap  overlayLayer;
        [SerializeField] private TileBase[] availableTiles;
        [SerializeField] private Camera   mapCamera;

        [Header("Settings")]
        [SerializeField] private int undoStackSize = 32;

        // ── State ─────────────────────────────────────────────────────────────
        public bool      IsEditMode   { get; private set; }
        public EditTool  CurrentTool  { get; private set; } = EditTool.Pencil;
        public int       SelectedTileIndex { get; private set; }

        private readonly Stack<MapEditOperation> _undoStack = new Stack<MapEditOperation>();
        private readonly Stack<MapEditOperation> _redoStack = new Stack<MapEditOperation>();

        // ── Operation record ──────────────────────────────────────────────────
        [Serializable]
        private struct MapEditOperation
        {
            public Vector3Int  cell;
            public string      tileBefore;  // tile name, "" = erase
            public string      tileAfter;
            public TilemapLayer layer;
        }

        // ── Events ────────────────────────────────────────────────────────────
        public event Action OnEditModeEntered;
        public event Action OnEditModeExited;

        // ── Public API ────────────────────────────────────────────────────────
        public void EnterEditMode()
        {
            IsEditMode = true;
            OnEditModeEntered?.Invoke();
            Debug.Log("[MapEditor] Edit mode ON");
        }

        public void ExitEditMode()
        {
            IsEditMode = false;
            OnEditModeExited?.Invoke();
            Debug.Log("[MapEditor] Edit mode OFF");
        }

        public void SelectTile(int index) =>
            SelectedTileIndex = Mathf.Clamp(index, 0, availableTiles.Length - 1);

        public void SelectTool(EditTool tool) => CurrentTool = tool;

        // ── Tile operations ───────────────────────────────────────────────────
        public void PlaceTile(Vector3Int cell, TileBase tile, TilemapLayer layer)
        {
            Tilemap tm    = GetTilemap(layer);
            TileBase prev = tm.GetTile(cell);
            if (prev == tile) return;

            PushUndo(new MapEditOperation
            {
                cell       = cell,
                tileBefore = prev != null ? prev.name : "",
                tileAfter  = tile != null ? tile.name : "",
                layer      = layer
            });

            tm.SetTile(cell, tile);
        }

        public void EraseTile(Vector3Int cell, TilemapLayer layer)
            => PlaceTile(cell, null, layer);

        public void FloodFill(Vector3Int origin, TileBase newTile, TilemapLayer layer)
        {
            Tilemap  tm       = GetTilemap(layer);
            TileBase oldTile  = tm.GetTile(origin);
            if (oldTile == newTile) return;

            var visited = new HashSet<Vector3Int>();
            var queue   = new Queue<Vector3Int>();
            queue.Enqueue(origin);

            while (queue.Count > 0)
            {
                Vector3Int cell = queue.Dequeue();
                if (!visited.Add(cell)) continue;
                if (tm.GetTile(cell) != oldTile) continue;

                PlaceTile(cell, newTile, layer);

                queue.Enqueue(cell + Vector3Int.up);
                queue.Enqueue(cell + Vector3Int.down);
                queue.Enqueue(cell + Vector3Int.left);
                queue.Enqueue(cell + Vector3Int.right);
            }
        }

        // ── Undo / Redo ───────────────────────────────────────────────────────
        public void Undo()
        {
            if (_undoStack.Count == 0) return;
            MapEditOperation op = _undoStack.Pop();
            _redoStack.Push(op);
            ApplyTile(op.cell, op.tileBefore, op.layer);
        }

        public void Redo()
        {
            if (_redoStack.Count == 0) return;
            MapEditOperation op = _redoStack.Pop();
            _undoStack.Push(op);
            ApplyTile(op.cell, op.tileAfter, op.layer);
        }

        // ── Serialisation ─────────────────────────────────────────────────────
        [Serializable]
        private class TileEntry
        {
            public int x, y, z;
            public string tileName;
            public int    layer;
        }

        [Serializable]
        private class MapData { public List<TileEntry> tiles = new List<TileEntry>(); }

        public string SerializeMap()
        {
            var data = new MapData();
            SerializeLayer(groundLayer,    data, (int)TilemapLayer.Ground);
            SerializeLayer(collisionLayer, data, (int)TilemapLayer.Collision);
            SerializeLayer(overlayLayer,   data, (int)TilemapLayer.Overlay);
            return JsonUtility.ToJson(data, true);
        }

        public void DeserializeMap(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            MapData data = JsonUtility.FromJson<MapData>(json);
            if (data == null) return;

            foreach (TileEntry e in data.tiles)
            {
                TileBase tile = FindTileByName(e.tileName);
                Tilemap  tm   = GetTilemap((TilemapLayer)e.layer);
                tm?.SetTile(new Vector3Int(e.x, e.y, e.z), tile);
            }
        }

        public void SaveMapToFile(string filename)
        {
            string path = Path.Combine(Application.persistentDataPath, filename + ".map");
            File.WriteAllText(path, SerializeMap());
            Debug.Log($"[MapEditor] Saved to {path}");
        }

        public void LoadMapFromFile(string filename)
        {
            string path = Path.Combine(Application.persistentDataPath, filename + ".map");
            if (!File.Exists(path)) { Debug.LogWarning($"[MapEditor] File not found: {path}"); return; }
            DeserializeMap(File.ReadAllText(path));
            Debug.Log($"[MapEditor] Loaded from {path}");
        }

        // ── Unity Update ──────────────────────────────────────────────────────
        private void Update()
        {
            if (!IsEditMode || mapCamera == null) return;

            if (!UnityEngine.InputSystem.Mouse.current.leftButton.isPressed) return;

            Vector2    mouseScreen = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            Vector3    worldPos    = mapCamera.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 0));
            Vector3Int cell        = groundLayer.WorldToCell(worldPos);

            switch (CurrentTool)
            {
                case EditTool.Pencil:
                    TileBase tile = availableTiles != null && availableTiles.Length > SelectedTileIndex
                        ? availableTiles[SelectedTileIndex] : null;
                    PlaceTile(cell, tile, TilemapLayer.Ground);
                    break;

                case EditTool.Eraser:
                    EraseTile(cell, TilemapLayer.Ground);
                    break;
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private Tilemap GetTilemap(TilemapLayer layer) => layer switch
        {
            TilemapLayer.Ground    => groundLayer,
            TilemapLayer.Collision => collisionLayer,
            TilemapLayer.Overlay   => overlayLayer,
            _                      => groundLayer
        };

        private void PushUndo(MapEditOperation op)
        {
            if (_undoStack.Count >= undoStackSize) return;
            _undoStack.Push(op);
            _redoStack.Clear();
        }

        private void ApplyTile(Vector3Int cell, string tileName, TilemapLayer layer)
        {
            TileBase tile = string.IsNullOrEmpty(tileName) ? null : FindTileByName(tileName);
            GetTilemap(layer)?.SetTile(cell, tile);
        }

        private TileBase FindTileByName(string tileName)
        {
            if (string.IsNullOrEmpty(tileName) || availableTiles == null) return null;
            foreach (var t in availableTiles)
                if (t != null && t.name == tileName) return t;
            return null;
        }

        private void SerializeLayer(Tilemap tm, MapData data, int layerIndex)
        {
            if (tm == null) return;
            BoundsInt bounds = tm.cellBounds;
            foreach (Vector3Int cell in bounds.allPositionsWithin)
            {
                TileBase tile = tm.GetTile(cell);
                if (tile == null) continue;
                data.tiles.Add(new TileEntry
                {
                    x = cell.x, y = cell.y, z = cell.z,
                    tileName = tile.name, layer = layerIndex
                });
            }
        }
    }
}
