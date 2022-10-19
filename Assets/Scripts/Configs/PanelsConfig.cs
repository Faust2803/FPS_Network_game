using System.Collections.Generic;
using Code.Windows;
using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "PanelsConfig", menuName = "SO Panels Config", order = 0)]
    public class PanelsConfig : ScriptableObject
    {
        public  GameObject [] PanelsPrefab = null;
    }
}