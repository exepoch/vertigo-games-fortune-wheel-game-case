using System.Collections.Generic;

namespace Features.WheelSpinGame.Core.Models
{
    // Builds a wheel view configuration based on current progression state
    // and runtime config, applying spin mode rules and slot composition logic.
    public class WheelViewSetupDto
    {
        public float SpinDuration;
        public int progressLevel;
        public SpinMod SpinMod;
        public List<WheelSlot> Slots;
        public List<WheelSlot> Pendings;
    }
}