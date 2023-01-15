using UnityEngine;

namespace MITHack.Robot.Utils
{
    public class BillboardComponent : MonoBehaviour
    {
        
        
        private Transform Target
        {
            get
            {
                if (CameraUtility.FirstCamera)
                {
                    return CameraUtility.FirstCamera.transform;
                }
                return null;
            }
        }
        
        private void Update()
        {
            var cachedTransform = transform;
            if (Target)
            {
                var directionToCamera = Target.position - cachedTransform.position;
                directionToCamera.Normalize();
                cachedTransform.forward = directionToCamera;
            }
        }
    }
}