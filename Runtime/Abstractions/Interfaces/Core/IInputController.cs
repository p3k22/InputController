namespace P3k.InputController.Abstractions.Interfaces.Core
{
   using P3k.InputController.Abstractions.Interfaces.Services;

   using System.Linq;

   public interface IInputController : IInputStateProvider,
                                       IInputFlagsService,
                                       IInputProfilesService,
                                       IInputRebindingService
   {
   }
}