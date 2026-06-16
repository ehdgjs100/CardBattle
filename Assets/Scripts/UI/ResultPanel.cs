using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Image panelBackground;
    [SerializeField] private Image screenFlash;
    [SerializeField] private Image darkOverlay;
    [SerializeField] private Image vignetteImage;
    [SerializeField] private GameObject winGO;
    [SerializeField] private GameObject loseGO;
    [SerializeField] private Image[] winLetters;
    [SerializeField] private Image[] loseLetters;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button lobbyButton;
    [SerializeField] private TMP_Text killCountText;
    [SerializeField] private TMP_Text turnCountText;

    [SerializeField] private Color winBgColor = new Color(0.06f, 0.06f, 0.08f, 0.9f);
    [SerializeField] private Color loseBgColor = new Color(0.08f, 0.08f, 0.1f, 0.9f);

    private Vector2 _retryBtnOrigPos;
    private Vector2 _lobbyBtnOrigPos;

    private void Awake()
    {
        panelRoot.SetActive(false);
        SetAlpha(screenFlash, 0f);
        SetAlpha(darkOverlay, 0f);
        InitVignette();
        winGO?.SetActive(false);
        loseGO?.SetActive(false);
        InitButtonsOffScreen();
        retryButton?.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
        lobbyButton?.onClick.AddListener(GoToLobby);
    }

    private void InitVignette()
    {
        if (vignetteImage == null) return;

        const int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size * 0.5f;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center;
                float alpha = Mathf.Clamp01((dist - 0.3f) / 0.7f);
                alpha = alpha * alpha;
                tex.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
            }
        tex.Apply();

        vignetteImage.sprite = Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f);
        SetAlpha(vignetteImage, 0f);
    }

    private void InitButtonsOffScreen()
    {
        if (retryButton != null)
        {
            var rt = (RectTransform)retryButton.transform;
            _retryBtnOrigPos = rt.anchoredPosition;
            rt.anchoredPosition = _retryBtnOrigPos + Vector2.down * 800f;
        }
        if (lobbyButton != null)
        {
            var rt = (RectTransform)lobbyButton.transform;
            _lobbyBtnOrigPos = rt.anchoredPosition;
            rt.anchoredPosition = _lobbyBtnOrigPos + Vector2.down * 800f;
        }
    }

    public void Show(GameResult result, int kills, int turns)
    {
        killCountText?.SetText(kills.ToString());
        turnCountText?.SetText(turns.ToString());

        if (result == GameResult.Win)
            PlayWin();
        else
            PlayLose();
    }

    private void PlayWin()
    {
        loseGO?.SetActive(false);

        if (winLetters != null)
            for (int i = 0; i < winLetters.Length; i++)
                if (winLetters[i] != null)
                    winLetters[i].transform.localScale = Vector3.zero;

        winGO?.SetActive(true);
        PlayFlash(Color.white, 0.9f, 0.08f, 0.45f);
        panelRoot.SetActive(true);

        if (panelBackground != null)
        {
            panelBackground.color = new Color(winBgColor.r, winBgColor.g, winBgColor.b, 0f);
            panelBackground.DOFade(winBgColor.a, 0.3f).SetDelay(0.1f);
        }

        if (winLetters != null)
            for (int i = 0; i < winLetters.Length; i++)
            {
                if (winLetters[i] == null) continue;
                winLetters[i].transform.DOScale(Vector3.one, 0.35f)
                    .SetEase(Ease.OutBack, 1.5f)
                    .SetDelay(0.1f + i * 0.08f);
            }

        PlayButtonsEntrance(0.4f);
    }

    private void PlayLose()
    {
        winGO?.SetActive(false);

        if (loseLetters != null)
            for (int i = 0; i < loseLetters.Length; i++)
                if (loseLetters[i] != null)
                {
                    Color c = loseLetters[i].color;
                    c.a = 0f;
                    loseLetters[i].color = c;
                }

        loseGO?.SetActive(true);

        if (darkOverlay != null)
        {
            darkOverlay.gameObject.SetActive(true);
            darkOverlay.DOFade(0.65f, 1.0f);
        }

        if (vignetteImage != null)
        {
            vignetteImage.gameObject.SetActive(true);
            vignetteImage.DOFade(0.9f, 1.4f);
        }

        panelRoot.SetActive(true);

        if (panelBackground != null)
        {
            panelBackground.color = new Color(loseBgColor.r, loseBgColor.g, loseBgColor.b, 0f);
            panelBackground.DOFade(loseBgColor.a, 0.6f).SetDelay(0.5f);
        }

        if (loseLetters != null)
            for (int i = 0; i < loseLetters.Length; i++)
            {
                if (loseLetters[i] == null) continue;
                loseLetters[i].DOFade(1f, 0.45f)
                    .SetDelay(0.85f + i * 0.1f);
            }

        PlayButtonsEntrance(1.1f);
    }

    private void PlayButtonsEntrance(float delay)
    {
        const float duration = 0.28f;
        const float overshoot = 1.3f;

        if (retryButton != null)
            ((RectTransform)retryButton.transform)
                .DOAnchorPos(_retryBtnOrigPos, duration)
                .SetEase(Ease.OutBack, overshoot)
                .SetDelay(delay);

        if (lobbyButton != null)
            ((RectTransform)lobbyButton.transform)
                .DOAnchorPos(_lobbyBtnOrigPos, duration)
                .SetEase(Ease.OutBack, overshoot)
                .SetDelay(delay + 0.07f);
    }

    private void GoToLobby()
    {
    }

    private void PlayFlash(Color color, float maxAlpha, float inDuration, float outDuration)
    {
        if (screenFlash == null) return;
        screenFlash.color = new Color(color.r, color.g, color.b, 0f);
        screenFlash.gameObject.SetActive(true);

        DOTween.Sequence()
            .Append(screenFlash.DOFade(maxAlpha, inDuration))
            .Append(screenFlash.DOFade(0f, outDuration))
            .OnComplete(() => screenFlash.gameObject.SetActive(false));
    }

    private void SetAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null) return;
        Color c = graphic.color;
        c.a = alpha;
        graphic.color = c;
    }
}
