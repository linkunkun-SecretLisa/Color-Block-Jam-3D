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

        public bool CanMoveInXZ(Vector3 targetPosition, Transform directionOrigin, out bool isCanMoveX, out bool isCanMoveZ, out float xhitDistance, out float zhitDistance)
        {
            Vector3 originPos = directionOrigin.position;
            Vector3 delta = targetPosition - originPos;
            isCanMoveX = true;
            isCanMoveZ = true;
            xhitDistance = raycastDistance;
            zhitDistance = raycastDistance;

            if (Mathf.Abs(delta.x) > 0.1f)
            {
                Vector3 directionX = new Vector3(Mathf.Sign(delta.x), 0, 0);
                float minHit = GetMinHitDistance(directionX);
                isCanMoveX = minHit >= raycastDistance;
                xhitDistance = minHit;
            }

            if (Mathf.Abs(delta.z) > 0.1f)
            {
                Vector3 directionZ = new Vector3(0, 0, Mathf.Sign(delta.z));
                float minHit = GetMinHitDistance(directionZ);
                isCanMoveZ = minHit >= raycastDistance;
                zhitDistance = minHit;
            }

            return isCanMoveX && isCanMoveZ;
        }

        private float GetMinHitDistance(Vector3 direction)
        {
            float minDistance = raycastDistance;
            Vector3[] raycastOrigins = new Vector3[3];

            if (Mathf.Abs(direction.x) > 0)
            {
                raycastOrigins[0] = transform.position; // Center
                raycastOrigins[1] = transform.position + new Vector3(0, 0, 0.48f); // Top
                raycastOrigins[2] = transform.position - new Vector3(0, 0, 0.48f); // Bottom
            }
            else if (Mathf.Abs(direction.z) > 0)
            {
                raycastOrigins[0] = transform.position; // Center
                raycastOrigins[1] = transform.position + new Vector3(0.48f, 0, 0); // Right
                raycastOrigins[2] = transform.position - new Vector3(0.48f, 0, 0); // Left
            }

            foreach (var origin in raycastOrigins)
            {
                Debug.DrawRay(origin, direction * 10f, Color.red);
                if (Physics.Raycast(origin, direction, out RaycastHit hit, Mathf.Infinity, obstacleLayerMask))
                {
                   
                    if (hit.distance < raycastDistance)
                    {
                        minDistance = Mathf.Min(minDistance, hit.distance);
                    }
                }
            }
            return minDistance;
        }
    }
}