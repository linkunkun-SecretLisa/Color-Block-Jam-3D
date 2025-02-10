using System;
using DG.Tweening;
using Runtime.Entities;
using Runtime.Enums;
using Runtime.Managers;
using Runtime.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Controllers
{
    public class BlockTriggerController : MonoBehaviour
    {
        [SerializeField] private GameColor triggerColor;
        [SerializeField] private ItemSize itemSize;
        [SerializeField] private List<Item> itemsInTrigger = new List<Item>();
        [SerializeField] private Transform blockDestroyingPosition;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(ConstantsUtilities.ItemTag))
            {
                var itemController = other.GetComponent<Item>();
                if (itemController != null && !itemsInTrigger.Contains(itemController))
                {
                    itemsInTrigger.Add(itemController);
                    CheckTrigger();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(ConstantsUtilities.ItemTag))
            {
                var itemController = other.GetComponent<Item>();
                if (itemController != null && itemsInTrigger.Contains(itemController) &&
                    itemController.isActiveAndEnabled)
                {
                    itemsInTrigger.Remove(itemController);
                    CheckTrigger();
                }
            }
        }

        private void CheckTrigger()
        {
            List<Item> itemsToRemove = new List<Item>();

            foreach (var itemController in itemsInTrigger)
            {
                if (itemController.itemColor == triggerColor && IsItemFitToBlock(itemController))
                {
                    if (itemController.CheckChildrenInfiniteRaycast(transform.position))
                    {
                        itemsToRemove.Add(itemController);
                    }
                }
            }

            foreach (var item in itemsToRemove)
            {
                BlockDestroyingAnimation(item);
            }
        }

        private void BlockDestroyingAnimation(Item blockObject)
        {
            itemsInTrigger.Remove(blockObject);
            if (MovementManager.Instance.GetSelectedItem() == blockObject)
             GridManager.Instance.RemoveItem(blockObject);
            blockObject.DisableColliders();

            transform.DOLocalMove(new Vector3(0, -0.25f, 0), 0.5f).SetRelative().SetEase(Ease.InExpo).OnComplete(() =>
            {
                blockObject.gameObject.transform.DOMove(blockDestroyingPosition.position, 0.1f).SetEase(Ease.Linear).OnComplete(() =>
                    {
                        MovementManager.Instance.SetSelectedItem(null);
                        blockObject.gameObject.transform.DOLocalMove(transform.position, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
                            {
                                Destroy(blockObject.gameObject);
                                BackToTheOriginalPosition();
                            });
                    });
            });
        }

        private void BackToTheOriginalPosition()
        {
            transform.DOLocalMove(new Vector3(0, 0.25f, 0), 0.5f).SetRelative().SetEase(Ease.InExpo);
        }

        private bool IsItemFitToBlock(Item item)
        {
            return (int)item.itemSize <= (int)itemSize;
        }
    }
}