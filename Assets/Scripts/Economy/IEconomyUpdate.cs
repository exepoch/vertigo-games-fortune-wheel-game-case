using Game.Core.Events;

public interface IEconomyUpdate
{
    public void Commit(GameContext context,IEventBus eventBus);
}
