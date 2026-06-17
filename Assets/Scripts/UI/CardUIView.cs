using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUIView : MonoBehaviour
{
    [SerializeField] private Image illustrationImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private Image frameOutline;
    [SerializeField] private Image typeIconImage;
    [SerializeField] private Image innerTypeIconImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text cardDescText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Image rarityBorderImage;

    [SerializeField] private CardTypeIconEntry[] innerTypeIcons;


    private void Awake()
    {
        if (frameOutline != null)
            frameOutline.transform
                .DOLocalRotate(new Vector3(0f, 0f, -360f), 10f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear);
    }

    public void Bind(CardDataBase data)
    {
        CardVisualConfig visual = data.visual;
        if (visual != null)
        {
            if (illustrationImage != null)
                illustrationImage.sprite = visual.illustration;

            if (frameImage != null)
                frameImage.color = visual.frameColor;

            if (typeIconImage != null)
            {
                typeIconImage.sprite = visual.typeIcon;
                typeIconImage.gameObject.SetActive(visual.typeIcon != null);
            }
        }

        if (innerTypeIconImage != null)
        {
            Sprite inner = GetInnerTypeIcon(data.CardType);
            innerTypeIconImage.sprite = inner;
            innerTypeIconImage.gameObject.SetActive(inner != null);
        }

        cardNameText?.SetText(data.cardName);
        cardDescText?.SetText(data.feature);
        hpText?.SetText(data.maxHP.ToString());

        if (rarityBorderImage != null)
            rarityBorderImage.color = data.GetRarityColor();
    }

    public void PlayPopIn(float delay)
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetDelay(delay);
    }

    private Sprite GetInnerTypeIcon(CardType type)
    {
        for (int i = 0; i < innerTypeIcons.Length; i++)
            if (innerTypeIcons[i].type == type)
                return innerTypeIcons[i].sprite;
        return null;
    }
}
