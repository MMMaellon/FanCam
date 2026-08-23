
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.FanCam
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FanCamPictureButton : UdonSharpBehaviour
    {
        public TMPro.TextMeshPro label;
        public RawImage image;
        public Button button;
        public UdonBehaviour targetBehaviour;
        public string targetEvent = "OnClickButton";
        public string targetProperty = "buttonId";
        public int Id = -1001;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Setup()
        {
            int foundIndex = -1001;
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                var listener = button.onClick.GetPersistentTarget(i);
                if (listener.GetType() == typeof(FanCamPictureButton) && (FanCamPictureButton)listener == this)
                {
                    foundIndex = i;
                    break;
                }
            }
            if (foundIndex < 0)
            {
                button.onClick.AddListener(delegate { SendCustomEvent("Click"); });
            }
        }
#endif
        public void Click()
        {
            Debug.LogWarningFormat("Clicked {}", name);
            targetBehaviour.SetProgramVariable(targetProperty, (object)Id);
            targetBehaviour.SendCustomEvent(targetEvent);
        }
    }
}
