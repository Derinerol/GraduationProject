
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneExit : MonoBehaviour
{
    [SerializeField] private string destinationScene = "Diner_Investigation";

    public void ExitScene()
    {
        SceneManager.LoadScene(destinationScene);
    }
}
