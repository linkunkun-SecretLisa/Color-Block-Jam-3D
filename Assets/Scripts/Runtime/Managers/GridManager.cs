#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Entities;
using Runtime.Data.ValueObject;
using Runtime.Enums;
using Runtime.Utilities;
using Unity.VisualScripting;
using UnityEngine.Serialization;

namespace Runtime.Managers
{
    public class GridManager : SingletonMonoBehaviour<GridManager>
    {
        [Header("Grid Settings")]
        private int _width;
        private int _height;
        private float _spaceModifier;

        [SerializeField] private GameObject cellParent;
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private GameObject obstacleParent;
        [SerializeField] private GameObject obstaclePrefab;
        private readonly List<Item> _itemsList = new List<Item>();
        private readonly List<Cell> _cells = new List<Cell>();


        public void Initialize(int width, int height, float spaceModifier)
        {
            _width = width;
            _height = height;
            _spaceModifier = spaceModifier;

            CreateGrid();
        }

        #region Level Creation

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

            for (int x = -1; x <= _width; x++)
            {
                for (int y = -1; y <= _height; y++)
                {
                    if ((x == -1 || x == _width) && (y >= 0 && y < _height))
                    {
                        if (y % Mathf.Max(1, _height / 4) == 0)
                        {
                            Vector3 position = GridSpaceToWorldSpace(new Vector2Int(x, y));
                            Quaternion rotation = (x == -1) ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(0, -90, 0);
                            GameObject obstacle = (GameObject)PrefabUtility.InstantiatePrefab(obstaclePrefab, obstacleParent.transform);
                            obstacle.transform.position = position;
                            obstacle.transform.rotation = rotation;
                        }
                    }
                    else if ((y == -1 || y == _height) && (x >= 0 && x < _width))
                    {
                        if (x % Mathf.Max(1, _width / 4) == 0)
                        {
                            Vector3 position = GridSpaceToWorldSpace(new Vector2Int(x, y));
                            Quaternion rotation = (y == -1) ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
                            GameObject obstacle = (GameObject)PrefabUtility.InstantiatePrefab(obstaclePrefab, obstacleParent.transform);
                            obstacle.transform.position = position;
                            obstacle.transform.rotation = rotation;
                        }
                    }
                }
            }
        }

        #endregion

        public void UpdateCellData(LevelData levelData)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    GridData gridData = levelData.GetGrid(x, y);
                    Cell cell = _cells[x * _width + y];

                    if (gridData.isOccupied && gridData.gameColor != GameColor.None)
                    {
                        cell.Init(new Vector2Int(x, y), true);
                    }
                    else
                    {
                        cell.Init(new Vector2Int(x, y), false);
                    }
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
                    Cell cell = _cells[gridPosition.x * _width + gridPosition.y];
                    cell.SetOccupied(true);
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