using System;
using Runtime.Entities;
using Runtime.Utilities;
using UnityEngine;

namespace Runtime.Managers
{
    public class InputManager : SingletonMonoBehaviour<InputManager>
    {
        // 用于射线检测的层遮罩，决定射线可以检测哪些层级的对象
        [SerializeField] private LayerMask layerMask;
        // 球形射线检测的半径，增加检测的容错范围
        [SerializeField] private float sphereCastRadius = 0.5f;
        // 输入锁定标志，用于控制是否处理输入
        [SerializeField] private bool isInputBlocked;
        // 主摄像机引用
        private Camera mainCamera;
        
        // 触摸抬手事件，允许其他组件订阅触摸事件
        public static event Action OnTouchEnd;

        protected override void Awake()
        {
            // 获取主摄像机引用
            mainCamera = Camera.main;
        }
    
        private void Update()
        {
            // 如果输入被锁定，直接返回
            if (isInputBlocked)
                return;
    
            // 处理输入
            ProcessInput();
        }
    
        private void ProcessInput()
        {
            // 检测是否有触摸输入，优先处理触摸
            if (Input.touchCount > 0)
            {
                ProcessTouchInput();
            }
            else
            {
                // 没有触摸输入时处理鼠标输入
                ProcessMouseInput();
            }
        }
    
        private void ProcessMouseInput()
        {
            // 鼠标按下，相当于触摸开始
            if (Input.GetMouseButtonDown(0))
            {
                HandleTouchStart(Input.mousePosition);
            }
            // 鼠标持续按住，相当于触摸移动
            else if (Input.GetMouseButton(0))
            {
                HandleTouch(Input.mousePosition);
            }
            // 鼠标释放，相当于触摸结束
            else if (Input.GetMouseButtonUp(0))
            {
                HandleTouchEnd();
            }
        }
    
        private void ProcessTouchInput()
        {
            // 获取第一个触摸点
            Touch touch = Input.GetTouch(0);
            // 根据触摸阶段处理不同的触摸状态
            switch (touch.phase)
            {
                case TouchPhase.Began:          // 触摸开始
                    HandleTouchStart(touch.position);
                    break;
                case TouchPhase.Moved:          // 触摸移动
                case TouchPhase.Stationary:     // 触摸静止
                    HandleTouch(touch.position);
                    break;
                case TouchPhase.Ended:          // 触摸结束
                case TouchPhase.Canceled:       // 触摸取消
                    HandleTouchEnd();
                    break;
            }
        }
    
        private void HandleTouchStart(Vector3 screenPosition)
        {
            // 从屏幕位置创建射线
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            // 使用球形射线检测，比普通射线检测有更大的容错范围
            if (Physics.SphereCast(ray, sphereCastRadius, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                // 尝试获取击中物体上的Item组件
                Item item = hit.transform.GetComponent<Item>();
                if (item != null)
                {
                    // 通知MovementManager开始移动该物品
                    MovementManager.Instance.StartMovement(item, ray);
                }
            }
        }
    
        private void HandleTouch(Vector3 screenPosition)
        {
            // 从当前触摸位置创建射线
            Ray currentRay = mainCamera.ScreenPointToRay(screenPosition);
            // 更新物品移动
            MovementManager.Instance.UpdateMovement(currentRay);
        }
    
        private void HandleTouchEnd()
        {
            // 通知MovementManager结束移动
            MovementManager.Instance.EndMovement();
            OnTouchEnd?.Invoke();
        }
    }
}