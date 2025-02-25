using UnityEngine;

namespace Base
{
    public class DescMono : MonoBehaviour
    {
        [SerializeField]
        [TextArea(3, 10)]
        private string _devDesc;
    }
}