using UnityEngine;

namespace Runtime.Utilities
{
    public static class ConstantsUtilities
    {
        #region Tags

        public static readonly string ItemTag= "Item";

        #endregion

        #region Layers

        public static readonly int ItemLayer = LayerMask.GetMask("Item");
        public static readonly int TriggerBlock = LayerMask.GetMask("TriggerBlock");

        #endregion

        #region Animator Hashes

        #endregion
    }
}