using System;
using UnityEngine;

namespace Runtime.Data.ValueObject
{
    [Serializable]
    public class LevelData
    {
        public int Width;
        public int Height;
        public GridData[] Grids;

        public GridData GetGrid(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                throw new IndexOutOfRangeException("Index was outside the bounds of the array.");
            }
            return Grids[y * Width + x];
        }

        public void SetGrid(int x, int y, GridData gridData)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                throw new IndexOutOfRangeException("Index was outside the bounds of the array.");
            }
            Grids[y * Width + x] = gridData;
        }
    }
}