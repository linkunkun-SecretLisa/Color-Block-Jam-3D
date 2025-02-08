using Runtime.Enums;
using UnityEngine;

namespace Runtime.Data.ValueObject
{
    [System.Serializable]
    public struct GamePrefabData
    {
        public ItemSize itemSize;
        public MonoBehaviour prefab;
    }
}
