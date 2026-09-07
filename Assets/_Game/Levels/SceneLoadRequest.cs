namespace Avoidance.Gameplay.Levels
{
    public enum SceneLoadState { Requested, Loading, AwaitingReady, Ready, Failed }
    public sealed class SceneLoadRequest
    {
        public string SceneName { get; internal set; }
        public string ModuleId { get; internal set; }
        public SceneLoadState State { get; internal set; }
        public float Progress { get; internal set; }
        public bool ActivationComplete { get; internal set; }
        public bool ModuleReady { get; internal set; }
        public string Error { get; internal set; }
        public bool IsTerminal => State == SceneLoadState.Ready || State == SceneLoadState.Failed;
        public override string ToString() => $"scene={SceneName} module={ModuleId} state={State} progress={Progress:0.00} activation={ActivationComplete} moduleReady={ModuleReady} error={Error}";
    }
}
