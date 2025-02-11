using System;
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
    public class BlockTriggerController : MonoBehaviour
    {
        [ShowInInspector] private Dictionary<Item, int> itemChildsCount = new Dictionary<Item, int>();
        [SerializeField] private List<Item> itemsInTrigger = new List<Item>();
        [SerializeField] private GameColor triggerColor;
        [SerializeField] private ItemSize itemSize;
        [SerializeField] private Transform blockDestroyingPosition;
        [SerializeField] private BoxCollider triggerCollider;
        private Vector3 originalPosition;


        private void Start()
        {
            originalPosition = transform.localPosition;
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            InputManager.OnTouch += CheckTriggerContuniously;
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(ConstantsUtilities.ItemTag))
            {
                var item = other.GetComponent<Item>();
                if (item != null && !itemsInTrigger.Contains(item))
                {
                    itemsInTrigger.Add(item);
                    CheckTrigger();
                }
            }
            else if (other.CompareTag("ItemChild"))
            {
                AddItemsChild(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(ConstantsUtilities.ItemTag))
            {
                var item = other.GetComponent<Item>();
                if (item != null && itemsInTrigger.Contains(item) && item.isActiveAndEnabled)
                {
                    itemsInTrigger.Remove(item);
                    CheckTrigger();
                }
            }
            else if (other.CompareTag("ItemChild"))
            {
                RemoveItemsChild(other);
            }
        }

        private void CheckTriggerContuniously()
        {
            foreach (var kvp in itemChildsCount)
            {
                if (kvp.Value == kvp.Key.GetChildCount())
                {
                    CheckTrigger();
                }
            }
        }

        private void AddItemsChild(Collider childCollider)
        {
            var parentItem = childCollider.GetComponentInParent<Item>();
            if (parentItem == null)
                return;

            if (itemChildsCount.ContainsKey(parentItem))
            {
                itemChildsCount[parentItem]++;
            }
            else
            {
                itemChildsCount[parentItem] = 1;
            }

            if (itemChildsCount[parentItem] == parentItem.GetChildCount())
            {
                CheckTrigger();
            }
        }

        private void RemoveItemsChild(Collider childCollider)
        {
            var parentItem = childCollider.GetComponentInParent<Item>();
            if (parentItem == null)
                return;

            if (itemChildsCount.ContainsKey(parentItem))
            {
                itemChildsCount[parentItem]--;
                if (itemChildsCount[parentItem] == parentItem.GetChildCount())
                {
                    CheckTrigger();
                }

                if (itemChildsCount[parentItem] <= 0)
                {
                    itemChildsCount.Remove(parentItem);
                }
            }
        }

        private void CheckTrigger()
        {
            var itemsToRemove = new List<Item>();
            foreach (var item in itemsInTrigger)
            {
                if (item.itemColor == triggerColor && IsItemFitToBlock(item))
                {
                    int childCount = itemChildsCount.ContainsKey(item) ? itemChildsCount[item] : 0;
                    if (childCount == item.GetChildCount() && item.CheckChildrenInfiniteRaycast(transform.position))
                    {
                        itemsToRemove.Add(item);
                    }
                }
            }

            foreach (var item in itemsToRemove)
            {
                BlockDestroyingAnimation(item);
            }
        }

        private void BlockDestroyingAnimation(Item blockItem)
        {
            itemsInTrigger.Remove(blockItem);
            itemChildsCount.Remove(blockItem);
            if (MovementManager.Instance.GetSelectedItem() == blockItem)
                GridManager.Instance.RemoveItem(blockItem);
            blockItem.DisableColliders();

            transform.DOLocalMoveY(originalPosition.y - 0.25f, 0.5f)
                .SetEase(Ease.InExpo)
                .OnComplete(() =>
                {
                    MovementManager.Instance.SetSelectedItem(null);
                    blockItem.transform.DOMove(blockDestroyingPosition.position, 0.1f)
                        .SetEase(Ease.Linear)
                        .OnComplete(() =>
                        {
                            blockItem.transform.DOLocalMove(transform.position, 0.2f)
                                .SetEase(Ease.Linear)
                                .OnComplete(() =>
                                {
                                    Destroy(blockItem.gameObject);
                                    BackToTheOriginalPosition();
                                });
                        });
                });
        }

        private void BackToTheOriginalPosition() => transform.DOLocalMoveY( originalPosition.y, 0.5f).SetEase(Ease.InExpo);

        private bool IsItemFitToBlock(Item item) => (int)item.itemSize <= (int)itemSize;

        private void OnDisable() => InputManager.OnTouch -= CheckTriggerContuniously;
    }
}