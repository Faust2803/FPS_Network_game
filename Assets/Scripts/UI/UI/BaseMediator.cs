using System;
using DG.Tweening;
using Managers;
using UnityEngine;
using Zenject;

namespace Code.Windows
{
    public class BaseMediator
    {

        private ManagerContext _managerContext = null;
        protected ManagerContext ManagerContext => _managerContext;
        
        protected const float MOVE_POSITION = -1500;
        protected const float ANIMATION_DURATION = 0.5F;
        protected float _moveto;
        protected Action _afterCloseCallback;
        protected object _data;
        public BaseWindowView View { get; set; }
        public virtual void SetManagers(ManagerContext managerContext)
        {
            _managerContext = managerContext;
        }

    }
}