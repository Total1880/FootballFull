using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface ISeasonService
    {
        IList<ClubLeagueCompetition> ClubLeagueCompetitions { get; }
        void InitializeNewSeason(IList<Club> clubs);
        void PlayMatchDay(IList<Fixture> fixtures, int matchDay);

    }
}