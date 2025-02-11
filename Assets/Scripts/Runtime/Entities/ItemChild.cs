using Runtime.Utilities;
using UnityEngine;

namespace Runtime.Entities
{
    public class ItemChild : MonoBehaviour
    {
        [SerializeField] private float raycastDistance = 0.5f;
        [SerializeField] private LayerMask obstacleLayerMask;
        [SerializeField] private new Renderer renderer;

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
            Vector3 rightOffset = new Vector3(0, 0, 1) * 0.45f;
            Vector3 upOffset = new Vector3(1, 0, 0) * 0.45f;

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

        public bool IsPathClearToPosition(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0;

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            {
                direction = new Vector3(Mathf.Sign(direction.x), 0, 0);
            }
            else
            {
                direction = new Vector3(0, 0, Mathf.Sign(direction.z));
            }

            int layerMask = ConstantsUtilities.TriggerBlockLayerMask | ConstantsUtilities.ItemLayerMask;

            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                Debug.DrawRay(transform.position, direction * hit.distance, Color.red, 2.0f);

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("TriggerBlock"))
                {
                    return true;
                }

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Item"))
                {
                    if (hit.transform != transform.parent && hit.transform.parent != transform.parent && hit.transform != transform)
                    {
                        return false;
                    }
                }
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
                {
                    return true;
                }

            }

            return true;
        }
    }
}