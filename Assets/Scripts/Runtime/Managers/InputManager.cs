using Runtime.Entities;
using Runtime.Enums;
using Runtime.Managers;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float sphereCastRadius;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Item selectedItem;
    [SerializeField] private bool isInputBlocked;

    void Update()
    {
        CheckInputBlock();
        if (!isInputBlocked) GetInput();
    }

    private void CheckInputBlock()
    {
        // if (GameManager.Instance.GameStates != GameStates.Gameplay)
        //     isInputBlocked = true;
        // else
        //     isInputBlocked = false;
    }

    private void GetInput()
    {
        #region Mobile Input

        // if (Input.touchCount > 0)
        // {
        //     Touch touch = Input.GetTouch(0);
        //     switch (touch.phase)
        //     {
        //         case TouchPhase.Began:
        //             OnTouchStart(touch.position);
        //             break;
        //         case TouchPhase.Moved:
        //         case TouchPhase.Stationary:
        //             OnTouch(touch.position);
        //             break;
        //         case TouchPhase.Ended:
        //         case TouchPhase.Canceled:
        //             OnTouchEnd(touch.position);
        //             break;
        //     }
        // }

        #endregion

        #region PC Input

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
            OnTouchEnd(Input.mousePosition);
        }

        #endregion
    }

    private void OnTouchStart(Vector3 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        RaycastHit hit;

        if (Physics.SphereCast(ray, sphereCastRadius, out hit, Mathf.Infinity, layerMask))
        {
            selectedItem = hit.transform.GetComponent<Item>();
            if (selectedItem != null)
            {
                // selectedItem.OnSelected();
            }
        }
    }

    private void OnTouch(Vector3 position)
    {
        if (selectedItem != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                Vector3 newPosition = hit.point;
                newPosition.y = selectedItem.transform.position.y; 

                bool isCanMoveX, isCanMoveZ;
                if (selectedItem.CanMoveInXZ(newPosition, out isCanMoveX, out isCanMoveZ))
                {
                    if (isCanMoveX)
                    {
                        selectedItem.transform.position = new Vector3(newPosition.x, selectedItem.transform.position.y, selectedItem.transform.position.z);
                    }
                    if (isCanMoveZ)
                    {
                        selectedItem.transform.position = new Vector3(selectedItem.transform.position.x, selectedItem.transform.position.y, newPosition.z);
                    }
                }
            }
        }
    }

    private void OnTouchEnd(Vector3 position)
    {
        if (selectedItem != null)
        {
            Vector2Int gridPosition = gridManager.WorldSpaceToGridSpace(selectedItem.transform.position);
            Vector3 snapPosition = gridManager.GridSpaceToWorldSpace(gridPosition);
            snapPosition.y = selectedItem.transform.position.y;
            selectedItem.transform.position = snapPosition;
            // selectedItem.OnReleased();
            selectedItem = null;
        }
    }
}