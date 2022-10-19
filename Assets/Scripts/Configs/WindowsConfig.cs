using System.Collections.Generic;
using Code.Windows;
using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "WindowsConfig", menuName = "SO Windows Config", order = 0)]
    public class WindowsConfig : ScriptableObject
    {
        public  GameObject [] WindowsPrefab = null;
    }
}