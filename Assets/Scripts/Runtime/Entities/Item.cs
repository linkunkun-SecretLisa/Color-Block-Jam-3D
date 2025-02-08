using Runtime.Data.UnityObject;
using Runtime.Enums;
using Runtime.Managers;
using UnityEngine;

namespace Runtime.Entities
{
    public class Item : MonoBehaviour
    {
        public Vector2Int GridPosition;
        public ItemChild[] childItems;
        public GameColor itemColor;
        // public ItemSize itemSize;
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

        public void OnSelected()
        {
            Renderer.material.SetFloat("_OutlineWidth", 5f);
        }

        public void OnDeselected(Vector2Int gridPos)
        {
            GridPosition = gridPos;
            Renderer.material.SetFloat("_OutlineWidth", 0.0f);
        }

        private void ApplyColor()
        {
            Renderer.sharedMaterial = colorData.gameColorsData[(int)itemColor].materialColor;
        }
        
        public bool CanMoveInXZ(Vector3 transformPosition, out bool canMoveX, out bool canMoveZ)
        {
            canMoveX = true;
            canMoveZ = true;
            foreach (var item in childItems)
            {
                if (!item.CanMoveInXZ(transformPosition, out bool isCanMoveX, out bool isCanMoveZ))
                {
                    canMoveX = isCanMoveX;
                    canMoveZ = isCanMoveZ;
                    return false;
                }
            }

            return true;
        }
    }
}