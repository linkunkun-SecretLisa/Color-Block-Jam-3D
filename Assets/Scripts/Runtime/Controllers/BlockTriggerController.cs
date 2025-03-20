using System.Collections.Generic;
using DG.Tweening;
using Runtime.Entities;
using Runtime.Enums;
using Runtime.Managers;
using Runtime.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runtime.Controllers
{
    /// <summary>
    /// 处理特定颜色和尺寸的物品与触发器的交互，包括物品销毁动画
    ///     触发器是朝外的，比如1x2，就是在出口的位置往棋盘内拓展出一个1x2的区域作为触发器
    /// </summary>
    public class BlockTriggerController : MonoBehaviour
    {
        // 完全在触发器内的物品列表
        [SerializeField] private List<Item> itemsInTrigger = new List<Item>();
        
        // 触发器对应的颜色（只接受此颜色的物品）
        [SerializeField] private GameColor triggerColor;
        
        // TriggerColor属性，提供外部访问和修改triggerColor的接口
        public GameColor TriggerColor
        {
            get { return triggerColor; }
            set { triggerColor = value; }
        }
        
        // 触发器接受的最大物品尺寸
        [SerializeField] private ItemSize itemSize;

        // 触发器类型
        [SerializeField] private TriggerType triggerType;
        
        // 物品销毁时的目标位置
        [SerializeField] private Transform blockDestroyingPosition;
        
        // 触发器的碰撞体（未在当前代码中使用）
        [SerializeField] private BoxCollider triggerCollider;
        
        // 触发器的原始位置（用于动画）
        private Vector3 originalPosition;
        
        private List<Item> itemsToRemoveCache = new List<Item>();
        
        [SerializeField] private float triggerSinkDepth = 0.25f;
        [SerializeField] private float triggerAnimationDuration = 0.5f;
        [SerializeField] private float itemMoveToDestroyDuration = 0.1f;
        [SerializeField] private float itemReturnDuration = 0.2f;
        /// <summary>
        /// 初始化并订阅事件
        /// </summary>
        private void Start()
        {
            originalPosition = transform.localPosition;
            SubscribeEvents();
        }

        /// <summary>
        /// 订阅输入事件以持续检查触发器状态
        /// </summary>
        private void SubscribeEvents()
        {
            InputManager.OnTouchEnd += CheckTriggerOnTouchEnd;
        }

        /// <summary>
        /// 当物体进入触发器时调用
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            // 检查是否为Item
            if (other.CompareTag(ConstantsUtilities.ItemTag))
            {
                var item = other.GetComponent<Item>();
                // 如果是有效物品且尚未在列表中，添加并检查触发条件
                if (item != null && !itemsInTrigger.Contains(item))
                {
                    //确保加进来的都是同色item
                    itemsInTrigger.Add(item);
                    Debug.Log("itemsInTrigger add item " + item.name + "  trigger:" + gameObject.name);
                    CheckTrigger();
                }
            }
        }


        /// <summary>
        /// 当物体离开触发器时调用
        /// </summary>
        private void OnTriggerExit(Collider other)
        {
            // 检查是否为主物品
            if (other.CompareTag(ConstantsUtilities.ItemTag))
            {
                var item = other.GetComponent<Item>();
                // 如果是列表中的有效物品，移除并重新检查触发条件
                if (item != null && itemsInTrigger.Contains(item) && item.isActiveAndEnabled)
                {
                    Debug.Log("itemsInTrigger remove item " + item.name + "  trigger:" + gameObject.name);
                    itemsInTrigger.Remove(item);
                    CheckTrigger();
                }
            }
        }

        /// <summary>
        /// 持续检查触发器状态（由输入事件触发）
        /// </summary>
        private void CheckTriggerOnTouchEnd()
        {
            CheckTrigger();
        }
        
        /// <summary>
        /// 根据触发器的旋转确定出口方向
        /// </summary>
        /// <param name="itemTrans">物品的Transform（当前未使用）</param>
        /// <returns>出口方向的单位向量</returns>
        Vector3 DetermineExitDirection(Transform itemTrans)
        {
            var rotation = transform.rotation;
            float angle = rotation.eulerAngles.y;
            // 默认情况：根据最接近的90度角确定方向
            int quadrant = Mathf.RoundToInt(angle / 90f) % 4;
            switch (quadrant)
            {
                case 0: return Vector3.back;
                case 1: return Vector3.left;
                case 2: return Vector3.forward;
                case 3: return Vector3.right;
                default: 
                    Debug.LogError("意外的象限值: " + quadrant + "，角度: " + angle);
                    return Vector3.back; // 额外的安全措施
            }
        }
        
        void CheckTrigger()
        {
            itemsToRemoveCache.Clear();
            foreach (var item in itemsInTrigger)
            {
                if (item == null)
                {
                    Debug.LogError( "itemsInTrigger item is null, trigger: " + gameObject.name );
                    continue;
                }
                var exitDirection = DetermineExitDirection(item.transform);
                if (item.itemColor == triggerColor && item.checkChildCanPassThrough(transform, exitDirection))
                {
                    itemsToRemoveCache.Add(item);
                }
            }
            foreach (var item in itemsToRemoveCache)
            {
                BlockDestroyingAnimation(item);
            }
        }

        
        /// <summary>
        /// 执行物品销毁的动画序列
        /// </summary>
        private void BlockDestroyingAnimation(Item blockItem)
        {
            // 从跟踪列表中移除物品
            itemsInTrigger.Remove(blockItem);
            Debug.Log("itemsInTrigger Remove item " + blockItem.name + ", trigger: " + gameObject.name);

            // 如果物品正被选中，从网格管理器中移除
            if (MovementManager.Instance.GetSelectedItem() == blockItem)
            {
                MovementManager.Instance.SetSelectedItem(null);// 解除移动选中
            }
                
            // 禁用物品碰撞体，防止进一步交互
            blockItem.DisableColliders();

            // 动画序列：触发器下沉 -> 物品移动到销毁位置 -> 物品回到触发器 -> 销毁物品 -> 触发器回到原位
            transform.DOLocalMoveY(originalPosition.y - triggerSinkDepth, triggerAnimationDuration)
                .SetEase(Ease.InExpo)
                .OnComplete(() =>
                {
                    // 确保物品不再被选中
                    MovementManager.Instance.SetSelectedItem(null);
                    
                    // 检查物品是否已被销毁
                    if (blockItem == null) 
                    {
                        BackToTheOriginalPosition();
                        return;
                    }
                    
                    // 物品移动,网格对齐
                    var gridPos = GridManager.Instance.WorldSpaceToGridSpace(blockItem.transform.position);
                    var alignPos = GridManager.Instance.GridSpaceToWorldSpace(gridPos);
                    blockItem.transform.DOMove(alignPos, itemMoveToDestroyDuration)
                       .SetEase(Ease.Linear)
                        .OnComplete(() =>
                        {
                            // 检查物品是否已被销毁
                            if (blockItem == null) 
                            {
                                BackToTheOriginalPosition();
                                return;
                            }
                            
                            // 物品往触发器方向移动
                            var exitDirection = DetermineExitDirection(blockItem.transform);
                            var targetPos = blockItem.transform.position + exitDirection * (int)blockItem.itemSize;
                            blockItem.transform.DOLocalMove(targetPos, itemReturnDuration)
                                .SetEase(Ease.Linear)
                                .OnComplete(() =>
                                {
                                    // 在销毁物品前先从网格移除
                                    if (blockItem != null)
                                    {
                                        GridManager.Instance.RemoveItem(blockItem);
                                        Destroy(blockItem.gameObject); //todo: 销毁动画
                                    }

                                    BackToTheOriginalPosition();
                                });
                        });
                });
        }


        /// <summary>
        /// 将触发器动画回到原始位置
        /// </summary>
        private void BackToTheOriginalPosition() => 
            transform.DOLocalMoveY(originalPosition.y, 0.5f).SetEase(Ease.InExpo);

        /// <summary>
        /// 检查物品尺寸是否符合触发器要求
        /// </summary>
        private bool IsItemFitToBlock(Item item) => (int)item.itemSize <= (int)itemSize;
        

        /// <summary>
        /// 取消事件订阅
        /// </summary>
        private void OnDisable() => InputManager.OnTouchEnd -= CheckTriggerOnTouchEnd;
        
        
    }
}