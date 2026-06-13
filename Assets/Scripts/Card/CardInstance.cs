using UnityEngine;

public class CardInstance
{
    public CardDataBase data;
    public CardEffect effect;
    public int currentHP;
    public bool isDeployed;
    public int slotIndex;
    public Owner owner;

    public bool IsAlive => currentHP > 0;

    public CardInstance(CardDataBase data, Owner owner)
    {
        this.data = data;
        this.owner = owner;
        effect = data.CreateEffect();
        currentHP = data.maxHP;
        isDeployed = false;
        slotIndex = -1;
    }

    public void TakeDamage(int amount)
    {
        currentHP = Mathf.Max(0, currentHP - amount);
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(data.maxHP, currentHP + amount);
    }
}
