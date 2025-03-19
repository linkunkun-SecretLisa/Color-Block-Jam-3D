using System;
using Runtime.Enums;
using UnityEngine;

namespace Runtime.Data.ValueObject
{
    [Serializable]
    public class LevelData
    {
        public int Width;
        public int Height;
        public GridData[] Grids;

        public GridData GetGrid(int x, int y, bool isSpawn = false)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                if (isSpawn)
                {
                    GridData defaultGrid = new GridData();
                    defaultGrid.gameColor = GameColor.None;
                    return defaultGrid;
                }
                throw new IndexOutOfRangeException($"GetGrid Index was outside the bounds of the array.  { x }  { y }" ); 
            }
            return Grids[y * Width + x];
        }

        public void SetGrid(int x, int y, GridData gridData, bool isSpawn = false)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                throw new IndexOutOfRangeException($"SetGrid Index was outside the bounds of the array.  { x }  { y }" ); 
            }
            Grids[y * Width + x] = gridData;
        }
    }
}