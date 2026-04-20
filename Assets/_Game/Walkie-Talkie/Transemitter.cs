using System.Collections;
using UnityEngine;

namespace Assets._Game.Walkie_Talkie
{
    using System.Collections;
    using UnityEngine;

    public class Transemitter : MonoBehaviour
    {
        [Header("Audio")]
        public AudioSource audioSource;

        [Header("Text")]
        [TextArea]
        public string message = "To send a message, I need to find the radio tower and climb to its top.";

        public float wordDelay = 0.25f;      // задержка между словами при наборе
        public float stayDuration = 3f;      // сколько текст висит целиком
        public float hideWordDelay = 0.15f;  // задержка между удалением слов

        private Coroutine dialogCoroutine;

        public void Play()
        {
            if (audioSource != null)
                audioSource.Play();

            if (dialogCoroutine != null)
                StopCoroutine(dialogCoroutine);

            dialogCoroutine = StartCoroutine(Dialog());
        }

        private IEnumerator Dialog()
        {
            // очищаем текст перед новым показом
            G.ui.Dialog.text = string.Empty;

            string[] words = message.Split(' ');

            // Набор текста по словам
            string currentText = "";
            for (int i = 0; i < words.Length; i++)
            {
                if (i == 0)
                    currentText = words[i];
                else
                    currentText += " " + words[i];

                G.ui.Dialog.text = currentText;
                yield return new WaitForSeconds(wordDelay);
            }

            yield return new WaitForSeconds(stayDuration);
            G.ui.Dialog.text = string.Empty;

            G.ui.Dialog.text = string.Empty;
            dialogCoroutine = null;
        }
    }
}