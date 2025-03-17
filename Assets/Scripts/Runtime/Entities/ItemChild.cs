using Runtime.Utilities;
using UnityEngine;

namespace Runtime.Entities
{
    /// <summary>
    /// 物品的子组件类，代表物品的一个组成部分
    /// </summary>
    public class ItemChild : MonoBehaviour
    {
        // 射线检测的距离
        [SerializeField] private float raycastDistance = 0.5f;
        // 障碍物层级遮罩
        [SerializeField] private LayerMask obstacleLayerMask;
        // 渲染器组件，用于材质和外观控制
        [SerializeField] private new Renderer renderer;
    
        /// <summary>
        /// 当物品被选中时调用，设置外边框宽度
        /// </summary>
        public void OnSelected()
        {
            renderer.material.SetFloat("_OutlineWidth", 2.5f);
        }
    
        /// <summary>
        /// 当物品取消选中时调用，移除外边框
        /// </summary>
        public void OnDeselected()
        {
            renderer.material.SetFloat("_OutlineWidth", 0.0f);
        }
    
        /// <summary>
        /// 应用新的材质颜色
        /// </summary>
        public void ApplyColor(Material material)
        {
            renderer.sharedMaterial = material;
        }
    
        /// <summary>
        /// 检查在XZ平面上是否可以移动到目标位置
        /// </summary>
        /// <param name="targetPosition">目标位置</param>
        /// <param name="directionOrigin">移动方向的原点</param>
        /// <param name="isCanMoveX">X轴是否可以移动</param>
        /// <param name="isCanMoveZ">Z轴是否可以移动</param>
        /// <returns>是否可以移动到目标位置</returns>
        public bool CanMoveInXZ(Vector3 targetPosition, Transform directionOrigin, out bool isCanMoveX, out bool isCanMoveZ)
        {
            // 计算移动方向和距离（计算从参考点（`directionOrigin.position`）到目标位置的向量差 `delta`）
            Vector3 originPos = directionOrigin.position;
            Vector3 delta = targetPosition - originPos;
    
            isCanMoveX = true;
            isCanMoveZ = true;
    
            // 检查X轴移动
            if (Mathf.Abs(delta.x) > 0.01f)
            {
                Vector3 directionX = new Vector3(Mathf.Sign(delta.x), 0, 0);
                isCanMoveX = CanMoveInAllDirections(directionX);
            }
    
            // 检查Z轴移动
            if (Mathf.Abs(delta.z) > 0.01f)
            {
                Vector3 directionZ = new Vector3(0, 0, Mathf.Sign(delta.z));
                isCanMoveZ = CanMoveInAllDirections(directionZ);
            }
    
            return isCanMoveX && isCanMoveZ;
        }
    
        /// <summary>
        /// 检查是否可以向指定方向移动（使用多条射线检测）
        ///     发射3条平行射线：中心点、正偏移点和负偏移点
        ///     偏移量为0.45单位（接近物体边缘但不超出）
        /// </summary>
        public bool CanMoveInAllDirections(Vector3 direction)
        {
            // 定义射线检测的偏移量
            Vector3 rightOffset = new Vector3(0, 0, 1) * 0.45f;
            Vector3 upOffset = new Vector3(1, 0, 0) * 0.45f;
    
            // 初始化射线起点数组
            Vector3[] raycastOrigins = new Vector3[3];
    
            // 根据移动方向设置射线起点
            if (Mathf.Abs(direction.x) > 0)
            {
                raycastOrigins[0] = transform.position; // 中心点
                raycastOrigins[1] = transform.position + rightOffset; // 上边缘
                raycastOrigins[2] = transform.position - rightOffset; // 下边缘
            }
            else if (Mathf.Abs(direction.z) > 0)
            {
                raycastOrigins[0] = transform.position; // 中心点
                raycastOrigins[1] = transform.position + upOffset; // 右边缘
                raycastOrigins[2] = transform.position - upOffset; // 左边缘
            }
    
            // 从每个起点发射射线（长度0.5）检测障碍物
            foreach (var origin in raycastOrigins)
            {
                if (Physics.Raycast(origin, direction, out RaycastHit hit, raycastDistance, obstacleLayerMask))
                {
                    // 如果碰到的不是父物体，则表示有障碍物
                    if (hit.transform != transform.parent)
                    {
                        return false;
                    }
                }
            }
    
            return true;
        }
    
        /// <summary>
        /// 检查到目标位置的路径是否畅通
        /// </summary>
        public bool IsPathClearToPosition(Vector3 targetPosition)
        {
            // 计算方向向量并忽略Y轴
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0;
    
            // 确定主要移动方向（X或Z轴）
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            {
                direction = new Vector3(Mathf.Sign(direction.x), 0, 0);
            }
            else
            {
                direction = new Vector3(0, 0, Mathf.Sign(direction.z));
            }
    
            // 设置检测的层级遮罩
            int layerMask = ConstantsUtilities.TriggerBlockLayerMask | ConstantsUtilities.ItemLayerMask;
    
            // 发射射线检测路径
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                // 如果碰到触发块，路径通畅
                if (hit.collider.gameObject.layer == ConstantsUtilities.TriggerBlockLayer)
                {
                    return true;
                }
    
                // 如果碰到其他物品，检查是否为自身或父物体
                if (hit.collider.gameObject.layer == ConstantsUtilities.ItemLayer)
                {
                    if (hit.transform != transform.parent && hit.transform.parent != transform.parent && hit.transform != transform)
                    {
                        return false;
                    }
                }
                // 如果碰到障碍物，路径通畅
                if (hit.collider.gameObject.layer == ConstantsUtilities.ObstacleLayer)
                {
                    return true;
                }
            }
    
            return true;
        }
    }
}