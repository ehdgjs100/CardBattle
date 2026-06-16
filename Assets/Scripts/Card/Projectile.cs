using DG.Tweening;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;

    public void Launch(Vector3 from, Vector3 to, System.Action onArrive)
    {
        transform.position = from;

        Vector3 dir = (to - from).normalized;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

        float duration = Vector3.Distance(from, to) / speed;
        transform.DOMove(to, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                onArrive?.Invoke();
                Destroy(gameObject);
            });
    }
}
