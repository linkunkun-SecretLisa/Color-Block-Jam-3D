using UnityEngine;

namespace Runtime.Entities
{
    public class ItemChild : MonoBehaviour
    {
        public Vector2Int gridPosition;
        [SerializeField] private float raycastDistance = 0.5f;
        [SerializeField] private LayerMask obstacleLayerMask;
        [SerializeField] private Renderer renderer;

        public void SetGridPosition(Vector2Int gridPosition)
        {
            this.gridPosition = gridPosition;
        }

        public void OnSelected()
        {
            renderer.material.SetFloat("_OutlineWidth", 2.5f);
        }
        
        public void OnDeselected()
        {
            renderer.material.SetFloat("_OutlineWidth", 0.0f);
        }
        
        public void ApplyColor(Material material)
        {
            renderer.sharedMaterial = material;
        }
        
        public bool CanMoveInXZ(Vector3 targetPosition, Transform directionOrigin, out bool isCanMoveX, out bool isCanMoveZ)
        {
            Vector3 originPos = directionOrigin.position;
            Vector3 delta = targetPosition - originPos;

            isCanMoveX = true;
            isCanMoveZ = true;

            if (Mathf.Abs(delta.x) > 0.01f)
            {
                Vector3 directionX = new Vector3(Mathf.Sign(delta.x), 0, 0);
                isCanMoveX = CanMoveInAllDirections(directionX);
            }

            if (Mathf.Abs(delta.z) > 0.01f)
            {
                Vector3 directionZ = new Vector3(0, 0, Mathf.Sign(delta.z));
                isCanMoveZ = CanMoveInAllDirections(directionZ);
            }

            return isCanMoveX && isCanMoveZ;
        }

        public bool CanMoveInAllDirections(Vector3 direction)
        {
            Vector3 rightOffset = new Vector3(0, 0, 1) * 0.48f;
            Vector3 upOffset = new Vector3(1, 0, 0) * 0.48f;

            Vector3[] raycastOrigins = new Vector3[3];

            if (Mathf.Abs(direction.x) > 0)
            {
                raycastOrigins[0] = transform.position; // Center
                raycastOrigins[1] = transform.position + rightOffset; // Top
                raycastOrigins[2] = transform.position - rightOffset; // Bottom
            }
            else if (Mathf.Abs(direction.z) > 0)
            {
                raycastOrigins[0] = transform.position; // Center
                raycastOrigins[1] = transform.position + upOffset; // Right
                raycastOrigins[2] = transform.position - upOffset; // Left
            }

            foreach (var origin in raycastOrigins)
            {
                Debug.DrawRay(origin, direction * raycastDistance, Color.red);
                if (Physics.Raycast(origin, direction, out RaycastHit hit, raycastDistance, obstacleLayerMask))
                {
                    if (hit.transform != transform.parent)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}