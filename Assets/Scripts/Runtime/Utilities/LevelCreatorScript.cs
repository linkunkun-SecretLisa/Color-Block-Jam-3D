using Runtime.Data.UnityObject;
using Runtime.Data.ValueObject;
using Runtime.Entities;
using Runtime.Enums;
using Runtime.Managers;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Runtime.Utilities
{
    [ExecuteInEditMode]
    public class LevelCreatorScript : MonoBehaviour
    {
        [Header("Grid Settings")] 
        public int Width;
        public int Height;
        [Range(0f, 100f)] public float spaceModifier = 50f;
        [Range(50f, 100f)] public float gridSize = 50f;

        [Header("References")]
        public CD_LevelData LevelData;
        public CD_GameColor colorData;
        public CD_GamePrefab itemPrefab;
        public GameObject itemsParentObject;
        public GridManager gridManager;
        public ItemSize itemSize; // item sizes like 1x1, 2x2, 3x2; when one is selected, the item can be placed on the grid
        public GameColor gameColor;
        private LevelData _currentLevelData;

        private void OnEnable()
        {
            if (LevelData == null)
            {
                Debug.LogError("LevelData is not assigned in the inspector!");
                return;
            }

            if (LevelData.levelData == null)
            {
                LevelData.levelData = new LevelData();
            }

            SetCurrentLevelData();
        }

        public void GenerateLevelData()
        {
            if (itemsParentObject != null)
            {
                DestroyImmediate(itemsParentObject);
                gridManager.ClearItems();
            }
            
            gridManager.Initialize(Width, Height, spaceModifier);
            itemsParentObject = new GameObject("LevelParent");

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    GridData gridCell = LevelData.levelData.GetGrid(x, y);
                    if (gridCell.isOccupied && gridCell.gameColor != GameColor.None &&
                        gridCell.ItemSize != ItemSize.None)
                    {
                        Vector3 spawnPosition = GridSpaceToWorldSpace(x, y);
                        SpawnItem(gridCell, spawnPosition, x, y);
                    }
                }
            }

            gridManager.UpdateCellData(LevelData.levelData);
            Debug.Log("Grid generated.");
        }

        private void SpawnItem(GridData gridCell, Vector3 spawnPosition, int x, int y)
        {
            if (gridCell.ItemSize == ItemSize.OneByOne)
            {
                MonoBehaviour item = (MonoBehaviour)PrefabUtility.InstantiatePrefab(
                    itemPrefab.gamePrefab[(int)gridCell.ItemSize].prefab, itemsParentObject.transform);
                item.transform.position = spawnPosition + new Vector3(0, 0.25f, 0);
                item.GetComponent<Item>().Init(new Vector2Int(x, y), gridCell.gameColor, gridManager);
            }

            if (gridCell.ItemSize == ItemSize.TwoByTwo)
            {
                var gridcellPos = gridCell.position;
                var left = LevelData.levelData.GetGrid(gridcellPos.x - 1, gridcellPos.y);
                var up = LevelData.levelData.GetGrid(gridcellPos.x, gridcellPos.y + 1);

                if ((gridCell.gameColor == up.gameColor) && (gridCell.gameColor == left.gameColor))
                {
                    MonoBehaviour item = (MonoBehaviour)PrefabUtility.InstantiatePrefab(
                        itemPrefab.gamePrefab[(int)gridCell.ItemSize].prefab, itemsParentObject.transform);
                    item.transform.position = spawnPosition + new Vector3(0, 0.25f, 0);
                    item.GetComponent<Item>().Init(new Vector2Int(x, y), gridCell.gameColor, gridManager);
                }
            }

            if (gridCell.ItemSize == ItemSize.ThreeByTwo)
            {
                var gridcellPos = gridCell.position;
                var left = LevelData.levelData.GetGrid(gridcellPos.x - 1, gridcellPos.y);
                var right = LevelData.levelData.GetGrid(gridcellPos.x + 1, gridcellPos.y);
                var up = LevelData.levelData.GetGrid(gridcellPos.x, gridcellPos.y + 1);

                if ((gridCell.gameColor == up.gameColor) && (gridCell.gameColor == left.gameColor) && (gridCell.gameColor == right.gameColor))
                {
                    MonoBehaviour item = (MonoBehaviour)PrefabUtility.InstantiatePrefab(itemPrefab.gamePrefab[(int)gridCell.ItemSize].prefab, itemsParentObject.transform);
                    item.transform.position = spawnPosition + new Vector3(0, 0.25f, 0);
                    item.GetComponent<Item>().Init(new Vector2Int(x, y), gridCell.gameColor, gridManager);
                }
            }
        }

        public Vector3 GridSpaceToWorldSpace(int x, int y)
        {
            return new Vector3(x * spaceModifier, 0, y * spaceModifier);
        }

        public Vector2Int WorldSpaceToGridSpace(Vector3 worldPosition)
        {
            int x = Mathf.RoundToInt(worldPosition.x / spaceModifier);
            int y = Mathf.RoundToInt(worldPosition.z / spaceModifier);
            return new Vector2Int(x, y);
        }

        private void SetCurrentLevelData()
        {
            if (LevelData.levelData.Grids == null || LevelData.levelData.Grids.Length != Width * Height)
            {
                LevelData.levelData.Width = Width;
                LevelData.levelData.Height = Height;
                LevelData.levelData.Grids = new GridData[Width * Height];
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        int index = y * Width + x;
                        LevelData.levelData.Grids[index] = new GridData
                        {
                            isOccupied = false,
                            gameColor = GameColor.None,
                            position = new Vector2Int(x, y),
                            ItemSize = ItemSize.None
                        };
                    }
                }
            }

            _currentLevelData = new LevelData
            {
                Width = Width,
                Height = Height,
                Grids = new GridData[LevelData.levelData.Grids.Length]
            };

            for (int i = 0; i < LevelData.levelData.Grids.Length; i++)
            {
                _currentLevelData.Grids[i] = LevelData.levelData.Grids[i];
            }
        }

        public void ToggleGridOccupancy(int x, int y)
        {
            Vector2Int[] offsets = GetOffsetsForItemSizeAndRotation(itemSize);
            foreach (var offset in offsets)
            {
                int targetX = x + offset.x;
                int targetY = y + offset.y;
                if (targetX >= 0 && targetX < Width && targetY >= 0 && targetY < Height)
                {
                    GridData grid = _currentLevelData.GetGrid(targetX, targetY);
                    grid.isOccupied = !grid.isOccupied;
                    grid.gameColor = gameColor;
                    grid.ItemSize = itemSize;
                    _currentLevelData.SetGrid(targetX, targetY, grid);
                }
            }
        }

        private Vector2Int[] GetOffsetsForItemSizeAndRotation(ItemSize size)
        {
            switch (size)
            {
                case ItemSize.OneByOne:
                    return new[] { new Vector2Int(0, 0) };
                case ItemSize.TwoByTwo:
                    return new[] { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1) };
                case ItemSize.ThreeByTwo:
                    return new[]
                        { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1) };
                default:
                    return new[] { new Vector2Int(0, 0) };
            }
        }

        public LevelData GetCurrentLevelData()
        {
            return _currentLevelData;
        }

        public int GetRows()
        {
            return Height;
        }

        public int GetColumns()
        {
            return Width;
        }

        public void SaveLevelData()
        {
            if (LevelData.levelData.Grids == null || LevelData.levelData.Grids.Length != _currentLevelData.Grids.Length)
            {
                LevelData.levelData.Grids = new GridData[_currentLevelData.Grids.Length];
            }

            for (int i = 0; i < _currentLevelData.Grids.Length; i++)
            {
                LevelData.levelData.Grids[i] = _currentLevelData.Grids[i];
            }

            LevelData.levelData.Width = _currentLevelData.Width;
            LevelData.levelData.Height = _currentLevelData.Height;
            Debug.Log("Level data saved.");
        }

        public void LoadLevelData()
        {
            SetCurrentLevelData();
            Debug.Log("Level data loaded.");
        }

        public void ResetGridData()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    GridData gridData = new GridData
                    {
                        isOccupied = false,
                        gameColor = GameColor.None,
                        position = new Vector2Int(x, y),
                        ItemSize = ItemSize.None
                    };
                    LevelData.levelData.SetGrid(x, y, gridData);
                    _currentLevelData.SetGrid(x, y, gridData);
                }
            }

            Debug.Log("Grid reset.");
        }

        public Color GetGridColor(Vector2Int position)
        {
            GridData grid = _currentLevelData.GetGrid(position.x, position.y);
            return grid.isOccupied ? colorData.gameColorsData[(int)grid.gameColor].color : Color.white;
        }

        public Color GetSelectedGridColor()
        {
            foreach (var data in colorData.gameColorsData)
            {
                if (data.gameColor == gameColor)
                {
                    return data.color;
                }
            }

            return Color.white;
        }

        public void SetGridColor(int x, int y)
        {
            GridData grid = _currentLevelData.GetGrid(x, y);
            grid.gameColor = gameColor;
            _currentLevelData.SetGrid(x, y, grid);
        }

        private void OnDrawGizmos()
        {
            if (_currentLevelData == null) return;

            Gizmos.color = Color.red;
            Vector2Int[] offsets = GetOffsetsForItemSizeAndRotation(itemSize);
            foreach (var offset in offsets)
            {
                Vector3 worldPos = GridSpaceToWorldSpace(offset.x, offset.y);
                Gizmos.DrawWireCube(worldPos, new Vector3(gridSize, 0.1f, gridSize));
            }
        }
    }
}