
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.FanCam
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(BoxCollider))]
    public class FanCamCameraMenu : UdonSharpBehaviour
    {
        public Animator animator;
        public Canvas canvas;
        public BoxCollider boxCollider;
        public FanCamPictureButton cameraButtonTemplate;
        public LayoutGroup layoutGroup;
        [HideInInspector]
        public DataList buttons;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Reset()
        {
            animator = GetComponent<Animator>();
            canvas = GetComponent<Canvas>();
            boxCollider = GetComponent<BoxCollider>();
        }
#endif

        public void AddButton(string labelText, Texture buttonImage, UdonBehaviour targetBehaviour, string targetEvent)
        {
            GameObject newButtonObj = Instantiate(cameraButtonTemplate.gameObject, layoutGroup.transform);
            var newButton = newButtonObj.GetComponent<FanCamPictureButton>();
            buttons.Add(newButton);
            newButton.label.text = labelText;
            newButton.image.texture = buttonImage;
            newButton.targetBehaviour = targetBehaviour;
            newButton.targetEvent = targetEvent;
            // layoutGroup.MarkDirty();
        }

        public void ClearButtons()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                Destroy(((FanCamPictureButton)buttons[0].Reference).gameObject);
            }
            buttons.Clear();
            // layoutGroup.MarkDirty();
        }
    }
}
