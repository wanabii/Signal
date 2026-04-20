using System.Collections;
using UnityEngine;

namespace Assets._Game.RadioStation
{
    public class RadioElectricGlitchPlayer: MonoBehaviour
    {
        public float glitchDelay = 8f;
        
        private AudioSource audioSource;
        
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }


        private void Start()
        {
            StartCoroutine(PlayAudio());
        }
        
        private IEnumerator PlayAudio()
        {
            while (true)
            {
                audioSource.Play();
                yield return new WaitForSeconds(glitchDelay);
            }
            
        }
    }
}