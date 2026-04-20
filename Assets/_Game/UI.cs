using TMPro;
using UnityEngine;

namespace Assets._Game
{
    public class UI : MonoBehaviour
    {
        public TMP_Text Dialog;

        private void Start()
        {
            G.ui = this;
        }
    }
}