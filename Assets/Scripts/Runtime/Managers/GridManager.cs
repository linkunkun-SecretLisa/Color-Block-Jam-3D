using System;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Entities;
using Runtime.Data.ValueObject;
using Runtime.Enums;

namespace Runtime.Managers
{
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings Only Debug Dont Change")]
        private int Width;

        private int Height;
        private float SpaceModifier;

        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private GameObject cellParent;
        private List<Item> itemsList = new List<Item>();
        private List<Cell> cells = new List<Cell>();

        private void Start()
        {
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
                    Cell cell = Instantiate(cellPrefab, position, Quaternion.identity, cellParent.transform)
                        .GetComponent<Cell>();
                    cell.name = $"Cell {x}x{y}";
                    cells.Add(cell);
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
                        cell.Init(new Vector2Int(x, y), gridData.gameColor, this);
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

        public void UpdateItemPosition(Item item, Vector2Int newPosition)
        {
            item.GridPosition = newPosition;
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

        public void SetDirty()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}