using System.Collections.Generic;
using UnityEngine;
using Runtime.Entities;
using Runtime.Data.ValueObject;
using Runtime.Enums;
using Runtime.Utilities;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Runtime.Managers
{
    public class GridManager : SingletonMonoBehaviour<GridManager>
    {
        [Header("Grid Settings")]
        public int _width;
        public int _height;
        public float _spaceModifier;

        [SerializeField] private GameObject cellParent;
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private GameObject obstacleParent;
        [SerializeField] private GameObject obstaclePrefab;
        [SerializeField] private List<Item> _itemsList = new List<Item>();
        [SerializeField] private List<Cell> _cells = new List<Cell>();

        private void OnEnable()
        {
            if (_cells == null) _cells = new List<Cell>();
            if (_itemsList == null) _itemsList = new List<Item>();
        }

        public void Initialize(int width, int height, float spaceModifier)
        {
            _width = width;
            _height = height;
            _spaceModifier = spaceModifier;

            CreateGrid();

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        #region Level Creation

#if UNITY_EDITOR
        private void CreateGrid()
        {
            ClearCells();

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Vector3 position = GridSpaceToWorldSpace(new Vector2Int(x, y));
                    var cellMono = PrefabUtility.InstantiatePrefab(cellPrefab, cellParent.transform);
                    Cell cell = cellMono.GetComponent<Cell>();
                    cell.transform.position = position;
                    cell.name = $"Cell {x}x{y}";
                    cell.Init(new Vector2Int(x, y), false);
                    _cells.Add(cell);

                    EditorUtility.SetDirty(cell.gameObject);
                }
            }

            CreateObstaclesAroundGrid();
        }

        private void CreateObstaclesAroundGrid()
        {
            if (obstacleParent != null)
            {
                DestroyImmediate(obstacleParent);
            }

            obstacleParent = new GameObject("ObstacleParent");

            // Iterate over the border region from x = -1 to _width, and y = -1 to _height.
            for (int x = -1; x <= _width; x++)
            {
                for (int y = -1; y <= _height; y++)
                {
                    // Skip internal cells and ensure only border cells are considered.
                    if (x != -1 && x != _width && y != -1 && y != _height)
                        continue;

                    // Skip the four corners.
                    if ((x == -1 && y == -1) ||
                        (x == -1 && y == _height) ||
                        (x == _width && y == -1) ||
                        (x == _width && y == _height))
                    {
                        continue;
                    }

                    Vector3 position = GridSpaceToWorldSpace(new Vector2Int(x, y));
                    Quaternion rotation = Quaternion.identity;

                    // Determine rotation based on which edge this obstacle is on.
                    if (x == -1 && y >= 0 && y < _height)
                    {
                        rotation = Quaternion.Euler(0, 90, 0);
                    }
                    else if (x == _width && y >= 0 && y < _height)
                    {
                        rotation = Quaternion.Euler(0, -90, 0);
                    }
                    else if (y == -1 && x >= 0 && x < _width)
                    {
                        rotation = Quaternion.Euler(0, 0, 0);
                    }
                    else if (y == _height && x >= 0 && x < _width)
                    {
                        rotation = Quaternion.Euler(0, 180, 0);
                    }

                    GameObject obstacle = (GameObject)PrefabUtility.InstantiatePrefab(obstaclePrefab, obstacleParent.transform);
                    obstacle.transform.position = position;
                    obstacle.transform.rotation = rotation;

                    EditorUtility.SetDirty(obstacle);
                }
            }
        }
#endif

        #endregion

        public void UpdateCellData(LevelData levelData)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    GridData gridData = levelData.GetGrid(x, y);
                    Cell cell = _cells[x * _height + y];

                    if (gridData.isOccupied && gridData.gameColor != GameColor.None)
                    {
                        cell.Init(new Vector2Int(x, y), true);
                    }
                    else
                    {
                        cell.Init(new Vector2Int(x, y), false);
                    }

#if UNITY_EDITOR
                    EditorUtility.SetDirty(cell.gameObject);
#endif
                }
            }
        }

        private void ClearCells()
        {
            if (cellParent != null)
            {
                DestroyImmediate(cellParent);
            }

            cellParent = new GameObject("CellParent");

            _cells.Clear();
        }

        public void AddItem(Item item)
        {
            _itemsList.Add(item);

#if UNITY_EDITOR
            EditorUtility.SetDirty(item.gameObject);
#endif
        }

        public void UpdateAllCellOccupied()
        {
            foreach (var cell in _cells)
            {
                cell.SetOccupied(false);
            }

            foreach (var item in _itemsList)
            {
                foreach (var childItem in item.childItems)
                {
                    Vector2Int gridPosition = WorldSpaceToGridSpace(childItem.transform.position);
                    Cell cell = _cells[gridPosition.x * _height + gridPosition.y];
                    cell.SetOccupied(true);

#if UNITY_EDITOR
                    EditorUtility.SetDirty(cell.gameObject);
#endif
                }
            }
        }

        public List<Item> GetItems()
        {
            return _itemsList;
        }

        public void RemoveItem(Item item)
        {
            if (_itemsList.Contains(item))
            {
                _itemsList.Remove(item);
                CheckItemCount();

#if UNITY_EDITOR
                EditorUtility.SetDirty(item.gameObject);
#endif
            }
        }

        private void CheckItemCount()
        {
            if (_itemsList.Count == 0)
            {
                GameManager.Instance.SetGameStateLevelComplete();
            }
        }

        public void ClearItems()
        {
            _itemsList.Clear();
        }

        public List<Cell> GetCells()
        {
            return _cells;
        }

        public Vector3 GridSpaceToWorldSpace(Vector2Int gridPosition)
        {
            return new Vector3(gridPosition.x * _spaceModifier, 0, gridPosition.y * _spaceModifier);
        }

        public Vector2Int WorldSpaceToGridSpace(Vector3 worldPosition)
        {
            int x = Mathf.RoundToInt(worldPosition.x / _spaceModifier);
            int y = Mathf.RoundToInt(worldPosition.z / _spaceModifier);

            x = Mathf.Clamp(x, 0, _width - 1);
            y = Mathf.Clamp(y, 0, _height - 1);

            return new Vector2Int(x, y);
        }
    }
}