using Code.Windows;
using Managers;
using Util;
using Zenject;
namespace Core
{
    public class GameInstaller: MonoInstaller
    {
        [Inject] private WindowsConfig _windowsConfig;
        [Inject] private PanelsConfig _panelsConfig;
        
        public override void InstallBindings()
        {
            Container.Bind<ManagerContext>().AsSingle();
            
            Container.Bind<GameManager>().AsSingle();
            //Container.BindInterfacesAndSelfTo<UpgradeManager>().AsSingle();
            //Container.BindInterfacesAndSelfTo<UiManagers>().AsSingle();
            //Container.BindInterfacesAndSelfTo<PlayerManager>().AsSingle();
            //Container.BindInterfacesAndSelfTo<LoadDataManager>().AsSingle();
            //Container.BindInterfacesAndSelfTo<LevelManager>().AsSingle();
            //Container.BindInterfacesAndSelfTo<PurchasingManager>().AsSingle();
            
            //Container.Bind<InstantiateManagers>().AsSingle();
            
            // Container.BindFactory<BulletController, FactoryBullet>()
            //     .FromComponentInNewPrefab(_soConfig.BulletPrefab)
            //     .WithGameObjectName("Bullet");
            //
            // Container.BindFactory<PointerController, FactoryMovePointer>()
            //     .FromComponentInNewPrefab(_soConfig.MovePointerPrefab)
            //     .WithGameObjectName("Pointer");
            
            
            //Container.BindFactory< int, LevelController, FactoryLevel>().FromMethod(InitLevel);
            //.BindFactory< int, EnemyMainController, FactoryEnemy>().FromMethod(InitEnemy);
            
            Container.BindFactory< WindowType, BaseWindowView, FactoryWindow>().FromMethod(InitWindow);
            Container.BindFactory< PanelType, BasePanelView, FactoryPanel>().FromMethod(InitPanels);
            //InstallSignals();
        }
        
        // private LevelController InitLevel(DiContainer container, int index)
        // {
        //    var level = _soConfig.LevelPrefab[index];
        //    return Container.InstantiatePrefabForComponent<LevelController>(level);
        // }
        //
        // private EnemyMainController InitEnemy(DiContainer container, int index)
        // {
        //     var level = _soConfig.EnemyPrefab[index];
        //     return Container.InstantiatePrefabForComponent<EnemyMainController>(level);
        // }
        
        private BaseWindowView InitWindow(DiContainer container, WindowType window)
        {
            var level = _windowsConfig.WindowsPrefab[(int)window];
            return Container.InstantiatePrefabForComponent<BaseWindowView>(level);
        }
        
        private BasePanelView InitPanels(DiContainer container, PanelType panel)
        {
            var level = _panelsConfig.PanelsPrefab[(int)panel];
            return Container.InstantiatePrefabForComponent<BasePanelView>(level);
        }
        

        // private void InstallSignals()
        // {
        //     SignalBusInstaller.Install(Container);
        //     
        //     Container.DeclareSignal<RedyGameParts>();
        // }
    }
}