using DG.Tweening;
using Runtime.Entities;
using Runtime.Enums;
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
            if (other.gameObject.layer == ConstantsUtilities.ItemLayer)
            {
                var itemController = other.GetComponent<Item>();
                if (itemController != null)
                {
                    if (itemController.itemColor == triggerColor && IsItemFitToBlock(itemController))
                    {
                        BlockDestroyingAnimation(other.gameObject);
                    }
                }
            }
        }
        
        
        
        
        private void BlockDestroyingAnimation(GameObject blockObject)
        {
            transform.DOLocalMove(new Vector3(0, -1, 0), 1f).SetRelative().SetEase(Ease.InOutBounce).OnComplete (() =>
            {
                Destroy(gameObject);
                BackToTheOriginalPosition();
            });
        }
        
        private void BackToTheOriginalPosition()
        {
            transform.DOLocalMove(new Vector3(0, 1, 0), 1f).SetRelative().SetEase(Ease.InOutBounce);
        }
        
        private bool IsItemFitToBlock(Item item)
        {
            //check if the item size is equal or bigger to the block size
            return true;
        }
    }
}