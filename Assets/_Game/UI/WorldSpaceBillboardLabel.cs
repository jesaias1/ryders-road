using UnityEngine;

namespace Avoidance.UI
{
    [DisallowMultipleComponent]
    public sealed class WorldSpaceBillboardLabel : MonoBehaviour
    {
        private void LateUpdate()
        {
            var camera = UnityEngine.Camera.main;
            if (camera == null)
            {
                return;
            }

            var awayFromCamera = transform.position - camera.transform.position;
            awayFromCamera.y = 0f;
            if (awayFromCamera.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(awayFromCamera.normalized, Vector3.up);
        }
    }
}
