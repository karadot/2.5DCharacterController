using UnityEngine;

    public abstract class StateBase : IState
    {
        protected bool isActive { get; private set; }
        public abstract bool canTransitionItSelf { get; set; }

        protected Transform transform;

        public StateBase(Transform t)
        {
            transform = t;
        }

        public abstract void Tick();

        public virtual void OnEnter()
        {
            isActive = true;
        }

        public virtual void OnExit()
        {
            isActive = false;
        }
    }
