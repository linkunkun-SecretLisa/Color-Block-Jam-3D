using Runtime.Data.UnityObject;
using Runtime.Enums;
using Runtime.Managers;
using UnityEngine;

namespace Runtime.Entities
{
    /// <summary>
    /// 游戏中的物品类，代表一个可移动的游戏对象
    /// </summary>
    public class Item : MonoBehaviour
    {
        // 物品的子项数组，通常是构成物品的各个部分
        public ItemChild[] childItems;
        // 物品的碰撞体数组
        public BoxCollider[] colliders;
        // 物品的颜色枚举
        public GameColor itemColor;
        // 颜色配置数据
        public CD_GameColor colorData;
        // 物品大小枚举
        public ItemSize itemSize;

        /// <summary>
        /// 初始化物品
        /// </summary>
        /// <param name="gameColor">物品颜色</param>
        /// <param name="gridManager">网格管理器引用</param>
        public void Init(GameColor gameColor, GridManager gridManager)
        {
            itemColor = gameColor;
            gridManager.AddItem(this);
            ApplyChildColor();
        }

        /// <summary>
        /// 物品被选中时的处理
        /// </summary>
        public void OnSelected()
        {
            foreach (var child in childItems)
            {
                child.OnSelected();
            }
        }

        /// <summary>
        /// 物品取消选中时的处理
        /// </summary>
        /// <param name="gridPos">网格位置</param>
        public void OnDeselected(Vector2Int gridPos)
        {
            foreach (var child in childItems)
            {
                child.OnDeselected();
            }
        }

        /// <summary>
        /// 禁用所有碰撞体
        /// </summary>
        public void DisableColliders()
        {
            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }
        }

        /// <summary>
        /// 应用颜色到所有子项
        /// </summary>
        public void ApplyChildColor()
        {
            foreach (var child in childItems)
            {
                child.ApplyColor(colorData.gameColorsData[(int)itemColor].materialColor);
            }
        }

        /// <summary>
        /// 检查所有子项是否可以在XZ平面上移动到目标位置
        /// </summary>
        /// <param name="targetPosition">目标位置</param>
        /// <param name="canMoveX">X轴是否可移动</param>
        /// <param name="canMoveZ">Z轴是否可移动</param>
        /// <returns>是否所有方向都可以移动</returns>
        public bool CanChildsMoveInXZ(Vector3 targetPosition, out bool canMoveX, out bool canMoveZ)
        {
            bool allCanMoveX = true;
            bool allCanMoveZ = true;

            // 检查每个子项的移动可行性
            foreach (var child in childItems)
            {
                bool childCanMoveX, childCanMoveZ;
                child.CanMoveInXZ(targetPosition, transform, out childCanMoveX, out childCanMoveZ);
                // 使用逻辑与运算符，所有子项都必须可以移动
                allCanMoveX &= childCanMoveX;
                allCanMoveZ &= childCanMoveZ;
            }

            canMoveX = allCanMoveX;
            canMoveZ = allCanMoveZ;
            return canMoveX && canMoveZ;
        }

        /// <summary>
        /// 检查是否所有子项都可以到达目标位置（无限射线检测）
        /// </summary>
        /// <param name="targetPosition">目标位置</param>
        /// <returns>是否可以到达</returns>
        public bool CheckChildrenInfiniteRaycast(Vector3 targetPosition)
        {
            bool canReach = true;
            foreach (var child in childItems)
            {
                canReach &= child.IsPathClearToPosition(targetPosition);
            }

            return canReach;
        }

        /// <summary>
        /// 获取子项数量
        /// </summary>
        /// <returns>子项数量</returns>
        public int GetChildCount()
        {
            return childItems.Length;
        }
    }
}