namespace P3k.InputController.Abstractions.Interfaces.Services
{
   using System.Linq;

   public interface IInputProfilesService
   {
      string CurrentProfile { get; }

      bool ProfileCanBeSaved(string profileName);

      bool ProfileExists(string profileName);

      void ProfileLoad(string profileName);

      void ProfileSave(string profileName);

      void ResetBindingsToDefault(string profileName = "");
   }
}
