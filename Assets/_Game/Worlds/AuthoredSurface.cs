using UnityEngine;
namespace Avoidance.Gameplay.Worlds
{
    public enum WorldGeometryKind { Traversable, FatalScenery, NonCollidingScenery }
    // The render mesh may be replaced by Unity static batching; the authored collision source is immutable.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
    public sealed class AuthoredSurface : MonoBehaviour
    {
        [SerializeField] private Mesh _sourceMesh;
        [SerializeField] private bool _restoreOnLanding;
        public bool RestoreOnLanding => _restoreOnLanding;
        public WorldGeometryKind GeometryKind => _restoreOnLanding ? WorldGeometryKind.FatalScenery : WorldGeometryKind.Traversable;
        public void SetRestoreOnLanding(bool value) { _restoreOnLanding = value; }
        public void SetSourceMesh(Mesh mesh) { _sourceMesh = mesh; }
        public string CollisionDetails => $"{name}: source={_sourceMesh?.name} collider={GetComponent<MeshCollider>().sharedMesh?.name}";
        public bool HasMatchingCollision => _sourceMesh != null && GetComponent<MeshCollider>().sharedMesh == _sourceMesh
            && (GetComponent<MeshFilter>().sharedMesh == _sourceMesh || GetComponent<Renderer>().isPartOfStaticBatch)
            && GetComponent<MeshCollider>().enabled && !GetComponent<MeshCollider>().isTrigger
            && !GetComponent<MeshCollider>().convex;
    }
}
