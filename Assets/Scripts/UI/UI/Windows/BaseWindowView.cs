using UnityEngine;

namespace Code.Windows
{
    public abstract class BaseWindowView : BaseView
    {
        
        [Header("Animation Wndow")]
        [SerializeField] private GameObject  _windowPanel;
        [SerializeField] private bool  _openAnimation;
        [SerializeField] private bool  _closeAnimation;
        
        protected BaseWindowMediator _mediator;
        
        public BaseWindowMediator BaseMediator
        {
            get { return _mediator; }
        }

        public GameObject WindowPanel
        {
            get { return _windowPanel; }
        }
        
        public void OnCreateMediator(out BaseWindowMediator mediator)
        {
            mediator = _mediator;
        }
        
        public bool OpenAnimation
        {
            get { return _openAnimation; }
        }
        
        public bool CloseAnimation
        {
            get { return _closeAnimation; }
        }
        
        
        public override void Init()
        {
            base.Init();
            _mediator.Mediate(this);
        }
    }
}