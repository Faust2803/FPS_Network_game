using Code;
using Code.Windows;
using Zenject;
namespace Util
{
    public class FactoryWindow : PlaceholderFactory<WindowType, BaseWindowView> { }
    public class FactoryPanel : PlaceholderFactory<PanelType, BasePanelView> { }
    // public class FactoryBullet : PlaceholderFactory<BulletController> { }
    // public class FactoryEnemy : PlaceholderFactory<int,EnemyMainController> { }
    // public class FactoryLevel : PlaceholderFactory<int, LevelController> { }
    // public class FactoryMovePointer : PlaceholderFactory<PointerController> { }
}