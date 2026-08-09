
using Cinemachine.Utility;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


namespace MMMaellon.FanCam
{
    [RequireComponent(typeof(Animator))]
    public class FanCamMenu : UdonSharpBehaviour
    {
        private int VisibleHash = Animator.StringToHash("visible");
        public Animator animator;
        public FanCamManager manager;
        public VRC_Pickup pickup;
        public bool startVisible = true;
        void Reset()
        {
            animator = GetComponent<Animator>();
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public static FanCamMenu[] All
        {
            get
            {
                return FindObjectsOfType<FanCamMenu>(true);
            }
        }
#endif

        public void OnManagerUpdate()
        {

        }

        VRCPlayerApi localPlayer;
        void Start()
        {
            localPlayer = Networking.LocalPlayer;
            SendCustomEventDelayedFrames(nameof(Loop), 0, VRC.Udon.Common.Enums.EventTiming.LateUpdate);
            if (startVisible)
            {
                ToggleOnMenu();
            }
            else
            {
                ToggleOffMenu();
            }
        }

        void Loop()
        {
            SendCustomEventDelayedFrames(nameof(Loop), 0, VRC.Udon.Common.Enums.EventTiming.LateUpdate);

            if (MenuVisible && Vector3.ProjectOnPlane(transform.position - localPlayer.GetPosition(), Vector3.up).magnitude > despawnDistance)
            {
                ToggleOffMenu();
            }
            if (!localPlayer.IsUserInVR())
            {

            }
        }

        float lastSecondNoInput = -1001;
        public float doubleClickDuration = 0.5f;
        public float doubleClickDistance = 0.2f;
        Vector3 headPos;
        public override void InputUse(bool value, VRC.Udon.Common.UdonInputEventArgs args)
        {
            if (!localPlayer.IsUserInVR())
            {
                return;
            }
            if (value)
            {
                headPos = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                float distance;
                if (args.handType == VRC.Udon.Common.HandType.LEFT)
                {
                    distance = Vector3.Distance(headPos, localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position);

                }
                else
                {
                    distance = Vector3.Distance(headPos, localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position);
                }
                if (distance <= doubleClickDistance)
                {
                    if (Time.timeSinceLevelLoad - lastSecondNoInput > doubleClickDuration)
                    {
                        lastSecondNoInput = Time.timeSinceLevelLoad;
                    }
                    else
                    {
                        ToggleMenu();
                    }
                }
            }
        }

        public bool MenuVisible
        {
            get
            {
                return animator.GetBool(VisibleHash);
            }
        }

        public void ToggleMenu()
        {
            if (MenuVisible)
            {
                ToggleOffMenu();
            }
            else
            {
                ToggleOnMenu();
            }
        }

        VRCPlayerApi.TrackingData localHeadTracking;
        public float spawnDistance = 0.5f;
        public float despawnDistance = 3f;
        public float spawnHeight = -0.1f;
        public void ToggleOnMenu()
        {
            localHeadTracking = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            transform.SetPositionAndRotation(localHeadTracking.position + localHeadTracking.rotation * new Vector3(0, spawnHeight, spawnDistance), localHeadTracking.rotation);
            animator.SetBool(VisibleHash, true);
            pickup.Drop();
            pickup.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        public void ToggleOffMenu()
        {
            pickup.Drop();
            animator.SetBool(VisibleHash, false);
        }
    }
}
