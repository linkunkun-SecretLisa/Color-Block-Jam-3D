using System;
using System.Collections.Generic;
using Runtime.Enums;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runtime.Data.ValueObject
{
    [Serializable]
    public class LevelData
    {
        public int Width;
        public int Height;
        public GridData[] Grids;
        // 使用字典存储触发器，键为"x_y"格式的坐标字符串
        [ShowInInspector]
        public Dictionary<string, TriggerData> Triggers = new Dictionary<string, TriggerData>();
        
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
        
        public void ResetGrid(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                throw new IndexOutOfRangeException($"SetGrid Index was outside the bounds of the array.  { x }  { y }" ); 
            }
            GridData gridData = new GridData
            {
                isOccupied = false,
                gameColor = GameColor.None,
                position = new Vector2Int(x, y),
                ItemSize = ItemSize.None,
                ItemPos = Vector2Int.zero, 
            };
            Grids[y * Width + x] = gridData;
        }
        
        // 获取指定位置的触发器
        public TriggerData GetTrigger(int x, int y)
        {
            string key = $"{x}_{y}";
            return Triggers.TryGetValue(key, out var trigger) ? trigger : new TriggerData();
        }
        
        // 设置指定位置的触发器
        public void SetTrigger(int x, int y, TriggerData trigger)
        {
            string key = $"{x}_{y}";
            Triggers[key] = trigger;
        }
        
        // 设置指定位置的触发器
        public void ResetTrigger(int x, int y)
        {
            string key = $"{x}_{y}";
            TriggerData trigger = new TriggerData
            {
                gameColor = GameColor.None,
                triggerType = TriggerType.None,
                // neighborGridPos = Vector2Int.zero,
            };
            Triggers[key] = trigger;
        }
    }
}