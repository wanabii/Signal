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
        UpdatePrompt();

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryPickUp();
        }
    }

    private void FindTarget()
    {
        currentTarget = null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance, pickupLayer, QueryTriggerInteraction.Ignore))
        {
            if (!hit.collider.TryGetComponent(out currentTarget))
            {
                currentTarget = hit.collider.GetComponentInParent<PickupItem>();
            }

            if (currentTarget != null && currentTarget.IsPickedUp)
            {
                currentTarget = null;
            }
        }
    }

    private void UpdatePrompt()
    {
        if (promptText == null)
            return;

        bool canShow = currentTarget != null && heldItem == null;

        promptText.gameObject.SetActive(canShow);
    }

    private void TryPickUp()
    {
        if (heldItem != null)
            return;

        if (currentTarget == null)
            return;

        heldItem = currentTarget;
        heldItem.PickUp(holdPoint);
        currentTarget = null;

        UpdatePrompt();
    }
}