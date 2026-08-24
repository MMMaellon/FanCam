
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class FrameCount : UdonSharpBehaviour
{
    void Start()
    {

    }
    int lastFrame = -1001;

    public void Count()
    {
        var frame = Time.renderedFrameCount;
        Debug.Log("frame: " + frame);
        if (frame == lastFrame)
        {
            Debug.LogWarning("DOUBLE COUNT!!!!!");
        }
        else if (lastFrame > 0 && frame - 1 != lastFrame)
        {
            Debug.LogWarning("SKIPPED FRAME");
        }
        lastFrame = Time.renderedFrameCount;
    }
}
