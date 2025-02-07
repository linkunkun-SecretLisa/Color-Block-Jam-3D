using Runtime.Data.UnityObject;
using Runtime.Enums;
using Runtime.Managers;
using UnityEngine;

namespace Runtime.Entities
{
    public class Item : MonoBehaviour
    {
        public Vector2Int GridPosition;
        public GameColor itemColor;
        public CD_ItemParameters itemParametersData;
        public Renderer Renderer;
        public CD_GameColor colorData;
        
        public void Init(Vector2Int gridPosition, GameColor gameColor, GridManager gridManager)
        {
            GridPosition = gridPosition;
            itemColor = gameColor;
                
            gridManager.AddItem(this);
            gridManager.SetDirty();
            ApplyColor();
        }

        private void ApplyColor()
        {
            // indexlemeyi enum sırasına göre yapıyoruz
            Renderer.sharedMaterial = colorData.gameColorsData[(int)itemColor].materialColor;
        }
    }
}