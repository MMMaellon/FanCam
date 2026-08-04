
using Cinemachine;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
#endif

namespace MMMaellon.FanCam
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FanCam : UdonSharpBehaviour
    {
        bool _isPlayerObject = false;
        public bool IsPlayerObject
        {
            get => _isPlayerObject;
        }
        public FanCamManager manager;
        public CinemachineVirtualCamera virtualCam;

        [UdonSynced]
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

                //PlayerObjects get negative Ids starting from -1
                if (IsPlayerObject)
                {
                    if (value < 0)
                    {
                        manager.AddFanCam(this);
                    }
                }
                //Non player objects get positive Ids starting from 1
                else if (value > 0)
                {
                    manager.AddFanCam(this);
                }
            }
        }

        VRCPlayerApi owner;
        void Start()
        {
            DisableCam();
            owner = Networking.GetOwner(gameObject);
            if (IsPlayerObject)
            {
                Debug.LogWarning("[FANCAM] Player Object detected");
                if (IsOwnerLocal())
                {
                    Id = (owner.playerId * -10000) - Id;
                }
            }
            else
            {
                Id = Id;
            }
#if !COMPILER_UDONSHARP && UNITY_EDITOR
            _dirty = true;
#endif
        }
        void OnDestroy()
        {
            manager.RemoveFanCam(this);
#if !COMPILER_UDONSHARP && UNITY_EDITOR
            _dirty = true;
#endif
        }
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
            virtualCam.enabled = true;
        }
        public void DisableCam()
        {
            virtualCam.enabled = false;
        }
        public bool CamActive
        {
            get => virtualCam.enabled;
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void CheckPlayerObject()
        {
            _isPlayerObject = Utilities.IsValid(GetComponent<VRCPlayerObject>());
        }

        public static bool Dirty
        {
            get => _dirty;
        }
        static bool _dirty = true;
        void Reset()
        {
            _dirty = true;
        }
        static FanCam[] allFanCams = { };
        public static FanCam[] All
        {
            get
            {
                if (_dirty)
                {
                    Debug.LogWarning("Updating Fan Cam List");
                    allFanCams = FindObjectsOfType<FanCam>(true);
                    _dirty = false;
                }
                return allFanCams;
            }
        }
        void OnValidate()
        {
            _dirty = true;
        }
        public void Setup()
        {
            Debug.LogWarning("Setup");
        }
#endif 
    }
}
