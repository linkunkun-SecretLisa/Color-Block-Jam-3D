#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Entities;
using Runtime.Data.ValueObject;
using Runtime.Enums;
using Runtime.Extensions;
using Unity.VisualScripting;

namespace Runtime.Managers
{
    public class GridManager : SingletonMonoBehaviour<GridManager>
    {
        [Header("Grid Settings Only Debug Dont Change")]
        private int Width;
        private int Height;
        private float SpaceModifier;

        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private GameObject ObstaclePrefab;
        [SerializeField] private GameObject cellParent;
        [SerializeField] private GameObject obstacleParent;
        private List<Item> itemsList = new List<Item>();
        private List<Cell> cells = new List<Cell>();

        private void Start()
        {
            UpdateAllCellOccupied();
        }

        public void Initialize(int width, int height, float spaceModifier)
        {
            Width = width;
            Height = height;
            SpaceModifier = spaceModifier;

            CreateGrid();
        }

        private void CreateGrid()
        {
            ClearCells();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Vector3 position = GridSpaceToWorldSpace(new Vector2Int(x, y));
                    var cellMono = PrefabUtility.InstantiatePrefab(cellPrefab, cellParent.transform);
                    Cell cell = cellMono.GetComponent<Cell>();
                    cell.transform.position = position;
                    cell.name = $"Cell {x}x{y}";
                    cell.Init(new Vector2Int(x, y), false);
                    cells.Add(cell);
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

            for (int x = -1; x <= Width; x++)
            {
                for (int y = -1; y <= Height; y++)
                {
                    if ((x == -1 || x == Width) && (y >= 0 && y < Height))
                    {
                        if (y % Mathf.Max(1, Height / 4) == 0)
                        {
                            Vector3 position = GridSpaceToWorldSpace(new Vector2Int(x, y));
                            Quaternion rotation = (x == -1) ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(0, -90, 0);
                            GameObject obstacle = (GameObject)PrefabUtility.InstantiatePrefab(ObstaclePrefab, obstacleParent.transform);
                            obstacle.transform.position = position;
                            obstacle.transform.rotation = rotation;
                        }
                    }
                    else if ((y == -1 || y == Height) && (x >= 0 && x < Width))
                    {
                        if (x % Mathf.Max(1, Width / 4) == 0)
                        {
                            Vector3 position = GridSpaceToWorldSpace(new Vector2Int(x, y));
                            Quaternion rotation = (y == -1) ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
                            GameObject obstacle = (GameObject)PrefabUtility.InstantiatePrefab(ObstaclePrefab, obstacleParent.transform);
                            obstacle.transform.position = position;
                            obstacle.transform.rotation = rotation;
                        }
                    }
                }
            }
        }

        public void UpdateCellData(LevelData levelData)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    GridData gridData = levelData.GetGrid(x, y);
                    Cell cell = cells[x * Width + y];

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

            cells.Clear();
        }

        public void AddItem(Item item)
        {
            itemsList.Add(item);
        }

        public void UpdateAllCellOccupied()
        {
            foreach (var cell in cells)
            {
                cell.SetOccupied(false);
            }

            foreach (var item in itemsList)
            {
                foreach (var childItem in item.childItems)
                {
                    Vector2Int gridPosition = WorldSpaceToGridSpace(childItem.transform.position);
                    Cell cell = cells[gridPosition.x * Width + gridPosition.y];
                    cell.SetOccupied(true);
                }
            }
        }

        public List<Item> GetItems()
        {
            return itemsList;
        }

        public void RemoveItem(Item item)
        {
            if (itemsList.Contains(item))
            {
                itemsList.Remove(item);
            }
        }

        public void ClearItems()
        {
            itemsList.Clear();
        }

        public List<Cell> GetCells()
        {
            return cells;
        }

        public Vector3 GridSpaceToWorldSpace(Vector2Int gridPosition)
        {
            return new Vector3(gridPosition.x * SpaceModifier, 0, gridPosition.y * SpaceModifier);
        }

        public Vector2Int WorldSpaceToGridSpace(Vector3 worldPosition)
        {
            int x = Mathf.RoundToInt(worldPosition.x / SpaceModifier);
            int y = Mathf.RoundToInt(worldPosition.z / SpaceModifier);

            x = Mathf.Clamp(x, 0, Width - 1);
            y = Mathf.Clamp(y, 0, Height - 1);

            return new Vector2Int(x, y);
        }
    }
}