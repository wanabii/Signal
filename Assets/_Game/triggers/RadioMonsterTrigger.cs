using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class RadioMonsterTrigger : MonoBehaviour
{
    [SerializeField] private PlayerPickup playerPickup;
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private Transform spawnPoint;
    
    
    [SerializeField] private CinemachineCamera monsterLookCam;
    [SerializeField] private float delayBeforeLook = 0.25f;
    [SerializeField] private float lookTime = 0.6f;
    [SerializeField] private int activePriority = 100;
    [SerializeField] private int idlePriority = -1;
    [SerializeField] private bool clearLookAtAfterRelease = true;

    [SerializeField] private bool triggerOnlyOnce = true;

    private bool triggered;
    private Coroutine lookRoutine;

    private void Awake()
    {
        if (monsterLookCam != null)
            monsterLookCam.Priority = idlePriority;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered && triggerOnlyOnce)
            return;

        if (playerPickup == null)
            return;

        if (other.GetComponentInParent<PlayerPickup>() != playerPickup)
            return;

        if (!playerPickup.IsHoldingRadio)
            return;

        if (monsterPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("monsterPrefab или spawnPoint не назначены.", this);
            return;
        }

        GameObject monster = Instantiate(monsterPrefab, spawnPoint.position, spawnPoint.rotation);

        if (monsterLookCam != null)
        {
            if (lookRoutine != null)
                StopCoroutine(lookRoutine);

            lookRoutine = StartCoroutine(LookAtMonsterRoutine(monster.transform));
        }

        triggered = true;
    }

    private IEnumerator LookAtMonsterRoutine(Transform target)
    {
        monsterLookCam.Priority = idlePriority;
        monsterLookCam.LookAt = target;

        if (delayBeforeLook > 0f)
            yield return new WaitForSeconds(delayBeforeLook);

        monsterLookCam.Priority = activePriority;

        if (lookTime > 0f)
            yield return new WaitForSeconds(lookTime);

        monsterLookCam.Priority = idlePriority;

        if (clearLookAtAfterRelease)
            monsterLookCam.LookAt = null;

        lookRoutine = null;
    }
}