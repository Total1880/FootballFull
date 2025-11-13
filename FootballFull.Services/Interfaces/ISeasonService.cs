using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface ISeasonService
    {
        void PlayMatchDay(IList<Fixture> fixtures, int matchDay);
    }
}