using System.Runtime.Serialization;
using Sirenix.OdinInspector;

namespace Runtime.Enums
{
    public enum ItemSize
    {
        [LabelText("1x1")]
        OneByOne,

        [LabelText("2x2")]
        TwoByTwo,

        [LabelText("3x2")]
        ThreeByThree,
    }
}