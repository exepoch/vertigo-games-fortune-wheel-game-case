namespace Game.Economy
{
    public interface IEconomyService
    {
        public GameContext GetContext();
        void Commit(IEconomyUpdate update);
    }
}