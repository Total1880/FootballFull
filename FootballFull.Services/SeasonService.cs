using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;
using OlavFramework;

namespace FootballFull.Services
{
    public class SeasonService : ISeasonService
    {
        private IRepository<Competition> _competitionRepository;
        private IClubService _clubService;
        private IFixtureService _fixtureService;
        private IList<ClubLeagueCompetition> _clubLeagueCompetitions;
        private IList<ClubPerCompetition> _clubsPerCompetition;
        private IList<Club> _clubs;
        public IList<ClubLeagueCompetition> ClubLeagueCompetitions => _clubLeagueCompetitions;

        public SeasonService(IRepository<Competition> competitionRepository, IClubService clubService, IFixtureService fixtureService)
        {
            _competitionRepository = competitionRepository;
            _clubService = clubService;
            _fixtureService = fixtureService;
        }
        public void Initialize(IList<ClubPerCompetition> clubsPerCompetition)
        {
            _clubsPerCompetition = clubsPerCompetition;
            _clubs = _clubService.GetClubs();
            InitializeNewSeason();
        }

        public void InitializeNewSeason()
        {
            RecalculateStrengths(Configuration.MinStrength, Configuration.MaxStrength);
            PromotionsAndRelegations();
            _clubLeagueCompetitions = _clubsPerCompetition.Select(club => new ClubLeagueCompetition
            {
                ClubId = club.ClubId,
                Points = 0,
                GoalsFor = 0,
                GoalsAgainst = 0,
                CompetitionId = club.CompetitionId
            }).ToList();
        }

        private void RecalculateStrengths(int minStrength = 1, int maxStrength = 9)
        {
            if (_clubLeagueCompetitions == null || !_clubLeagueCompetitions.Any())
                return;

            // Per competitie de ranking bepalen
            var competitions = _clubLeagueCompetitions
                .GroupBy(clc => clc.CompetitionId);

            foreach (var competitionGroup in competitions)
            {
                var ranked = competitionGroup
                    .OrderByDescending(c => c.Points)
                    .ThenByDescending(c => c.GoalsFor - c.GoalsAgainst)
                    .ThenByDescending(c => c.GoalsFor)
                    .ToList();

                int teamCount = ranked.Count;

                for (int i = 0; i < teamCount; i++)
                {
                    var entry = ranked[i];
                    int position = i + 1;
                    double percentile = position / (double)teamCount;

                    int delta = 0;

                    // Top 20% stijgt
                    if (percentile <= 0.20)
                        delta = +1;
                    // Onderste 20% daalt
                    else if (percentile >= 0.80)
                        delta = -1;

                    if (delta == 0)
                        continue;

                    var club = _clubService.GetClubById(entry.ClubId);
                    if (club == null)
                        continue;

                    var newStrength = club.Strength + delta;
                    if (newStrength < minStrength) newStrength = minStrength;
                    if (newStrength > maxStrength) newStrength = maxStrength;

                    club.Strength = newStrength;
                    club.Last5Games = "";
                    _clubService.Update(club);
                }
            }
        }

        private void PromotionsAndRelegations()
        {
            if (_clubLeagueCompetitions == null)
                return;

            const int promotionsAndRelegationPlaces = 2;

            var competitions = _competitionRepository.Load();

            foreach (var competition in competitions)
            {
                var clubsInCompetition = _clubLeagueCompetitions
                    .Where(_ => _.CompetitionId == competition.Id)
                    .OrderByDescending(_ => _.Points)
                    .ThenByDescending(_ => _.GoalsFor - _.GoalsAgainst)
                    .ThenByDescending(_ => _.GoalsFor)
                    .ToList();

                if (clubsInCompetition.Count == 0)
                    continue;

                var relegatedClubs = clubsInCompetition
                    .Skip(clubsInCompetition.Count - promotionsAndRelegationPlaces)
                    .ToList();

                var promotedClubs = clubsInCompetition
                    .Take(promotionsAndRelegationPlaces)
                    .ToList();

                // 1. Strength aanpassen op basis van promotie/degradatie

                // Promotie: iets sterker
                foreach (var promoted in promotedClubs)
                {
                    var club = _clubService.GetClubById(promoted.ClubId);
                    if (club == null) continue;

                    club.Strength = Math.Min(club.Strength + 1, Configuration.MaxStrength);
                    _clubService.Update(club);
                }

                // Degradatie: iets zwakker
                foreach (var relegated in relegatedClubs)
                {
                    var club = _clubService.GetClubById(relegated.ClubId);
                    if (club == null) continue;

                    club.Strength = Math.Max(club.Strength - 1, Configuration.MinStrength);
                    _clubService.Update(club);
                }

                // 2. Competitie-id updaten (promotie/degradatie zelf)

                // Promotie: naar hogere tier (lager getal)
                if (competition.Tier > 1)
                {
                    var higherCompetition = competitions
                        .FirstOrDefault(_ => _.Tier == competition.Tier - 1 && _.CountryId == competition.CountryId);

                    if (higherCompetition != null)
                    {
                        foreach (var promotedClub in promotedClubs)
                        {
                            var link = _clubsPerCompetition.FirstOrDefault(_ => _.ClubId == promotedClub.ClubId);
                            if (link != null)
                                link.CompetitionId = higherCompetition.Id;
                        }
                    }
                }

                // Degradatie: naar lagere tier (hoger getal)
                var maxTier = competitions.Max(_ => _.Tier);
                if (competition.Tier < maxTier)
                {
                    var lowerCompetition = competitions
                        .FirstOrDefault(_ => _.Tier == competition.Tier + 1 && _.CountryId == competition.CountryId);

                    if (lowerCompetition != null)
                    {
                        foreach (var relegatedClub in relegatedClubs)
                        {
                            var link = _clubsPerCompetition.FirstOrDefault(_ => _.ClubId == relegatedClub.ClubId);
                            if (link != null)
                                link.CompetitionId = lowerCompetition.Id;
                        }
                    }
                }
            }
        }

        public void PlayMatchDay(IList<Fixture> fixtures, int matchDay, bool isSuddenDeath = false, Guid? playerClubId = null)
        {
            if (_clubLeagueCompetitions == null)
                throw new InvalidOperationException("Club league competitions not initialized.");

            var todaysFixtures = fixtures.Where(_ => _.MatchDay == matchDay).ToList();

            foreach (var fixture in todaysFixtures)
            {
                if (fixture.HomeTeamId == Guid.Empty)
                {
                    // Bye voor thuisteam
                    fixture.HomeScore = 0;
                    fixture.AwayScore = 3;
                    continue;
                }
                else if (fixture.AwayTeamId == Guid.Empty)
                {
                    // Bye voor uitteam
                    fixture.HomeScore = 3;
                    fixture.AwayScore = 0;
                    continue;
                }

                bool isPlayerMatch = playerClubId.HasValue &&
                                     (fixture.HomeTeam.Id == playerClubId.Value ||
                                      fixture.AwayTeam.Id == playerClubId.Value);

                if (isPlayerMatch)
                {
                    PlayInteractiveFixture(fixture, playerClubId.Value, isSuddenDeath);
                }
                else
                {
                    SimulateFixtureAutomatically(fixture, isSuddenDeath);
                }

                if (!isSuddenDeath)
                    ApplyResultToTable(fixture);

                UpdateClubMomentum(fixture.HomeTeamId, fixture.HomeScore, fixture.AwayScore);
                UpdateClubMomentum(fixture.AwayTeamId, fixture.AwayScore, fixture.HomeScore);
            }
        }

        private void SimulateFixtureAutomatically(Fixture fixture, bool isSuddenDeath)
        {
            // Bye-afhandeling
            if (fixture.HomeTeamId == Guid.Empty)
            {
                fixture.HomeScore = 0;
                fixture.AwayScore = 3;
                return;
            }
            else if (fixture.AwayTeamId == Guid.Empty)
            {
                fixture.HomeScore = 3;
                fixture.AwayScore = 0;
                return;
            }

            const int phases = 9; // 6 fases = 90 min
            int homeGoals = 0;
            int awayGoals = 0;

            // Momentum ophalen
            var homeClub = _clubs.First(_ => _.Id == fixture.HomeTeamId);
            var awayClub = _clubs.First(_ => _.Id == fixture.AwayTeamId);

            int homeMomentumMod = MapMomentumToModifier(homeClub.Momentum);
            int awayMomentumMod = MapMomentumToModifier(awayClub.Momentum);

            // Home advantage
            int homeAdvantage = 1;

            var homeStrength = fixture.HomeTeam.Strength + homeMomentumMod + homeAdvantage;
            var awayStrength = fixture.AwayTeam.Strength + awayMomentumMod;
            var difference = homeStrength - awayStrength;

            if (homeStrength > 5)
            {
                homeStrength = 5;
                if (difference < 0)
                    homeStrength += difference;
            }
            if (awayStrength > 5)
            {
                awayStrength = 5;
                if (difference > 0)
                    awayStrength -= difference;
            }

            while (homeStrength < 1 || awayStrength < 1)
            {
                homeStrength++;
                awayStrength++;
            }

            // Simuleer fases
            for (int phase = 1; phase <= phases; phase++)
            {
                if (RollChanceToScore(homeStrength))
                    homeGoals++;

                if (RollChanceToScore(awayStrength))
                    awayGoals++;
            }

            // Sudden death als nodig (beker)
            if (isSuddenDeath && homeGoals == awayGoals)
            {
                // Gewoon lichte bias op de sterkere ploeg
                int totalStrength = homeStrength + awayStrength;
                int roll = Random.Shared.Next(0, totalStrength);

                if (roll < homeStrength)
                    homeGoals++;
                else
                    awayGoals++;
            }

            fixture.HomeScore = homeGoals;
            fixture.AwayScore = awayGoals;
        }

        private enum TacticChoice
        {
            Attack,
            Defend,
            Balanced
        }

        private void PlayInteractiveFixture(Fixture fixture, Guid playerClubId, bool isSuddenDeath)
        {
            int homeGoals = 0;
            int awayGoals = 0;
            const int phases = 9; // 6 fases = ± 15 min per fase
            var commentary = new List<string>();

            bool playerIsHome = fixture.HomeTeam.Id == playerClubId;
            var playerClub = playerIsHome ? fixture.HomeTeam : fixture.AwayTeam;
            var opponent = playerIsHome ? fixture.AwayTeam : fixture.HomeTeam;
            var homeMomentum = MapMomentumToModifier(_clubs.First(_ => _.Id == fixture.HomeTeamId).Momentum);
            var awayMomentum = MapMomentumToModifier(_clubs.First(_ => _.Id == fixture.AwayTeamId).Momentum);

            int startHomeStrength = fixture.HomeTeam.Strength + homeMomentum;
            int startAwayStrength = fixture.AwayTeam.Strength + awayMomentum - 1;
            int difference = startHomeStrength - startAwayStrength;
            var homeNegativeEffects = 0;
            var awayNegativeEffects = 0;

            Console.Clear();
            Console.WriteLine();
            Console.WriteLine($"Speeldag {fixture.MatchDay}");
            Console.WriteLine($"{fixture.HomeTeam.Name} {homeGoals} - {awayGoals} {fixture.AwayTeam.Name}");

            for (int phase = 1; phase <= phases; phase++)
            {

                var choice = AskTacticChoice(playerClub.Name);
                var effectiveHomeStrength = startHomeStrength;
                var effectiveAwayStrength = startAwayStrength;
                var updateDifference = false;
                if (effectiveHomeStrength > 5)
                {
                    effectiveHomeStrength = 5;
                    updateDifference = true;

                }

                if (effectiveAwayStrength > 5)
                {
                    effectiveAwayStrength = 5;
                    updateDifference = true;

                };

                if (updateDifference)
                {
                    if (difference < 0)
                        effectiveHomeStrength += difference;
                    if (difference > 0)
                        effectiveAwayStrength -= difference;
                }

                // Tactiek van speler toepassen aan juiste kant
                if (playerIsHome)
                {
                    ApplyTactic(choice, ref effectiveHomeStrength, ref effectiveAwayStrength);
                }
                else
                {
                    ApplyTactic(choice, ref effectiveAwayStrength, ref effectiveHomeStrength);
                }

                if (RollCard())
                {
                    // 50/50 welke ploeg de kaart krijgt
                    bool cardForHome = Random.Shared.Next(0, 2) == 0;
                    if (cardForHome)
                    {
                        homeNegativeEffects = Math.Max(0, effectiveHomeStrength - 1);
                        commentary.Add($"Min {phase * 10}: Rode kaart voor {fixture.HomeTeam.Name}! Ze verliezen grip op de wedstrijd.");
                    }
                    else
                    {
                        awayNegativeEffects = Math.Max(0, effectiveAwayStrength - 1);
                        commentary.Add($"Min {phase * 10}: Rode kaart voor {fixture.AwayTeam.Name}!");
                    }
                }

                effectiveHomeStrength -= homeNegativeEffects;
                effectiveAwayStrength -= awayNegativeEffects;
                while (effectiveHomeStrength < 1 || effectiveAwayStrength < 1)
                {
                    effectiveHomeStrength++;
                    effectiveAwayStrength++;
                }
                // Elke fase: kans op goal voor beide teams
                bool homeChance = RollChanceToScore(effectiveHomeStrength);
                bool awayChance = RollChanceToScore(effectiveAwayStrength);

                if (homeChance)
                {
                    homeGoals++;
                    commentary.Add($"Min {phase * 10}: Doelpunt voor {fixture.HomeTeam.Name}!");
                }
                else if (awayChance)
                {
                    awayGoals++;
                    commentary.Add($"Min {phase * 10}: Doelpunt voor {fixture.AwayTeam.Name}!");
                }
                else
                {
                    commentary.Add($"Min {phase * 10}: Geen grote kansen in deze fase.");
                }

                Console.Clear();

                Console.WriteLine();
                Console.WriteLine($"Speeldag {fixture.MatchDay}");
                Console.WriteLine($"{fixture.HomeTeam.Name} {homeGoals} - {awayGoals} {fixture.AwayTeam.Name}");
                Console.WriteLine($"Minuut {phase * 10}");
                Console.WriteLine();
                Console.WriteLine("Wedstrijdverslag:");
                foreach (var line in commentary)
                {
                    Console.WriteLine(line);
                }
                
            }

            if (isSuddenDeath && homeGoals == awayGoals)
            {
                Console.WriteLine();
                Console.WriteLine("Sudden death! Volgende goal wint...");
                Console.ReadKey(true);
                while (homeGoals == awayGoals)
                {
                    // je kan hier eventueel 'mini-fases' doen met minuut +1 etc.
                    if (RollChanceToScore(startHomeStrength))
                    {
                        homeGoals++;
                        Console.WriteLine($"{fixture.HomeTeam.Name} scoort in sudden death!");
                    }
                    else if (RollChanceToScore(startAwayStrength))
                    {
                        awayGoals++;
                        Console.WriteLine($"{fixture.AwayTeam.Name} scoort in sudden death!");
                    }
                }
            }
            Console.ReadKey(true);

            fixture.HomeScore = homeGoals;
            fixture.AwayScore = awayGoals;

            Console.WriteLine("Einde wedstrijd!");
            Console.WriteLine($"{fixture.HomeTeam.Name} {fixture.HomeScore} - {fixture.AwayScore} {fixture.AwayTeam.Name}");
            Console.WriteLine("Druk op een toets om verder te gaan...");
            Console.ReadKey(true);
        }

        private int MapMomentumToModifier(int momentum)
        {
            if (momentum <= 4) return -1;
            if (momentum >= 11) return 1;
            return 0;
        }

        private bool RollCard()
        {
            int roll = Random.Shared.Next(0, 100);
            return roll < 5; // 5% kans op kaart in een fase
        }


        private TacticChoice AskTacticChoice(string clubName)
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine($"{clubName}: kies je tactiek voor deze fase:");
                Console.WriteLine("1) Aanvallend");
                Console.WriteLine("2) Defensief");
                Console.WriteLine("3) Gebalanceerd");

                var key = Console.ReadKey(true).KeyChar;
                Console.WriteLine();

                switch (key)
                {
                    case '1': return TacticChoice.Attack;
                    case '2': return TacticChoice.Defend;
                    case '3': return TacticChoice.Balanced;
                }

                Console.WriteLine("Ongeldige keuze, probeer opnieuw.");
            }
        }

        private void ApplyTactic(TacticChoice choice, ref int ownStrength, ref int opponentStrength)
        {
            switch (choice)
            {
                case TacticChoice.Attack:
                    ownStrength += 2;      // meer kans op scoren
                    opponentStrength += 1; // je laat ruimte
                    break;
                case TacticChoice.Defend:
                    ownStrength -= 1;      // minder eigen kansen
                    opponentStrength -= 2; // maar ook minder tegenkansen
                    break;
                case TacticChoice.Balanced:
                    // geen wijziging
                    break;
            }

            if (ownStrength < 0) ownStrength = 0;
            if (opponentStrength < 0) opponentStrength = 0;
        }

        private bool RollChanceToScore(int effectiveStrength)
        {
            if (effectiveStrength <= 0)
                effectiveStrength = 1;

            int chance = 10 + effectiveStrength * 3;
            int roll = Random.Shared.Next(0, 100);
            return roll < chance;
        }

        private void ApplyResultToTable(Fixture fixture)
        {
            if (fixture.HomeScore > fixture.AwayScore)
            {
                UpdateClubStats(fixture.HomeTeam.Id, fixture.HomeScore, fixture.AwayScore, 3);
                UpdateClubStats(fixture.AwayTeam.Id, fixture.AwayScore, fixture.HomeScore, 0);
            }
            else if (fixture.HomeScore < fixture.AwayScore)
            {
                UpdateClubStats(fixture.HomeTeam.Id, fixture.HomeScore, fixture.AwayScore, 0);
                UpdateClubStats(fixture.AwayTeam.Id, fixture.AwayScore, fixture.HomeScore, 3);
            }
            else
            {
                UpdateClubStats(fixture.HomeTeam.Id, fixture.HomeScore, fixture.AwayScore, 1);
                UpdateClubStats(fixture.AwayTeam.Id, fixture.AwayScore, fixture.HomeScore, 1);
            }
        }

        private void UpdateClubMomentum(Guid clubId, int goalsFor, int goalsAgainst)
        {
            var club = _clubs.FirstOrDefault(_ => _.Id == clubId);
            if (club == null) return;

            var resultChar = goalsFor > goalsAgainst ? 'W' :
                             goalsFor < goalsAgainst ? 'L' : 'D';

            var last = club.Last5Games ?? "";

            last = (last + resultChar);

            if (last.Length > 5)
                last = last[^5..]; // pak laatste 5 chars

            club.Last5Games = last;
        }

        private void UpdateClubStats(Guid clubId, int goalsFor, int goalsAgainst, int points)
        {
            var record = _clubLeagueCompetitions.First(_ => _.ClubId == clubId);
            
            record.GoalsFor += goalsFor;
            record.GoalsAgainst += goalsAgainst;
            record.Points += points;
            if (goalsFor > goalsAgainst)
            {
                record.Won++;
            }
            else if (goalsAgainst > goalsFor)
            {
                record.Lost++;
            }
            else
            {
                record.Draw++;
            }
            record.MatchesPlayed++;
        }

        public Guid ChoosePlayerClub()
        {
            // Haal clubs op
            var clubs = _clubService.GetClubs();

            Console.WriteLine("Kies je club:");
            for (int i = 0; i < clubs.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {clubs[i].Name}");
            }

            Console.Write("\nGeef het nummer van je club: ");
            var input = Console.ReadLine();

            if (int.TryParse(input, out int chosenIndex) &&
                chosenIndex > 0 &&
                chosenIndex <= clubs.Count)
            {

                Console.WriteLine($"Je hebt gekozen: {clubs[chosenIndex - 1].Name}");
                return clubs[chosenIndex - 1].Id;
            }
            else
            {
                Console.WriteLine("Ongeldige keuze. Standaardclub wordt gebruikt.");
                // fallback club
                return clubs[0].Id;
            }
        }

        public IList<Fixture> InitializeInternationalGames()
        {
            var competitions = _competitionRepository.Load()
                .Where(_ => _.Tier == 1 && _.Type == Competition.CompetitionType.League);
            var internationalCompetition = _competitionRepository.Load()
                .First(_ => _.Type == Competition.CompetitionType.International);

            var leagueWinners = new List<ClubPerCompetition>();

            foreach (var competition in competitions)
            {
                var leagueWinner = _clubLeagueCompetitions
                    .Where(_ => _.CompetitionId == competition.Id)
                    .OrderByDescending(_ => _.Points)
                    .ThenByDescending(_ => _.GoalsFor - _.GoalsAgainst)
                    .ThenByDescending(_ => _.GoalsFor)
                    .First();

                leagueWinners.Add(new ClubPerCompetition
                {
                    ClubId = leagueWinner.ClubId,
                    CompetitionId = internationalCompetition.Id
                });
            }

            // Cup aanmaken voor de league-winnaars
            var fixtures = _fixtureService.GenerateCupFixtures(leagueWinners, internationalCompetition);

            // Belangrijk: zorg dat MatchDay gezet is (bv. gelijk aan RoundNo),
            // zodat je bestaande PlayMatchDay-logica hergebruikt kan worden.
            foreach (var f in fixtures)
            {
                if (f.MatchDay == 0)
                    f.MatchDay = f.RoundNo;
            }

            return fixtures;
        }

        public IList<Fixture> InitializeNationalCups()
        {
            var competitions = _competitionRepository.Load();
            var fixtures = new List<Fixture>();
            // All cup competitions grouped by country
            var cupsPerCountry = competitions
                .Where(c => c.Type == Competition.CompetitionType.Cup)
                .GroupBy(c => c.CountryId);

            foreach (var cupGroup in cupsPerCountry)
            {
                var countryId = cupGroup.Key;
                var cup = cupGroup.First(); // assume 1 cup per country for now

                // Find all league clubs in that country
                var leagueCompetitionsInCountry = competitions
                    .Where(c => c.CountryId == countryId && c.Type == Competition.CompetitionType.League)
                    .ToList();

                var cupClubs = _clubLeagueCompetitions
                    .Where(cl => leagueCompetitionsInCountry.Any(lc => lc.Id == cl.CompetitionId))
                    .Select(cl => new ClubPerCompetition
                    {
                        ClubId = cl.ClubId,
                        CompetitionId = cup.Id
                    })
                    .ToList();

                if (cupClubs.Count < 2)
                    continue; // nothing to do

                var cupFixtures = _fixtureService.GenerateCupFixtures(cupClubs, cup);

                fixtures.AddRange(cupFixtures);
            }

            return fixtures;
        }
    }
}
