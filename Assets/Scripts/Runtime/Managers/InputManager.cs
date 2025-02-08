using Runtime.Entities;
using Runtime.Managers;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float sphereCastRadius;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Item selectedItem;
    [SerializeField] private bool isInputBlocked;
    [SerializeField] private float moveSpeed = 10f;

    // The world coordinate on the plane (at selected item’s Y) when the touch begins.
    private Vector3 initialTouchWorldPosition;
    // The offset between the object's position and the touch point. This is computed at the start of a drag.
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
                // Use the object's current Y position as the plane height.
                Plane plane = new Plane(Vector3.up, new Vector3(0, selectedItem.transform.position.y, 0));
                if (plane.Raycast(ray, out float enter))
                {
                    initialTouchWorldPosition = ray.GetPoint(enter);
                    // Compute the offset between the object's current position and the touch world position.
                    touchOffset = selectedItem.transform.position - initialTouchWorldPosition;
                }
            }
        }
    }

    private void OnTouch(Vector3 screenPosition)
    {
        if (selectedItem != null)
        {
            // Use the current Y of the object as the plane's height.
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            Plane plane = new Plane(Vector3.up, new Vector3(0, selectedItem.transform.position.y, 0));
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 currentTouchWorldPosition = ray.GetPoint(enter);
                // The intended position is the new touch position plus the offset computed at touch start.
                Vector3 intendedPosition = currentTouchWorldPosition + touchOffset;
                intendedPosition.y = selectedItem.transform.position.y; // keep Y constant

                // Compute th delta from the current position.
                Vector3 delta = intendedPosition - selectedItem.transform.position;

                bool canMoveX, canMoveZ;
                Vector3 allowedDelta = Vector3.zero;

                if (selectedItem.CanMoveInXZ(selectedItem.transform.position + new Vector3(delta.x, 0, 0), out canMoveX, out canMoveZ))
                {
                    allowedDelta.x = canMoveX ? delta.x : 0;
                }
                // Check if movement in Z axis is allowed.
                if (selectedItem.CanMoveInXZ(selectedItem.transform.position + new Vector3(0, 0, delta.z), out canMoveX, out canMoveZ))
                {
                    allowedDelta.z = canMoveZ ? delta.z : 0;
                }

                // Calculate new target position based on allowed movement.
                Vector3 targetPosition = selectedItem.transform.position + allowedDelta;
                targetPosition.y = selectedItem.transform.position.y;

                // Use Lerp for smooth movement.
                selectedItem.transform.position = Vector3.Lerp(selectedItem.transform.position, targetPosition, Time.deltaTime * moveSpeed);
            }
        }
    }

    private void OnTouchEnd()
    {
        if (selectedItem != null)
        {
            // Snap the item to grid position.
            Vector2Int gridPosition = gridManager.WorldSpaceToGridSpace(selectedItem.transform.position);
            Vector3 snapPosition = gridManager.GridSpaceToWorldSpace(gridPosition);
            snapPosition.y = selectedItem.transform.position.y;
            selectedItem.transform.position = snapPosition;

            // Clear the selection.
            selectedItem = null;
        }
    }
} 