using System;
using Runtime.Data.UnityObject;
using Runtime.Data.ValueObject;
using Runtime.Enums;
using Runtime.Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Entities
{
    public class Item : MonoBehaviour
    {
        public Vector2Int GridPosition;
        [FormerlySerializedAs("ItemColor")] public GameColor itemColor;
        public CD_ItemParameters itemParametersData;
        public Renderer Renderer;
        public CD_GameColor colorData;
        

        public void Init(Vector2Int gridPosition, GameColor gameColor ,GridManager gridManager)
        {
            GridPosition = gridPosition;
            itemColor = gameColor;
                
            gridManager.AddItem(this);
            gridManager.SetDirty();
            ApplyColor();
        }

        private void ApplyColor()
        {
          Renderer.sharedMaterial = colorData.gameColorsData[(int)itemColor].materialColor;
        }

       
    }
}