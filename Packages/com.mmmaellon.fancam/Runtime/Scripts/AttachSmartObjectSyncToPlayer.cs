
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
            float bestPlayerScore = float.MaxValue;
            var pos = transform.position;
            var vector = transform.rotation * forwardVector;
            var newVector = Vector3.zero;
            foreach (var player in players)
            {
                newVector = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position - pos;
                float dot = Vector3.Dot(newVector.normalized, vector.normalized) + 1.01f;
                if (dot <= 0)
                {
                    continue;
                }
                float newScore = newVector.magnitude / dot;
                if (newScore < bestPlayerScore)
                {
                    bestPlayerId = player.playerId;
                    bestPlayerScore = newScore;
                }
            }
            TargetPlayerId = bestPlayerId;
        }
    }
}
