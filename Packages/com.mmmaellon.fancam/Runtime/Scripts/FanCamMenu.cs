
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
        public FanCamManager manager;
        public Animator animator;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Reset()
        {
            animator = GetComponent<Animator>();
        }
#endif
        public void Start()
        {
            manager.menu = this;
        }

        public void Cam0()
        {
            manager.Cam0();
        }
        public void Cam1()
        {
            manager.Cam1();
        }
        public void Cam2()
        {
            manager.Cam2();
        }
        public void Cam3()
        {
            manager.Cam3();
        }
        public void Cam4()
        {
            manager.Cam4();
        }
        public void Cam5()
        {
            manager.Cam5();
        }
        public void Cam6()
        {
            manager.Cam6();
        }
        public void Cam7()
        {
            manager.Cam7();
        }
        public void Cam8()
        {
            manager.Cam8();
        }
        public void Cam9()
        {
            manager.Cam9();
        }

        public string cameraControlsParameter = "camera controls";
        public void CameraSwitcherBtn()
        {
            if (animator.GetInteger(cameraControlsParameter) == 2)
            {

                animator.SetInteger(cameraControlsParameter, 0);
            }
            else
            {
                animator.SetInteger(cameraControlsParameter, 2);
            }
        }

        public void OperatorControlBtn()
        {
            if (animator.GetInteger(cameraControlsParameter) == 1)
            {

                animator.SetInteger(cameraControlsParameter, 0);
            }
            else
            {
                animator.SetInteger(cameraControlsParameter, 1);
            }
        }

        public void BecomeDirectorBtn()
        {
            manager.TakeOwnership();
        }
    }
}
