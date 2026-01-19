using System.Collections.Generic;
using Features.WheelSpinGame.Core.Models;

namespace Features.WheelSpinGame.Network.DTO
{
    public struct WheelSpinProgressState
    {
        public int ProgressLevel;
        public int spinModIndex;
        public List<WheelSlot> Pendings;
    }
}
