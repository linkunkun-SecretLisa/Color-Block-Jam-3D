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
            if (Input.touchCount > 0)
            {
                ProcessTouchInput();
            }
            else
            {
                ProcessMouseInput();
            }
        }

        private void ProcessMouseInput()
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

        private void ProcessTouchInput()
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    HandleTouchStart(touch.position);
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    HandleTouch(touch.position);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    HandleTouchEnd();
                    break;
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