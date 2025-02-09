using UnityEngine;
using Sirenix.OdinInspector;

namespace Runtime.Entities
{
    public class Cell : MonoBehaviour
    {
        public Vector2Int GridPosition;
        public bool IsOccupied;


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