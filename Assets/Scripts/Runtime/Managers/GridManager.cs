using System.Collections.Generic;
using UnityEngine;
using Runtime.Entities;
using Runtime.Data.ValueObject;
using Runtime.Enums;
using Runtime.Utilities;
using Unity.VisualScripting;
using Runtime.Data.UnityObject;
using Runtime.Controllers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Runtime.Managers
{
    public class GridManager : SingletonMonoBehaviour<GridManager>
    {
        [Header("Grid Settings")]
        // 网格宽度
        public int _width;
        // 网格高度
        public int _height;
        // 网格间距修改器，控制格子之间的实际距离
        public float _spaceModifier;
        // 关卡数据
        public LevelData _levelData;
        // 所有格子的父物体
        [SerializeField] private GameObject cellParent;
        [Header("References")]
        // 颜色数据，定义游戏中使用的颜色
        public CD_GameColor colorData;
    
        // 格子预制体
        [SerializeField] private GameObject cellPrefab;
        // 障碍物预制体
        [SerializeField] private GameObject obstaclePrefab;
        
        // 触发器预制体
        [Header("触发器预制体")]
        [SerializeField] private GameObject triggerOnePrefab;   // 类型1触发器
        [SerializeField] private GameObject triggerTwoPrefab;   // 类型2触发器
        [SerializeField] private GameObject triggerThreePrefab; // 类型3触发器
        // 所有触发器的父物体
        // 网格中所有物品的列表
        [SerializeField] private List<Item> _itemsList = new List<Item>();
        // 网格中所有格子的列表
        [SerializeField] private List<Cell> _cells = new List<Cell>();
    
        private void OnEnable()
        {
            // 确保列表被正确初始化
            if (_cells == null) _cells = new List<Cell>();
            if (_itemsList == null) _itemsList = new List<Item>();
        }
    
        // 初始化网格系统
        public void Initialize(int width, int height, float spaceModifier, LevelData levelData)
        {
            _width = width;
            _height = height;
            _spaceModifier = spaceModifier;
            _levelData = levelData;
    
            // 标记对象为脏，确保Unity保存更改
    #if UNITY_EDITOR
            CreateGrid();

            EditorUtility.SetDirty(this);
    #endif
        }
    
        #region 关卡创建
    
    #if UNITY_EDITOR
        // 创建网格
        private void CreateGrid()
        {
            // 清除现有的格子
            ClearCells();
    
            // 创建新的网格
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    // 计算世界坐标位置
                    Vector3 position = GridSpaceToWorldSpace(new Vector2Int(x, y));
                    // 实例化格子预制体
                    var cellMono = PrefabUtility.InstantiatePrefab(cellPrefab, cellParent.transform);
                    Cell cell = cellMono.GetComponent<Cell>();
                    cell.transform.position = position;
                    cell.name = $"Cell {x}x{y}";
                    // 初始化格子数据
                    cell.Init(new Vector2Int(x, y), false);
                    _cells.Add(cell);
    
                    EditorUtility.SetDirty(cell.gameObject);
                }
            }
    
            // 在网格周围创建障碍物
            CreateObstaclesAndTriggersAroundGrid();
        }
    
        // 在网格周围创建障碍物
        private void CreateObstaclesAndTriggersAroundGrid()
        {
            // 清除现有的障碍物父物体
            var obstacleParent = LevelCreatorScript.DestroyAndCreateNewGameObjectByName("ObstacleParent");
            var triggersParent = LevelCreatorScript.DestroyAndCreateNewGameObjectByName("TriggerParent");

            //记录一下已经创建的trigger根据x y坐标
            Dictionary<Vector2Int, bool> createdTriggers = new Dictionary<Vector2Int, bool>();
    
            // 在网格边界创建障碍物
            for (int x = -1; x <= _width; x++)
            {
                for (int y = -1; y <= _height; y++)
                {
                    // 跳过网格内部位置
                    if (x != -1 && x != _width && y != -1 && y != _height)
                        continue;
    
                    // 跳过四个角落位置
                    if ((x == -1 && y == -1) ||
                        (x == -1 && y == _height) ||
                        (x == _width && y == -1) ||
                        (x == _width && y == _height))
                    {
                        continue;
                    }
    
                    // 计算障碍物位置
                    Vector3 position = GridSpaceToWorldSpace(new Vector2Int(x, y));



                    // 计算旋转角度
                    Quaternion rotation = Quaternion.identity;
    
                    // 根据位置设置障碍物旋转角度
                    if (x == -1 && y >= 0 && y < _height)
                    {
                        rotation = Quaternion.Euler(0, 90, 0);  // 左边界
                    }
                    else if (x == _width && y >= 0 && y < _height)
                    {
                        rotation = Quaternion.Euler(0, -90, 0); // 右边界
                    }
                    else if (y == -1 && x >= 0 && x < _width)
                    {
                        rotation = Quaternion.Euler(0, 0, 0);   // 下边界
                    }
                    else if (y == _height && x >= 0 && x < _width)
                    {
                        rotation = Quaternion.Euler(0, 180, 0); // 上边界
                    }
                    //后缀名
                    string key = $"{x}_{y}";
                    // 获取障碍物数据
                    TriggerData triggerData = _levelData.GetTrigger(x, y);
                
                    if (triggerData.triggerType != TriggerType.None)
                    {
                        var triggerOriginalPos = triggerData.position;
                        //如果已经创建过，则跳过
                        if (createdTriggers.ContainsKey(new Vector2Int(triggerOriginalPos.x, triggerOriginalPos.y)))
                        {
                            continue;
                        }
                        // 根据触发器类型获取预制体
                        GameObject prefabToUse = GetTriggerPrefabByType(triggerData.triggerType);
                        
                        // 如果预制体为null，记录警告并跳过
                        if (prefabToUse == null)
                        {
                            Debug.LogWarning($"未找到TriggerType为{triggerData.triggerType}的预制体！");
                            continue;
                        }
                        
                        // 实例化触发器预制体
                        GameObject trigger = (GameObject)PrefabUtility.InstantiatePrefab(prefabToUse, triggersParent.transform);
                        //调整trigger的位置
                        trigger.transform.position = GetTriggerPosition(position, triggerOriginalPos, triggerData.triggerType);
                        trigger.transform.rotation = rotation;
                        
                        // 设置触发器颜色
                        SetTriggerColor(trigger, triggerData.gameColor);
                        
                        EditorUtility.SetDirty(trigger);
                        //记录一下已经创建的trigger根据x y坐标
                        createdTriggers.Add(new Vector2Int(triggerOriginalPos.x, triggerOriginalPos.y), true);
                    }
                    else
                    {
                        // 创建障碍物
                        GameObject obstacle = (GameObject)PrefabUtility.InstantiatePrefab(obstaclePrefab, obstacleParent.transform);
                        obstacle.transform.position = position;
                        obstacle.transform.rotation = rotation;
                        obstacle.name = obstaclePrefab.name + "_" + x + "x" + y;     
                        EditorUtility.SetDirty(obstacle);
                    }
                }
            }
        }
#endif
    
        #endregion

        //调整trigger的位置
        private Vector3 GetTriggerPosition(Vector3 position, Vector2Int triggerOriginalPos, TriggerType triggerType)
        {
            Vector3 triggerPosition = position;
            var (isColumn, isRow) = LevelCreatorScript.GetBoundaryType(triggerOriginalPos.x, triggerOriginalPos.y, _width, _height);
            if (isRow)
            {
                //根据trigger type不同决定偏移
                if (triggerType == TriggerType.Two)
                {
                    triggerPosition.x += 0.5f;
                }
                else if (triggerType == TriggerType.Three)
                {
                    triggerPosition.x += 1f;
                }
            }
            else if (isColumn)
            {
                //根据trigger type不同决定偏移
                if (triggerType == TriggerType.Two)
                {
                    triggerPosition.z += 0.5f;
                }
                else if (triggerType == TriggerType.Three)
                {
                    triggerPosition.z += 1f;
                }
            }
            return triggerPosition;
        }
    
        // 根据关卡数据更新格子状态
        public void UpdateCellData(LevelData levelData)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    GridData gridData = levelData.GetGrid(x, y);
                    Cell cell = _cells[x * _height + y];
    
                    // 根据关卡数据设置格子状态
                    if (gridData.isOccupied && gridData.gameColor != GameColor.None)
                    {
                        cell.Init(new Vector2Int(x, y), true);
                    }
                    else
                    {
                        cell.Init(new Vector2Int(x, y), false);
                    }
    
#if UNITY_EDITOR
                    EditorUtility.SetDirty(cell.gameObject);
#endif
                }
            }
        }
    
        // 清除所有格子
        private void ClearCells()
        {
            // 所有格子的父物体
            cellParent = LevelCreatorScript.DestroyAndCreateNewGameObjectByName("CellParent");    
            _cells.Clear();
        }
    
        // 添加物品到网格
        public void AddItem(Item item)
        {
            _itemsList.Add(item);
    
#if UNITY_EDITOR
            EditorUtility.SetDirty(item.gameObject);
#endif
        }
    
        // 更新所有格子的占用状态
        public void UpdateAllCellOccupied()
        {
            // 首先将所有格子设置为未占用
            foreach (var cell in _cells)
            {
                cell.SetOccupied(false);
            }
    
            // 根据物品位置更新格子占用状态
            foreach (var item in _itemsList)
            {
                if (item == null)
                {
                    Debug.LogError("item is null!");
                    continue;
                }
                foreach (var childItem in item.childItems)
                {
                    if (childItem != null)
                    {
                        Vector2Int gridPosition = WorldSpaceToGridSpace(childItem.transform.position);
                        Cell cell = _cells[gridPosition.x * _height + gridPosition.y];
                        cell.SetOccupied(true);
#if UNITY_EDITOR
                        EditorUtility.SetDirty(cell.gameObject);
#endif
                    }
                }
            }
        }
    
        // 获取所有物品
        public List<Item> GetItems()
        {
            return _itemsList;
        }
    
        // 移除物品
        public void RemoveItem(Item item)
        {
            if (_itemsList.Contains(item))
            {
                _itemsList.Remove(item);
                
                CheckItemCount();
    
#if UNITY_EDITOR
                EditorUtility.SetDirty(item.gameObject);
#endif
            }
        }
    
        // 检查物品数量，判断是否完成关卡
        private void CheckItemCount()
        {
            // Debug.Log("CheckItemCount" + _itemsList.Count);
            if (_itemsList.Count == 0)
            {
                GameManager.Instance.SetGameStateLevelComplete();
            }
        }
    
        // 清除所有物品
        public void ClearItems()
        {
            _itemsList.Clear();
        }
    
        // 获取所有格子
        public List<Cell> GetCells()
        {
            return _cells;
        }
    
        // 网格坐标转换为世界坐标
        public Vector3 GridSpaceToWorldSpace(Vector2Int gridPosition)
        {
            return new Vector3(gridPosition.x * _spaceModifier, 0, gridPosition.y * _spaceModifier);
        }
    
        // 世界坐标转换为网格坐标
        public Vector2Int WorldSpaceToGridSpace(Vector3 worldPosition)
        {
            // 计算网格坐标并确保在有效范围内 round means round to nearest integer
            int x = Mathf.RoundToInt(worldPosition.x / _spaceModifier);
            int y = Mathf.RoundToInt(worldPosition.z / _spaceModifier);
    
            x = Mathf.Clamp(x, 0, _width - 1);
            y = Mathf.Clamp(y, 0, _height - 1);
    
            return new Vector2Int(x, y);
        }
    
        // 根据触发器类型获取预制体
        private GameObject GetTriggerPrefabByType(TriggerType triggerType)
        {
            // 使用switch直接返回对应的预制体
            switch (triggerType)
            {
                case TriggerType.One:
                    if (triggerOnePrefab == null)
                    {
                        Debug.LogError("触发器类型One的预制体未分配！请在Inspector中设置");
                    }
                    return triggerOnePrefab;
                case TriggerType.Two:
                    if (triggerTwoPrefab == null)
                    {
                        Debug.LogError("触发器类型Two的预制体未分配！请在Inspector中设置");
                    }
                    return triggerTwoPrefab;
                case TriggerType.Three:
                    if (triggerThreePrefab == null)
                    {
                        Debug.LogError("触发器类型Three的预制体未分配！请在Inspector中设置");
                    }
                    return triggerThreePrefab;
                default:
                    Debug.LogWarning($"未知的触发器类型: {triggerType}");
                    return null;
            }
        }
    
        // 设置触发器颜色
        private void SetTriggerColor(GameObject triggerObject, GameColor gameColor)
        {
            // 检查触发器对象是否为null
            if (triggerObject == null)
            {
                Debug.LogError("触发器对象为null，无法设置颜色");
                return;
            }
            
            // 如果没有颜色数据或颜色是None，跳过
            if (colorData == null || gameColor == GameColor.None)
            {
                Debug.LogWarning("颜色数据为空或颜色类型为None");
                return;
            }
            
            // 检查gameColor值是否在有效范围内
            if ((int)gameColor < 0 || (int)gameColor >= colorData.gameColorsData.Length)
            {
                Debug.LogError($"游戏颜色索引{(int)gameColor}超出范围(0-{colorData.gameColorsData.Length-1})");
                return;
            }

            //改变TriggerColor
            BlockTriggerController triggerController = triggerObject.GetComponent<BlockTriggerController>();
            triggerController.TriggerColor = gameColor;
            
            // 获取渲染器组件
            Renderer renderer = triggerObject.GetComponent<Renderer>();
            if (renderer == null)
            {
                // 如果在父对象上没找到，尝试在子对象中查找
                renderer = triggerObject.GetComponentInChildren<Renderer>();
                if (renderer == null)
                {
                    Debug.LogWarning($"在触发器{triggerObject.name}上找不到Renderer组件");
                    return;
                }
            }
            
            try
            {
                // 从颜色数据中获取材质颜色
                Material material = renderer.sharedMaterial;
                Material colorMaterial = colorData.gameColorsData[(int)gameColor].materialColor;
                
                // 如果有材质颜色，直接应用材质
                if (colorMaterial != null)
                {
                    renderer.sharedMaterial = colorMaterial;
                }
                // 否则使用颜色值
                else
                {
                    Color colorToApply = colorData.gameColorsData[(int)gameColor].color;
                    
                    // 创建新材质的副本，避免修改原始材质
                    Material newMaterial = new Material(renderer.sharedMaterial);
                    
                    // 设置主颜色
                    newMaterial.color = colorToApply;
                    
                    // 如果使用的是Toony Colors Pro 2着色器，尝试设置其他相关属性
                    if (newMaterial.HasProperty("_Color"))
                    {
                        newMaterial.SetColor("_Color", colorToApply);
                    }
                    else if (newMaterial.HasProperty("_BaseColor"))
                    {
                        newMaterial.SetColor("_BaseColor", colorToApply);
                    }
                    
                    // 应用新材质
                    renderer.sharedMaterial = newMaterial;
                }
                
                // 确保修改后的材质被保存
#if UNITY_EDITOR
                EditorUtility.SetDirty(renderer.sharedMaterial);
#endif
            }
            catch (System.Exception e)
            {
                Debug.LogError($"设置触发器颜色时发生错误: {e.Message}");
            }
        }
    }
}