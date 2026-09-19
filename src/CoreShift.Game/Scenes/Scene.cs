namespace CoreShift.Game;

public interface Scene
{
    void Enter(GameApp app);
    void Update(GameApp app, float dt);
    void Draw(GameApp app);
}
