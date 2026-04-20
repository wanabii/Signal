using UnityEngine;

public class ChangeTransformAndPlaySoundOnTrigger : MonoBehaviour
{
    [Header("Object To Change")]
    [SerializeField] private Transform targetToChange;

    [Header("New Local Transform")]
    [SerializeField] private Vector3 newLocalPosition;
    [SerializeField] private Vector3 newLocalRotation;
    [SerializeField] private Vector3 newLocalScale = Vector3.one;

    [Header("What To Change")]
    [SerializeField] private bool changePosition = true;
    [SerializeField] private bool changeRotation = false;
    [SerializeField] private bool changeScale = false;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip soundClip;

    [Header("Settings")]
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool _triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered && triggerOnlyOnce)
            return;

        if (targetToChange == null)
        {
            Debug.LogWarning("targetToChange не назначен", this);
            return;
        }

        if (changePosition)
            targetToChange.localPosition = newLocalPosition;

        if (changeRotation)
            targetToChange.localRotation = Quaternion.Euler(newLocalRotation);

        if (changeScale)
            targetToChange.localScale = newLocalScale;

        if (audioSource != null && soundClip != null)
            audioSource.PlayOneShot(soundClip);

        _triggered = true;
    }
}