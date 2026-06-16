using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class CardDeathAnimator : MonoBehaviour
{
    [SerializeField] private Shader sliceShader;

    private RectTransform _rect;
    private static readonly int CutDirId = Shader.PropertyToID("_CutDir");

    private void Awake()
    {
        _rect = (RectTransform)transform;
    }

    public void Play(Action onComplete)
    {
        StartCoroutine(DeathCoroutine(onComplete));
    }

    private IEnumerator DeathCoroutine(Action onComplete)
    {
        yield return new WaitForEndOfFrame();

        Canvas rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;

        Texture2D snap = CaptureCard(cam);
        GetPanelTransform(rootCanvas, cam, out Vector2 localCenter, out Vector2 panelSize);

        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;

        RectTransform upper = CreatePanel((RectTransform)rootCanvas.transform, snap, 1f, localCenter, panelSize);
        RectTransform lower = CreatePanel((RectTransform)rootCanvas.transform, snap, -1f, localCenter, panelSize);

        const float dist = 90f;
        const float dur = 0.55f;

        Sequence seq = DOTween.Sequence();
        seq.Join(upper.DOAnchorPos(localCenter + new Vector2(-dist, dist), dur).SetEase(Ease.OutQuad));
        seq.Join(upper.DOLocalRotate(new Vector3(0f, 0f, -15f), dur).SetEase(Ease.OutQuad));
        seq.Join(upper.GetComponent<CanvasGroup>().DOFade(0f, dur * 0.7f).SetDelay(dur * 0.3f));
        seq.Join(lower.DOAnchorPos(localCenter + new Vector2(dist, -dist), dur).SetEase(Ease.OutQuad));
        seq.Join(lower.DOLocalRotate(new Vector3(0f, 0f, 15f), dur).SetEase(Ease.OutQuad));
        seq.Join(lower.GetComponent<CanvasGroup>().DOFade(0f, dur * 0.7f).SetDelay(dur * 0.3f));
        seq.OnComplete(() =>
        {
            Destroy(upper.gameObject);
            Destroy(lower.gameObject);
            Destroy(snap);
            onComplete?.Invoke();
        });
    }

    private Texture2D CaptureCard(Camera cam)
    {
        Vector3[] corners = new Vector3[4];
        _rect.GetWorldCorners(corners);

        Vector2 bl = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
        Vector2 tr = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);

        int w = Mathf.Max(1, Mathf.RoundToInt(tr.x - bl.x));
        int h = Mathf.Max(1, Mathf.RoundToInt(tr.y - bl.y));

        Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(bl.x, bl.y, w, h), 0, 0);
        tex.Apply();
        return tex;
    }

    private void GetPanelTransform(Canvas rootCanvas, Camera cam, out Vector2 localCenter, out Vector2 panelSize)
    {
        Vector3[] corners = new Vector3[4];
        _rect.GetWorldCorners(corners);

        Vector2 screenBL = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
        Vector2 screenTR = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);
        Vector2 screenCenter = (screenBL + screenTR) * 0.5f;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)rootCanvas.transform, screenCenter, cam, out localCenter);

        panelSize = (screenTR - screenBL) / rootCanvas.scaleFactor;
    }

    private RectTransform CreatePanel(RectTransform root, Texture2D tex, float cutDir, Vector2 localCenter, Vector2 size)
    {
        GameObject go = new GameObject(cutDir > 0f ? "DeathUpper" : "DeathLower");
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.SetParent(root, false);
        rt.SetAsLastSibling();

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = localCenter;
        rt.sizeDelta = size;

        RawImage img = go.AddComponent<RawImage>();
        img.texture = tex;

        if (sliceShader != null)
        {
            Material mat = new Material(sliceShader);
            mat.SetFloat(CutDirId, cutDir);
            img.material = mat;
        }

        go.AddComponent<CanvasGroup>();
        return rt;
    }
}
