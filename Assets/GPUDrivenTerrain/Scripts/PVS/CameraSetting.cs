using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PVS
{
    public class CameraSetting
    {
        public  struct CameraSetParam
        {
            public bool allowHDR;
            public bool allowMSAA;
            public CameraClearFlags cameraClearFlags;
            public Color backgroundColor;
        }

        private static Dictionary<Camera, CameraSetParam> s_OldCameraCamera = new Dictionary<Camera, CameraSetParam>();
        public static void Init(Camera camera)
        {
            CameraSetParam oldParam = new CameraSetParam();
            oldParam.allowHDR = camera.allowHDR;
            oldParam.allowMSAA = camera.allowMSAA;
            oldParam.cameraClearFlags = camera.clearFlags;
            oldParam.backgroundColor = camera.backgroundColor;
            if (s_OldCameraCamera.ContainsKey(camera))
            {
                s_OldCameraCamera[camera] = oldParam;
            }
            else
            {
                s_OldCameraCamera.Add(camera, oldParam);
            }
            
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            //TODO:还有其他的一些设置，比如Layer的设置
        }

        public static void Restore(Camera camera)
        {
            if (!s_OldCameraCamera.ContainsKey(camera))
            {
                return;
            }
            CameraSetParam oldParam = s_OldCameraCamera[camera];
            camera.allowHDR = oldParam.allowHDR;
            camera.allowMSAA = oldParam.allowMSAA;
            camera.clearFlags = oldParam.cameraClearFlags;
            camera.backgroundColor = oldParam.backgroundColor;
        }
    }
}

