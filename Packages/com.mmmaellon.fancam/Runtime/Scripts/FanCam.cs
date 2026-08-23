
using Cinemachine;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace MMMaellon.FanCam
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [RequireComponent(typeof(Animator))]
    public class FanCam : UdonSharpBehaviour
    {
        private readonly int RecHash = Animator.StringToHash("rec");
        private readonly int EditHash = Animator.StringToHash("edit");
        private readonly int ZoomHash = Animator.StringToHash("zoom");
        public FanCamManager manager;
        public CinemachineVirtualCamera virtualCam;
        public Animator animator;

        // [UdonSynced]
        [SerializeField]
        [FieldChangeCallback(nameof(Id))]
        int _id = 0;//0 is a null value
        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                if (IsOwnerLocal())
                {
                    RequestSerialization();
                }

                //Non player objects get positive Ids starting from 1
                // manager.AddFanCam(this);
            }
        }

        public bool Rec
        {
            get => manager.ActiveCamIndex == Id;
            set
            {
                if (value)
                {
                    virtualCam.Priority = 1001;
                }
                else
                {
                    virtualCam.Priority = 0;
                }
            }
        }

        public bool RecVisual
        {
            get => animator.GetBool(RecHash);
            set
            {
                animator.SetBool(RecHash, value);
            }
        }

        [UdonSynced]
        [System.NonSerialized]
        bool _edit = false;
        public bool Edit
        {
            get => _edit;
            set
            {
                _edit = value;
                animator.SetBool(EditHash, value && IsOwnerLocal());
                if (IsOwnerLocal())
                {
                    RequestSerialization();
                }
            }
        }

        VRCPlayerApi owner;
        void Start()
        {
            Rec = Rec;
            Edit = Edit;
            owner = Networking.GetOwner(gameObject);
            Id = Id;
        }
        // void OnDestroy()
        // {
        //     manager.RemoveFanCam(this);
        // }
        public VRCPlayerApi Owner
        {
            get => owner;
        }
        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            owner = player;
        }

        public bool IsOwnerLocal()
        {
            return Utilities.IsValid(owner) && owner.isLocal;
        }

        public void EnableCam()
        {
            Networking.SetOwner(Networking.LocalPlayer, manager.gameObject);
            manager.ActiveCamIndex = Id;
        }

        public void ToggleTarget()
        {

        }

        // public void Click()
        // {
        //     if (!CamActive)
        //     {
        //         EnableCam();
        //     }
        //     else
        //     {
        //         ToggleTarget();
        //     }
        // }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        void Reset()
        {
            animator = GetComponent<Animator>();
        }
#endif 

        FanCamPlayerTarget target;
        public FanCamPlayerTarget Target
        {
            get => target;
        }

        [UdonSynced]
        [FieldChangeCallback(nameof(TargetPlayerId))]
        int targetId = -1001;
        public int TargetPlayerId
        {
            get => targetId;
            set
            {
                targetId = value;
                if (IsOwnerLocal())
                {
                    RequestSerialization();
                }
                if (manager.PlayerTargets.TryGetValue(value, TokenType.Reference, out DataToken token))
                {
                    target = (FanCamPlayerTarget)token.Reference;
                }
                else
                {
                    target = null;
                }
            }
        }

        [System.NonSerialized, UdonSynced, FieldChangeCallback(nameof(Zoom))]
        public float _zoom = 0.2f;
        public float Zoom
        {
            get => _zoom;
            set
            {
                _zoom = Mathf.Clamp01(value);
                if (Networking.LocalPlayer.IsOwner(gameObject))
                {
                    RequestSerialization();
                }
                // animator.SetFloat("Zoom", Mathf.Sqrt(_zoom));
                // LerpZoom();
                SendCustomEventDelayedFrames(nameof(LerpZoom), 1);
            }
        }

        public float AnimatorZoom
        {
            get => Mathf.Pow(animator.GetFloat(ZoomHash), 2f);
            set
            {
                animator.SetFloat(ZoomHash, Mathf.Pow(value, 0.5f));
            }
        }

        readonly float maxZoomSpeed = 0.15f;
        readonly float zoomAcceleration = 0.2f;

        float zoomSpeed = 0;

        int lastZoomLerp;
        public void LerpZoom()
        {
            if (lastZoomLerp == Time.renderedFrameCount)
            {
                return;
            }
            // Fixed version with proper handling of negative direction movement
            var startZoom = AnimatorZoom;
            var startZoomSpeed = zoomSpeed;
            var targetDistance = Zoom - startZoom;

            bool stop = false;
            bool coast = false;

            if (targetDistance > 0)
            {
                // Moving in positive direction
                // Calculate stopping distance for positive velocity
                var stoppingDist = ConstAccelerationDistance(zoomSpeed, 0, -zoomAcceleration);

                if (zoomSpeed * Time.deltaTime + stoppingDist < targetDistance)
                {
                    // Need to speed up - we won't reach target even if coasting then braking
                    zoomSpeed = Mathf.Min(maxZoomSpeed, zoomSpeed + zoomAcceleration * Time.deltaTime);
                }
                else if (stoppingDist >= targetDistance)
                {
                    // Need to slow down - would overshoot if we don't brake
                    zoomSpeed = Mathf.Max(0, zoomSpeed - zoomAcceleration * Time.deltaTime);
                    if (zoomSpeed <= 0)
                    {
                        stop = true;
                    }
                }
                else
                {
                    coast = true;
                }
            }
            else if (targetDistance < 0)
            {
                // Moving in negative direction
                // For negative movement, stopping distance is from negative speed to 0
                var stoppingDist = ConstAccelerationDistance(zoomSpeed, 0, zoomAcceleration); // Note: positive acceleration to stop from negative speed

                // stoppingDist will be negative when zoomSpeed is negative (which is what we want)
                if (zoomSpeed * Time.deltaTime + stoppingDist > targetDistance)
                {
                    // Need to speed up in negative direction (make more negative)
                    zoomSpeed = Mathf.Max(-maxZoomSpeed, zoomSpeed - zoomAcceleration * Time.deltaTime);
                }
                else if (stoppingDist <= targetDistance)
                {
                    // Need to slow down (brake from negative speed toward 0)
                    zoomSpeed = Mathf.Min(0, zoomSpeed + zoomAcceleration * Time.deltaTime);
                    if (zoomSpeed >= 0)
                    {
                        stop = true;
                    }
                }
                else
                {
                    coast = true;
                }
            }
            else
            {
                stop = true;
                // // targetDistance == 0, we're at the target
                // if (Mathf.Abs(zoomSpeed) < zoomAcceleration * Time.deltaTime)
                // {
                // }
                // else if (zoomSpeed > 0)
                // {
                //     zoomSpeed = Mathf.Max(0, zoomSpeed - zoomAcceleration * Time.deltaTime);
                // }
                // else
                // {
                //     zoomSpeed = Mathf.Min(0, zoomSpeed + zoomAcceleration * Time.deltaTime);
                // }
            }

            if (stop)
            {
                AnimatorZoom = Zoom;
                zoomSpeed = 0; // Make sure to reset speed when stopping
                return;
            }

            if (coast)
            {
                AnimatorZoom = startZoom + zoomSpeed * Time.deltaTime;
            }
            else
            {
                // Using kinematic equation: d = v₀t + 0.5at²
                // Distance traveled this frame with constant acceleration
                float accel = (zoomSpeed - startZoomSpeed) / Time.deltaTime; // actual acceleration this frame
                var accelerationDistance = startZoomSpeed * Time.deltaTime + 0.5f * accel * Time.deltaTime * Time.deltaTime;
                AnimatorZoom = startZoom + accelerationDistance;
            }
            lastZoomLerp = Time.renderedFrameCount;
            SendCustomEventDelayedFrames(nameof(LerpZoom), 1);
        }
        public float ConstAccelerationDistance(float startSpeed, float endSpeed, float acceleration)
        {
            return (Mathf.Pow(endSpeed, 2) - Mathf.Pow(startSpeed, 2)) / (2 * acceleration);
        }
    }
}
