using UnityEngine;

[CreateAssetMenu(fileName = "RadioVoiceBank", menuName = "Game/Radio/Voice Bank")]
public class RadioVoiceBank : ScriptableObject
{
    [Header("Клипы для очень плохого сигнала")]
    [SerializeField] private AudioClip[] weakClips;

    [Header("Клипы для среднего сигнала")]
    [SerializeField] private AudioClip[] mediumClips;

    [Header("Клипы для хорошего сигнала")]
    [SerializeField] private AudioClip[] strongClips;

    /// <summary>
    /// Возвращает случайный клип в зависимости от качества сигнала.
    /// 0 = почти ничего не ловится
    /// 1 = сигнал почти чистый
    /// </summary>
    public AudioClip GetRandomClip(float quality)
    {
        AudioClip[] pool;

        if (quality < 0.33f)
            pool = weakClips;
        else if (quality < 0.66f)
            pool = mediumClips;
        else
            pool = strongClips;

        if (pool == null || pool.Length == 0)
            return null;

        return pool[Random.Range(0, pool.Length)];
    }
}