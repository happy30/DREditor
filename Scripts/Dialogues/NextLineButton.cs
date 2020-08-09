using System.Collections;
using System.Collections.Generic;
using EventObjects;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class NextLineButton : MonoBehaviour
{
   public BoolVariable LineCompleted;
   private Button _button;

   void Awake()
   {
      _button = GetComponent<Button>();
      
   }

   void OnEnable()
   {
      LineCompleted.Register();
   }

   private void OnDisable()
   {
      LineCompleted.Unregister();
   }


   void Update()
   {
      _button.interactable = LineCompleted.Value;
   }
}
