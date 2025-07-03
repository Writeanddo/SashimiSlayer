using UnityEngine;

namespace Menus.PauseMenu
{
    public abstract class PauseMenuView : MonoBehaviour
    {
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        public virtual void ViewAwake()
        {
        }

        public virtual void ViewStart()
        {
        }
    }
}