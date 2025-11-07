using UnityEngine;

namespace MegaBonkPlusMod.GameLogic.Common
{
    public static class TeleportService
    {
        private static GameObject _targetPlayer = null;
        private static GameObject _targetLocation = null;
        private static bool _isTeleporting = false;

        private const float SAFE_RANGE = 10.0f;
        private const float Y_OFFSET = 8.0f;
        
        public static void StartTeleport(GameObject player, GameObject target)
        {
            _targetPlayer = player;
            _targetLocation = target;
            _isTeleporting = true;
        }
        
        public static void Update()
        {
            if (!_isTeleporting) return;

            if (_targetPlayer == null || _targetLocation == null)
            {
                _isTeleporting = false;
                return;
            }

            float distance = Vector3.Distance(
                _targetPlayer.transform.position, 
                _targetLocation.transform.position
            );

            if (distance <= SAFE_RANGE)
            {
                _isTeleporting = false;
                _targetPlayer = null;
                _targetLocation = null;
            }
            else
            {
                _targetPlayer.transform.position = _targetLocation.transform.position + new Vector3(0, Y_OFFSET, 0);
            }
        }
    }
}