using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
public class StartGame : MonoBehaviour
{
    float progress = 0;
    public Text loadingText;
    public GameObject loadingPanel;
    IEnumerator Start()
    {
       while(progress <= 100)
        {
            progress +=90 * Time.deltaTime;
            loadingText.text = "loading" + progress.ToString("0") + "%";
            yield return null;
        }
        loadingPanel.SetActive(false);
       
    }

    public void StartButton() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ExitApp()
    {
        Application.Quit();
    }


   
}
