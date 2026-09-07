using UnityEngine;

namespace Avoidance.Gameplay.Camera
{
    [DisallowMultipleComponent]
    public sealed class RouteCameraDebugView : MonoBehaviour
    {
        private RouteCameraGraph _graph;
        private bool _visible;

        public void Initialize(RouteCameraGraph graph, bool visible)
        {
            _graph = graph;
            _visible = visible;
        }

        public void SetVisible(bool visible)
        {
            _visible = visible;
        }

        private void OnDrawGizmos()
        {
            if (!_visible || _graph == null || !_graph.IsValid)
            {
                return;
            }

            for (var branchIndex = 0; branchIndex < _graph.BranchCount; branchIndex++)
            {
                var branch = _graph.BranchAt(branchIndex);
                if (branch == null || branch.NodeCount < 2)
                {
                    continue;
                }

                Gizmos.color = branch.Kind == RouteCameraBranchKind.Shortcut
                    ? new Color(1f, 0.75f, 0.1f, 0.85f)
                    : new Color(0.1f, 0.95f, 1f, 0.85f);
                for (var nodeIndex = 0; nodeIndex < branch.NodeCount - 1; nodeIndex++)
                {
                    var a = branch.NodeAt(nodeIndex).Position;
                    var b = branch.NodeAt(nodeIndex + 1).Position;
                    Gizmos.DrawLine(a, b);
                    Gizmos.DrawSphere(a, 0.35f);
                }

                Gizmos.DrawSphere(branch.NodeAt(branch.NodeCount - 1).Position, 0.35f);
            }
        }
    }
}
