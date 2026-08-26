
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.FanCam
{
    public class AttachSmartObjectSyncToPlayer : UdonSharpBehaviour
    {
        public SmartObjectSync sync;
        public Vector3 forwardVector = Vector3.forward;
        public float holdDuration = 0.5f;
        public override void OnPickupUseDown()
        {
            Debug.LogWarning("ASDF");
            startTime = Time.timeSinceLevelLoad;
            SendCustomEventDelayedSeconds(nameof(CheckHold), holdDuration);
        }
        float startTime = -1001f;

        public override void OnPickupUseUp()
        {
            startTime = -1001f;
        }

        public override void OnPickup()
        {
            TargetPlayerId = -1001;
        }

        public void CheckHold()
        {
            Debug.LogWarning("Check Hold");
            if (Time.timeSinceLevelLoad - startTime < holdDuration - Time.deltaTime)
            {
                return;
            }
            FindBestTarget();
        }

        public void Reset()
        {
            sync = GetComponent<SmartObjectSync>();
        }

        [UdonSynced]
        [FieldChangeCallback(nameof(TargetPlayerId))]
        [System.NonSerialized]
        int _targetPlayerId = -1001;
        public int TargetPlayerId
        {
            get => _targetPlayerId;
            set
            {
                _targetPlayerId = value;
                if (Networking.LocalPlayer.IsOwner(gameObject))
                {
                    RequestSerialization();
                }

                if (value == Networking.LocalPlayer.playerId)
                {
                    Attach();
                }
            }
        }

        public void Attach()
        {
            sync.state = SmartObjectSync.STATE_ATTACHED_TO_PLAYSPACE;
        }

        public void FindBestTarget()
        {
            Debug.LogWarning("Find best target");
            if (!Networking.LocalPlayer.IsOwner(gameObject))
            {
                return;
            }

            var players = VRCPlayerApi.GetPlayers();
            int bestPlayerId = -1001;
            float bestPlayerScore = 0f;
            var pos = transform.position;
            var vector = transform.rotation * forwardVector;
            var newHead = Vector3.zero;
            var newVector = Vector3.zero;
            foreach (var player in players)
            {
                Debug.LogWarning("checking player " + player.playerId);
                newVector = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position - pos;
                float dot = Vector3.Dot(newVector.normalized, vector.normalized) + 1;
                Debug.LogWarning("dot " + dot);
                if (newVector.magnitude == 0)
                {
                    Debug.LogWarning("zero magnitude vector");
                    continue;
                }
                float newScore = dot / newVector.magnitude;
                if (newScore > bestPlayerScore)
                {
                    bestPlayerId = player.playerId;
                }
            }
            TargetPlayerId = bestPlayerId;
        }
    }
}
