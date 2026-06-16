using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Image panelBackground;
    [SerializeField] private Image screenFlash;
    [SerializeField] private Image darkOverlay;
    [SerializeField] private Image vignetteImage;

    private static readonly Color WinBgColor = new Color(1f, 0.82f, 0.08f, 0.93f);
    private static readonly Color LoseBgColor = new Color(0.08f, 0.08f, 0.1f, 0.9f);

    private void Awake()
    {
        panelRoot.SetActive(false);
        SetAlpha(screenFlash, 0f);
        SetAlpha(darkOverlay, 0f);
        InitVignette();
    }

    private void InitVignette()
    {
        if (vignetteImage == null) return;

        const int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center;
                float alpha = Mathf.Clamp01((dist - 0.3f) / 0.7f);
                alpha = alpha * alpha;
                tex.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
            }
        }
        tex.Apply();

        vignetteImage.sprite = Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f);
        SetAlpha(vignetteImage, 0f);
    }

    public void Show(GameResult result)
    {
        if (result == GameResult.Win)
            PlayWin();
        else
            PlayLose();
    }

    private void PlayWin()
    {
        PlayFlash(Color.white, 0.9f, 0.08f, 0.45f);

        panelRoot.SetActive(true);

        if (panelBackground != null)
        {
            panelBackground.color = new Color(WinBgColor.r, WinBgColor.g, WinBgColor.b, 0f);
            panelBackground.DOFade(WinBgColor.a, 0.3f).SetDelay(0.1f);
        }

        resultText.text = "Victory";
        SetAlpha(resultText, 0f);
        resultText.transform.localScale = Vector3.one * 2.8f;

        DOTween.Sequence().SetDelay(0.08f)
            .Append(resultText.DOFade(1f, 0.12f))
            .Join(resultText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack, 1.6f));
    }

    private void PlayLose()
    {
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
            panelBackground.color = new Color(LoseBgColor.r, LoseBgColor.g, LoseBgColor.b, 0f);
            panelBackground.DOFade(LoseBgColor.a, 0.6f).SetDelay(0.5f);
        }

        resultText.text = "Defeat";
        SetAlpha(resultText, 0f);
        resultText.transform.localScale = Vector3.one;
        resultText.DOFade(1f, 0.7f).SetDelay(0.9f);
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
