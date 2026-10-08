using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    /// <summary>
    /// Handles the player's end-of-season expansion choices.
    /// Returns true when a new competition was created.
    /// </summary>
    public interface IEndOfSeasonChoicesService
    {
        bool HandleChoices(Guid countryId, FootballAssociation association);
    }
}
