using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickup : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private TMP_Text promptText;

    [Header("Settings")]
    [SerializeField] private float pickupDistance = 3f;
    [SerializeField] private LayerMask pickupLayer;

    private PickupItem currentTarget;
    private PickupItem heldItem;
    private RadioItem heldRadio;

    public PickupItem HeldItem => heldItem;
    public bool IsHoldingSomething => heldItem != null;
    public bool IsHoldingRadio => heldRadio != null;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        FindTarget();

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryPickUp();
        }
    }

    private void FindTarget()
    {
        currentTarget = null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * pickupDistance, Color.red);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance, pickupLayer, QueryTriggerInteraction.Ignore))
        {
            promptText.gameObject.SetActive(true);

            
            if (!hit.collider.TryGetComponent(out currentTarget))
            {
                currentTarget = hit.collider.GetComponentInParent<PickupItem>();
            }

            if (currentTarget != null && currentTarget.IsPickedUp)
            {
                currentTarget = null;
            }
        }
        else
        {
            promptText.gameObject.SetActive(false);
        }
        
        
    }

    private void TryPickUp()
    {
        if (heldItem != null)
            return;

        if (currentTarget == null)
            return;

        heldItem = currentTarget;
        heldItem.PickUp(holdPoint);

        heldRadio = heldItem.GetComponent<RadioItem>();

        currentTarget = null;
    }

    public void ClearHeldItem()
    {
        heldItem = null;
        heldRadio = null;
    }
}