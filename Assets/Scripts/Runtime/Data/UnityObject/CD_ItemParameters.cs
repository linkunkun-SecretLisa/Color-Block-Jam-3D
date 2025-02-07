using Runtime.Data.ValueObject;
using UnityEngine;

namespace Runtime.Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_ItemParameters", menuName = "ScriptableObjects/CD_ItemParameters", order = 0)]
    public class CD_ItemParameters : ScriptableObject
    {
         public ItemParametersData itemParametersData;
    }
}