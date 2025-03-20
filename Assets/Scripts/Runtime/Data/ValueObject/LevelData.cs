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
        
        // 使用两个列表来存储触发器数据，以便Unity可以序列化
        [ShowInInspector]
        [SerializeField]
        private List<string> triggerKeys = new List<string>();
        
        [ShowInInspector]
        [SerializeField]
        private List<TriggerData> triggerValues = new List<TriggerData>();
        
        // 获取所有触发器的键值对
        public Dictionary<string, TriggerData> GetAllTriggers()
        {
            var triggers = new Dictionary<string, TriggerData>();
            for (int i = 0; i < triggerKeys.Count; i++)
            {
                triggers[triggerKeys[i]] = triggerValues[i];
            }
            return triggers;
        }
        
        // 设置所有触发器
        public void SetAllTriggers(Dictionary<string, TriggerData> triggers)
        {
            triggerKeys.Clear();
            triggerValues.Clear();
            foreach (var kvp in triggers)
            {
                triggerKeys.Add(kvp.Key);
                triggerValues.Add(kvp.Value);
            }
        }
        
        // 获取指定位置的触发器
        public TriggerData GetTrigger(int x, int y)
        {
            string key = $"{x}_{y}";
            int index = triggerKeys.IndexOf(key);
            return index >= 0 ? triggerValues[index] : new TriggerData();
        }
        
        // 设置指定位置的触发器
        public void SetTrigger(int x, int y, TriggerData trigger)
        {
            string key = $"{x}_{y}";
            int index = triggerKeys.IndexOf(key);
            if (index >= 0)
            {
                triggerValues[index] = trigger;
            }
            else
            {
                triggerKeys.Add(key);
                triggerValues.Add(trigger);
            }
        }
        
        // 重置指定位置的触发器
        public void ResetTrigger(int x, int y)
        {
            string key = $"{x}_{y}";
            int index = triggerKeys.IndexOf(key);
            if (index >= 0)
            {
                triggerValues[index] = new TriggerData
                {
                    gameColor = GameColor.None,
                    triggerType = TriggerType.None,
                };
            }
            else
            {
                triggerKeys.Add(key);
                triggerValues.Add(new TriggerData
                {
                    gameColor = GameColor.None,
                    triggerType = TriggerType.None,
                });
            }
        }

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
    }
}