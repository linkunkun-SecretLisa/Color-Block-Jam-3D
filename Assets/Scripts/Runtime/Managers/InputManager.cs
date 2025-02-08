using Runtime.Entities;
using UnityEngine;

namespace Runtime.Managers
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float sphereCastRadius;
        [SerializeField] private GridManager gridManager;
        [SerializeField] private Item selectedItem;
        [SerializeField] private bool isInputBlocked;
        [SerializeField] private float moveSpeed = 10f;

        private Vector3 initialTouchWorldPosition;
        private Vector3 touchOffset;

        void Update()
        {
            if (!isInputBlocked)
                GetInput();
        }

        private void GetInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnTouchStart(Input.mousePosition);
            }

            if (Input.GetMouseButton(0))
            {
                OnTouch(Input.mousePosition);
            }

            if (Input.GetMouseButtonUp(0))
            {
                OnTouchEnd();
            }
        }

        private void OnTouchStart(Vector3 screenPosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            if (Physics.SphereCast(ray, sphereCastRadius, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                selectedItem = hit.transform.GetComponent<Item>();
                if (selectedItem != null)
                {
                    Plane plane = new Plane(Vector3.up, new Vector3(0, selectedItem.transform.position.y, 0));
                    if (plane.Raycast(ray, out float enter))
                    {
                        initialTouchWorldPosition = ray.GetPoint(enter);
                        touchOffset = selectedItem.transform.position - initialTouchWorldPosition;
                        selectedItem.OnSelected();
                    }
                }
            }
        }

        private void OnTouch(Vector3 screenPosition)
        {
            if (selectedItem != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(screenPosition);
                Plane plane = new Plane(Vector3.up, new Vector3(0, selectedItem.transform.position.y, 0));
                if (plane.Raycast(ray, out float enter))
                {
                    Vector3 currentTouchWorldPosition = ray.GetPoint(enter);
                    Vector3 intendedPosition = currentTouchWorldPosition + touchOffset; intendedPosition.y = selectedItem.transform.position.y; 
                    Vector3 delta = intendedPosition - selectedItem.transform.position;
                    Vector3 allowedDelta = Vector3.zero;
                    bool canMoveX, canMoveZ;


                    if (selectedItem.CanMoveInXZ(selectedItem.transform.position + new Vector3(delta.x, 0, 0), out canMoveX, out canMoveZ))
                    {
                        allowedDelta.x = canMoveX ? delta.x : 0;
                    }

                    if (selectedItem.CanMoveInXZ(selectedItem.transform.position + new Vector3(0, 0, delta.z), out canMoveX, out canMoveZ))
                    {
                        allowedDelta.z = canMoveZ ? delta.z : 0;
                    }

                    Vector3 targetPosition = selectedItem.transform.position + allowedDelta;
                    targetPosition.y = selectedItem.transform.position.y;

                    selectedItem.transform.position = Vector3.Lerp(selectedItem.transform.position, targetPosition, Time.deltaTime * moveSpeed);
                }
            }
        }

        private void OnTouchEnd()
        {
            if (selectedItem != null)
            {
                Vector2Int gridPosition = gridManager.WorldSpaceToGridSpace(selectedItem.transform.position);
                Vector3 snapPosition = gridManager.GridSpaceToWorldSpace(gridPosition);
                snapPosition.y = selectedItem.transform.position.y;
                selectedItem.transform.position = snapPosition;
                selectedItem.OnDeselected();
                selectedItem = null;
            }
        }
    }
}