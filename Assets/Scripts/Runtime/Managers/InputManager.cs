using System;
using Runtime.Entities;
using Runtime.Utilities;
using UnityEngine;

namespace Runtime.Managers
{
    public class InputManager : SingletonMonoBehaviour<InputManager>
    {
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float sphereCastRadius = 0.5f;
        [SerializeField] private bool isInputBlocked;
        private Camera mainCamera;

        public event Action OnTouch;

        protected override void Awake() 
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (isInputBlocked)
                return;
            
            ProcessInput();
        }

        private void ProcessInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleTouchStart(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0))
            {
                HandleTouch(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                HandleTouchEnd();
            }
        }

        private void HandleTouchStart(Vector3 screenPosition)
        {
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            if (Physics.SphereCast(ray, sphereCastRadius, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                Item item = hit.transform.GetComponent<Item>();
                if (item != null)
                {
                    MovementManager.Instance.StartMovement(item, ray);
                }
            }
        }

        private void HandleTouch(Vector3 screenPosition)
        {
            Ray currentRay = mainCamera.ScreenPointToRay(screenPosition);
            MovementManager.Instance.UpdateMovement(currentRay);
            OnTouch?.Invoke();
        }

        private void HandleTouchEnd()
        {
            MovementManager.Instance.EndMovement();
        }
    }
}