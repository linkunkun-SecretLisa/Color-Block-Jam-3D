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
    /// 作为LevelManager的辅助工具，负责实际的关卡元素生成
    /// </summary>
    public class RuntimeLevelGenerator : MonoBehaviour
    {
        [Header("预制体引用")]
        public GameObject cellPrefab;        // 格子预制体
        public GameObject obstaclePrefab;    // 障碍物预制体
        public GameObject triggerOnePrefab;  // 触发器1预制体
        public GameObject triggerTwoPrefab;  // 触发器2预制体
        public GameObject triggerThreePrefab; // 触发器3预制体
        
        [Header("生成选项")]
        public bool generateItemsAutomatically = true;  // 是否自动生成物品
        public bool createCellsForGrid = true;          // 是否为网格创建单元格
        public bool createBoundaries = true;            // 是否创建边界
        
        [Header("引用")]
        [SerializeField] private GridManager gridManager;  // 网格管理器引用
        
        // 父物体引用
        private GameObject _levelParent;     // 所有生成物体的总父物体
        private GameObject _cellParent;      // 格子的父物体
        private GameObject _obstacleParent;  // 障碍物的父物体
        private GameObject _triggerParent;   // 触发器的父物体
        private GameObject _itemParent;      // 物品的父物体
        
        // 格子列表
        private List<Cell> _cells = new List<Cell>();
        
        private LevelData currentLevelData;     // 当前关卡数据
        private CD_GameColor colorData;         // 颜色数据
        private CD_GamePrefab itemPrefab;       // 物品预制体数据
        private float gridSpacing = 50f;        // 网格间距
        
        private void Awake()
        {
            if (gridManager == null)
            {
                gridManager = GridManager.Instance;
            }
        }
        
        /// <summary>
        /// 使用提供的数据生成关卡
        /// </summary>
        /// <param name="levelData">关卡数据</param>
        /// <param name="colorDataAsset">颜色数据资产</param>
        /// <param name="itemPrefabAsset">物品预制体数据资产</param>
        /// <param name="spacing">网格间距</param>
        public void GenerateLevel(LevelData levelData, CD_GameColor colorDataAsset, CD_GamePrefab itemPrefabAsset, float spacing = 1f)
        {
            // 保存参数
            currentLevelData = levelData;
            colorData = colorDataAsset;
            itemPrefab = itemPrefabAsset;
            gridSpacing = spacing;
            
            // 验证参数
            if (!ValidateGeneration())
            {
                Debug.LogError("关卡生成参数无效，无法生成关卡");
                return;
            }
            
            // 创建父物体
            CreateParents();
            
            // 根据设置创建网格和边界
            if (createCellsForGrid)
            {
                CreateGrid();
            }
            
            if (createBoundaries)
            {
                CreateBoundaries();
            }
            
            // 生成物品
            if (generateItemsAutomatically && itemPrefab != null)
            {
                CreateItems();
            }
            
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
            _cellParent = CreateChildParent(_levelParent, "CellParent");
            _obstacleParent = CreateChildParent(_levelParent, "ObstacleParent");
            _triggerParent = CreateChildParent(_levelParent, "TriggerParent");
            _itemParent = CreateChildParent(_levelParent, "ItemParent");
        }
        
        /// <summary>
        /// 创建子物体
        /// </summary>
        private GameObject CreateChildParent(GameObject parent, string name)
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
            
            foreach (Transform child in transform)
            {
                if (System.Array.IndexOf(parentNames, child.name) >= 0)
                {
                    DestroyImmediate(child.gameObject);
                }
            }
            
            _cells.Clear();
        }
        
        /// <summary>
        /// 创建网格
        /// </summary>
        private void CreateGrid()
        {
            int width = currentLevelData.Width;
            int height = currentLevelData.Height;
            
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
                        GridData gridData = currentLevelData.GetGrid(x, y);
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
            int width = currentLevelData.Width;
            int height = currentLevelData.Height;
            
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
                    TriggerData triggerData = currentLevelData.GetTrigger(x, y);
                    
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
            int width = currentLevelData.Width;
            int height = currentLevelData.Height;
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    GridData gridData = currentLevelData.GetGrid(x, y);
                    
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
        /// 验证生成所需的参数是否有效
        /// </summary>
        private bool ValidateGeneration()
        {
            if (currentLevelData == null)
            {
                Debug.LogError("缺失关卡数据");
                return false;
            }
            
            if (colorData == null)
            {
                Debug.LogWarning("缺失颜色数据，使用默认颜色");
            }
            
            if (generateItemsAutomatically && itemPrefab == null)
            {
                Debug.LogError("启用了自动生成物品，但缺失物品预制体数据");
                return false;
            }
            
            if (gridManager == null)
            {
                gridManager = GridManager.Instance;
                if (gridManager == null)
                {
                    Debug.LogError("找不到GridManager实例");
                    return false;
                }
            }
            
            return true;
        }
    }
} 