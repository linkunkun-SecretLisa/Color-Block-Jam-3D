using Runtime.Entities;
using Runtime.Utilities;
using UnityEngine;

namespace Runtime.Managers
{
    public class MovementManager : SingletonMonoBehaviour<MovementManager>
    {
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private GridManager gridManager;
        private Item selectedItem;
        private Vector3 initialTouchWorldPosition;
        private Vector3 touchOffset;


        public void StartMovement(Item item, Ray ray)
        {
            selectedItem = item;
            Plane plane = new Plane(Vector3.up, new Vector3(0, selectedItem.transform.position.y, 0));
            if (plane.Raycast(ray, out float enter))
            {
                initialTouchWorldPosition = ray.GetPoint(enter);
                touchOffset = selectedItem.transform.position - initialTouchWorldPosition;
                selectedItem.OnSelected();
            }
        }

        public void UpdateMovement(Ray ray)
        {
            // Physics.SyncTransforms(); solution tried but not worked
            if (selectedItem != null)
            {
                Plane plane = new Plane(Vector3.up, new Vector3(0, selectedItem.transform.position.y, 0));
                if (plane.Raycast(ray, out float enter))
                {
                    Vector3 currentTouchWorldPosition = ray.GetPoint(enter);
                    Vector3 intendedPosition = currentTouchWorldPosition + touchOffset;
                    intendedPosition.y = selectedItem.transform.position.y;

                    Vector3 delta = intendedPosition - selectedItem.transform.position;


                    Vector3 allowedDelta = Vector3.zero;
                    bool canMoveX, canMoveZ;

                    if (selectedItem.CanChildsMoveInXZ(selectedItem.transform.position + new Vector3(delta.x, 0, 0), out canMoveX, out canMoveZ))
                        allowedDelta.x = canMoveX ? delta.x : 0;

                    if (selectedItem.CanChildsMoveInXZ(selectedItem.transform.position + new Vector3(0, 0, delta.z), out canMoveX, out canMoveZ))
                        allowedDelta.z = canMoveZ ? delta.z : 0;

                    Vector3 targetPosition = selectedItem.transform.position + allowedDelta;
                    targetPosition.y = selectedItem.transform.position.y;

                    selectedItem.transform.position = Vector3.Lerp(selectedItem.transform.position, targetPosition, Time.deltaTime * moveSpeed);
                }
            }
        }

        public void EndMovement()
        {
            if (selectedItem != null)
            {
                Vector2Int gridPosition = gridManager.WorldSpaceToGridSpace(selectedItem.transform.position);
                Vector3 snapPosition = gridManager.GridSpaceToWorldSpace(gridPosition);
                var selectedItemTransform = selectedItem.transform;
                snapPosition.y = selectedItemTransform.position.y;
                selectedItemTransform.position = snapPosition;
                selectedItem.OnDeselected(gridPosition);
                selectedItem = null;
                gridManager.UpdateAllCellOccupied();
            }
        }


        public Item GetSelectedItem()
        {
            return selectedItem;
        }

        public void SetSelectedItem(Item item)
        {
            selectedItem = item;
        }
    }
}