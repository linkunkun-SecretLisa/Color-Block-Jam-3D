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
        public CD_GameColor colorData;
        public ItemSize itemSize;

        public void Init(GameColor gameColor, GridManager gridManager)
        {
            itemColor = gameColor;
            gridManager.AddItem(this);
            ApplyChildColor();
        }

        public void OnSelected()
        {
            foreach (var child in childItems)
            {
                child.OnSelected();
            }
        }

        public void OnDeselected(Vector2Int gridPos)
        {
            foreach (var child in childItems)
            {
                child.OnDeselected();
            }
        }

        public void ApplyChildColor()
        {
            foreach (var child in childItems)
            {
                child.ApplyColor(colorData.gameColorsData[(int)itemColor].materialColor);
            }
        }

        public bool CanChildsMoveInXZ(Vector3 targetPosition, out bool canMoveX, out bool canMoveZ)
        {
            bool allCanMoveX = true;
            bool allCanMoveZ = true;

            foreach (var child in childItems)
            {
                bool childCanMoveX, childCanMoveZ;
                child.CanMoveInXZ(targetPosition, transform, out childCanMoveX, out childCanMoveZ);
                allCanMoveX &= childCanMoveX;
                allCanMoveZ &= childCanMoveZ;
            }

            canMoveX = allCanMoveX;
            canMoveZ = allCanMoveZ;
            return canMoveX && canMoveZ;
        }

        public bool CheckChildrenInfiniteRaycast(Vector3 targetPosition)
        {
            bool canReach = true;
            foreach (var child in childItems)
            {
                canReach &= child.IsPathClearToPosition(targetPosition);
            }

            return canReach;
        }
    }
}