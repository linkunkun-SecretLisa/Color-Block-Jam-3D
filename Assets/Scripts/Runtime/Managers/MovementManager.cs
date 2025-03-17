using Runtime.Entities;
using Runtime.Utilities;
using UnityEngine;

namespace Runtime.Managers
{
    public class MovementManager : SingletonMonoBehaviour<MovementManager>
    {
        // 物品移动速度
        [SerializeField] private float moveSpeed = 10f;
        // 网格管理器引用
        [SerializeField] private GridManager gridManager;
        // 当前选中的物品
        private Item selectedItem;
        // 初始触摸点在世界空间中的位置
        private Vector3 initialTouchWorldPosition;
        // 物品位置与触摸点的偏移量
        private Vector3 touchOffset;
        
        public Item GetSelectedItem()
        {
            return selectedItem;
        }

        public void SetSelectedItem(Item item)
        {
            selectedItem = item;
        }

        // 开始移动物品
        public void StartMovement(Item item, Ray ray)
        {
            if (item == null)
            {
                Debug.LogError( "selectedItem is null");
                return;
            }

            selectedItem = item;
            // 创建一个与物品高度相同的水平面
            Plane plane = new Plane(Vector3.up, new Vector3(0, selectedItem.transform.position.y, 0));
            if (!plane.Raycast(ray, out var enter)) return;
            
            // 计算触摸点在世界空间中的位置
            initialTouchWorldPosition = ray.GetPoint(enter);
            // 计算物品位置与触摸点的偏移
            touchOffset = selectedItem.transform.position - initialTouchWorldPosition;
            // 通知物品被选中
            selectedItem.OnSelected();
        }

        // 更新物品移动
        public void UpdateMovement(Ray ray)
        {
            if (selectedItem == null)
            {
                return;
            }

            // 创建一个与物品高度相同的水平面
            Plane plane = new Plane(Vector3.up, new Vector3(0, selectedItem.transform.position.y, 0));
            if (!plane.Raycast(ray, out float enter)) return;
            // 计算当前触摸点在世界空间中的位置
            Vector3 currentTouchWorldPosition = ray.GetPoint(enter);
            // 计算物品的目标位置
            Vector3 intendedPosition = currentTouchWorldPosition + touchOffset;
            // 保持物品的高度不变
            intendedPosition.y = selectedItem.transform.position.y;

            // 计算位移向量 （预期位置与当前位置之间的差值）
            Vector3 delta = intendedPosition - selectedItem.transform.position;

            // 初始化允许的移动量
            Vector3 allowedDelta = Vector3.zero;
            bool canMoveX, canMoveZ;

            // 分别检查X轴和Z轴方向的移动是否合法
            if (selectedItem.CanChildsMoveInXZ(selectedItem.transform.position + new Vector3(delta.x, 0, 0), 
                    out canMoveX, out canMoveZ))
            {
                allowedDelta.x = canMoveX ? delta.x : 0;
            }

            if (selectedItem.CanChildsMoveInXZ(selectedItem.transform.position + new Vector3(0, 0, delta.z), 
                    out canMoveX, out canMoveZ))
            {
                allowedDelta.z = canMoveZ ? delta.z : 0;
            }

            // 计算最终目标位置
            Vector3 targetPosition = selectedItem.transform.position + allowedDelta;
            targetPosition.y = selectedItem.transform.position.y;

            // 平滑移动到目标位置 ｜ 每帧移动总距离的一小部分（例如 `moveSpeed=10` 时，每秒移动 10% 的剩余距离）
            selectedItem.transform.position = Vector3.Lerp(
                selectedItem.transform.position, 
                targetPosition, 
                Time.deltaTime * moveSpeed);
        }

        // 结束移动
        public void EndMovement()
        {
            if (selectedItem == null)
            {
                Debug.LogError( "selectedItem is null");
                return;
            }

            // 将物品对齐到网格
            Vector2Int gridPosition = gridManager.WorldSpaceToGridSpace(selectedItem.transform.position);
            Vector3 snapPosition = gridManager.GridSpaceToWorldSpace(gridPosition);
            var selectedItemTransform = selectedItem.transform;
            snapPosition.y = selectedItemTransform.position.y;
            selectedItemTransform.position = snapPosition;
            
            // 通知物品取消选中并更新网格状态
            selectedItem.OnDeselected(gridPosition);
            selectedItem = null;
            gridManager.UpdateAllCellOccupied();
        }
    }
}