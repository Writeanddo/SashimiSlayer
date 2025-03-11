using UnityEngine;

namespace Feel
{
    public class ScreenShakeMono : MonoBehaviour
    {
        public void ShakeScreen(ScreenShakeSO screenShakeSO)
        {
            ScreenShakeService.Instance.ShakeScreen(screenShakeSO);
        }
    }
}