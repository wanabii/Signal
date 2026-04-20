using System.Collections;
using Assets._Game;
using UnityEngine;
using Unity.Cinemachine;

public class RadioMonsterTrigger : MonoBehaviour
{
    [SerializeField] private PlayerPickup playerPickup;
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private Transform spawnPoint;
    
    
    [SerializeField] private CinemachineCamera monsterLookCam;
    [SerializeField] private CinemachineCamera playerLookCam;
    
    [SerializeField] private float delayBeforeLook = 0.25f;
    [SerializeField] private float lookTime = 0.6f;
    [SerializeField] private int activePriority = 100;
    [SerializeField] private int idlePriority = -1;
    [SerializeField] private bool clearLookAtAfterRelease = true;

    [SerializeField] private bool triggerOnlyOnce = true;
    [SerializeField] private Camera mainCamera;

    

    private bool triggered;
    private Coroutine lookRoutine;

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
        G.sfxPlayer.PlayAlertSound();
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
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogWarning("Не найдена mainCamera.", this);
            yield break;
        }

        CinemachineBrain brain = mainCamera.GetComponent<CinemachineBrain>();
        if (brain == null)
        {
            Debug.LogWarning("На mainCamera нет CinemachineBrain.", this);
            yield break;
        }

        if (delayBeforeLook > 0f)
            yield return new WaitForSeconds(delayBeforeLook);

        // Берём текущую активную Cinemachine-камеру
        CinemachineVirtualCameraBase activeCam = brain.ActiveVirtualCamera as CinemachineVirtualCameraBase;
        if (activeCam == null)
        {
            Debug.LogWarning("Не удалось получить активную CinemachineCamera.", this);
            yield break;
        }

        Transform oldLookAt = activeCam.LookAt;
        activeCam.LookAt = target;

        if (lookTime > 0f)
            yield return new WaitForSeconds(lookTime);

        if (clearLookAtAfterRelease)
            activeCam.LookAt = oldLookAt;

        lookRoutine = null;
    }
}