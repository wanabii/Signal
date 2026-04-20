using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Info")]
    [SerializeField] private string itemName = "предмет";

    [Header("Position in hand")]
    [SerializeField] private Vector3 holdLocalPosition = new Vector3(0.3f, -0.25f, 0.6f);
    [SerializeField] private Vector3 holdLocalRotation = Vector3.zero;
    [SerializeField] private Vector3 holdLocalScale = Vector3.one;

    private Rigidbody rb;
    private Collider[] allColliders;

    public bool IsPickedUp { get; private set; }

    public string GetPromptText()
    {
        return $"E - поднять {itemName}";
    }

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
    }
}