using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private Button gameStartButton;
    [SerializeField] private Button cardEditButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private LobbyPanel cardEditPanel;
    [SerializeField] private LobbyPanel shopPanel;

    private void Awake()
    {
        gameStartButton?.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.Game));
        cardEditButton?.onClick.AddListener(() => cardEditPanel?.Show());
        shopButton?.onClick.AddListener(() => shopPanel?.Show());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
