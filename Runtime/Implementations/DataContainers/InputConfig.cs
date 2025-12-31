namespace P3k.InputController.Implementations.DataContainers
{
   using P3k.InputController.Abstractions.Interfaces.Configurations;

   using System.Collections.Generic;
   using System.Linq;

   using UnityEngine;
   using UnityEngine.InputSystem;

   [CreateAssetMenu(fileName = "InputConfig", menuName = "P3k/Input Config")]
   public class InputConfig : ScriptableObject, IInputConfig
   {
      [SerializeField]
      private List<InputDefinition> _inputs = new();

      [SerializeField]
      [Tooltip("Auto-generated InputActionAsset synced with this config. Use this to assign Action References in the inspector.")]
      private InputActionAsset _generatedActionAsset;

      /// <summary>
      /// The auto-generated InputActionAsset that mirrors this config.
      /// Can be used to assign InputActionReferences in the inspector.
      /// </summary>
      public InputActionAsset GeneratedActionAsset
      {
         get => _generatedActionAsset;
         set => _generatedActionAsset = value;
      }

      public List<IInputDefinition> Inputs => _inputs.Cast<IInputDefinition>().ToList();
   }
}
