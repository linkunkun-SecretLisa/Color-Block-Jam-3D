using Runtime.Data.ValueObject;
using Runtime.Enums;
using UnityEngine;

namespace Runtime.Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_GamePrefab", menuName = "ScriptableObjects/CD_GamePrefab", order = 0)]
    public class CD_GamePrefab : ScriptableObject
    {
        public GamePrefabData[] gamePrefab;

        public MonoBehaviour GetPrefab(ItemSize itemSize)
        {
            foreach (var prefab in gamePrefab)
            {
                if (prefab.itemSize == itemSize)
                {
                    return prefab.prefab;
                }
            }

            return null;
        }
    }
}