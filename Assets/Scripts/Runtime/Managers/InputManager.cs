using Runtime.Entities;
using Runtime.Extensions;
using UnityEngine;

namespace Runtime.Managers
{
    public class InputManager : SingletonMonoBehaviour<InputManager>
    {
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float sphereCastRadius;
        [SerializeField] private bool isInputBlocked;

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
                Item selectedItem = hit.transform.GetComponent<Item>();
                if (selectedItem != null)
                {
                    MovementManager.Instance.StartMovement(selectedItem, ray);
                }
            }
        }

        private void OnTouch(Vector3 screenPosition)
        {
            MovementManager.Instance.UpdateMovement(Camera.main.ScreenPointToRay(screenPosition));
        }

        private void OnTouchEnd()
        {
            MovementManager.Instance.EndMovement();
        }
    }
}