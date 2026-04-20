using UnityEngine;

public class RadioSignalTower : MonoBehaviour
{
    [Header("Точка, из которой как бы идёт радиосигнал")]
    [SerializeField] private Transform signalPoint;

    [Header("Дистанция")]
    [Min(0.1f)]
    [SerializeField] private float minClearDistance = 18f;

    [Min(0.1f)]
    [SerializeField] private float maxSignalDistance = 180f;

    [Tooltip("Кривая качества по дистанции. X = нормализованная дистанция, Y = итоговое качество.")]
    [SerializeField] private AnimationCurve distanceCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Препятствия")]
    [SerializeField] private LayerMask obstacleMask;

    [Range(0f, 1f)]
    [SerializeField] private float occlusionPenalty = 0.35f;

    [SerializeField] private float listenerHeightOffset = 1.6f;
    [SerializeField] private float defaultTowerHeightOffset = 8f;

    /// <summary>
    /// Возвращает мировую позицию сигнала.
    /// Если signalPoint не задан, берём позицию объекта плюс сдвиг вверх.
    /// </summary>
    public Vector3 GetSignalPosition()
    {
        if (signalPoint != null)
            return signalPoint.position;

        return transform.position + Vector3.up * defaultTowerHeightOffset;
    }

    /// <summary>
    /// Качество по дистанции.
    /// 0 = далеко и плохо, 1 = близко и хорошо.
    /// </summary>
    public float GetDistanceQuality(Vector3 listenerPosition)
    {
        float distance = Vector3.Distance(listenerPosition, GetSignalPosition());

        // Обратный InverseLerp:
        // maxSignalDistance -> 0
        // minClearDistance -> 1
        float t = Mathf.InverseLerp(maxSignalDistance, minClearDistance, distance);
        t = Mathf.Clamp01(t);

        return Mathf.Clamp01(distanceCurve.Evaluate(t));
    }

    /// <summary>
    /// Возвращает множитель качества с учетом препятствий.
    /// 1 = препятствий нет
    /// меньше 1 = между игроком и вышкой что-то мешает
    /// </summary>
    public float GetOcclusionMultiplier(Vector3 listenerPosition)
    {
        Vector3 from = listenerPosition + Vector3.up * listenerHeightOffset;
        Vector3 to = GetSignalPosition();

        // Если между игроком и вышкой есть объект из obstacleMask,
        // значит сигнал должен стать хуже.
        if (Physics.Linecast(from, to, obstacleMask))
            return 1f - occlusionPenalty;

        return 1f;
    }

    public float GetDistanceToTower(Vector3 listenerPosition)
    {
        return Vector3.Distance(listenerPosition, GetSignalPosition());
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 p = GetSignalPosition();

        Gizmos.color = new Color(0f, 1f, 0.2f, 0.25f);
        Gizmos.DrawSphere(p, 1.0f);

        Gizmos.color = new Color(0f, 1f, 0.2f, 0.15f);
        Gizmos.DrawWireSphere(p, minClearDistance);

        Gizmos.color = new Color(1f, 0.8f, 0f, 0.12f);
        Gizmos.DrawWireSphere(p, maxSignalDistance);
    }

}