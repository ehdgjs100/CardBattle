using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CardEditPanel : MonoBehaviour
{
    [Header("Deck Slots")]
    [SerializeField] private DeckSlotView[] deckSlots;

    [Header("Collection")]
    [SerializeField] private Transform collectionContainer;
    [SerializeField] private CardEditCardView cardEditPrefab;

    [Header("Detail Panel")]
    [SerializeField] private CardDetailPanel cardDetailPanel;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;

    private readonly Dictionary<OwnedCardEntry, CardEditCardView> _cardViews = new();

    private void Awake()
    {
        closeButton?.onClick.AddListener(OnClose);
    }

    private void OnEnable()
    {
        BuildViews();
        LayoutCards();
    }

    private void OnDisable()
    {
        foreach (CardEditCardView v in _cardViews.Values)
            Destroy(v.gameObject);
        _cardViews.Clear();
    }

    private void BuildViews()
    {
        if (CardManager.Instance == null) return;
        IReadOnlyList<OwnedCardEntry> owned = CardManager.Instance.OwnedCards;
        for (int i = 0; i < owned.Count; i++)
        {
            OwnedCardEntry entry = owned[i];
            if (_cardViews.ContainsKey(entry)) continue;
            _cardViews[entry] = Instantiate(cardEditPrefab, collectionContainer);
        }
    }

    private void LayoutCards()
    {
        if (CardManager.Instance == null) return;
        IReadOnlyList<OwnedCardEntry> owned = CardManager.Instance.OwnedCards;
        IReadOnlyList<OwnedCardEntry> deck = CardManager.Instance.PlayerDeck;

        for (int i = 0; i < owned.Count; i++)
        {
            OwnedCardEntry entry = owned[i];
            if (!_cardViews.TryGetValue(entry, out CardEditCardView view)) continue;

            int deckIdx = FindDeckIndex(entry, deck);
            if (deckIdx >= 0)
            {
                view.transform.SetParent(deckSlots[deckIdx].SlotRect, false);
                ((RectTransform)view.transform).anchoredPosition = Vector2.zero;
                view.Bind(entry, i, true, OnCardClick);
            }
            else
            {
                view.transform.SetParent(collectionContainer, false);
                view.Bind(entry, i, false, OnCardClick);
            }
        }
    }

    private void OnCardClick(int ownedIndex)
    {
        if (CardManager.Instance == null) return;
        IReadOnlyList<OwnedCardEntry> owned = CardManager.Instance.OwnedCards;
        IReadOnlyList<OwnedCardEntry> deck = CardManager.Instance.PlayerDeck;
        OwnedCardEntry entry = owned[ownedIndex];
        if (!_cardViews.TryGetValue(entry, out CardEditCardView view)) return;

        int deckIdx = FindDeckIndex(entry, deck);
        if (deckIdx >= 0)
        {
            CardManager.Instance.RemoveFromDeckByEntry(entry);
            ReturnToCollection(view, ownedIndex, entry);
        }
        else
        {
            cardDetailPanel?.Show(entry, ownedIndex, this);
        }
    }

    public bool IsInDeck(OwnedCardEntry entry) =>
        FindDeckIndex(entry, CardManager.Instance.PlayerDeck) >= 0;

    public void AddCardToDeck(int ownedIndex)
    {
        if (CardManager.Instance == null) return;
        IReadOnlyList<OwnedCardEntry> deck = CardManager.Instance.PlayerDeck;
        IReadOnlyList<OwnedCardEntry> owned = CardManager.Instance.OwnedCards;
        if (ownedIndex < 0 || ownedIndex >= owned.Count) return;
        OwnedCardEntry entry = owned[ownedIndex];
        if (!_cardViews.TryGetValue(entry, out CardEditCardView view)) return;
        if (deck.Count >= CardManager.MaxDeckSize) return;
        int nextSlot = FindFirstEmptySlot();
        if (nextSlot < 0) return;
        CardManager.Instance.AddToDeck(ownedIndex);
        AnimateToSlot(view, ownedIndex, entry, nextSlot);
    }

    private int FindFirstEmptySlot()
    {
        for (int i = 0; i < deckSlots.Length; i++)
            if (deckSlots[i].SlotRect.childCount == 0)
                return i;
        return -1;
    }

    private void AnimateToSlot(CardEditCardView view, int ownedIndex, OwnedCardEntry entry, int slotIndex)
    {
        RectTransform slotRect = deckSlots[slotIndex].SlotRect;
        RectTransform cardRect = (RectTransform)view.transform;

        DOTween.Kill(cardRect);

        Vector3 worldPos = cardRect.position;

        view.transform.SetParent(slotRect, false);
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.position = worldPos;

        view.GetComponent<Button>().interactable = false;

        cardRect.DOAnchorPos(Vector2.zero, 0.3f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                view.GetComponent<Button>().interactable = true;
                view.Bind(entry, ownedIndex, true, OnCardClick);
            });
    }

    private void ReturnToCollection(CardEditCardView view, int ownedIndex, OwnedCardEntry entry)
    {
        DOTween.Kill((RectTransform)view.transform);
        view.transform.SetParent(collectionContainer, false);
        view.Bind(entry, ownedIndex, false, OnCardClick);
    }

    private static int FindDeckIndex(OwnedCardEntry entry, IReadOnlyList<OwnedCardEntry> deck)
    {
        for (int i = 0; i < deck.Count; i++)
            if (deck[i] == entry) return i;
        return -1;
    }

    private void OnClose()
    {
        GetComponent<LobbyPanel>()?.Hide();
    }
}
