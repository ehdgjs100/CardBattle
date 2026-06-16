using System.Collections.Generic;

public class CardField
{
    public const int SlotCount = 3;

    public Owner Owner { get; }
    public CardInstance[] Slots { get; } = new CardInstance[SlotCount];

    private readonly Queue<CardInstance> _waitingCards = new Queue<CardInstance>();

    public int WaitingCount => _waitingCards.Count;

    public CardField(IReadOnlyList<CardDataBase> cardDataList, Owner owner)
    {
        Owner = owner;

        for (int i = 0; i < cardDataList.Count; i++)
        {
            CardInstance instance = new CardInstance(cardDataList[i], owner);

            if (i < SlotCount)
            {
                instance.isDeployed = true;
                instance.slotIndex = i;
                Slots[i] = instance;
            }
            else
            {
                _waitingCards.Enqueue(instance);
            }
        }
    }

    public bool HasActiveTanker()
    {
        for (int i = 0; i < SlotCount; i++)
        {
            if (Slots[i] != null && Slots[i].IsAlive && Slots[i].data.CardType == CardType.Tanker)
                return true;
        }
        return false;
    }

    public bool IsDefeated()
    {
        if (_waitingCards.Count > 0)
            return false;

        for (int i = 0; i < SlotCount; i++)
        {
            if (Slots[i] != null)
                return false;
        }

        return true;
    }

    public bool ProcessDeaths()
    {
        bool changed = false;

        for (int i = 0; i < SlotCount; i++)
        {
            if (Slots[i] != null && !Slots[i].IsAlive)
            {
                Slots[i] = null;
                changed = true;
            }

            if (Slots[i] == null && _waitingCards.Count > 0)
            {
                CardInstance next = _waitingCards.Dequeue();
                next.isDeployed = true;
                next.slotIndex = i;
                Slots[i] = next;
                changed = true;
            }
        }

        return changed;
    }
}
