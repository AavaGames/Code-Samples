using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileInputDetector : MonoBehaviour
{
#if (UNITY_ANDROID || UNITY_IOS)
   public static bool usingController = false;
   public bool forceController = false;
   private bool Xbox_Wireless_Controller = false;
   private bool PS4_Controller = false;

   private void Update()
   {
      if (forceController)
      {
         usingController = true;
      }
      else
      {
         string[] names = Input.GetJoystickNames();

         for (int x = 0; x < names.Length; x++)
         {
            Debug.Log(names[x]);
            Debug.Log(names[x].Length);
            if (names[x].Length <= 1)
            {
               PS4_Controller = false;
               Xbox_Wireless_Controller = false;
            }
            else if (names[x].Length == 24)
            {
               PS4_Controller = false;
               Xbox_Wireless_Controller = true;
            }
         }

         if (Xbox_Wireless_Controller || PS4_Controller)
         {
            usingController = true;
         }
         else
         {
            usingController = false;
         }
      }
   }
#endif
}
