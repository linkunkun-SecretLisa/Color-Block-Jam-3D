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
        [SerializeField] private float raycastDistance = 1.0f;
        [SerializeField] private LayerMask obstacleLayerMask;

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
            Renderer.sharedMaterial = colorData.gameColorsData[(int)itemColor].materialColor;
        }

        public bool CanMove(Vector3 direction)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, raycastDistance, obstacleLayerMask))
            {
                return hit.transform == transform;
            }
            return true;
        }

        public bool CanMoveInDirection(Vector3 direction)
        {
            return CanMove(direction);
        }

        public bool CanMoveInXZ(Vector3 targetPosition, out bool isCanMoveX, out bool isCanMoveZ)
        {
            Vector3 currentPos = transform.position;
            Vector3 delta = targetPosition - currentPos;

            isCanMoveX = true;
            isCanMoveZ = true;

            if (Mathf.Abs(delta.x) > 0.01f)
            {
                Vector3 directionX = new Vector3(Mathf.Sign(delta.x), 0, 0);
                isCanMoveX = CanMoveInDirection(directionX);
            }

            if (Mathf.Abs(delta.z) > 0.01f)
            {
                Vector3 directionZ = new Vector3(0, 0, Mathf.Sign(delta.z));
                isCanMoveZ = CanMoveInDirection(directionZ);
            }

            return isCanMoveX && isCanMoveZ;
        }

        private void Update()
        {
            DrawRaycasts();
        }

        private void DrawRaycasts()
        {
            Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
            foreach (var direction in directions)
            {
                Debug.DrawRay(transform.position, direction * raycastDistance, Color.red);
            }
        }
    }
}