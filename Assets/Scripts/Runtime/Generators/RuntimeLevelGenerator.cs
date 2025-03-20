using System.Collections.Generic;
using UnityEngine;
using Runtime.Data.UnityObject;
using Runtime.Data.ValueObject;
using Runtime.Enums;
using Runtime.Entities;
using Runtime.Controllers;
using Runtime.Managers;
using Runtime.Utilities;

namespace Runtime.Generators
{
    /// <summary>
    /// 运行时关卡生成器：专注于游戏运行时快速生成关卡
    /// </summary>
    public class RuntimeLevelGenerator : MonoBehaviour
    {
        [Header("关卡配置")]
        public CD_LevelData levelData;       // 关卡数据配置
        public CD_GameColor colorData;       // 颜色数据配置
        public CD_GamePrefab itemPrefab;     // 物品预制体数据
        public float gridSpacing = 1f;      // 网格间距
        
        [Header("预制体引用")]
        public GameObject cellPrefab;        // 格子预制体
        public GameObject obstaclePrefab;    // 障碍物预制体
        public GameObject triggerOnePrefab;  // 触发器1预制体
        public GameObject triggerTwoPrefab;  // 触发器2预制体
        public GameObject triggerThreePrefab; // 触发器3预制体
        
        [Header("生成选项")]
        public bool generateOnStart = true;  // 是否在Start中自动生成
        
        [Header("引用")]
        public GridManager gridManager;      // GridManager引用，必须设置
        
        // 生成的对象的父物体
        private GameObject _levelParent;     // 所有生成物体的总父物体
        private GameObject _cellParent;      // 格子的父物体
        private GameObject _obstacleParent;  // 障碍物的父物体
        private GameObject _triggerParent;   // 触发器的父物体
        private GameObject _itemParent;      // 物品的父物体
        
        // 生成的格子列表
        private List<Cell> _cells = new List<Cell>();
        
        private void Start()
        {
            if (generateOnStart)
            {
                GenerateLevel();
            }
        }
        
        /// <summary>
        /// 生成整个关卡
        /// </summary>
        public void GenerateLevel()
        {
            if (levelData == null || levelData.levelData == null)
            {
                Debug.LogError("无法生成关卡：levelData 未设置!");
                return;
            }
            
            if (gridManager == null)
            {
                gridManager = FindObjectOfType<GridManager>();
                if (gridManager == null)
                {
                    Debug.LogError("无法生成关卡：未找到GridManager!");
                    return;
                }
            }
            
            // 创建父物体
            CreateParents();
            
            // 初始化GridManager
            gridManager._width = levelData.levelData.Width;
            gridManager._height = levelData.levelData.Height;
            gridManager._spaceModifier = gridSpacing;
            gridManager._levelData = levelData.levelData;
            
            // 生成网格
            CreateGrid();
            
            // 生成边界障碍物和触发器
            CreateBoundaries();
            
            // 生成游戏物品
            CreateItems();
            
            // 通知GridManager更新
            gridManager.UpdateCellData(levelData.levelData);
            
            // 验证生成是否成功
            ValidateGeneration();
            
            Debug.Log("关卡生成完成");
        }
        
        /// <summary>
        /// 创建所有父物体
        /// </summary>
        private void CreateParents()
        {
            // 清理可能存在的旧对象
            CleanUp();
            
            // 创建总父物体
            _levelParent = new GameObject("LevelParent");
            _levelParent.transform.SetParent(transform);
            
            // 创建其他父物体
            _cellParent = CreateChild(_levelParent, "CellParent");
            _obstacleParent = CreateChild(_levelParent, "ObstacleParent");
            _triggerParent = CreateChild(_levelParent, "TriggerParent");
            _itemParent = CreateChild(_levelParent, "ItemParent");
        }
        
        /// <summary>
        /// 创建子物体
        /// </summary>
        private GameObject CreateChild(GameObject parent, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent.transform);
            return child;
        }
        
        /// <summary>
        /// 清理已有的对象
        /// </summary>
        private void CleanUp()
        {
            string[] parentNames = { "LevelParent", "CellParent", "ObstacleParent", "TriggerParent", "ItemParent" };
            
            foreach (string name in parentNames)
            {
                GameObject obj = GameObject.Find(name);
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            
            _cells.Clear();
        }
        
        /// <summary>
        /// 创建网格
        /// </summary>
        private void CreateGrid()
        {
            int width = levelData.levelData.Width;
            int height = levelData.levelData.Height;
            
            // 清除GridManager中现有的cells
            gridManager.GetCells().Clear();
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 position = LevelGenerationUtility.GridToWorld(x, y, gridSpacing);
                    
                    // 实例化格子
                    GameObject cellObject = Instantiate(cellPrefab, position, Quaternion.identity, _cellParent.transform);
                    cellObject.name = $"Cell_{x}x{y}";
                    
                    // 初始化格子
                    Cell cell = cellObject.GetComponent<Cell>();
                    if (cell != null)
                    {
                        GridData gridData = levelData.levelData.GetGrid(x, y);
                        cell.Init(new Vector2Int(x, y), gridData.isOccupied);
                        _cells.Add(cell);
                        
                        // 确保Cell被添加到GridManager
                        if (!gridManager.GetCells().Contains(cell))
                        {
                            gridManager.GetCells().Add(cell);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 创建边界障碍物和触发器
        /// </summary>
        private void CreateBoundaries()
        {
            int width = levelData.levelData.Width;
            int height = levelData.levelData.Height;
            
            // 用于跟踪已创建的触发器
            Dictionary<Vector2Int, bool> createdTriggers = new Dictionary<Vector2Int, bool>();
            
            // 遍历边界位置
            for (int x = -1; x <= width; x++)
            {
                for (int y = -1; y <= height; y++)
                {
                    // 跳过内部和四个角落
                    if ((x != -1 && x != width && y != -1 && y != height) ||
                        (x == -1 && y == -1) || (x == -1 && y == height) ||
                        (x == width && y == -1) || (x == width && y == height))
                    {
                        continue;
                    }
                    
                    // 计算位置和旋转
                    Vector3 position = LevelGenerationUtility.GridToWorld(x, y, gridSpacing);
                    Quaternion rotation = LevelGenerationUtility.GetBoundaryRotation(x, y, width, height);
                    
                    // 获取触发器数据
                    TriggerData triggerData = levelData.levelData.GetTrigger(x, y);
                    
                    if (triggerData.triggerType != TriggerType.None)
                    {
                        // 如果是触发器，生成触发器
                        var triggerPos = triggerData.position;
                        
                        // 检查是否已创建
                        if (createdTriggers.ContainsKey(new Vector2Int(triggerPos.x, triggerPos.y)))
                        {
                            continue;
                        }
                        
                        // 获取触发器预制体
                        GameObject prefab = GetTriggerPrefab(triggerData.triggerType);
                        if (prefab == null) continue;
                        
                        // 计算触发器位置
                        Vector3 triggerPosition = LevelGenerationUtility.GetTriggerPosition(position, triggerPos, triggerData.triggerType, width, height, gridSpacing);
                        
                        // 实例化触发器
                        GameObject trigger = Instantiate(prefab, triggerPosition, rotation, _triggerParent.transform);
                        trigger.name = $"Trigger_{triggerData.triggerType}_{x}x{y}";
                        
                        // 设置触发器颜色
                        LevelGenerationUtility.SetTriggerColor(trigger, triggerData.gameColor, colorData);
                        
                        // 记录已创建
                        createdTriggers.Add(new Vector2Int(triggerPos.x, triggerPos.y), true);
                    }
                    else
                    {
                        // 否则生成障碍物
                        GameObject obstacle = Instantiate(obstaclePrefab, position, rotation, _obstacleParent.transform);
                        obstacle.name = $"Obstacle_{x}x{y}";
                    }
                }
            }
        }
        
        /// <summary>
        /// 创建游戏物品
        /// </summary>
        private void CreateItems()
        {
            int width = levelData.levelData.Width;
            int height = levelData.levelData.Height;
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    GridData gridData = levelData.levelData.GetGrid(x, y);
                    
                    // 检查是否需要生成物品
                    if (gridData.isOccupied && gridData.gameColor != GameColor.None && gridData.ItemSize != ItemSize.None)
                    {
                        // 检查是否是物品的主位置
                        if (gridData.position.x == x && gridData.position.y == y)
                        {
                            Vector3 position = LevelGenerationUtility.GridToWorld(x, y, gridSpacing);
                            SpawnItem(gridData, position);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 生成游戏物品
        /// </summary>
        private void SpawnItem(GridData gridData, Vector3 position)
        {
            // 获取预制体
            if (itemPrefab == null || (int)gridData.ItemSize >= itemPrefab.gamePrefab.Length)
            {
                Debug.LogError($"无法生成物品: 物品预制体未设置或索引{(int)gridData.ItemSize}超出范围");
                return;
            }
            
            MonoBehaviour prefab = itemPrefab.gamePrefab[(int)gridData.ItemSize].prefab;
            if (prefab == null)
            {
                Debug.LogError($"物品尺寸{gridData.ItemSize}的预制体为空");
                return;
            }
            
            // 实例化物品
            MonoBehaviour itemObject = Instantiate(prefab, position + new Vector3(0, 0.25f, 0), Quaternion.identity, _itemParent.transform);
            
            // 初始化物品
            Item item = itemObject.GetComponent<Item>();
            if (item != null)
            {
                // 初始化Item并添加到GridManager
                item.Init(gridData.gameColor, gridManager);
                
                // 确保Item被添加到GridManager的列表中
                if (!gridManager.GetItems().Contains(item))
                {
                    gridManager.AddItem(item);
                }
            }
        }
        
        /// <summary>
        /// 获取边界旋转角度
        /// </summary>
        private Quaternion GetBoundaryRotation(int x, int y, int width, int height)
        {
            return LevelGenerationUtility.GetBoundaryRotation(x, y, width, height);
        }
        
        /// <summary>
        /// 获取触发器预制体
        /// </summary>
        private GameObject GetTriggerPrefab(TriggerType type)
        {
            switch (type)
            {
                case TriggerType.One:
                    return triggerOnePrefab;
                case TriggerType.Two:
                    return triggerTwoPrefab;
                case TriggerType.Three:
                    return triggerThreePrefab;
                default:
                    Debug.LogWarning($"未知的触发器类型: {type}");
                    return null;
            }
        }
        
        /// <summary>
        /// 网格坐标转换为世界坐标
        /// </summary>
        private Vector3 GridToWorld(int x, int y)
        {
            return LevelGenerationUtility.GridToWorld(x, y, gridSpacing);
        }
        
        /// <summary>
        /// 世界坐标转换为网格坐标
        /// </summary>
        private Vector2Int WorldToGrid(Vector3 worldPos)
        {
            return LevelGenerationUtility.WorldToGrid(worldPos, gridSpacing);
        }
        
        /// <summary>
        /// 验证生成是否成功
        /// </summary>
        private void ValidateGeneration()
        {
            // 检查GridManager中Cell数量是否正确
            int expectedCellCount = levelData.levelData.Width * levelData.levelData.Height;
            if (gridManager.GetCells().Count != expectedCellCount)
            {
                Debug.LogWarning($"生成警告: GridManager中Cell数量({gridManager.GetCells().Count})与预期({expectedCellCount})不符");
            }
            
            // 检查GridManager中是否有空Cell
            for (int i = 0; i < gridManager.GetCells().Count; i++)
            { 
                if (gridManager.GetCells()[i] == null)
                {
                    Debug.LogWarning($"生成警告: GridManager中第{i}个Cell为空");
                }
            }
            
            // 在检查Item之前先移除所有空Item
            var itemList = gridManager.GetItems();
            itemList.RemoveAll(item => item == null);
            
            // 检查GridManager中是否有空Item
            for (int i = 0; i < gridManager.GetItems().Count; i++)
            {
                if (gridManager.GetItems()[i] == null)
                {
                    Debug.LogWarning($"生成警告: GridManager中第{i}个Item为空");
                }
            }
        }
    }
} 