using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepByPosition : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private float movementThreshold = 0.01f;
    [SerializeField] private bool checkOnlyXZ = false;

    private AudioSource audioSource;
    private Vector3 lastPosition;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        lastPosition = transform.position;

        // Для шагов обычно нужен зацикленный звук
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    private void Update()
    {
        Vector3 currentPosition = transform.position;
        Vector3 delta = currentPosition - lastPosition;

        float distanceMoved;

        if (checkOnlyXZ)
        {
            delta.y = 0f;
            distanceMoved = delta.magnitude;
        }
        else
        {
            distanceMoved = delta.magnitude;
        }

        bool isMoving = distanceMoved > movementThreshold;

        if (isMoving)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }

        lastPosition = currentPosition;
    }
}