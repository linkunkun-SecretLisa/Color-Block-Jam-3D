using System.Collections.Generic;
using UnityEngine;
using Runtime.Entities;
using Runtime.Data.ValueObject;
using Runtime.Enums;
using Runtime.Utilities;
using Unity.VisualScripting;
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
    
        // 所有格子的父物体
        [SerializeField] private GameObject cellParent;
        // 格子预制体
        [SerializeField] private GameObject cellPrefab;
        // 所有障碍物的父物体
        [SerializeField] private GameObject obstacleParent;
        // 障碍物预制体
        [SerializeField] private GameObject obstaclePrefab;
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
        public void Initialize(int width, int height, float spaceModifier)
        {
            _width = width;
            _height = height;
            _spaceModifier = spaceModifier;
    
            CreateGrid();
    
            // 标记对象为脏，确保Unity保存更改
    #if UNITY_EDITOR
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
            CreateObstaclesAroundGrid();
        }
    
        // 在网格周围创建障碍物
        private void CreateObstaclesAroundGrid()
        {
            // 清除现有的障碍物父物体
            if (obstacleParent != null)
            {
                DestroyImmediate(obstacleParent);
            }
    
            obstacleParent = new GameObject("ObstacleParent");
    
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
    
                    // 创建障碍物
                    GameObject obstacle = (GameObject)PrefabUtility.InstantiatePrefab(obstaclePrefab, obstacleParent.transform);
                    obstacle.transform.position = position;
                    obstacle.transform.rotation = rotation;
    
                    EditorUtility.SetDirty(obstacle);
                }
            }
        }
#endif
    
        #endregion
    
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
            if (cellParent != null)
            {
                DestroyImmediate(cellParent);
            }
    
            cellParent = new GameObject("CellParent");
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
                foreach (var childItem in item.childItems)
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
    }
}