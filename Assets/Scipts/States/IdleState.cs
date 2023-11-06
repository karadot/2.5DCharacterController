namespace Scipts.States
{
    public class IdleState: IState
    {
        public bool canTransitionItSelf { get; set; } = false;
        public void Tick()
        {
        }

        public void OnEnter()
        {
        }

        public void OnExit()
        {
        }
    }
}