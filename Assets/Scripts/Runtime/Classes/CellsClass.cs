using System.Collections.Generic;
using Runtime.Data.ValueObject;
using Runtime.Entities;
using Runtime.Enums;
using Runtime.Managers;
using UnityEngine;

namespace Runtime.Classes
{
    public class CellsClass
    {
        private int Width;
        private int Height;
        private GameObject cellPrefab;
        private GameObject cellParent;
        private float SpaceModifier;
        private List<Cell> cells = new List<Cell>();

        public CellsClass(int width, int height, GameObject cellPrefab, GameObject cellParent, float spaceModifier)
        {
            Width = width;
            Height = height;
            this.cellPrefab = cellPrefab;
            this.cellParent = cellParent;
            SpaceModifier = spaceModifier;
        }

        public void CreateCell(int x, int y)
        {
            Vector3 position = GridSpaceToWorldSpace(new Vector2Int(x, y));
            Cell cell = Object.Instantiate(cellPrefab, position, Quaternion.identity, cellParent.transform).GetComponent<Cell>();
            cell.name = $"Cell {x}x{y}";
            cells.Add(cell);
        }

        public void ClearCells()
        {
            if (GridManager.Instance.cellParent != null)
            {
                Object.DestroyImmediate(GridManager.Instance.cellParent);
            }

            GridManager.Instance.cellParent = new GameObject("CellParent");
            cellParent = GridManager.Instance.cellParent;
            cells.Clear();
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

        public void UpdateAllCellOccupied(List<Item> itemsList)
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