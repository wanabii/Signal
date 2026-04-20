using UnityEngine;

public class Main : MonoBehaviour
{
   private void Awake()
   {
      Application.targetFrameRate = 60;
      
      Cursor.visible = false;
      Cursor.lockState = CursorLockMode.Locked;
   }
}
