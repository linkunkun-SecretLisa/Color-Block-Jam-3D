using Runtime.Data.UnityObject;
using UnityEngine;
using Runtime.Enums;
using Runtime.Helpers;
using Runtime.Managers;
using Sirenix.OdinInspector;

namespace Runtime.Entities
{
    public class Cell : MonoBehaviour
    {
        [ShowInInspector]  public Vector2Int GridPosition { get; private set; }
        [ShowInInspector]  public GameColor CellColor { get; private set; }
        [ShowInInspector] public bool IsOccupied { get; private set; }

        private Renderer _renderer;
        public CD_GameColor colorData;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        public void Init(Vector2Int position, GameColor color, bool isOccupied)
        {
            GridPosition = position;
            CellColor = color;
            IsOccupied = isOccupied;
            ApplyColor();
        }

        public void SetOccupied(bool occupied)
        {
            IsOccupied = occupied;
        }

        public void SetColor(GameColor newColor)
        {
            CellColor = newColor;
            ApplyColor();
        }

        private void ApplyColor()
        {
            if (_renderer != null)
            {
                _renderer.material.color = CellColor != GameColor.None
                    ? colorData.gameColorsData[(int)CellColor].color
                    : Color.white;
            }
        }
    }
}