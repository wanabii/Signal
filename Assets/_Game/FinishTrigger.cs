using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishTrigger : MonoBehaviour
{
   private void OnTriggerEnter(Collider other)
   {
      if (other.tag == "Player")
      {
         SceneManager.LoadScene(1);
      }
   }
}
