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

        public void Init(Vector2Int gridPosition, GameColor gameColor, GridManager gridManager)
        {
            itemColor = gameColor;
            gridManager.AddItem(this);
            ApplyChildColor();
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
            SetChildsGridPosition(gridPos);
        }

        public void ApplyChildColor()
        {
            foreach (var child in childItems)
            {
                child.ApplyColor(colorData.gameColorsData[(int)itemColor].materialColor);
            }
        }
        
        public bool CanChildsMoveInXZ(Vector3 targetPosition, out bool canMoveX, out bool canMoveZ, out float xhitDistance, out float zhitDistance)
        {
            bool allCanMoveX = true;
            bool allCanMoveZ = true;
            xhitDistance = 0.5f;
            zhitDistance = 0.5f;

            foreach (var child in childItems)
            {
                bool childCanMoveX, childCanMoveZ;
                float childX, childZ;
                child.CanMoveInXZ(targetPosition, transform, out childCanMoveX, out childCanMoveZ, out childX, out childZ);
                allCanMoveX &= childCanMoveX;
                allCanMoveZ &= childCanMoveZ;
                if (childX < xhitDistance)
                {
                    xhitDistance = childX;
                }
                if (childZ < zhitDistance)
                {
                    zhitDistance = childZ;
                }
            }

            Debug.Log($"X: {xhitDistance} Z: {zhitDistance}");
            canMoveX = allCanMoveX;
            canMoveZ = allCanMoveZ;
            return canMoveX && canMoveZ;
        }
    }
}