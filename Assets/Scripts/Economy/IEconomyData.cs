namespace Game.Economy
{
    public interface IEconomyData<out T>
    {
        public T Retrieve();
    }
}