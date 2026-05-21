using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneFunction : MonoBehaviour
{
    void Start()
    {
        SoundManager.Instance.PlayBGM("titlebgm");
    }

    void Update()
    {
        if (Input.anyKey)
        {
            SoundManager.Instance.StopBGM();
            SceneManager.LoadScene("GameScene");
        }
    }
}
