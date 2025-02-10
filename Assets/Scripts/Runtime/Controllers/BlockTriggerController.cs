using DG.Tweening;
using Runtime.Entities;
using Runtime.Enums;
using Runtime.Managers;
using Runtime.Utilities;
using UnityEngine;

namespace Runtime.Controllers
{
    public class BlockTriggerController : MonoBehaviour
    {
        public GameColor triggerColor;
        public ItemSize itemSize;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(ConstantsUtilities.ItemTag))
            {
                Debug.Log("Item Triggered");
                var itemController = other.GetComponent<Item>();
                if (itemController != null)
                {
                    if (itemController.itemColor == triggerColor && IsItemFitToBlock(itemController))
                    {
                        BlockDestroyingAnimation(itemController);
                    }
                }
            }
        }


        private void BlockDestroyingAnimation(Item blockObject)
        {
         
            
            
            transform.DOLocalMove(new Vector3(0, -0.25f, 0), 0.5f).SetRelative().SetEase(Ease.InExpo).OnComplete(() =>
            {
                GridManager.Instance.RemoveItem(blockObject);
                
            });
            blockObject.gameObject.transform.DOLocalMove(transform.position, 0.25f).SetEase(Ease.Flash).OnComplete(() =>
            {
                if(MovementManager.Instance.GetSelectedItem() == blockObject)
                {
                    MovementManager.Instance.SetSelectedItem(null);
                }
                blockObject.transform.DOScale( Vector3.zero, 0.5f).SetEase(Ease.InExpo).OnComplete(() =>
                {
                    Destroy(blockObject.gameObject);
                    BackToTheOriginalPosition();
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