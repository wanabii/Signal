using UnityEngine;

namespace Assets._Game
{
    public class SfxPlayer : MonoBehaviour
    {
        [SerializeField] public AudioClip alertClip;
        
        private AudioSource audioSource;
        
        private void Start()
        {
            G.sfxPlayer = this;
            audioSource = GetComponent<AudioSource>();
        }
        
        public void PlayAlertSound()
        {
            audioSource.PlayOneShot(alertClip);
        }
    }
}