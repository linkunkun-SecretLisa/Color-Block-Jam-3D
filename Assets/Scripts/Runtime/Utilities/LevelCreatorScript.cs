using System.Collections.Generic;
using Runtime.Data.UnityObject;
using Runtime.Data.ValueObject;
using Runtime.Entities;
using Runtime.Enums;
using Runtime.Managers;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;  // 仅在Unity编辑器模式下引入编辑器相关功能
#endif

namespace Runtime.Utilities
{
    /// <summary>
    /// 关卡创建器脚本：用于在编辑器中创建和编辑关卡数据
    /// </summary>
    [ExecuteInEditMode]  // 使脚本在编辑器模式下也能执行
    public class LevelCreatorScript : MonoBehaviour
    {
        [Header("Grid Settings")] 
        public int Width;  // 网格宽度
        public int Height;  // 网格高度
        [Range(0f, 100f)] public float spaceModifier = 50f;  // 网格间距修改器，控制格子之间的距离
        [Range(50f, 100f)] public float gridSize = 50f;  // 单个格子的大小

        [Header("References")] 
        public CD_LevelData LevelData;  // 关卡数据容器，存储关卡的所有信息
        public CD_GameColor colorData;  // 游戏颜色数据，定义游戏中使用的颜色
        public CD_GamePrefab itemPrefab;  // 物品预制体数据，用于实例化游戏物体
        public GameObject itemsParentObject;  // 所有物品的父物体，用于组织场景层次结构
        public GridManager gridManager;  // 网格管理器引用，用于管理网格相关操作

        public ItemSize itemSize; // 当前选择的物品尺寸，如1x1, 2x2, 3x2等
        public TriggerType triggerType; // 当前选择的物品尺寸，如1x1, 2x2, 3x2等

        public GameColor gameColor;  // 当前选择的游戏颜色
        private LevelData _currentLevelData;  // 当前正在编辑的关卡数据

        /// <summary>
        /// 当脚本启用时初始化关卡数据
        /// </summary>
        private void OnEnable()
        {
            // 检查LevelData是否已分配
            if (LevelData == null)
            {
                Debug.LogError("LevelData is not assigned in the inspector!");
                return;
            }

            // 如果LevelData中没有数据，创建新的
            if (LevelData.levelData == null)
            {
                LevelData.levelData = new LevelData();
            }

            SetCurrentLevelData();  // 设置当前关卡数据
        }

        /// <summary>
        /// 根据当前设置生成关卡数据和游戏物体
        /// </summary>
        public void GenerateLevelData()
        {
            // 如果已存在物品父物体，则销毁它并清除网格管理器中的物品
            if (itemsParentObject != null)
            {
                DestroyImmediate(itemsParentObject);
                gridManager.ClearItems();
            }


            // 初始化网格管理器
            gridManager.Initialize(Width, Height, spaceModifier, LevelData.levelData);

            //如果兄弟节点中有LevelParent，则销毁它
            var existingLevelParent = transform.parent?.Find("LevelParent");
            if (existingLevelParent != null)
            {
                DestroyImmediate(existingLevelParent.gameObject);
            }

            // 创建新的物品父物体
            itemsParentObject = new GameObject("LevelParent");


            // 遍历所有网格单元
            for (int x = 0; x < Width; x++)//todo:lkk
            {
                for (int y = 0; y < Height; y++)
                {
                    GridData gridCell = LevelData.levelData.GetGrid(x, y);
                    // 如果网格单元被占用且有有效的颜色和尺寸，则生成物品
                    if (gridCell.isOccupied && gridCell.gameColor != GameColor.None &&
                        gridCell.ItemSize != ItemSize.None)
                    {
                        Vector3 spawnPosition = GridSpaceToWorldSpace(x, y);  // 计算生成位置
                        SpawnItem(gridCell, spawnPosition, x, y);  // 生成物品
                    }
                }
            }


            // 更新网格管理器中的单元格数据
            gridManager.UpdateCellData(LevelData.levelData);
            Debug.Log("Grid generated.");
        }

        /// <summary>
        /// 根据网格数据在指定位置生成物品
        /// </summary>
        /// <param name="gridCell">网格数据</param>
        /// <param name="spawnPosition">生成位置</param>
        /// <param name="x">网格X坐标</param>
        /// <param name="y">网格Y坐标</param>
        private void SpawnItem(GridData gridCell, Vector3 spawnPosition, int x, int y)
        {
            // 处理1x1大小的物品
            if (gridCell.ItemSize == ItemSize.OneByOne)
            {
#if UNITY_EDITOR
                // 在编辑器模式下使用PrefabUtility实例化预制体，保持预制体连接
                MonoBehaviour item = (MonoBehaviour)PrefabUtility.InstantiatePrefab(itemPrefab.gamePrefab[(int)gridCell.ItemSize].prefab, itemsParentObject.transform);
#else
                // 在运行时使用Instantiate
                MonoBehaviour item = Instantiate(itemPrefab.gamePrefab[(int)gridCell.ItemSize].prefab, itemsParentObject.transform);
#endif
                // 设置物品位置，稍微抬高以避免Z-fighting
                item.transform.position = spawnPosition + new Vector3(0, 0.25f, 0);
                // 初始化物品，设置颜色和网格管理器引用
                item.GetComponent<Item>().Init(gridCell.gameColor, gridManager);
            }

            // 处理2x2大小的物品
            if (gridCell.ItemSize == ItemSize.TwoByTwo)
            {
                var gridcellPos = gridCell.position;
                // 检查左侧和上方的网格单元
                var left = LevelData.levelData.GetGrid(gridcellPos.x - 1, gridcellPos.y, true);
                var up = LevelData.levelData.GetGrid(gridcellPos.x, gridcellPos.y + 1, true);

                // 确保左侧和上方的网格单元与当前单元颜色相同
                if ((gridCell.gameColor == up.gameColor) && (gridCell.gameColor == left.gameColor))
                {
#if UNITY_EDITOR
                    MonoBehaviour item = (MonoBehaviour)PrefabUtility.InstantiatePrefab(itemPrefab.gamePrefab[(int)gridCell.ItemSize].prefab, itemsParentObject.transform);
#else
                    MonoBehaviour item = Instantiate(itemPrefab.gamePrefab[(int)gridCell.ItemSize].prefab, itemsParentObject.transform);
#endif
                    item.transform.position = spawnPosition + new Vector3(0, 0.25f, 0);
                    item.GetComponent<Item>().Init(gridCell.gameColor, gridManager);
                }
            }

            // 处理3x2大小的物品
            if (gridCell.ItemSize == ItemSize.ThreeByTwo)
            {
                var gridcellPos = gridCell.position;
                // 检查左侧、右侧和上方的网格单元
                var left = LevelData.levelData.GetGrid(gridcellPos.x - 1, gridcellPos.y, true);
                var right = LevelData.levelData.GetGrid(gridcellPos.x + 1, gridcellPos.y, true);
                var up = LevelData.levelData.GetGrid(gridcellPos.x, gridcellPos.y + 1, true);

                // 确保左侧、右侧和上方的网格单元与当前单元颜色相同
                if ((gridCell.gameColor == up.gameColor) && (gridCell.gameColor == left.gameColor) && (gridCell.gameColor == right.gameColor))
                {
#if UNITY_EDITOR
                    MonoBehaviour item = (MonoBehaviour)PrefabUtility.InstantiatePrefab(itemPrefab.gamePrefab[(int)gridCell.ItemSize].prefab, itemsParentObject.transform);
#else
                    MonoBehaviour item = Instantiate(itemPrefab.gamePrefab[(int)gridCell.ItemSize].prefab, itemsParentObject.transform);
#endif
                    item.transform.position = spawnPosition + new Vector3(0, 0.25f, 0);
                    item.GetComponent<Item>().Init(gridCell.gameColor, gridManager);
                }
            }
            
            // 处理3x3大小的物品
            if(gridCell.ItemSize == ItemSize.ThreeByThree)
            {
                var gridcellPos = gridCell.position;
                // 检查左侧、右侧、上方和下方的网格单元
                var left = LevelData.levelData.GetGrid(gridcellPos.x - 1, gridcellPos.y, true);
                var right = LevelData.levelData.GetGrid(gridcellPos.x + 1, gridcellPos.y, true);
                var up = LevelData.levelData.GetGrid(gridcellPos.x, gridcellPos.y + 1, true);
                var down = LevelData.levelData.GetGrid(gridcellPos.x, gridcellPos.y - 1, true);

                // 确保左侧、右侧、上方和下方的网格单元与当前单元颜色相同
                if ((gridCell.gameColor == up.gameColor) && (gridCell.gameColor == left.gameColor) && (gridCell.gameColor == right.gameColor) && (gridCell.gameColor == down.gameColor))
                {
#if UNITY_EDITOR
                    MonoBehaviour item = (MonoBehaviour)PrefabUtility.InstantiatePrefab(itemPrefab.gamePrefab[(int)gridCell.ItemSize].prefab, itemsParentObject.transform);
#else
                    MonoBehaviour item = Instantiate(itemPrefab.gamePrefab[(int)gridCell.ItemSize].prefab, itemsParentObject.transform);
#endif
                    item.transform.position = spawnPosition + new Vector3(0, 0.25f, 0);
                    item.GetComponent<Item>().Init(gridCell.gameColor, gridManager);
                }
            }
        }

        /// <summary>
        /// 将网格坐标转换为世界坐标
        /// </summary>
        /// <param name="x">网格X坐标</param>
        /// <param name="y">网格Y坐标</param>
        /// <returns>世界坐标</returns>
        public Vector3 GridSpaceToWorldSpace(int x, int y)
        {
            // 使用spaceModifier作为网格间距，将网格坐标转换为世界坐标
            return new Vector3(x * spaceModifier, 0, y * spaceModifier);
        }

        /// <summary>
        /// 将世界坐标转换为网格坐标
        /// </summary>
        /// <param name="worldPosition">世界坐标</param>
        /// <returns>网格坐标</returns>
        public Vector2Int WorldSpaceToGridSpace(Vector3 worldPosition)
        {
            // 将世界坐标除以间距并四舍五入，得到最近的网格坐标
            int x = Mathf.RoundToInt(worldPosition.x / spaceModifier);
            int y = Mathf.RoundToInt(worldPosition.z / spaceModifier);
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// 设置当前关卡数据，如果需要则初始化
        /// </summary>
        private void SetCurrentLevelData()
        {
            // 如果网格数据不存在或尺寸不匹配，重新初始化
            if (LevelData.levelData.Grids == null || LevelData.levelData.Grids.Length != Width * Height)
            {
                LevelData.levelData.Width = Width;
                LevelData.levelData.Height = Height;
                LevelData.levelData.Grids = new GridData[Width * Height];
                
                // 初始化所有网格单元
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        int index = y * Width + x;
                        LevelData.levelData.Grids[index] = new GridData
                        {
                            isOccupied = false,
                            gameColor = GameColor.None,
                            position = new Vector2Int(x, y),
                            ItemSize = ItemSize.None
                        };
                    }
                }
            }

            // 创建当前关卡数据的副本
            _currentLevelData = new LevelData
            {
                Width = Width,
                Height = Height,
                Grids = new GridData[LevelData.levelData.Grids.Length],
                Triggers = new Dictionary<string, TriggerData>(LevelData.levelData.Triggers) // 深拷贝触发器数据
            };

            // 复制所有网格单元数据
            for (int i = 0; i < LevelData.levelData.Grids.Length; i++)
            {
                _currentLevelData.Grids[i] = LevelData.levelData.Grids[i]; //结构体 深拷贝 值复制
            }
        }

        /// <summary>
        /// 判断指定坐标位置是否为列或行边界
        /// </summary>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        /// <param name="width">网格宽度</param>
        /// <param name="height">网格高度</param>
        /// <returns>返回一个元组,isColumn表示是否为列边界,isRow表示是否为行边界</returns>
        public static (bool isColumn, bool isRow) GetBoundaryType(int x, int y, int width, int height)
        {
            bool isColumn = x == -1 || x == width;
            bool isRow = y == -1 || y == height;
            return (isColumn, isRow);
        }

        //设置trigger
        public bool SetBlockTrigger(int x, int y)
        {
            if (this.gameColor == GameColor.None || this.triggerType == TriggerType.None)
            {
                Debug.LogError(" gameColor or triggerType is None!");
                return false;
            }
            
            TriggerType triggerType = this.triggerType;
            GameColor gameColor = this.gameColor;

            var (isColumn, isRow) = GetBoundaryType(x, y, Width, Height);
            
            Vector2Int[] offsets = GetOffsetsForTriggerSizeAndRotation(triggerType, x, y);


            //
            if (isColumn)
            {
                foreach (var offset in offsets)
                {
                    int targetY = y + offset.y;
                    if (targetY < 0 || targetY >= Height)
                    {
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.DisplayDialog(
                            "Warning", 
                            $"放置位置超出网格范围！triggerType: {triggerType}, 位置: ({x}, {y})", 
                            "确定");
#endif
                        Debug.LogWarning($"放置位置超出网格范围！triggerType: {triggerType}, 位置: ({x}, {y})");
                        return false;
                    }
                
                }
            }
            else if (isRow)
            {
                foreach (var offset in offsets)
                {
                    int targetX = x + offset.x;
                    if (targetX < 0 || targetX >= Width)
                    {
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.DisplayDialog(
                            "Warning", 
                            $"放置位置超出网格范围！triggerType: {triggerType}, 位置: ({x}, {y})", 
                            "确定");
#endif
                        Debug.LogWarning($"放置位置超出网格范围！triggerType: {triggerType}, 位置: ({x}, {y})");
                        return false;
                    }
                
                }
            }
            //先清除之前的trigger，如有
            foreach (var offset in offsets)
            {
                var relatedTrigger = _currentLevelData.GetTrigger(x + offset.x, y + offset.y);
                if (relatedTrigger.triggerType != TriggerType.None)
                {
                    var pos2 = relatedTrigger.position;
                    var offset2 = GetOffsetsForTriggerSizeAndRotation(relatedTrigger.triggerType, pos2.x, pos2.y);
                    foreach (var offsetPos in offset2)
                    {
                        _currentLevelData.ResetTrigger(pos2.x + offsetPos.x, pos2.y + offsetPos.y);
                    }
                }
            }
            
            //再设置现在的trigger
            foreach (var offset in offsets)
            {
                _currentLevelData.SetTrigger(x + offset.x, y + offset.y, new TriggerData()
                    {
                        triggerType = triggerType,
                        gameColor = gameColor,
                        position = new Vector2Int(x, y),
                    }
                );
            }
            return true;
        }

        /// <summary>
        /// 切换网格占用状态，用于编辑器中放置和移除物品
        /// </summary>
        /// <param name="x">网格X坐标</param>
        /// <param name="y">网格Y坐标</param>
        /// <returns>是否成功切换</returns>
        public bool ToggleGridOccupancy(int x, int y)
        {
            // 获取当前物品尺寸的偏移量
            Vector2Int[] offsets = GetOffsetsForItemSizeAndRotation(itemSize);
            
            // 预先检查所有目标位置是否在范围内
            foreach (var offset in offsets)
            {
                int targetX = x + offset.x;
                int targetY = y + offset.y;
                if (targetX < 0 || targetX >= Width || targetY < 0 || targetY >= Height)
                {
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.DisplayDialog(
                        "警告", 
                        $"放置位置超出网格范围！物品尺寸: {itemSize}, 位置: ({x}, {y})", 
                        "确定");
#endif
                    Debug.LogWarning($"放置位置超出网格范围！物品尺寸: {itemSize}, 位置: ({x}, {y})");
                    return false;
                }
            }
            
            // 预先检查当前的位置是否已经被占用，如果占用清除当前的方块的所有格子占用
            foreach (var offset in offsets)
            {
                int targetX = x + offset.x;
                int targetY = y + offset.y;
                GridData grid = _currentLevelData.GetGrid(targetX, targetY);
                if (grid.isOccupied){
                    ClearOccupiedGrids(grid);
                }
            }

            // 所有位置都在范围内，执行放置
            foreach (var offset in offsets)
            {
                int targetX = x + offset.x;
                int targetY = y + offset.y;
                GridData grid = _currentLevelData.GetGrid(targetX, targetY);
                grid.isOccupied = !grid.isOccupied;
                grid.gameColor = gameColor;
                grid.ItemSize = itemSize;
                grid.ItemPos = new Vector2Int(x, y);
                _currentLevelData.SetGrid(targetX, targetY, grid);
            }
            
            //每次修改确保合法，然后保存
            SaveLevelData();
            return true;
        }

        private void ClearOccupiedGrids(GridData grid)
        {
            Debug.Log("ClearOccupiedGrids: " + grid.ToString());
            var offsets = GetOffsetsForItemSizeAndRotation(grid.ItemSize);
            if (!grid.isOccupied || grid.ItemSize == ItemSize.None)
            {
                Debug.LogError( "ClearOccupiedGrids: grid is not occupied or item size is none: " + grid.position);
                return;
            }
            var curItemPos = grid.ItemPos;
            foreach (var offset in offsets)
            {
                int targetX = curItemPos.x + offset.x;
                int targetY = curItemPos.y + offset.y;
                // 更新两个数据源
                LevelData.levelData.ResetGrid(targetX, targetY);
                _currentLevelData.ResetGrid(targetX, targetY);
            }

        }

        /// <summary>
        /// 根据Trigger Type获取偏移量数组
        /// </summary>
        private Vector2Int[] GetOffsetsForTriggerSizeAndRotation(TriggerType triggerType, int x, int y)
        {
            var isColumn = x == -1 || x == Width;
            switch (triggerType)
            {
                case TriggerType.One:  //  trigger 只占用1个格子
                    return new[] { new Vector2Int(0, 0) };
                    
                case TriggerType.Two:  // trigger占用2个格子
                    
                    return isColumn ? 
                        new[]
                        {
                            new Vector2Int(0, 0),   // 中心点
                            new Vector2Int(0, 1),  // 上方
                            
                        } :
                        new[] {
                            new Vector2Int(0, 0),   // 中心点侧
                            new Vector2Int(1, 0),    // 右方
                    };
                    
                case TriggerType.Three:  //  trigger占用3个格子
                    return isColumn ? 
                        new[]
                        {
                            new Vector2Int(0, -1),  // 下方
                            new Vector2Int(0, 0),   // 中心点
                            new Vector2Int(0, 1),  // 上方
                            
                        } :
                        new[] {
                            new Vector2Int(-1, 0),  // 左方
                            new Vector2Int(0, 0),   // 中心点
                            new Vector2Int(1, 0)    // 右方
                        };
                    
                default:  // 默认情况下只占用一个格子
                    return new[] { new Vector2Int(0, 0) };
            }
        }
        
        /// <summary>
        /// 根据物品尺寸获取偏移量数组
        /// </summary>
        private Vector2Int[] GetOffsetsForItemSizeAndRotation(ItemSize size)
        {
            switch (size)
            {
                case ItemSize.OneByOne:  // 1x1方块只占用一个格子
                    return new[] { new Vector2Int(0, 0) };
                    
                case ItemSize.TwoByTwo:  // 2x2方块占用左上三个格子
                    return new[] {
                        new Vector2Int(0, 0),   // 中心点
                        new Vector2Int(-1, 0),  // 左侧
                        new Vector2Int(0, 1)    // 上方
                    };
                    
                case ItemSize.ThreeByTwo:  // 3x2方块占用中心点周围四个格子
                    return new[] {
                        new Vector2Int(0, 0),   // 中心点
                        new Vector2Int(1, 0),   // 右侧
                        new Vector2Int(-1, 0),  // 左侧
                        new Vector2Int(0, 1)    // 上方
                    };
                    
                case ItemSize.ThreeByThree:  // 3x3方块占用十字形五个格子
                    return new[] {
                        new Vector2Int(0, 0),   // 中心点
                        new Vector2Int(1, 0),   // 右侧
                        new Vector2Int(-1, 0),  // 左侧
                        new Vector2Int(0, 1),   // 上方
                        new Vector2Int(0, -1),  // 下方
                    };
                    
                default:  // 默认情况下只占用一个格子
                    return new[] { new Vector2Int(0, 0) };
            }
        }

        // 获取当前关卡数据
        public LevelData GetCurrentLevelData() => _currentLevelData;

        // 获取行数（高度）
        public int GetRows() => Height;

        // 获取列数（宽度）
        public int GetColumns() => Width;

        /// <summary>
        /// 保存当前关卡数据到LevelData对象
        /// </summary>
        public void SaveLevelData()
        {
            // 确保目标数组大小正确
            if (LevelData.levelData.Grids == null || LevelData.levelData.Grids.Length != _currentLevelData.Grids.Length)
            {
                LevelData.levelData.Grids = new GridData[_currentLevelData.Grids.Length];
            }

            // 复制所有网格数据
            for (int i = 0; i < _currentLevelData.Grids.Length; i++)
            {
                LevelData.levelData.Grids[i] = _currentLevelData.Grids[i];
            }
            
            // 复制所有触发器数据
            LevelData.levelData.Triggers = new Dictionary<string, TriggerData>(_currentLevelData.Triggers);

            // 更新尺寸信息
            LevelData.levelData.Width = _currentLevelData.Width;
            LevelData.levelData.Height = _currentLevelData.Height;
            Debug.Log("Level data saved.");
        }

        /// <summary>
        /// 加载关卡数据
        /// </summary>
        public void LoadLevelData()
        {
            SetCurrentLevelData();
            Debug.Log("Level data loaded.");
        }

        /// <summary>
        /// 重置所有网格数据为初始状态，并重置触发器数据
        /// </summary>
        public void ResetGridData()
        {
            // 遍历所有网格
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    // 更新两个数据源的网格数据
                    LevelData.levelData.ResetGrid(x, y);
                    _currentLevelData.ResetGrid(x, y);
                }
            }

            // 重置触发器数据
            LevelData.levelData.Triggers.Clear();
            _currentLevelData.Triggers.Clear();

            Debug.Log("Grid and triggers reset.");
        }

        /// <summary>
        /// 获取指定位置的网格颜色
        /// </summary>
        public Color GetGridColor(Vector2Int position)
        {
            GridData grid = _currentLevelData.GetGrid(position.x, position.y, true);
            // 如果格子被占用返回对应颜色，否则返回白色
            return grid.isOccupied ? colorData.gameColorsData[(int)grid.gameColor].color : Color.white;
        }
        
        
        /// <summary>
        /// 获取指定位置的Trigger网格颜色
        /// </summary>
        public Color GetTriggerGridColor(Vector2Int position)
        {
            TriggerData trigger = _currentLevelData.GetTrigger(position.x, position.y);
            if (trigger.triggerType == TriggerType.None)
            {
                return Color.white;
            }
            return colorData.gameColorsData[(int)trigger.gameColor].color;
        }

        /// <summary>
        /// 获取当前选中的颜色
        /// </summary>
        public Color GetSelectedGridColor()
        {
            // 在颜色数据中查找当前选中的颜色
            foreach (var data in colorData.gameColorsData)
            {
                if (data.gameColor == gameColor)
                {
                    return data.color;
                }
            }
            return Color.white;  // 默认返回白色
        }
        
        /// <summary>
        /// 设置指定位置的Trigger颜色
        /// </summary>
        public void SetTriggerColor(int x, int y)
        {
            TriggerData trigger = _currentLevelData.GetTrigger(x, y);
            trigger.gameColor = gameColor;
            _currentLevelData.SetTrigger(x, y, trigger);
        }

        /// <summary>
        /// 设置指定位置的网格颜色
        /// </summary>
        public void SetGridColor(int x, int y)
        {
            GridData grid = _currentLevelData.GetGrid(x, y);
            grid.gameColor = gameColor;
            _currentLevelData.SetGrid(x, y, grid);
        }

        /// <summary>
        /// 在场景视图中绘制网格预览 (没啥用)
        /// </summary>
        private void OnDrawGizmos()
        {
            return;
            // if (_currentLevelData == null) return;

            // // 绘制整体网格
            // Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 0.3f); // 浅灰色，低透明度
            // for (int x = 0; x < Width; x++)
            // {
            //     for (int y = 0; y < Height; y++)
            //     {
            //         Vector3 pos = GridSpaceToWorldSpace(x, y);
            //         Gizmos.DrawWireCube(pos + new Vector3(0, 0.01f, 0), new Vector3(gridSize * 0.9f, 0.01f, gridSize * 0.9f));
            //     }
            // }

            // // 绘制当前选中的格子
            // Gizmos.color = new Color(1f, 0f, 0f, 0.4f); // 红色，半透明
            // Vector2Int[] offsets = GetOffsetsForItemSizeAndRotation(itemSize);
            // foreach (var offset in offsets)
            // {
            //     Vector3 worldPos = GridSpaceToWorldSpace(offset.x, offset.y);
            //     // 绘制线框
            //     Gizmos.DrawWireCube(worldPos + new Vector3(0, 0.1f, 0), new Vector3(gridSize * 0.95f, 0.1f, gridSize * 0.95f));
            //     // 绘制半透明填充
            //     Gizmos.color = new Color(1f, 0.5f, 0.5f, 0.2f); // 浅红色，更低透明度
            //     Gizmos.DrawCube(worldPos + new Vector3(0, 0.05f, 0), new Vector3(gridSize * 0.9f, 0.05f, gridSize * 0.9f));
            // }
        }
    }
}