using System.Collections;
using TMPro;
using UnityEngine;

public class TurnBanner : MonoBehaviour
{
    [SerializeField] private TMP_Text bannerText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float holdDuration = 0.7f;

    public void Show(string text)
    {
        bannerText.text = text;
        StopAllCoroutines();
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        yield return Fade(0f, 1f);
        yield return new WaitForSeconds(holdDuration);
        yield return Fade(1f, 0f);
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}
