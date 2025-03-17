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
        // 跟踪每个物品有多少子物体在触发器内
        [ShowInInspector] private Dictionary<Item, int> itemChildsCount = new Dictionary<Item, int>();
        
        // 完全在触发器内的物品列表
        [SerializeField] private List<Item> itemsInTrigger = new List<Item>();
        
        // 触发器对应的颜色（只接受此颜色的物品）
        [SerializeField] private GameColor triggerColor;
        
        // 触发器接受的最大物品尺寸
        [SerializeField] private ItemSize itemSize;
        
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
            InputManager.OnTouch += CheckTriggerContinuously;
        }

        /// <summary>
        /// 当物体进入触发器时调用
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            // 检查是否为Item
            if (other.CompareTag(ConstantsUtilities.ItemTag))
            {
                Debug.LogError( "OnTriggerEnter 检查是否为Item");

                var item = other.GetComponent<Item>();
                // 如果是有效物品且尚未在列表中，添加并检查触发条件
                if (item != null && !itemsInTrigger.Contains(item))
                {
                    itemsInTrigger.Add(item);
                    CheckTrigger();
                }
            }
            // 检查是否为物品的子部件
            else if (other.CompareTag(ConstantsUtilities.ItemChildTag))
            {
                Debug.LogError( "OnTriggerEnter 检查是否为物品的子部件"); //when to execute this code?
                AddItemsChild(other);
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
                    itemsInTrigger.Remove(item);
                    CheckTrigger();
                }
            }
            // 检查是否为物品的子部件
            else if (other.CompareTag(ConstantsUtilities.ItemChildTag))
            {
                RemoveItemsChild(other);
            }
        }

        /// <summary>
        /// 持续检查触发器状态（由输入事件触发）
        /// </summary>
        private void CheckTriggerContinuously()
        {
            // 检查每个跟踪的物品是否所有子部件都在触发器内
            foreach (var kvp in itemChildsCount)
            {
                if (kvp.Value == kvp.Key.GetChildCount())
                {
                    CheckTrigger();
                }
            }
        }

        /// <summary>
        /// 添加物品子部件到计数器
        /// </summary>
        private void AddItemsChild(Collider childCollider)
        {
            // 获取子部件的父物品
            var parentItem = childCollider.GetComponentInParent<Item>();
            if (parentItem == null)
                return;

            // 更新子部件计数
            if (itemChildsCount.ContainsKey(parentItem))
            {
                itemChildsCount[parentItem]++;
            }
            else
            {
                itemChildsCount[parentItem] = 1;
            }

            // 如果所有子部件都在触发器内，检查触发条件
            if (itemChildsCount[parentItem] == parentItem.GetChildCount())
            {
                CheckTrigger();
            }
        }

        /// <summary>
        /// 从计数器中移除物品子部件
        /// </summary>
        private void RemoveItemsChild(Collider childCollider)
        {
            // 获取子部件的父物品
            var parentItem = childCollider.GetComponentInParent<Item>();
            if (parentItem == null)
                return;

            // 更新子部件计数
            if (itemChildsCount.ContainsKey(parentItem))
            {
                itemChildsCount[parentItem]--;
                
                if (itemChildsCount[parentItem] == parentItem.GetChildCount())
                {
                    CheckTrigger();//todo:lkk this code seems never be executed ? #1
                }

                // 如果没有子部件在触发器内，移除物品计数
                if (itemChildsCount[parentItem] <= 0)
                {
                    itemChildsCount.Remove(parentItem);
                }
            }
        }

        /// <summary>
        /// 检查触发器条件并处理符合条件的物品
        /// </summary>
        private void CheckTrigger()
        {
            itemsToRemoveCache.Clear();
            
            // 遍历所有在触发器内的物品
            foreach (var item in itemsInTrigger)
            {
                // 检查物品颜色和尺寸是否符合要求
                if (item.itemColor == triggerColor && IsItemFitToBlock(item))
                {
                    // 获取该物品在触发器内的子部件数量
                    int childCount = itemChildsCount.ContainsKey(item) ? itemChildsCount[item] : 0;
                    
                    // 如果所有子部件都在触发器内且通过射线检测，添加到待移除列表
                    if (childCount == item.GetChildCount() && item.CheckChildrenInfiniteRaycast(transform.position))
                    {
                        itemsToRemoveCache.Add(item);
                    }
                }
            }

            // 对符合条件的物品执行销毁动画
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
            itemChildsCount.Remove(blockItem);
            
            // 如果物品正被选中，从网格管理器中移除
            if (MovementManager.Instance.GetSelectedItem() == blockItem)
                GridManager.Instance.RemoveItem(blockItem);
                
            // 禁用物品碰撞体，防止进一步交互
            blockItem.DisableColliders();

            // 动画序列：触发器下沉 -> 物品移动到销毁位置 -> 物品回到触发器 -> 销毁物品 -> 触发器回到原位
            transform.DOLocalMoveY(originalPosition.y - triggerSinkDepth, triggerAnimationDuration)
                .SetEase(Ease.InExpo)
                .OnComplete(() =>
                {
                    // 确保物品不再被选中
                    MovementManager.Instance.SetSelectedItem(null);
                    
                    // 物品移动到销毁位置
                    blockItem.transform.DOMove(blockDestroyingPosition.position, itemMoveToDestroyDuration)
                        .SetEase(Ease.Linear)
                        .OnComplete(() =>
                        {
                            // 物品移回触发器位置
                            blockItem.transform.DOLocalMove(transform.position, itemReturnDuration)
                                .SetEase(Ease.Linear)
                                .OnComplete(() =>
                                {
                                    // 销毁物品并恢复触发器位置
                                    Destroy(blockItem.gameObject);
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
        private void OnDisable() => InputManager.OnTouch -= CheckTriggerContinuously;
    }
}