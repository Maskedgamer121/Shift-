using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
    public class CavrnusRtcController : MonoBehaviour
    {
        public static CavrnusRtcController Instance;
        
        public CavrnusVideoController Video { get; private set; }

        private void Awake()
        {
            Instance = this;
            Video = new CavrnusVideoController();
        }
    }
}