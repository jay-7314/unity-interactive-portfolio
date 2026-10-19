public interface IBallManager
{
    void ReturnBall(int ballId, int factoryIndex, Ball ball);
    void StartWarning();
    bool IsFailed { get; }
}