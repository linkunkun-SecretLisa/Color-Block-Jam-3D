using Runtime.Data.ValueObject;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Enums
{
    public enum ItemSize
    {
        None, // Added to represent no selection.
        
        [LabelText("1x1")]
        OneByOne,

        [LabelText("2x2")]
        TwoByTwo,

        [LabelText("3x2")]
        ThreeByTwo,
    }
}