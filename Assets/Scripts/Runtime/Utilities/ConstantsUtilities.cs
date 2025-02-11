using UnityEngine;

namespace Runtime.Utilities
{
    public static class ConstantsUtilities
    {
        #region Tags

        public static readonly string ItemTag= "Item";
        public static readonly string ItemChildTag = "ItemChild";

        #endregion

        #region Layers

        public static readonly int ItemLayer = LayerMask.NameToLayer("Item");
        public static readonly int TriggerBlockLayer = LayerMask.NameToLayer("TriggerBlock");
        public static readonly int ObstacleLayer = LayerMask.NameToLayer("Obstacle");

        #endregion

        #region Layer Masks
        
        public static readonly LayerMask ItemLayerMask = LayerMask.GetMask("Item");
        public static readonly LayerMask TriggerBlockLayerMask = LayerMask.GetMask("TriggerBlock");
        public static readonly LayerMask ObstacleLayerMask = LayerMask.GetMask("Obstacle");

        #endregion

        #region Animator Hashes

        #endregion
    }
}