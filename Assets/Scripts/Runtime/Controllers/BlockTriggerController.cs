using Runtime.Entities;
using Runtime.Enums;
using Runtime.Utilities;
using UnityEngine;

namespace Runtime.Controllers
{
    public class BlockTriggerController : MonoBehaviour
    {
        public GameColor triggerColor;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == ConstantsUtilities.ItemLayer)
            {
                var itemController = other.GetComponent<Item>();
                if (itemController != null)
                {
                    if (itemController.itemColor == triggerColor)
                    {
                        Debug.Log("Correct Color");
                    }
                    else
                    {
                        Debug.Log("Wrong Color");
                    }
                }
            }
        }
    }
}