using Runtime.Enums;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Data.ValueObject
{
    [System.Serializable]
    public struct GridData
    {
        public bool isOccupied;
        public GameColor gameColor;
        public Vector2Int position;
    }
}