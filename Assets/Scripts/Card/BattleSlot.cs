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
            emptyVisual.SetActive(false);

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
            TurnManager.Instance.SelectAttacker(Card);
        else if (Card.owner == Owner.Enemy && state == GameState.PlayerSelectTarget)
            TurnManager.Instance.SelectTarget(Card);
    }
}
