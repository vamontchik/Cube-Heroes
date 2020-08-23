using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public void LoadAreaOne()
    {
        SceneManager.LoadScene(SceneIndex.FIGHT_INDEX);
    }
}
