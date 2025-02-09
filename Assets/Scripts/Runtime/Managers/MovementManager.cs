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
            // Physics.SyncTransforms(); solution tried but not worked
            if (_selectedItem != null)
            {
                Plane plane = new Plane(Vector3.up, new Vector3(0, _selectedItem.transform.position.y, 0));
                if (plane.Raycast(ray, out float enter))
                {
                    Vector3 currentTouchWorldPosition = ray.GetPoint(enter);
                    Vector3 intendedPosition = currentTouchWorldPosition + _touchOffset;
                    intendedPosition.y = _selectedItem.transform.position.y;

                    Vector3 delta = intendedPosition - _selectedItem.transform.position;


                    Vector3 allowedDelta = Vector3.zero;
                    bool canMoveX, canMoveZ;

                    if (_selectedItem.CanChildsMoveInXZ(_selectedItem.transform.position + new Vector3(delta.x, 0, 0),
                            out canMoveX, out canMoveZ))
                    {
                        allowedDelta.x = canMoveX ? delta.x : 0;
                    }

                    if (_selectedItem.CanChildsMoveInXZ(_selectedItem.transform.position + new Vector3(0, 0, delta.z),
                            out canMoveX, out canMoveZ))
                    {
                        allowedDelta.z = canMoveZ ? delta.z : 0;
                    }

                    Vector3 targetPosition = _selectedItem.transform.position + allowedDelta;
                    targetPosition.y = _selectedItem.transform.position.y;

                    _selectedItem.transform.position = Vector3.Lerp(_selectedItem.transform.position, targetPosition,
                        Time.deltaTime * moveSpeed);
                }
            }
        }

        public void EndMovement()
        {
            if (_selectedItem != null)
            {
                Vector2Int gridPosition = gridManager.WorldSpaceToGridSpace(_selectedItem.transform.position);
                Vector3 snapPosition = gridManager.GridSpaceToWorldSpace(gridPosition);

                var selectedItemTransform = _selectedItem.transform;
                snapPosition.y = selectedItemTransform.position.y;
                selectedItemTransform.position = snapPosition;
                _selectedItem.OnDeselected(gridPosition);
                _selectedItem = null;
                gridManager.UpdateAllCellOccupied();
            }
        }
    }
}