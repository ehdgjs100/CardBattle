using UnityEngine;
using UnityEngine.EventSystems;

public class BattleSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private int slotIndex;
    [SerializeField] private CardView cardView;
    [SerializeField] private GameObject emptyVisual;
    [SerializeField] private GameObject highlightVisual;

    public int SlotIndex => slotIndex;
    public CardInstance Card { get; private set; }
    public CardView CardView => cardView;

    public void SetCardView(CardView view)
    {
        cardView = view;
    }

    public void Bind(CardInstance card)
    {
        Card = card;

        if (card == null)
        {
            cardView.gameObject.SetActive(false);

            if (emptyVisual != null)
                emptyVisual.SetActive(true);

            return;
        }

        if (emptyVisual != null)
        {
            emptyVisual.SetActive(true);
            emptyVisual.transform.SetAsFirstSibling();
        }

        cardView.gameObject.SetActive(true);
        cardView.Bind(card);
    }

    public void SetHighlight(bool active)
    {
        if (highlightVisual != null)
            highlightVisual.SetActive(active);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick();
    }

    public void OnClick()
    {
        if (Card == null)
            return;

        GameState state = GameManager.Instance.CurrentState;

        if (Card.owner == Owner.Player && state == GameState.PlayerSelectCard)
        {
            if (Card.data.CardType == CardType.Tanker)
            {
                cardView.PlayReject();
                return;
            }
            TurnManager.Instance.SelectAttacker(Card);
        }
        else if (Card.owner == Owner.Enemy && state == GameState.PlayerSelectCard)
        {
            if (TurnManager.Instance.SelectedAttacker != null &&
                GameManager.Instance.EnemyField.HasActiveTanker() &&
                Card.data.CardType != CardType.Tanker)
            {
                UIManager.Instance.PlayTankerBlockReject();
                return;
            }
            TurnManager.Instance.SelectTarget(Card);
        }
    }
}
