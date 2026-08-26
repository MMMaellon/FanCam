using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.FanCam
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FanCamTrackFollower : UdonSharpBehaviour
    {
        public FanCamTrackPoint[] points;
        [System.NonSerialized]
        public Vector3[] wayPoints = { };
        public SmartObjectSync targetBall;
        VRCTweenHandle pathHandle;
        VRCTweenHandle previewHandle;


        // public float duration = 4f;
        [System.NonSerialized]
        float _speed = 4f;
        public float Speed
        {
            get => _speed;
            set
            {
                // Debug.LogWarning("Speed change");
                // var elapsed = pathHandle.Elapsed % duration;
                // int loopCount = Mathf.FloorToInt(pathHandle.Elapsed / duration);
                // var elapsedRatio = elapsed / duration;
                // duration = 240f / value;
                // if (pathHandle.IsPlaying)
                // {
                //     Debug.LogWarning("We were playing");
                //     pathHandle.SetDuration(duration);
                //     pathHandle.Goto(elapsedRatio * duration, true);
                //     // if (loopCount % 2 == 1)
                //     // {
                //     //     pathHandle.Flip();
                //     //     pathHandle.SetLoops(-1, VRCTweenLoopType.Yoyo);
                //     // }
                // }
                _speed = value;
                if (pathHandle.IsPlaying)
                {
                    PopulateWaypoints();
                }
            }
        }

        void Start()
        {
            foreach (var point in points)
            {
                point.track = this;
            }
            // Speed = Speed;
        }

        // public void OnEnable()
        // {
        //     // PopulateWaypoints();
        // }

        public void PopulateWaypoints()
        {
            previewPos = null;
            previewHandle.Pause();
            if (points.Length == 0 || !Utilities.IsValid(points[0].transform.parent))
            {
                return;
            }
            wayPoints = new Vector3[points.Length];
            for (int i = 0; i < wayPoints.Length; i++)
            {
                wayPoints[i] = points[i].transform.position;
            }
            pathHandle.Kill();
            transform.SetPositionAndRotation(points[0].transform.position, points[0].transform.parent.rotation);
            pathHandle = VRCTween.TweenPath(transform, wayPoints, _speed, VRCTweenPathType.CatmullRom, false, 10, VRCTweenEase.InOutSine)
                // .SetDuration(duration)
                .SetSpeedBased()
                .SetLoops(-1, VRCTweenLoopType.Yoyo)
                .OnComplete(this, nameof(PreviewLoop));
        }

        public void Restart()
        {
            pathHandle.Restart();
            // if (points.Length > 0 && !Utilities.IsValid(points[0].transform.parent))
            // {
            //     transform.rotation = points[0].transform.parent.rotation;
            // }
        }

        public void StartTrack()
        {
            PopulateWaypoints();
        }

        public void StopTrack()
        {
            pathHandle.Kill();
        }

        void OnDestroy()
        {
            gameObject.KillAllTweens();
        }

        SmartObjectSync previewPos;
        public void ForcePreview(SmartObjectSync sync)
        {
            previewPos = sync;
            pathHandle.Kill();
            PreviewLoop();
        }

        bool firstPreview = true;
        public void PreviewLoop()
        {
            if (!Utilities.IsValid(previewPos))
            {
                return;
            }

            if (firstPreview)
            {
                previewHandle = VRCTween.TweenPosition(transform, previewPos.transform.position, 0.2f, VRCTweenEase.OutSine);
                previewHandle.OnComplete(this, nameof(PreviewLoop));
            }
            else
            {
                previewHandle.ChangeEndValue(previewPos.transform.position, true);
            }
            previewHandle.Play();
        }
    }
}
