using UnityEngine;
using UnityEngine.UI;

public class CardDetailPanel : MonoBehaviour
{
    [SerializeField] private CardUIView cardUIView;
    [SerializeField] private Button equipButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button closeButton;

    private OwnedCardEntry _entry;
    private int _ownedIndex;
    private CardEditPanel _cardEditPanel;

    private void Awake()
    {
        closeButton?.onClick.AddListener(Close);
        equipButton?.onClick.AddListener(OnEquip);
        upgradeButton?.onClick.AddListener(OnUpgrade);
    }

    public void Show(OwnedCardEntry entry, int ownedIndex, CardEditPanel cardEditPanel)
    {
        _entry = entry;
        _ownedIndex = ownedIndex;
        _cardEditPanel = cardEditPanel;
        gameObject.SetActive(true);
        Refresh();
    }

    private void Refresh()
    {
        cardUIView?.Bind(_entry);

        bool isInDeck = _cardEditPanel.IsInDeck(_entry);
        bool deckFull = CardManager.Instance.PlayerDeck.Count >= CardManager.MaxDeckSize;
        bool maxUpgraded = _entry.upgradeLevel >= CardManager.MaxUpgradeLevel;

        if (equipButton != null) equipButton.interactable = !isInDeck && !deckFull;
        if (upgradeButton != null) upgradeButton.interactable = !maxUpgraded;
    }

    private void OnEquip()
    {
        _cardEditPanel.AddCardToDeck(_ownedIndex);
        Close();
    }

    private void OnUpgrade()
    {
        if (CardManager.Instance.UpgradeCard(_ownedIndex))
        {
            _cardEditPanel.RefreshCardView(_ownedIndex);
            Refresh();
        }
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }
}
