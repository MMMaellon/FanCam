#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Collections;
#endif
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.FanCam
{
    [RequireComponent(typeof(CinemachineStateDrivenCamera))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FanCamSwitcher : UdonSharpBehaviour
    {
        private readonly int OwnerHash = Animator.StringToHash("owner");
        private readonly int camParameterHash = Animator.StringToHash("cam");


        public Animator animator;
        [SerializeField]
        [HideInInspector]
        CinemachineVirtualCameraBase switcher;
        [SerializeField]
        [HideInInspector]
        GameObject[] subCams;

        VRCPlayerApi owner;
        public void Start()
        {
            owner = Networking.GetOwner(gameObject);
            Cam = Cam;
        }

        [UdonSynced]
        [System.NonSerialized]
        int _cam = 0;

        public int Cam
        {
            get => _cam;
            set
            {
                _cam = value;
                animator.SetInteger(camParameterHash, value);
                if (IsOwnerLocal())
                {
                    RequestSerialization();
                }
            }
        }

        public bool IsOwnerLocal()
        {
            return Utilities.IsValid(owner) && owner.isLocal;
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            owner = player;
            animator.SetBool(OwnerHash, IsOwnerLocal());
        }

        public void Cam1()
        {
            if (!IsOwnerLocal())
            {
                return;
            }
            Cam = 1;
        }
        public void Cam2()
        {
            if (!IsOwnerLocal())
            {
                return;
            }
            Cam = 2;
        }
        public void Cam3()
        {
            if (!IsOwnerLocal())
            {
                return;
            }
            Cam = 3;
        }
        public void Cam4()
        {
            if (!IsOwnerLocal())
            {
                return;
            }
            Cam = 4;
        }
        public void Cam5()
        {
            if (!IsOwnerLocal())
            {
                return;
            }
            Cam = 5;
        }
        public void Cam6()
        {
            if (!IsOwnerLocal())
            {
                return;
            }
            Cam = 6;
        }
        public void Cam7()
        {
            if (!IsOwnerLocal())
            {
                return;
            }
            Cam = 7;
        }
        public void Cam8()
        {
            if (!IsOwnerLocal())
            {
                return;
            }
            Cam = 8;
        }
        public void Cam9()
        {
            if (!IsOwnerLocal())
            {
                return;
            }
            Cam = 9;
        }
        public void Cam0()
        {
            if (!IsOwnerLocal())
            {
                return;
            }
            Cam = 0;
        }

        public void TakeOwnership()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Reset()
        {
            switcher = GetComponent<CinemachineStateDrivenCamera>();
        }

        public void Setup()
        {
            var children = new List<GameObject>();
            foreach (var instruction in ((CinemachineStateDrivenCamera)switcher).m_Instructions)
            {
                if (Utilities.IsValid(instruction.m_VirtualCamera))
                {
                    children.Append(instruction.m_VirtualCamera.gameObject);
                }
                else
                {
                    children.Append(null);
                }
            }
            subCams = children.ToArray();
        }
#endif
    }
}
