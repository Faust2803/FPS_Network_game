using UnityEngine;

namespace Code.Windows
{
    public abstract class BaseView : MonoBehaviour
    {
        public virtual void Init()
        {
            CreateMediator();
        }
        
        protected abstract void CreateMediator();

        public void ShowStart()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}