using System.Collections.Generic;
using UnityEngine;
using Runtime.Enums;
using Runtime.Data.ValueObject;
using Runtime.Controllers;
using Runtime.Data.UnityObject;

namespace Runtime.Utilities
{
    /// <summary>
    /// 关卡生成工具类：提供关卡生成相关的公共方法
    /// 供LevelCreatorScript和RuntimeLevelGenerator共用
    /// </summary>
    public static class LevelGenerationUtility
    {
        /// <summary>
        /// 将网格坐标转换为世界坐标
        /// </summary>
        public static Vector3 GridToWorld(int x, int y, float spacing)
        {
            return new Vector3(x * spacing, 0, y * spacing);
        }
        
        /// <summary>
        /// 将世界坐标转换为网格坐标
        /// </summary>
        public static Vector2Int WorldToGrid(Vector3 worldPos, float spacing)
        {
            int x = Mathf.RoundToInt(worldPos.x / spacing);
            int y = Mathf.RoundToInt(worldPos.z / spacing);
            return new Vector2Int(x, y);
        }
        
        /// <summary>
        /// 获取边界的旋转角度
        /// </summary>
        public static Quaternion GetBoundaryRotation(int x, int y, int width, int height)
        {
            if (x == -1) // 左边界
                return Quaternion.Euler(0, 90, 0);
            if (x == width) // 右边界
                return Quaternion.Euler(0, -90, 0);
            if (y == -1) // 下边界
                return Quaternion.Euler(0, 0, 0);
            if (y == height) // 上边界
                return Quaternion.Euler(0, 180, 0);
                
            return Quaternion.identity;
        }
        
        /// <summary>
        /// 获取触发器在边界上的位置
        /// </summary>
        public static Vector3 GetTriggerPosition(Vector3 position, Vector2Int originalPos, TriggerType type, int width, int height, float spacing)
        {
            Vector3 result = position;
            
            // 判断是边界的哪一侧
            bool isColumn = originalPos.x == -1 || originalPos.x == width;
            
            // 根据触发器类型调整位置
            if (isColumn)
            {
                // 竖边界上的触发器
                if (type == TriggerType.Two)
                    result.z += 0.5f * spacing;
                else if (type == TriggerType.Three)
                    result.z += 1.0f * spacing;
            }
            else
            {
                // 横边界上的触发器
                if (type == TriggerType.Two)
                    result.x += 0.5f * spacing;
                else if (type == TriggerType.Three)
                    result.x += 1.0f * spacing;
            }
            
            return result;
        }
        
        /// <summary>
        /// 设置触发器颜色
        /// </summary>
        public static void SetTriggerColor(GameObject trigger, GameColor gameColor, CD_GameColor colorData)
        {
            // 检查是否有颜色数据
            if (colorData == null || gameColor == GameColor.None)
            {
                Debug.LogWarning("颜色数据未设置或颜色类型为None，无法设置触发器颜色");
                return;
            }
            
            // 获取触发器控制器
            BlockTriggerController controller = trigger.GetComponent<BlockTriggerController>();
            if (controller != null)
            {
                controller.TriggerColor = gameColor;
            }
            
            // 设置渲染器材质
            Renderer renderer = trigger.GetComponent<Renderer>();
            if (renderer == null)
            {
                renderer = trigger.GetComponentInChildren<Renderer>();
            }
            
            if (renderer != null)
            {
                // 获取颜色材质
                Material colorMaterial = colorData.gameColorsData[(int)gameColor].materialColor;
                
                if (colorMaterial != null)
                {
                    // 使用预定义的材质
                    renderer.material = colorMaterial;
                }
                else
                {
                    // 使用颜色值
                    Color color = colorData.gameColorsData[(int)gameColor].color;
                    
                    Material newMat = new Material(renderer.material);
                    newMat.color = color;
                    
                    // 尝试设置不同着色器属性
                    if (newMat.HasProperty("_Color"))
                    {
                        newMat.SetColor("_Color", color);
                    }
                    else if (newMat.HasProperty("_BaseColor"))
                    {
                        newMat.SetColor("_BaseColor", color);
                    }
                    
                    renderer.material = newMat;
                }
            }
        }
        
        /// <summary>
        /// 判断指定坐标位置是否为列或行边界
        /// </summary>
        public static (bool isColumn, bool isRow) GetBoundaryType(int x, int y, int width, int height)
        {
            bool isColumn = x == -1 || x == width;
            bool isRow = y == -1 || y == height;
            return (isColumn, isRow);
        }
        
        /// <summary>
        /// 根据物品尺寸获取偏移量数组
        /// </summary>
        public static Vector2Int[] GetOffsetsForItemSize(ItemSize size)
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
        
        /// <summary>
        /// 根据触发器类型获取偏移量数组
        /// </summary>
        public static Vector2Int[] GetOffsetsForTriggerType(TriggerType triggerType, int x, int y, int width, int height)
        {
            var (isColumn, isRow) = GetBoundaryType(x, y, width, height);
            
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
    }
} 