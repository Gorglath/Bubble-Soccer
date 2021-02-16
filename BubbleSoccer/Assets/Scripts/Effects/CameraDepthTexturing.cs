using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleSoccer
{
    [ExecuteInEditMode]
    public class CameraDepthTexturing : MonoBehaviour
    {
        private void Awake()
        {
            Camera.main.depthTextureMode = DepthTextureMode.Depth;
        }
    }
}
