using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepByPosition : MonoBehaviour
{
    [Serializable]
    private class SurfaceSoundSetup
    {
        public FootstepSurfaceType surfaceType = FootstepSurfaceType.Grass;
        public List<AudioClip> sounds = new();
    }

    [Header("Настройки движения")]
    [SerializeField] private float movementThreshold = 0.01f;
    [SerializeField] private bool checkOnlyXZ = false;
    [SerializeField] private float footstepDelay = 0.5f;

    [Header("Проверка поверхности")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckDistance = 1.5f;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Звуки")]
    [SerializeField] private List<AudioClip> defaultFootstepSounds = new();
    [SerializeField] private List<SurfaceSoundSetup> surfaceSounds = new();

    [Header("Дополнительно")]
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    private float footstepDelayTimer;
    private AudioSource audioSource;
    private Vector3 lastPosition;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        lastPosition = transform.position;
    }

    private void Update()
    {
        Vector3 currentPosition = transform.position;
        Vector3 delta = currentPosition - lastPosition;

        if (checkOnlyXZ)
            delta.y = 0f;

        float distanceMoved = delta.magnitude;
        bool isMoving = distanceMoved > movementThreshold;

        if (isMoving)
        {
            footstepDelayTimer -= Time.deltaTime;

            if (footstepDelayTimer <= 0f)
            {
                PlayFootstep();
                footstepDelayTimer = footstepDelay;
            }
        }
        else
        {
            footstepDelayTimer = 0f;
        }

        lastPosition = currentPosition;
    }

    private void PlayFootstep()
    {
        List<AudioClip> sounds = GetCurrentSurfaceSounds();

        if (sounds == null || sounds.Count == 0)
            return;

        AudioClip clip = sounds[UnityEngine.Random.Range(0, sounds.Count)];
        if (clip == null)
            return;

        audioSource.pitch = UnityEngine.Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(clip, volume);
    }

    private List<AudioClip> GetCurrentSurfaceSounds()
    {
        Vector3 origin = groundCheckPoint != null ? groundCheckPoint.position : transform.position;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.TryGetComponent(out FootstepSurface surface))
            {
                List<AudioClip> clips = FindSurfaceSounds(surface.SurfaceType);
                if (clips != null && clips.Count > 0)
                    return clips;
            }
        }

        return defaultFootstepSounds;
    }

    private List<AudioClip> FindSurfaceSounds(FootstepSurfaceType surfaceType)
    {
        for (int i = 0; i < surfaceSounds.Count; i++)
        {
            if (surfaceSounds[i].surfaceType == surfaceType)
                return surfaceSounds[i].sounds;
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = groundCheckPoint != null ? groundCheckPoint.position : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
        Gizmos.DrawWireSphere(origin + Vector3.down * groundCheckDistance, 0.05f);
    }
}