using Assets.CrossPlatformInput.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Utility
{
    [RequireComponent(typeof (GUITexture))]
    public class ForcedReset : MonoBehaviour
    {
        private void Update()
        {
            // if we have forced a reset ...
            if (CrossPlatformInputManager.GetButtonDown("ResetObject"))
            {
                //... reload the scene
                SceneManager.LoadScene(SceneManager.GetSceneAt(0).path);
            }
        }
    }
}
