using System.Threading.Tasks;
using Features.WheelSpinGame.Network.DTO;

namespace Game.Features.Wheel.Network
{
    public interface IWheelApi : IApi
    {
        Task<GameContext> LoadUserContext();
        Task<WheelSpinProgressState> GetProgress();
        Task<WheelSpinResponse> Spin();
        Task<bool> Cashout();
        Task<bool> Continue();
        Task GiveUp();
        void AddGold(int amount);
    }
}