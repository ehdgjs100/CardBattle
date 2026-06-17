using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardEditCardView : MonoBehaviour
{
    [SerializeField] private Image illustrationImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private Image typeIconImage;
    [SerializeField] private Image innerTypeIconImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Image rarityBorderImage;
    [SerializeField] private GameObject inDeckOverlay;

    [SerializeField] private CardTypeIconEntry[] innerTypeIcons;

    private Button _button;
    private int _ownedIndex;
    private System.Action<int> _onClick;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    public void Bind(OwnedCardEntry entry, int ownedIndex, bool isInDeck, System.Action<int> onClick)
    {
        _ownedIndex = ownedIndex;
        _onClick = onClick;

        _button?.onClick.RemoveAllListeners();
        _button?.onClick.AddListener(() => _onClick?.Invoke(_ownedIndex));

        CardVisualConfig visual = entry.cardData.visual;
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
            Sprite inner = GetInnerTypeIcon(entry.cardData.CardType);
            innerTypeIconImage.sprite = inner;
            innerTypeIconImage.gameObject.SetActive(inner != null);
        }

        string displayName = entry.upgradeLevel > 0
            ? entry.cardData.cardName + "+"
            : entry.cardData.cardName;
        int displayHP = entry.cardData.maxHP + entry.cardData.hpPerUpgrade * entry.upgradeLevel;
        if (cardNameText != null)
        {
            cardNameText.SetText(displayName);
            cardNameText.color = entry.cardData.GetRarityColor();
        }
        hpText?.SetText(displayHP.ToString());

        if (rarityBorderImage != null)
            rarityBorderImage.color = entry.cardData.GetRarityColor();

        if (inDeckOverlay != null)
            inDeckOverlay.SetActive(isInDeck);
    }

    private Sprite GetInnerTypeIcon(CardType type)
    {
        for (int i = 0; i < innerTypeIcons.Length; i++)
            if (innerTypeIcons[i].type == type)
                return innerTypeIcons[i].sprite;
        return null;
    }
}
