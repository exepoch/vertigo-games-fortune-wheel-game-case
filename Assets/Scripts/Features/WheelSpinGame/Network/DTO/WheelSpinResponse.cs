namespace Features.WheelSpinGame.Network.DTO
{
    public struct WheelSpinResponse
    {
        public int RewardId;
        public int Amount;
        public int CurrentProgressLevel;
        public bool IsBomb;
        public bool IsLastSpin;
        public int NextSpinModIndex;
    }
}