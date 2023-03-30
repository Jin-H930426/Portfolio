using UnityEngine;

namespace JH
{
    public class JHUtility
    {
        public static bool IsInScene(GameObject obj)
        {
            var sceneName = obj.scene.name;
            return !string.IsNullOrEmpty(sceneName) &&
                   (sceneName.Equals(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name) || sceneName.Equals("DontDestroyOnLoad")) 
                   && obj.activeInHierarchy;
        }
        public static bool IsInScene(MonoBehaviour mono)
        {
            GameObject obj = mono.gameObject;

            return IsInScene(obj);
        }
    }
}