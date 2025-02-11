using UnityEngine;

namespace Runtime.Entities
{
    public class Cell : MonoBehaviour
    {
        public Vector2Int gridPosition;
        public bool isOccupied;


        public void Init(Vector2Int position, bool isOccupied)
        {
            gridPosition = position;
            this.isOccupied = isOccupied;
        }

        public void SetOccupied(bool occupied)
        {
            isOccupied = occupied;
        }
    }
}