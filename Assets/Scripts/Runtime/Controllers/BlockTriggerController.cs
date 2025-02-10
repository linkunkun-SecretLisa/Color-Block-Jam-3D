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
        public GameColor triggerColor;
        public ItemSize itemSize;

        [SerializeField] private List<Item> itemsInTrigger = new List<Item>();

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
                if (itemController != null && itemsInTrigger.Contains(itemController) && itemController.isActiveAndEnabled)
                {
                    itemsInTrigger.Remove(itemController);
                    CheckTrigger();
                }
            }
        }

        private void CheckTrigger()
        {
            List<Item> itemsToRemove = new List<Item>(); // To avoid concurrent modification exception

            foreach (var itemController in itemsInTrigger)
            {
                if (itemController.itemColor == triggerColor && IsItemFitToBlock(itemController))
                {
                    if(itemController.CheckChildrenInfiniteRaycast(transform.position))
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
            if (itemsInTrigger.Contains(blockObject)) itemsInTrigger.Remove(blockObject);
            if (MovementManager.Instance.GetSelectedItem() == blockObject) MovementManager.Instance.SetSelectedItem(null);
            GridManager.Instance.RemoveItem(blockObject);

            transform.DOLocalMove(new Vector3(0, -0.25f, 0), 0.25f).SetRelative().SetEase(Ease.InExpo).OnComplete(() =>
            {
                Destroy(blockObject.gameObject);
                BackToTheOriginalPosition();
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