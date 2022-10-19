using UnityEngine;

namespace Code.Pulls
{
    public class WindowsPull : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}