using UnityEngine;
using UnityEngine.UI;

public class ActionPanel : MonoBehaviour
{
    [SerializeField] private Button attackButton;

    private void Awake()
    {
        attackButton.onClick.AddListener(OnAttackButtonClicked);
    }

    public void SetInteractable(bool interactable)
    {
        attackButton.interactable = interactable;
    }

    private void OnAttackButtonClicked()
    {
        TurnManager.Instance.ConfirmAttack();
    }
}
