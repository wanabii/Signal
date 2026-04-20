using UnityEngine;

public class RadioReceiver : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Transform listener;               // Обычно камера игрока
    [SerializeField] private RadioSignalTower tower;          // Вышка
    [SerializeField] private RadioVoiceBank voiceBank;        // Набор голосовых фрагментов

    [Header("Audio Sources")]
    [SerializeField] private AudioSource staticSource;        // Постоянный шум
    [SerializeField] private AudioSource signalSource;        // Несущая / бип / радиомаяк
    [SerializeField] private AudioSource voiceSource;         // Голосовые куски

    [Header("Фильтры")]
    [SerializeField] private AudioLowPassFilter signalLowPass;
    [SerializeField] private AudioDistortionFilter signalDistortion;

    [Header("Направление взгляда")]
    [SerializeField] private bool useFacing = true;

    [Range(0f, 1f)]
    [SerializeField] private float facingInfluence = 0.35f;

    [Header("Сглаживание")]
    [Tooltip("Скорость, с которой текущее качество сигнала догоняет целевое. Чем выше, тем резче смены.")]
    [SerializeField] private float qualityResponseSpeed = 2.5f;

    [Header("Громкость шума")]
    [SerializeField] private float staticVolumeWorst = 1f;    // Когда сигнал плохой
    [SerializeField] private float staticVolumeBest = 0.15f;  // Когда сигнал хороший

    [Header("Громкость сигнала")]
    [SerializeField] private float signalVolumeWorst = 0.03f;
    [SerializeField] private float signalVolumeBest = 1f;

    [Header("Фильтры сигнала")]
    [SerializeField] private float lowPassWorst = 900f;
    [SerializeField] private float lowPassBest = 7000f;

    [SerializeField] private float distortionWorst = 0.8f;
    [SerializeField] private float distortionBest = 0.02f;

    [Header("Обрывы сигнала")]
    [Tooltip("Как часто могут происходить обрывы, когда сигнал очень плохой")]
    [SerializeField] private float dropoutChanceWorst = 0.25f;

    [Tooltip("Как часто могут происходить обрывы, когда сигнал почти хороший")]
    [SerializeField] private float dropoutChanceBest = 0.01f;

    [SerializeField] private Vector2 dropoutDurationRange = new Vector2(0.03f, 0.15f);

    [Tooltip("Насколько сильно проседает сигнал во время обрыва")]
    [Range(0f, 1f)]
    [SerializeField] private float dropoutOutputMultiplier = 0.15f;

    [Header("Голосовые фрагменты")]
    [SerializeField] private bool useVoiceFragments = true;

    [Tooltip("Минимальное качество сигнала, после которого рация вообще начинает иногда ловить речь")]
    [Range(0f, 1f)]
    [SerializeField] private float minQualityForVoices = 0.18f;

    [SerializeField] private Vector2 voiceIntervalRange = new Vector2(4f, 9f);

    [Tooltip("Шанс, что в подходящий момент вообще будет проигран голосовой кусок")]
    [Range(0f, 1f)]
    [SerializeField] private float voicePlayChanceWorst = 0.15f;

    [Range(0f, 1f)]
    [SerializeField] private float voicePlayChanceBest = 0.65f;

    [SerializeField] private float voiceVolumeWorst = 0.2f;
    [SerializeField] private float voiceVolumeBest = 1f;

    [SerializeField] private Vector2 voicePitchRange = new Vector2(0.96f, 1.04f);

    [Header("Отладка")]
    [SerializeField] private bool radioEnabled = true;

    [SerializeField, Range(0f, 1f)]
    private float currentQuality;

    [SerializeField, Range(0f, 1f)]
    private float targetQuality;

    private bool isDropout;
    private float dropoutTimer;
    private float nextVoiceTime;

    private void Start()
    {
        PrepareLoopSource(staticSource);
        PrepareLoopSource(signalSource);

        // Голос не должен быть loop.
        if (voiceSource != null)
            voiceSource.loop = false;

        ScheduleNextVoice();
    }

    private void Update()
    {
        if (!radioEnabled)
        {
            ApplyMutedState();
            return;
        }

        if (listener == null || tower == null)
        {
            ApplyMutedState();
            return;
        }

        EnsureLoopPlaying(staticSource);
        EnsureLoopPlaying(signalSource);

        // Считаем к какой "чистоте" сигнала нужно прийти.
        targetQuality = CalculateTargetQuality();

        // Плавно дотягиваем текущее качество к целевому,
        // чтобы звук не дёргался как нервный холодильник.
        currentQuality = Mathf.MoveTowards(
            currentQuality,
            targetQuality,
            qualityResponseSpeed * Time.deltaTime
        );

        UpdateDropout(currentQuality);

        // Во время обрыва итоговый слышимый сигнал режем.
        float audibleQuality = isDropout
            ? currentQuality * dropoutOutputMultiplier
            : currentQuality;

        audibleQuality = Mathf.Clamp01(audibleQuality);

        ApplyAudioState(audibleQuality);
        UpdateVoicePlayback(audibleQuality);
    }

    /// <summary>
    /// Считает итоговое качество сигнала.
    /// Берём качество от расстояния,
    /// потом правим его взглядом,
    /// потом правим препятствиями.
    /// </summary>
    private float CalculateTargetQuality()
    {
        float quality = tower.GetDistanceQuality(listener.position);

        // Дополнительный бонус/штраф за направление взгляда.
        // Если игрок смотрит ближе к башне, сигнал лучше.
        if (useFacing)
        {
            Vector3 toTower = tower.GetSignalPosition() - listener.position;

            // Убираем вертикальную составляющую, чтобы смотреть "по горизонту".
            Vector3 forwardFlat = Vector3.ProjectOnPlane(listener.forward, Vector3.up).normalized;
            Vector3 dirFlat = Vector3.ProjectOnPlane(toTower, Vector3.up).normalized;

            if (forwardFlat.sqrMagnitude > 0.0001f && dirFlat.sqrMagnitude > 0.0001f)
            {
                // dot:
                // -1 = смотрим в противоположную сторону
                //  0 = боком
                //  1 = точно на вышку
                float dot = Vector3.Dot(forwardFlat, dirFlat);

                // Приводим диапазон -1..1 к 0..1
                float facing01 = Mathf.InverseLerp(-1f, 1f, dot);

                // Множитель:
                // если смотрим плохо, качество немного режется
                // если смотрим хорошо, остаётся полным
                float facingMultiplier = Mathf.Lerp(1f - facingInfluence, 1f, facing01);
                quality *= facingMultiplier;
            }
        }

        // Учитываем препятствия.
        quality *= tower.GetOcclusionMultiplier(listener.position);

        return Mathf.Clamp01(quality);
    }

    /// <summary>
    /// Обновляет состояние кратких обрывов сигнала.
    /// Далеко от башни они происходят чаще.
    /// </summary>
    private void UpdateDropout(float quality)
    {
        if (dropoutTimer > 0f)
        {
            dropoutTimer -= Time.deltaTime;

            if (dropoutTimer <= 0f)
                isDropout = false;

            return;
        }

        float chancePerSecond = Mathf.Lerp(dropoutChanceWorst, dropoutChanceBest, quality);

        if (Random.value < chancePerSecond * Time.deltaTime)
        {
            isDropout = true;
            dropoutTimer = Random.Range(dropoutDurationRange.x, dropoutDurationRange.y);
        }
    }

    /// <summary>
    /// Применяет качество сигнала к громкости и фильтрам.
    /// </summary>
    private void ApplyAudioState(float quality)
    {
        if (staticSource != null)
            staticSource.volume = Mathf.Lerp(staticVolumeWorst, staticVolumeBest, quality);

        if (signalSource != null)
            signalSource.volume = Mathf.Lerp(signalVolumeWorst, signalVolumeBest, quality);

        if (signalLowPass != null)
            signalLowPass.cutoffFrequency = Mathf.Lerp(lowPassWorst, lowPassBest, quality);

        if (signalDistortion != null)
            signalDistortion.distortionLevel = Mathf.Lerp(distortionWorst, distortionBest, quality);
    }

    /// <summary>
    /// Иногда проигрывает голосовые куски.
    /// Чем лучше сигнал, тем выше шанс и громкость.
    /// </summary>
    private void UpdateVoicePlayback(float quality)
    {
        if (!useVoiceFragments)
            return;

        if (voiceSource == null || voiceBank == null)
            return;

        // Во время обрыва не даём речи играть, иначе будет странно.
        if (isDropout)
            return;

        // Если сигнал совсем плохой, голос пока не ловим.
        if (quality < minQualityForVoices)
            return;

        // Пока не настал следующий момент проверки, ничего не делаем.
        if (Time.time < nextVoiceTime)
            return;

        // Не запускаем новый фрагмент, если старый ещё играет.
        if (voiceSource.isPlaying)
            return;

        float playChance = Mathf.Lerp(voicePlayChanceWorst, voicePlayChanceBest, quality);

        if (Random.value <= playChance)
        {
            AudioClip clip = voiceBank.GetRandomClip(quality);

            if (clip != null)
            {
                voiceSource.clip = clip;
                voiceSource.volume = Mathf.Lerp(voiceVolumeWorst, voiceVolumeBest, quality);
                voiceSource.pitch = Random.Range(voicePitchRange.x, voicePitchRange.y);
                voiceSource.Play();
            }
        }

        ScheduleNextVoice();
    }

    private void ScheduleNextVoice()
    {
        nextVoiceTime = Time.time + Random.Range(voiceIntervalRange.x, voiceIntervalRange.y);
    }

    private void PrepareLoopSource(AudioSource source)
    {
        if (source == null)
            return;

        source.loop = true;
        source.playOnAwake = false;

        EnsureLoopPlaying(source);
    }

    private void EnsureLoopPlaying(AudioSource source)
    {
        if (source == null)
            return;

        if (source.clip != null && !source.isPlaying)
            source.Play();
    }

    private void ApplyMutedState()
    {
        if (staticSource != null)
            staticSource.volume = 0f;

        if (signalSource != null)
            signalSource.volume = 0f;

        if (voiceSource != null && voiceSource.isPlaying)
            voiceSource.Stop();
    }

    /// <summary>
    /// Можно вызывать из другого скрипта, если игрок включает/выключает рацию.
    /// </summary>
    public void SetRadioEnabled(bool value)
    {
        radioEnabled = value;

        if (!radioEnabled)
            ApplyMutedState();
    }

    /// <summary>
    /// Просто для UI/дебага.
    /// </summary>
    public float GetCurrentQuality()
    {
        return currentQuality;
    }
}