using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button draw1Button;
    [SerializeField] private Button draw10Button;
    [SerializeField] private Button closeButton;

    [Header("Chest")]
    [SerializeField] private Image chest1Image;
    [SerializeField] private Sprite chest1Before;
    [SerializeField] private Sprite chest1After;
    [SerializeField] private Image chest10Image;
    [SerializeField] private Sprite chest10Before;
    [SerializeField] private Sprite chest10After;

    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private CardUIView cardUIPrefab;
    [SerializeField] private Button confirmButton;

    private readonly List<CardDataBase> _pendingCards = new List<CardDataBase>();

    private void Awake()
    {
        draw1Button?.onClick.AddListener(() => Draw(1));
        draw10Button?.onClick.AddListener(() => Draw(9));
        closeButton?.onClick.AddListener(OnClose);
        confirmButton?.onClick.AddListener(OnConfirm);

        resultPanel?.SetActive(false);
    }

    private void OnEnable()
    {
        ResetChests();
    }

    private void Draw(int count)
    {
        if (CardManager.Instance == null) return;

        _pendingCards.Clear();
        for (int i = 0; i < count; i++)
        {
            CardDataBase card = CardManager.Instance.DrawRandom();
            if (card != null)
                _pendingCards.Add(card);
        }

        UpdateChestSprite(count);
        ShowResults();
    }

    private void UpdateChestSprite(int count)
    {
        if (count == 1 && chest1Image != null && chest1After != null)
            chest1Image.sprite = chest1After;
        else if (count == 9 && chest10Image != null && chest10After != null)
            chest10Image.sprite = chest10After;
    }

    private void ResetChests()
    {
        if (chest1Image != null && chest1Before != null)
            chest1Image.sprite = chest1Before;
        if (chest10Image != null && chest10Before != null)
            chest10Image.sprite = chest10Before;
    }

    private void ShowResults()
    {
        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        resultPanel.SetActive(true);

        for (int i = 0; i < _pendingCards.Count; i++)
        {
            CardUIView item = Instantiate(cardUIPrefab, cardContainer);
            item.Bind(_pendingCards[i]);
            item.PlayPopIn(i * 0.07f);
        }
    }

    private void OnConfirm()
    {
        foreach (CardDataBase card in _pendingCards)
            CardManager.Instance?.AddToCollection(card);

        _pendingCards.Clear();
        resultPanel.SetActive(false);
        ResetChests();
    }

    private void OnClose()
    {
        _pendingCards.Clear();
        resultPanel.SetActive(false);
        ResetChests();
        GetComponent<LobbyPanel>()?.Hide();
    }
}
