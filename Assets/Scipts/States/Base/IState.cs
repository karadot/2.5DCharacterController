public interface IState
{
    bool canTransitionItSelf { get; set; }
    void Tick();
    void OnEnter();
    void OnExit();
}