using System.Collections;
using UnityEngine;

public class PickupItem : MonoBehaviour
{

    [Header("Position in hand")]
    [SerializeField] private Vector3 holdLocalPosition = new Vector3(0.3f, -0.25f, 0.6f);
    [SerializeField] private Vector3 holdLocalRotation = Vector3.zero;
    [SerializeField] private Vector3 holdLocalScale = Vector3.one;

    [Header("Object to show when picked up")]
    [SerializeField] private GameObject objectToShow;

    [Header("Another object local transform to change")]
    [SerializeField] private Transform targetToChange;
    [SerializeField] private Vector3 newLocalPosition;
    [SerializeField] private Vector3 newLocalRotation;
    [SerializeField] private Vector3 newLocalScale = Vector3.one;

    [Header("What to change")]
    [SerializeField] private bool changePosition = true;
    [SerializeField] private bool changeRotation = false;
    [SerializeField] private bool changeScale = false;

    [Header("Delay before changing transform")]
    [SerializeField] private float transformChangeDelay = 0f;
    
    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip soundClip;

    

    private Rigidbody rb;
    private Collider[] allColliders;

    public bool IsPickedUp { get; private set; }
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        allColliders = GetComponentsInChildren<Collider>();
    }

    public void PickUp(Transform holdPoint)
    {
        if (IsPickedUp)
            return;

        IsPickedUp = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        foreach (var col in allColliders)
        {
            col.enabled = false;
        }

        transform.SetParent(holdPoint);
        transform.localPosition = holdLocalPosition;
        transform.localRotation = Quaternion.Euler(holdLocalRotation);
        transform.localScale = holdLocalScale;

        if (objectToShow != null)
            objectToShow.SetActive(true);

        if (transformChangeDelay > 0f)
            StartCoroutine(ChangeTargetTransformWithDelay());
        else
            ChangeTargetTransform();
    }

    private IEnumerator ChangeTargetTransformWithDelay()
    {
        yield return new WaitForSeconds(transformChangeDelay);
        ChangeTargetTransform();
    }

    private void ChangeTargetTransform()
    {
        if (targetToChange == null)
            return;

        if (changePosition)
            targetToChange.localPosition = newLocalPosition;

        if (changeRotation)
            targetToChange.localRotation = Quaternion.Euler(newLocalRotation);

        if (changeScale)
            targetToChange.localScale = newLocalScale;
        
        if (audioSource != null && soundClip != null)
            audioSource.PlayOneShot(soundClip);
        
    }
}