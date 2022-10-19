using System;
using System.Collections.Generic;
using Code.Pulls;
using Code.Windows;
using UnityEngine;
using Util;
using Zenject;

namespace Managers
{
    public class UiManagers:IInitializable,  IDisposable
    { 
        [Inject] private FactoryWindow _factoryWindow;
        [Inject] private FactoryPanel _factoryPanels;
        
        [Inject] private WindowsPull _windowPull;
        [Inject] private PanelsPull _panelPull;

        [Inject] private ManagerContext _managerContext;
        
        //readonly SignalBus _signalBus = null;
        
        private BaseWindowMediator _currentWindow;
        
        private Dictionary<WindowType, BaseWindowMediator> _allWindows;
        private Dictionary<PanelType, BasePanelMediator> _allPanels;
        

        public void Initialize()
        {
            _allWindows = new Dictionary<WindowType, BaseWindowMediator>();
            _allPanels = new Dictionary<PanelType, BasePanelMediator>();
            //_signalBus.Fire(new RedyGameParts() { GameParts = GameParts.UI });
        }

        public void Dispose()
        {

        }
        
        public void Open(WindowType windowType, object data = null)
        {
            OpenWindow(windowType, data);
        }
        
        public void ForceOpen(WindowType windowType, object data = null)
        {
            OpenWindow(windowType, data);
        }
        
        public void OpenPanel(PanelType panelType, object data = null)
        {
            CreatePanel(panelType, data);
        }
        
        public void ForceOpenPanel(PanelType panelType, object data = null)
        {
            CreatePanel(panelType, data);
        }
        
        public void Close(Action callback = null)
        {
            _currentWindow.Close(callback);
        }
        
        public void ClosePanel(PanelType panelType, Action callback = null)
        {
            if (!_allPanels.ContainsKey(panelType)) return;
            var panel = _allPanels[panelType];
            panel.Close();
        }
        
       
        private void OpenWindow(WindowType windowType, object data = null)
        {
            if (_allWindows.ContainsKey(windowType))
            {
                _currentWindow = _allWindows[windowType];
            }
            else
            {
                _currentWindow = GetWindow(windowType);
        
                if (_currentWindow == null) return;
        
                _allWindows.Add(windowType, _currentWindow);
            }
        
            _currentWindow.SetData(data);
            _currentWindow.Show();
        }
        
        private void CreatePanel(PanelType panelType, object data = null)
        {
            BasePanelMediator panel;
            if (_allPanels.ContainsKey(panelType))
            {
                panel = _allPanels[panelType];
            }
            else
            {
                panel = GetPanel(panelType);
            
                if (panel == null) return;
            
                _allPanels.Add(panelType, panel);
            }
            
            panel.SetData(data);
            panel.Show();
        }
        
        private BaseWindowMediator GetWindow(WindowType windowType)
        {
            var view = LoadPrefab(windowType);
            if (view == null) return null;
        
            view.Init();
            BaseWindowMediator mediator;
            view.OnCreateMediator(out mediator);
            mediator.SetType(windowType);
            mediator.SetManagers(_managerContext);
            view.gameObject.SetActive(false);
        
            return mediator;
        }
        
        private BasePanelMediator GetPanel(PanelType panelType)
        {
            var view = LoadPanelPrefab(panelType);
            if (view == null) return null;
        
            view.Init();
            BasePanelMediator mediator;
            view.OnCreateMediator(out mediator);
            mediator.SetType(panelType);
            mediator.SetManagers(_managerContext);
            view.gameObject.SetActive(false);
        
            return mediator;
        }
        
        private BaseWindowView LoadPrefab(WindowType windowType)
        {
            var view = _factoryWindow.Create(windowType);
            view.gameObject.transform.SetParent(_windowPull.transform,false);
            return view;
        }
        
        private BasePanelView LoadPanelPrefab(PanelType panelType)
        {
            var view = _factoryPanels.Create(panelType);
            view.gameObject.transform.SetParent(_panelPull.transform,false);
            return view;
        }
    }
}