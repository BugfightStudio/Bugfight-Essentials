namespace Bugfight.Systems.ScriptableHarmony.Internal
{
    public enum Access { Getter, Setter }
    public enum SOType { Variable, List, RuntimeSet, RuntimeSingle }
    public enum RuntimeOptions { ResetOnSceneLoad, InvokeOnSceneLoad, None }
    public enum RuntimeSetLifetime { OnAwakeOnDestroy, OnEnableOnDisable, RemoveOnDestroy, RemoveOnDestroyAndDisable }
}

