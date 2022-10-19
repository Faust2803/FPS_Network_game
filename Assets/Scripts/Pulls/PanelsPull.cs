using UnityEngine;

namespace Code.Pulls
{
    public class PanelsPull : MonoBehaviour
    {
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}