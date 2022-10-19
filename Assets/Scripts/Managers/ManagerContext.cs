using UnityEngine;
using Zenject;

namespace Managers
{
    public class ManagerContext
    {
        // [Inject] private PlayerManager _playerManager;
        // [Inject] private LoadDataManager _loadDataManager;
        [Inject] private GameManager _gameManager;
        //[Inject] private UiManagers _uiManagers;
        // [Inject] private InstantiateManagers _instantiateManagers;
        // [Inject] private LevelManager _levelManager;
        // [Inject] private UpgradeManager _upgradeManager;
        // [Inject] private PurchasingManager _purchasingManager;

        // public PlayerManager PlayerManager => _playerManager;
        // public LoadDataManager LoadDataManager => _loadDataManager;
        public GameManager GameManager => _gameManager;
        //public UiManagers UiManagers => _uiManagers;
        // public InstantiateManagers InstantiateManagers => _instantiateManagers;
        // public LevelManager LevelManager => _levelManager;
        // public UpgradeManager UpgradeManager => _upgradeManager;
        //public PurchasingManager PurchasingManager => _purchasingManager;
        
    }
}