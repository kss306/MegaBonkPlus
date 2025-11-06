using System;

namespace MegaBonkPlusMod.GameLogic.Common
{
    public static class MinimapCaptureController
    {
        public static event Action OnMinimapCaptureStart;
        
        public static event Action OnMinimapCaptureEnd;

        public static void TriggerCaptureStart()
        {
            OnMinimapCaptureStart?.Invoke();
        }

        public static void TriggerCaptureEnd()
        {
            OnMinimapCaptureEnd?.Invoke();
        }
    }
}