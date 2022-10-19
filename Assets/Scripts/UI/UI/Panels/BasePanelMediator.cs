using System;
using DG.Tweening;
using Managers;
using UnityEngine;
using Zenject;

namespace Code.Windows
{
    public class BasePanelMediator: BaseMediator
    {
        public BasePanelView View { get; set; }
        
        private PanelType _panelType;
        
        public PanelType PanelType
        {
            get { return _panelType; }
        }

        public virtual void Mediate(BasePanelView value)
        {
            View =  value;
            _moveto  = View.Panel.transform.position.y;
        }
        
        public virtual void SetData(object data)
        {
            _data = data;
        }
        
        public void SetType(PanelType windowType)
        {
            _panelType = windowType;
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
            if (View.Panel == null || !View.OpenAnimation)
            {
                ShowEnd();
                return;
            }
            
            View.Panel.transform.position = new Vector3(View.Panel.transform.position.x,
                MOVE_POSITION,
                   View.Panel.transform.position.z);
            View.Panel.transform.DOMoveY(_moveto, ANIMATION_DURATION).OnComplete(ShowEnd);
        }
        
        protected virtual void ShowEnd()
        {

        }
        
        protected virtual void CloseStart()
        { 
            if (View.Panel == null || !View.CloseAnimation)
            {
                CloseFinish();
                return;
            }
            View.Panel.transform.DOMoveY(MOVE_POSITION, ANIMATION_DURATION).OnComplete(CloseFinish);
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
    
    public abstract class BasePanelMediator<T, Z> : BasePanelMediator where T : BasePanelView where Z : PanelData
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