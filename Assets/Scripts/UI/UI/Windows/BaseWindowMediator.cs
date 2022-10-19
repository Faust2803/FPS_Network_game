using System;
using DG.Tweening;
using Managers;
using UnityEngine;
using Zenject;

namespace Code.Windows
{
    public class BaseWindowMediator: BaseMediator
    {
        public BaseWindowView View { get; set; }
        
        private WindowType _windowType;
        
        public WindowType WindowType
        {
            get { return _windowType; }
        }

        public virtual void Mediate(BaseWindowView value)
        {
            View =  value;
            _moveto  = View.WindowPanel.transform.position.y;
        }
        
        public virtual void SetData(object data)
        {
            _data = data;
        }
        
        public void SetType(WindowType windowType)
        {
            _windowType = windowType;
        }
        
        public virtual void Show()
        {
            ShowStart();
            View.ShowStart();
        }
        
        public void Close(Action callback = null)
        {
            if (callback!= null)
            {
                _afterCloseCallback = callback;
            }
            CloseStart();
        }

        protected virtual void ShowStart()
        {
            if (View.WindowPanel == null || !View.OpenAnimation)
            {
                ShowEnd();
                return;
            }
            
            View.WindowPanel.transform.position = new Vector3(View.WindowPanel.transform.position.x,
                MOVE_POSITION,
                   View.WindowPanel.transform.position.z);
            View.WindowPanel.transform.DOMoveY(_moveto, ANIMATION_DURATION).OnComplete(ShowEnd);
        }
        
        protected virtual void ShowEnd()
        {

        }
        
        protected virtual void CloseStart()
        { 
            if (View.WindowPanel == null || !View.CloseAnimation)
            {
                CloseFinish();
                return;
            }
            View.WindowPanel.transform.DOMoveY(MOVE_POSITION, ANIMATION_DURATION).OnComplete(CloseFinish);
        }
        
        protected virtual void CloseFinish()
        { 
            View.Close();
            if (_afterCloseCallback!= null)
            {
                _afterCloseCallback.Invoke();
            }
        }
    }
    
    public abstract class BaseWindowMediator<T, Z> : BaseWindowMediator where T : BaseWindowView where Z : WindowData
    {
        public T Target
        {
            get { return View as T; }
        }

        public Z Data
        {
            get { return _data as Z; }
        }
    }
}