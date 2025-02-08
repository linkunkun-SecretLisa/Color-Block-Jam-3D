using System.Collections.Generic;
using Runtime.Classes;
using UnityEngine;
using Runtime.Entities;
using Runtime.Data.ValueObject;
using Runtime.Extensions;
using UnityEngine.Serialization;

namespace Runtime.Managers
{
    public class GridManager : SingletonMonoBehaviour<GridManager>
    {
        [Header("Grid Settings Only Debug Dont Change")]
        private int Width;
        private int Height;
        private float SpaceModifier;

        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private GameObject obstaclePrefab;
        [SerializeField] public GameObject cellParent;
        [SerializeField] private GameObject obstacleParent;
        private List<Item> _itemsList = new List<Item>();
        private CellsClass _cellsClass;

        public void Initialize(int width, int height, float spaceModifier)
        {
            Width = width;
            Height = height;
            SpaceModifier = spaceModifier;
            _cellsClass = new CellsClass(Width, Height, cellPrefab, cellParent, SpaceModifier);
            CreateGrid();
        }

        private void CreateGrid()
        {
            _cellsClass.ClearCells();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    _cellsClass.CreateCell(x, y);
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
                            Instantiate(obstaclePrefab, position, rotation, obstacleParent.transform);
                        }
                    }
                    else if ((y == -1 || y == Height) && (x >= 0 && x < Width))
                    {
                        if (x % Mathf.Max(1, Width / 4) == 0)
                        {
                            Vector3 position = GridSpaceToWorldSpace(new Vector2Int(x, y));
                            Quaternion rotation = (y == -1) ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
                            Instantiate(obstaclePrefab, position, rotation, obstacleParent.transform);
                        }
                    }
                }
            }
        }

        public void UpdateCellData(LevelData levelData)
        {
            _cellsClass.UpdateCellData(levelData);
        }
        public void UpdateAllCellOccupied()
        {
            _cellsClass.UpdateAllCellOccupied(_itemsList);
        }

        public void AddItem(Item item)
        {
            _itemsList.Add(item);
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
            return _cellsClass.GetCells();
        }

        public Vector3 GridSpaceToWorldSpace(Vector2Int gridPosition)
        {
            return new Vector3(gridPosition.x * SpaceModifier, 0, gridPosition.y * SpaceModifier);
        }

        public Vector2Int WorldSpaceToGridSpace(Vector3 worldPosition)
        {
            return _cellsClass.WorldSpaceToGridSpace(worldPosition);
        }
    }
}