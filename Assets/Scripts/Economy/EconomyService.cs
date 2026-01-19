using Game.Core.Events;
using Game.Economy;

public class EconomyService : IEconomyService
{
    private readonly GameContext gameContext;
    private readonly IEventBus eventBus;

    public EconomyService(GameContext context,IEventBus bus)
    {
        gameContext = context;
        eventBus = bus;
    }

    public GameContext GetContext() => gameContext;

    public void Commit(IEconomyUpdate update) => update.Commit(gameContext,eventBus);
}