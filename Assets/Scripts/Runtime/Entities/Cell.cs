using UnityEngine;
using Sirenix.OdinInspector;

namespace Runtime.Entities
{
    public class Cell : MonoBehaviour
    {
        [ShowInInspector] public Vector2Int GridPosition { get; private set; }
        [ShowInInspector] public bool IsOccupied { get; private set; }


        public void Init(Vector2Int position, bool isOccupied)
        {
            GridPosition = position;
            IsOccupied = isOccupied;
        }

        public void SetOccupied(bool occupied)
        {
            IsOccupied = occupied;
        }
    }
}