using Runtime.Data.UnityObject;
using Runtime.Enums;
using Runtime.Managers;
using UnityEngine;

namespace Runtime.Entities
{
    public class Item : MonoBehaviour
    {
        public ItemChild[] childItems;
        public GameColor itemColor;
        public Renderer Renderer;
        public CD_GameColor colorData;

        public void Init(Vector2Int gridPosition, GameColor gameColor, GridManager gridManager)
        {
            SetChildsGridPosition(gridPosition);
            itemColor = gameColor;
            gridManager.AddItem(this);
            ApplyColor();

       
        }

        public void SetChildsGridPosition(Vector2Int gridPosition)
        {
            foreach (var child in childItems)
            {
                child.SetGridPosition(gridPosition);
            }
        }

        public void OnSelected()
        {
            Renderer.material.SetFloat("_OutlineWidth", 5f);
        }

        public void OnDeselected(Vector2Int gridPos)
        {
            Renderer.material.SetFloat("_OutlineWidth", 0.0f);
            SetChildsGridPosition(gridPos);
        }

        public void ApplyColor()
        {
            Renderer.sharedMaterial = colorData.gameColorsData[(int)itemColor].materialColor;
        }
        
        public bool CanChildsMoveInXZ(Vector3 targetPosition, out bool canMoveX, out bool canMoveZ)
        {
            bool allCanMoveX = true;
            bool allCanMoveZ = true;

            foreach (var child in childItems)
            {
                bool childCanMoveX, childCanMoveZ;
                child.CanMoveInXZ(targetPosition,transform, out childCanMoveX, out childCanMoveZ);
                allCanMoveX &= childCanMoveX;
                allCanMoveZ &= childCanMoveZ;
            }

            canMoveX = allCanMoveX;
            canMoveZ = allCanMoveZ;
            return canMoveX && canMoveZ;
        }
    }
}