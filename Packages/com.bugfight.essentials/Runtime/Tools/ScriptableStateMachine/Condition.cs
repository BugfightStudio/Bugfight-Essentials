using UnityEngine;

namespace Bugfight.Tools.ScriptableStateMachine
{
    public abstract class Condition<TContext> : ScriptableObject
    {
        protected abstract bool IsConditionMet(TContext context);
        public bool Evaluate(TContext context) => IsConditionMet(context);
    }
}