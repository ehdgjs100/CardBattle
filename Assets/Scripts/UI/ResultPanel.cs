using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button retryButton;

    private void Awake()
    {
        panelRoot.SetActive(false);
        retryButton.onClick.AddListener(Retry);
    }

    public void Show(GameResult result)
    {
        resultText.text = result == GameResult.Win ? "Victory" : "Defeat";
        panelRoot.SetActive(true);
    }

    private void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
