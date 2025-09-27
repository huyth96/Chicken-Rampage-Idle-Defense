using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    public AudioMixer audioMixer;

    public void OnStartClick()
    {
        SceneManager.LoadScene("Game");
    }

    public void OnExitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("Volume", volume);
    }
}
