using UnityEngine;

namespace Runtime.Entities
{
    public class ItemChild : MonoBehaviour
    {
        [SerializeField] private float raycastDistance = 0.5f;
        [SerializeField] private LayerMask obstacleLayerMask;
        
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
        
        public bool CanMoveInDirection(Vector3 direction)
        {
            Vector3 rightOffset = new Vector3(0, 0, 1) * 0.48f;
            Vector3 upOffset = new Vector3(1, 0, 0) * 0.48f;

            Vector3[] raycastOrigins = new Vector3[3];

            if (Mathf.Abs(direction.x) > 0)
            {
                raycastOrigins[0] = transform.position; // Merkez
                raycastOrigins[1] = transform.position + rightOffset; // Üst
                raycastOrigins[2] = transform.position - rightOffset; // Alt
            }
            else if (Mathf.Abs(direction.z) > 0)
            {
                raycastOrigins[0] = transform.position; // Merkez
                raycastOrigins[1] = transform.position + upOffset; // Sağ
                raycastOrigins[2] = transform.position - upOffset; // Sol
            }

            foreach (var origin in raycastOrigins)
            {
                Debug.DrawRay(origin, direction * raycastDistance, Color.red);
                if (Physics.Raycast(origin, direction, out RaycastHit hit, raycastDistance, obstacleLayerMask))
                {
                    return false;
                }
            }

            return true;
        }

        
    }
}
