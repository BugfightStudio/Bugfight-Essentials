using System;
using System.Collections.Generic;
using Bugfight.Attributes;
using UnityEditor;
using UnityEngine;

namespace Bugfight.Tools.ScriptableStateMachine
{
    public abstract class StateMachine<TContext> : MonoBehaviour
    {
        [Serializable]
        public class StateTransitionPair
        {
            [field: SerializeField] public State<TContext> State { get; internal set; }
            [field: SerializeField] public Transition<TContext>[] Transitions { get; protected set; }
        }

        public event Action<TContext, State<TContext>> OnStateChanged = delegate { };
        
        [field: ShowInInspector] public State<TContext> CurrentState { get; private set; }
        public State<TContext> TransitioningState { get; private set; }

        [SerializeField] State<TContext> initialState;
        [field: SerializeField] public TContext Context { get; private set; }

        [SerializeField] List<StateTransitionPair> stateTransitions;
        [SerializeField] List<StateTransitionPair> extraStateTransitions;

        [Header("Debug State Replacement")] 
        [SerializeField] State<TContext> debugOldState;
        [SerializeField] State<TContext> debugNewState;
        
        Dictionary<State<TContext>, State<TContext>> _prefabInstanceDictionary = new();
        Dictionary<State<TContext>, Transition<TContext>[]> _transitionsDictionary = new();

        public virtual void Initialize(TContext context) 
        {
            Context = context;
            
            _transitionsDictionary.Clear();
            
            foreach (var stateTransitionPair in stateTransitions)
            {
                if (stateTransitionPair.State != null)
                {
                    _transitionsDictionary.Add(stateTransitionPair.State, stateTransitionPair.Transitions);
                }
            }
            
            foreach (var stateTransitionPair in extraStateTransitions)
            {
                if (stateTransitionPair.State != null)
                {
                    _transitionsDictionary.Add(stateTransitionPair.State, stateTransitionPair.Transitions);
                }
            }

            Transition(initialState);
        }

        public void Transition(State<TContext> newState)
        {
            TransitioningState = newState;
            CurrentState?.Exit(Context);
            CurrentState = newState;
            CurrentState.Enter(Context);
            OnStateChanged.Invoke(Context, CurrentState);
            TransitioningState = null;
        }
        
        public void ResetToInitialState() => Transition(initialState);

        public void Update()
        {
            if (CurrentState == null) return;
            
            //check if we should transition before iterating the current state
            if (_transitionsDictionary.TryGetValue(CurrentState, out Transition<TContext>[] transitions))
            {
                foreach (Transition<TContext> transition in transitions)
                {
                    if (transition.ShouldTransition(Context))
                    {
                        Transition(transition.TargetState);
                        return;
                    }
                }
            }

            CurrentState.FrameUpdate(Context);
        }

        public void LateUpdate()
        {
            CurrentState?.LateFrameUpdate(Context);
        }

        public void FixedUpdate() => CurrentState?.PhysicsUpdate(Context);

        void OnCollisionEnter(Collision other) => CurrentState?.CollisionEnter(Context, other);
        void OnCollisionStay(Collision other) => CurrentState?.CollisionStay(Context, other);
        void OnCollisionExit(Collision other) => CurrentState?.CollisionExit(Context, other);

        void OnTriggerEnter(Collider other) => CurrentState?.TriggerEnter(Context, other);
        void OnTriggerStay(Collider other) => CurrentState?.TriggerStay(Context, other);
        void OnTriggerExit(Collider other) => CurrentState?.TriggerExit(Context, other);
        void OnDrawGizmos() => CurrentState?.DrawGizmos(Context);

        #if UNITY_EDITOR
        [MethodButton("Debug Replace States")]
        void MethodButton_ReplaceStates()
        {
            if (debugOldState == null || debugNewState == null)
            {
                Debug.LogWarning("Invalid: Null debug old state or debug new state");
                return;
            }

            Replace(stateTransitions);
            Replace(extraStateTransitions);

            debugOldState = null;
            debugNewState = null;

            EditorUtility.SetDirty(this);
            return;

            void Replace(List<StateTransitionPair> list)
            {
                foreach (StateTransitionPair transitionPair in list)
                {
                    if (transitionPair.State == debugOldState) transitionPair.State = debugNewState;

                    foreach (Transition<TContext> transition in transitionPair.Transitions)
                    {
                        if (transition.TargetState == debugOldState) transition.SetTargetStateEditor(debugNewState);
                    }
                }
            }
        }
        #endif
    }
    
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(StateMachine<>.StateTransitionPair))]
    public class StateTransitionPairPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                SerializedProperty transitionsProperty = property.FindPropertyRelative("<Transitions>k__BackingField");
                height += EditorGUI.GetPropertyHeight(transitionsProperty, true);
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Start property scope to handle context menu and other default behavior
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty stateProperty = property.FindPropertyRelative("<State>k__BackingField");
            SerializedProperty transitionsProperty = property.FindPropertyRelative("<Transitions>k__BackingField");

            // Draw foldout and State property on the same line, with State taking 75% of the space
            Rect labelRect = new Rect(position.x, position.y, position.width * 0.25f, EditorGUIUtility.singleLineHeight);  // 25% for the label
            Rect stateRect = new Rect(position.x + labelRect.width + 5, position.y, position.width * 0.75f - 5, EditorGUIUtility.singleLineHeight);  // 75% for the State

            property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label, true);
            EditorGUI.PropertyField(stateRect, stateProperty, GUIContent.none);

            // Draw transitions if expanded
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                Rect transitionsRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, position.height);
                EditorGUI.PropertyField(transitionsRect, transitionsProperty, new GUIContent("Transitions"), true);
                EditorGUI.indentLevel--;
            }

            // End property scope
            EditorGUI.EndProperty();
        }
    }
    #endif
}