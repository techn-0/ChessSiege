using UnityEngine;
using UnityEngine.SceneManagement;   // SceneManager (씬 매니저)

public class TitleSceneController : MonoBehaviour
{
    [SerializeField] GameObject instructionsPanel;   // 설명 패널 참조

    public void OnStartGame()
    {
        Debug.Log("OnStartGame 호출됨");
        SceneManager.LoadScene("Stage1");
    }

    public void OnOpenInstructions()  => instructionsPanel.SetActive(true);
    public void OnCloseInstructions() => instructionsPanel.SetActive(false);
}
