using UnityEngine;

namespace Code.Windows
{
    public abstract class BasePanelView : BaseView
    {
        
        [Header("Animation Panel")]
        
        [SerializeField] private bool  _openAnimation;
        [SerializeField] private bool  _closeAnimation;
        
        protected BasePanelMediator _mediator;
        
        public BasePanelMediator BaseMediator
        {
            get { return _mediator; }
        }
        
        public GameObject Panel
        {
            get { return this.gameObject; }
        }
        
        public void OnCreateMediator(out BasePanelMediator mediator)
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