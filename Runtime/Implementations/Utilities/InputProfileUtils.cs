namespace P3k.InputController.Implementations.Utilities
{
   using System;
   using System.Collections.Generic;
   using System.IO;
   using System.Linq;

   using UnityEngine;
   using UnityEngine.InputSystem;

   /// <summary>
   ///    Helper methods for storing and loading input binding profiles to disk.
   ///    Profiles are serialized to JSON under <see cref="Application.persistentDataPath" />/InputProfiles.
   /// </summary>
   internal static class InputProfileUtils
   {
      internal static string ProfilesDir => Path.Combine(Application.persistentDataPath, "InputProfiles");

      /// <summary>
      ///    Delete a saved profile file for <paramref name="profileName" /> if it exists.
      /// </summary>
      /// <param name="profileName">Profile name to delete.</param>
      internal static void Delete(string profileName)
      {
         var path = GetProfilePath(profileName);
         if (File.Exists(path))
         {
            File.Delete(path);
         }
      }

      /// <summary>
      ///    Returns true when a saved profile file exists for <paramref name="profileName" />.
      /// </summary>
      /// <param name="profileName">Profile name to check.</param>
      internal static bool Exists(string profileName)
      {
         return !string.IsNullOrEmpty(profileName) && File.Exists(GetProfilePath(profileName));
      }

      /// <summary>
      ///    Enumerate saved profile names (without extension) from the profiles directory.
      /// </summary>
      internal static IReadOnlyList<string> GetProfiles()
      {
         EnsureDir();

         return Directory.GetFiles(ProfilesDir, "*.json").Select(Path.GetFileNameWithoutExtension).OrderBy(x => x)
            .ToList();
      }

      /// <summary>
      ///    Load a profile from disk and apply binding overrides to the provided
      ///    <paramref name="asset" />. The three out parameters return any stored
      ///    per-input flags (analog keyboard, analog gamepad, inverted Y) as sets.
      /// </summary>
      /// <param name="asset">InputActionAsset to apply binding overrides to.</param>
      /// <param name="profileName">Name of the profile to load.</param>
      /// <param name="analogKeyboard">Outputs ids marked as analog keyboard.</param>
      /// <param name="analogGamepad">Outputs ids marked as analog gamepad.</param>
      /// <param name="invertedYInputs">Outputs ids marked as inverted Y.</param>
      internal static bool Load(
         InputActionAsset asset,
         string profileName,
         out HashSet<string> analogKeyboard,
         out HashSet<string> analogGamepad,
         out HashSet<string> invertedYInputs)
      {
         analogKeyboard = new HashSet<string>();
         analogGamepad = new HashSet<string>();
         invertedYInputs = new HashSet<string>();

         var path = GetProfilePath(profileName);
         if (!File.Exists(path))
         {
            // No saved profile available.
            return false;
         }

         try
         {
            var json = File.ReadAllText(path);
            var settings = JsonUtility.FromJson<InputSettingsJson>(json);

            // Apply binding overrides contained in the profile JSON to the action asset.
            if (!string.IsNullOrEmpty(settings.BindingOverrides))
            {
               asset.LoadBindingOverridesFromJson(settings.BindingOverrides);
            }

            // Copy optional lists into output hash sets. Null-checks because older
            // profiles may omit fields.
            if (settings.AnalogKeyboards != null)
            {
               foreach (var id in settings.AnalogKeyboards)
               {
                  analogKeyboard.Add(id);
               }
            }

            if (settings.AnalogGamepads != null)
            {
               foreach (var id in settings.AnalogGamepads)
               {
                  analogGamepad.Add(id);
               }
            }

            if (settings.InvertedYInputs != null)
            {
               foreach (var id in settings.InvertedYInputs)
               {
                  invertedYInputs.Add(id);
               }
            }

            return true;
         }
         catch (Exception e)
         {
            Debug.LogWarning($"Failed to load input profile '{profileName}': {e.Message}");
            return false;
         }
      }

      /// <summary>
      ///    Save the provided action asset's binding overrides and the supplied
      ///    input flag sets to a profile file on disk.
      /// </summary>
      internal static void Save(
         InputActionAsset asset,
         string profileName,
         HashSet<string> analogKeyboard,
         HashSet<string> analogGamepad,
         HashSet<string> invertedYInputs)
      {
         try
         {
            var settings = new InputSettingsJson(
            asset.SaveBindingOverridesAsJson(),
            new List<string>(analogKeyboard),
            new List<string>(analogGamepad),
            new List<string>(invertedYInputs));

            var json = JsonUtility.ToJson(settings, true);
            File.WriteAllText(GetProfilePath(profileName), json);
         }
         catch (Exception e)
         {
            Debug.LogError($"Failed to save input profile '{profileName}': {e.Message}");
         }
      }

      /// <summary>
      ///    Compare the provided asset state and option sets against a saved profile
      ///    file and return true when saving would produce a different file (i.e. a
      ///    change exists). This is used to avoid unnecessary writes.
      /// </summary>
      internal static bool WouldSaveChange(
         InputActionAsset asset,
         string profileName,
         HashSet<string> analogKeyboard,
         HashSet<string> analogGamepad,
         HashSet<string> invertedYInputs)
      {
         var path = GetProfilePath(profileName);
         if (!File.Exists(path))
         {
            // No existing file means save would create a change.
            return true;
         }

         try
         {
            var json = File.ReadAllText(path);
            var saved = JsonUtility.FromJson<InputSettingsJson>(json);

            // Compare binding overrides as a single JSON blob.
            var currentOverrides = asset.SaveBindingOverridesAsJson();
            var savedOverrides = saved.BindingOverrides ?? string.Empty;

            if (!string.Equals(currentOverrides, savedOverrides, StringComparison.Ordinal))
            {
               return true;
            }

            // Compare the three option sets using set equality so ordering differences
            // in lists don't cause spurious changes.
            var savedAnalogKb = saved.AnalogKeyboards != null ?
                                   new HashSet<string>(saved.AnalogKeyboards) :
                                   new HashSet<string>();

            var savedAnalogGp = saved.AnalogGamepads != null ?
                                   new HashSet<string>(saved.AnalogGamepads) :
                                   new HashSet<string>();

            var savedInvert = saved.InvertedYInputs != null ?
                                 new HashSet<string>(saved.InvertedYInputs) :
                                 new HashSet<string>();

            if (!savedAnalogKb.SetEquals(analogKeyboard))
            {
               return true;
            }

            if (!savedAnalogGp.SetEquals(analogGamepad))
            {
               return true;
            }

            if (!savedInvert.SetEquals(invertedYInputs))
            {
               return true;
            }

            return false;
         }
         catch
         {
            // If reading/parsing the saved file fails assume the save would change it.
            return true;
         }
      }

      /// <summary>
      ///    Ensure the profiles directory exists on disk. This is a no-op if the
      ///    directory already exists.
      /// </summary>
      private static void EnsureDir()
      {
         if (!Directory.Exists(ProfilesDir))
         {
            Directory.CreateDirectory(ProfilesDir);
         }
      }

      /// <summary>
      ///    Returns the full file path for the profile JSON for <paramref name="profileName" />.
      ///    Ensures the profiles directory exists before returning the path.
      /// </summary>
      /// <param name="profileName">Profile name.</param>
      private static string GetProfilePath(string profileName)
      {
         EnsureDir();
         return Path.Combine(ProfilesDir, $"{profileName}.json");
      }

      /// <summary>
      ///    Internal data container used for JSON serialization of a profile.
      ///    Fields are public so Unity's JsonUtility can serialize/deserialize them.
      /// </summary>
      [Serializable]
      private sealed class InputSettingsJson
      {
         [SerializeField]
         private List<string> _analogGamepads;

         [SerializeField]
         private List<string> _analogKeyboards;

         [SerializeField]
         private string _bindingOverrides;

         [SerializeField]
         private List<string> _invertedYInputs;

         public List<string> AnalogGamepads => _analogGamepads;

         public List<string> AnalogKeyboards => _analogKeyboards;

         public string BindingOverrides => _bindingOverrides;

         public List<string> InvertedYInputs => _invertedYInputs;

         public InputSettingsJson(
            string bindingOverrides,
            List<string> analogKeyboards,
            List<string> analogGamepads,
            List<string> invertedYInputs)
         {
            _bindingOverrides = bindingOverrides;
            _analogKeyboards = analogKeyboards;
            _analogGamepads = analogGamepads;
            _invertedYInputs = invertedYInputs;
         }
      }
   }
}