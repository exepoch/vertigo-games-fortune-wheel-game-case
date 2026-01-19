public interface IEconomyDataRetriever<T> where T : class
{
    public T Pop(GameContext context);
}
