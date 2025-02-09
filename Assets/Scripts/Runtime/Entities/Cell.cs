using UnityEngine;

namespace Runtime.Entities
{
    public class Cell : MonoBehaviour
    {
         public Vector2Int gridPosition;
          public bool ısOccupied;


        public void Init(Vector2Int position, bool isOccupied)
        {
            gridPosition = position;
            ısOccupied = isOccupied;
        }

        public void SetOccupied(bool occupied)
        {
            ısOccupied = occupied;
        }
    }
}