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
        public ItemSize ItemSize;
        public Vector2Int position;
        public Vector2Int ItemPos;  //如果当前格子存在Item方块，则这里记录方块的位置（方块所占的所有格子共用这个位置） 这个字段要结合ItemSize来使用
        public override string ToString()
        {
            return $"GridData(Position: {position}, Occupied: {isOccupied}, " +
                   $"Color: {gameColor}, Size: {ItemSize}, ItemPos: {ItemPos})";
        }
    }


    [System.Serializable]
    public struct TriggerData
    {
        public GameColor gameColor;
        public TriggerType triggerType;
        //这个位置是临近的格子坐标，并非实际位置，还需要换算（位置和方向）
        public Vector2Int position;
    }
}