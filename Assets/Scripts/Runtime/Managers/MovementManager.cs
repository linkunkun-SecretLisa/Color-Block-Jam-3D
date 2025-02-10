using Runtime.Entities;
using Runtime.Utilities;
using UnityEngine;

namespace Runtime.Managers
{
    public class MovementManager : SingletonMonoBehaviour<MovementManager>
    {
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private GridManager gridManager;
        private Item _selectedItem;
        private Vector3 _initialTouchWorldPosition;
        private Vector3 _touchOffset;
        private const float defaultRaycastDistance = 0.5f;

        public void StartMovement(Item item, Ray ray)
        {
            _selectedItem = item;
            Plane plane = new Plane(Vector3.up, new Vector3(0, _selectedItem.transform.position.y, 0));
            if (plane.Raycast(ray, out float enter))
            {
                _initialTouchWorldPosition = ray.GetPoint(enter);
                _touchOffset = _selectedItem.transform.position - _initialTouchWorldPosition;
                _selectedItem.OnSelected();
            }
        }

        public void UpdateMovement(Ray ray)
        {
            if (_selectedItem == null) return;

            Plane plane = new Plane(Vector3.up, new Vector3(0, _selectedItem.transform.position.y, 0));
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 currentTouchWorldPosition = ray.GetPoint(enter);
                Vector3 intendedPosition = currentTouchWorldPosition + _touchOffset;
                intendedPosition.y = _selectedItem.transform.position.y;
                Vector3 delta = intendedPosition - _selectedItem.transform.position;

                Vector3 allowedDelta = CalculateAllowedDelta(delta);
                MoveSelectedItem(allowedDelta);
                ApplyPushBack(delta);
            }
        }

        private Vector3 CalculateAllowedDelta(Vector3 delta)
        {
            Vector3 allowedDelta = Vector3.zero;
            bool canMoveX, canMoveZ;
            float xhitDistance, zhitDistance;

            _selectedItem.CanChildsMoveInXZ(_selectedItem.transform.position + new Vector3(delta.x, 0, 0), out canMoveX, out _, out xhitDistance, out _);
            allowedDelta.x = canMoveX ? delta.x : 0;

            _selectedItem.CanChildsMoveInXZ(_selectedItem.transform.position + new Vector3(0, 0, delta.z), out _, out canMoveZ, out _, out zhitDistance);
            allowedDelta.z = canMoveZ ? delta.z : 0;

            return allowedDelta;
        }

        private void MoveSelectedItem(Vector3 allowedDelta)
        {
            Vector3 targetPosition = _selectedItem.transform.position + allowedDelta;
            targetPosition.y = _selectedItem.transform.position.y;
            _selectedItem.transform.position = Vector3.Lerp(_selectedItem.transform.position, targetPosition, Time.deltaTime * moveSpeed);
        }

        private void ApplyPushBack(Vector3 delta)
        {
            bool canMoveX, canMoveZ;
            float xhitDistance, zhitDistance;

            _selectedItem.CanChildsMoveInXZ(_selectedItem.transform.position, out canMoveX, out canMoveZ, out xhitDistance, out zhitDistance);

            Vector3 pushBack = Vector3.zero;
            if (xhitDistance < defaultRaycastDistance)
            {
                float pushAmount = defaultRaycastDistance - xhitDistance;
                pushBack.x = -Mathf.Sign(delta.x) * pushAmount;
            }
            if (zhitDistance < defaultRaycastDistance)
            {
                float pushAmount = defaultRaycastDistance - zhitDistance;
                pushBack.z = -Mathf.Sign(delta.z) * pushAmount;
            }
            _selectedItem.transform.position += pushBack;
        }

        public void EndMovement()
        {
            if (_selectedItem == null) return;

            Vector2Int gridPosition = gridManager.WorldSpaceToGridSpace(_selectedItem.transform.position);
            Vector3 snapPosition = gridManager.GridSpaceToWorldSpace(gridPosition);
            snapPosition.y = _selectedItem.transform.position.y;
            _selectedItem.transform.position = snapPosition;
            _selectedItem.OnDeselected(gridPosition);
            _selectedItem = null;
            gridManager.UpdateAllCellOccupied();
        }
    }
}