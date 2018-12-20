using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneScript : MonoBehaviour
{
  public void LoadSceneUI ()
  {
    LoadScene ("Scene-UI");
  }

  public void LoadSceneMR ()
  {
    LoadScene ("Scene-MR");
  }

  private void LoadScene (string sceneName)
  {
    SceneManager.LoadScene (sceneName, LoadSceneMode.Single);
  }
}