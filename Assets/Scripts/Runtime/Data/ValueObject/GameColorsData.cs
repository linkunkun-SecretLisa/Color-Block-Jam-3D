using Runtime.Enums;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Data.ValueObject
{
    [System.Serializable]
    public struct GameColorsData
    { 
        [Header("Game Color Data")]
        public GameColor gameColor;
        public Color color;
        public Material materialColor;
    }
}