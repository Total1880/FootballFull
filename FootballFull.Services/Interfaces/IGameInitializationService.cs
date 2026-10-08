using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface IGameInitializationService
    {
        GameInitializationResult Initialize(bool isNew);
    }
}
