using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;
using OlavFramework;

namespace FootballFull.Services
{
    public class SeasonService : ISeasonService
    {
        private IRepository<Competition> _competitionRepository;
        private readonly ITrainerService _trainerService;
        private IClubService _clubService;
        private IFixtureService _fixtureService;
        private ICountryService _countryService;
        private ICompetitionService _competitionService;
        private IClubPerCompetitionService _clubPerCompetitionService;
        private IList<ClubLeagueCompetition> _clubLeagueCompetitions;
        private IList<ClubPerCompetition> _clubsPerCompetition;
        private IList<Club> _clubs;
        private IList<Trainer> _trainers;
        private IList<NewsMessage> _newsMessages;
        private IList<ClubInternationalRanking> _clubInternationalRankings;
        private IList<Competition> _competitions;
        private IList<Country> _countries;
        private int _year;

        public IList<ClubLeagueCompetition> ClubLeagueCompetitions => _clubLeagueCompetitions;
        public IList<NewsMessage> NewsMessages => _newsMessages;
        public IList<ClubInternationalRanking> ClubInternationalRankings => _clubInternationalRankings;

        public int Year { get => _year; set => _year = value; }

        public SeasonService(
            IRepository<Competition> competitionRepository,
            IClubService clubService,
            IFixtureService fixtureService,
            ITrainerService trainerService,
            ICountryService countryService,
            ICompetitionService competitionService,
            IClubPerCompetitionService clubPerCompetitionService)
        {
            _competitionRepository = competitionRepository;
            _clubService = clubService;
            _fixtureService = fixtureService;
            _trainerService = trainerService;
            _countryService = countryService;
            _competitionService = competitionService;
            _clubPerCompetitionService = clubPerCompetitionService;

            _newsMessages = new List<NewsMessage>();
            _clubInternationalRankings = new List<ClubInternationalRanking>();
            _competitions = _competitionRepository.Load();
            _countries = _countryService.GetCountries();
        }
        public void Initialize(IList<ClubPerCompetition> clubsPerCompetition)
        {
            _clubsPerCompetition = clubsPerCompetition;
            _clubs = _clubService.GetClubs();
            _trainers = _trainerService.Load();

            InitializeNewSeason(0);
        }

        public void InitializeNewSeason(int year)
        {
            _year = year;
            RecalculateCompetitionStrenghts(Configuration.MinStrength, Configuration.MaxStrength);
            RecalculateClubStrengths(Configuration.MinStrength, Configuration.MaxStrength);
            PromotionsAndRelegations();
            _clubLeagueCompetitions = _clubsPerCompetition.Select(club => new ClubLeagueCompetition
            {
                ClubId = club.ClubId,
                Points = 0,
                GoalsFor = 0,
                GoalsAgainst = 0,
                CompetitionId = club.CompetitionId
            }).ToList();
            _clubs = _clubService.GetClubs();
            ResetClubRuntimeState();
        }

        private void RecalculateCompetitionStrenghts(int minStrength, int maxStrength)
        {
            var initial = Configuration.MaxStrength - Configuration.MinStrength;
            var countryRankings = _clubInternationalRankings
.GroupBy(c => c.CountryId)
.Select(g => new
{
    CountryId = g.Key,
    PointsPerClub = g.Average(c => c.TotalPoints(_year)) // = totaal / aantal clubs
})
.OrderByDescending(x => x.PointsPerClub)
.ToList();

            var competitions = _competitionRepository.Load().Where(_ => _.Type == Competition.CompetitionType.League);
            foreach (var country in countryRankings)
            {
                var current = initial;
                var competitionsCountry = competitions.Where(_ => _.CountryId == country.CountryId).OrderBy(_ => _.Tier).ToList();
                var step = current / (competitionsCountry.Count == 0 ? 1 : competitionsCountry.Count);

                foreach (var competition in competitionsCountry)
                {
                    competition.Strength = current;
                    _competitionRepository.Update(competition);
                    current -= step;

                }
                initial--;
            }
        }

        private void RecalculateClubStrengths(int minStrength = 1, int maxStrength = 9)
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

                    var competitionStrength = _competitionRepository.Load()
                        .First(c => c.Id == entry.CompetitionId).Strength;

                    if (delta < 0 && club.Strength + 3 <= competitionStrength)
                        if (club.Strength + 5 <= competitionStrength)
                            delta = 1;
                        else
                            continue;
                    if (delta > 0 && club.Strength - 3 >= competitionStrength)
                        if (club.Strength - 5 >= competitionStrength)
                            delta = -1;
                        else
                            continue;

                    var newStrength = club.Strength + delta;
                    if (newStrength < minStrength) newStrength = minStrength;
                    if (newStrength > maxStrength) newStrength = maxStrength;

                    club.Strength = newStrength;
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
                if (_competitions.Where(_ => _.Type == Competition.CompetitionType.International && _.Id == fixture.CompetitionId).Count() > 0)
                    ApplyResultToInternationalRanking(fixture);

                UpdateClubMomentumAndMorale(fixture.HomeTeamId, fixture.HomeScore, fixture.AwayScore);
                UpdateClubMomentumAndMorale(fixture.AwayTeamId, fixture.AwayScore, fixture.HomeScore);

                _clubs.First(_ => _.Id == fixture.HomeTeamId).HasTrainerSinceWeek++;
                _clubs.First(_ => _.Id == fixture.AwayTeamId).HasTrainerSinceWeek++;
            }
        }

        private void ApplyResultToInternationalRanking(Fixture fixture)
        {
            if (fixture.HomeScore > fixture.AwayScore)
            {
                _clubInternationalRankings.First(_ => _.ClubId == fixture.HomeTeamId).UpdatePoints(_year, 2);
            }
            else if (fixture.HomeScore < fixture.AwayScore)
            {
                _clubInternationalRankings.First(_ => _.ClubId == fixture.AwayTeamId).UpdatePoints(_year, 2);
            }
            else
            {
                _clubInternationalRankings.First(_ => _.ClubId == fixture.HomeTeamId).UpdatePoints(_year, 1);
                _clubInternationalRankings.First(_ => _.ClubId == fixture.AwayTeamId).UpdatePoints(_year, 1);
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

            int homeStrength, awayStrength, difference, homeNegativeEffects, awayNegativeEffects;
            CalculateStrength(fixture, out homeStrength, out awayStrength, out difference, out homeNegativeEffects, out awayNegativeEffects);

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
            int startHomeStrength, startAwayStrength, difference, homeNegativeEffects, awayNegativeEffects;
            CalculateStrength(fixture, out startHomeStrength, out startAwayStrength, out difference, out homeNegativeEffects, out awayNegativeEffects);

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

                }
                ;

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

        private void CalculateStrength(Fixture fixture, out int startHomeStrength, out int startAwayStrength, out int difference, out int homeNegativeEffects, out int awayNegativeEffects)
        {
            var homeMomentum = MapMomentumToModifier(_clubs.First(_ => _.Id == fixture.HomeTeamId).Momentum);
            var awayMomentum = MapMomentumToModifier(_clubs.First(_ => _.Id == fixture.AwayTeamId).Momentum);

            // --- Morale omzetten naar kleine buff ---
            int homeMoraleBonus = (_clubs.First(_ => _.Id == fixture.HomeTeamId).Morale - 5) / 2;
            int awayMoraleBonus = (_clubs.First(_ => _.Id == fixture.AwayTeamId).Morale - 5) / 2;

            // --- Trainer ophalen ---
            var homeTrainer = _trainers.FirstOrDefault(t => t.ClubId == fixture.HomeTeamId);
            var awayTrainer = _trainers.FirstOrDefault(t => t.ClubId == fixture.AwayTeamId);

            int homeTrainerBonus = 0;
            int awayTrainerBonus = 0;

            if (homeTrainer != null)
            {
                // Kleine buff: tactiek heeft iets meer impact dan motivatie
                homeTrainerBonus += homeTrainer.TacticalSkill / 2;        // max +2
                if (homeMoraleBonus < 0)
                    homeTrainerBonus += homeTrainer.Motivation >= 4 ? 1 : 0;   // +1 bij hoge motivatie
                else if (homeMoraleBonus == 0 && Random.Shared.Next(0, 2) == 0)
                    homeTrainerBonus += homeTrainer.Motivation >= 4 ? 1 : 0;   // +1 bij hoge motivatie
            }

            if (awayTrainer != null)
            {
                awayTrainerBonus += awayTrainer.TacticalSkill / 2;
                if (awayMoraleBonus < 0)
                    awayTrainerBonus += awayTrainer.Motivation >= 4 ? 1 : 0;
                else if (awayTrainerBonus == 0 && Random.Shared.Next(0, 2) == 0)
                    awayTrainerBonus += awayTrainer.Motivation >= 4 ? 1 : 0;   // +1 bij hoge motivatie
            }



            startHomeStrength = fixture.HomeTeam.Strength + homeMomentum + homeMoraleBonus + homeTrainerBonus;
            startAwayStrength = fixture.AwayTeam.Strength + awayMomentum + awayMoraleBonus + awayTrainerBonus - 1;
            difference = startHomeStrength - startAwayStrength;
            homeNegativeEffects = 0;
            awayNegativeEffects = 0;
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

        private bool IsCupWorthy(int count)
        {
            // Count moet > 1 zijn en een macht van 2
            return count > 1 && (count & (count - 1)) == 0;
        }

        public IList<Fixture> InitializeInternationalGames()
        {
            var competitions = _competitionRepository.Load()
                .Where(_ => _.Tier == 1 && _.Type == Competition.CompetitionType.League);
            var internationalCompetition = _competitionRepository.Load()
                .First(_ => _.Type == Competition.CompetitionType.International);

            var leagueWinners = new List<ClubPerCompetition>();
            var countryRankings = _clubInternationalRankings
    .GroupBy(c => c.CountryId)
    .Select(g => new
    {
        CountryId = g.Key,
        PointsPerClub = g.Average(c => c.TotalPoints(_year)) // = totaal / aantal clubs
    })
    .OrderByDescending(x => x.PointsPerClub)
    .ToList();
            var orderCountries = countryRankings.Select(_ => _.CountryId).ToList();
            if (orderCountries.Count == 0)
                orderCountries = _countries.Select(_ => _.Id).ToList();
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

                if (!_clubInternationalRankings.Any(_ => _.ClubId == leagueWinner.ClubId))
                    _clubInternationalRankings.Add(new ClubInternationalRanking
                    {
                        ClubId = leagueWinner.ClubId,
                        Club = _clubs.First(_ => _.Id == leagueWinner.ClubId),
                        CountryId = competition.CountryId,
                        Country = _countries.First(_ => _.Id == competition.CountryId)
                    });
            }

            var counterCountry = 0;
            var counterPosition = 1;
            while (!IsCupWorthy(leagueWinners.Count))
            {
                var country = orderCountries[counterCountry];
                var competitionId = competitions.First(_ => _.CountryId == country);
                var extra = _clubLeagueCompetitions
    .Where(_ => _.CompetitionId == competitionId.Id)
    .OrderByDescending(_ => _.Points)
    .ThenByDescending(_ => _.GoalsFor - _.GoalsAgainst)
    .ThenByDescending(_ => _.GoalsFor)
    .Skip(counterPosition)
    .First();
                leagueWinners.Add(new ClubPerCompetition
                {
                    ClubId = extra.ClubId,
                    CompetitionId = internationalCompetition.Id
                });

                if (!_clubInternationalRankings.Any(_ => _.ClubId == extra.ClubId))
                    _clubInternationalRankings.Add(new ClubInternationalRanking
                    {
                        ClubId = extra.ClubId,
                        Club = _clubs.First(_ => _.Id == extra.ClubId),
                        CountryId = country,
                        Country = _countries.First(_ => _.Id == country)
                    });

                counterCountry++;
                if (counterCountry > orderCountries.Count())
                {
                    counterCountry = 0;
                    counterPosition++;
                }
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

        private void ResetClubRuntimeState()
        {
            foreach (var c in _clubs) // of waar je je Club-lijst hebt
            {
                c.Last5Games = string.Empty;
                c.Morale = 5;
            }
        }

        private void UpdateClubMomentumAndMorale(Guid clubId, int goalsFor, int goalsAgainst)
        {
            var club = _clubs.First(_ => _.Id == clubId); // waar _clubs je lijst Clubs is

            // 1) Last5Games bijwerken (W/D/L)
            var resultChar = goalsFor > goalsAgainst ? 'W'
                           : goalsFor < goalsAgainst ? 'L'
                           : 'D';

            if (string.IsNullOrEmpty(club.Last5Games))
                club.Last5Games = resultChar.ToString();
            else
            {
                var last = club.Last5Games;
                if (last.Length >= 5)
                    last = last.Substring(1); // eerste droppen

                club.Last5Games = last + resultChar;
            }

            // 2) Morale aanpassen op basis van resultaat & doelpuntverschil
            int diff = goalsFor - goalsAgainst;

            if (diff > 0)
            {
                club.Morale += 1;
                if (diff >= 3)
                    club.Morale += 1; // grote winst
            }
            else if (diff < 0)
            {
                club.Morale -= 1;
                if (diff <= -3)
                    club.Morale -= 1; // zware nederlaag
            }
            else
            {
                // gelijkspel: lichte demping of niets
                // club.Morale += 0;
            }

            // 3) Clamp Morale tussen 1 en 10
            if (club.Morale < 1) club.Morale = 1;
            if (club.Morale > 10) club.Morale = 10;
        }
        public Trainer NewTrainer(Guid clubId, int matchDay = 0)
        {
            var existingTrainer = _trainers.FirstOrDefault(_ => _.ClubId == clubId);
            if (existingTrainer != null)
                existingTrainer.ClubId = Guid.Empty;
            var newTrainer = _trainerService.CreateRandomTrainer(clubId);
            AssignTrainer(clubId, newTrainer, matchDay);
            _trainers.Add(newTrainer);
            return newTrainer;
        }

        private void AssignTrainer(Guid clubId, Trainer trainer, int matchDay)
        {
            var club = _clubs.First(_ => _.Id == clubId);
            club.HasTrainerSinceWeek = 0;
            club.HasFiredTrainerInWeek = matchDay;
            if (trainer.ClubId != Guid.Empty && trainer.ClubId != clubId)
            {
                var nextClub = _clubs.First(_ => _.Id == trainer.ClubId);

                FindNewTrainer(nextClub, matchDay, nextClub.Strength, trainer.Id, false);
            }
            trainer.ClubId = clubId;
        }
        private void ClubsFireTrainer(Guid userClubId, int matchDay)
        {
            var clubs = _clubs.Where(_ => _.Id != userClubId && _.Momentum < 3 && _.Morale < 3 && _.HasTrainerSinceWeek > 5).ToList();

            foreach (var club in clubs)
            {
                var maxValue = 20 - club.HasTrainerSinceWeek;
                if (Random.Shared.Next(0, maxValue > 0 ? maxValue : 0) == 0)
                {
                    var firedTrainer = FireTrainer(club.Id, club.Strength);
                    var newTrainer = FindNewTrainer(club, matchDay, club.Strength, firedTrainer, true);
                }
            }
        }

        private Trainer? FindNewTrainer(Club club, int matchDay, int strength, Guid firedTrainerId, bool isFired = true)
        {
            var firedTrainer = _trainers.First(_ => _.Id == firedTrainerId);
            var message = new NewsMessage { ClubId = club.Id, CountryId = club.CountryId, MatchDay = matchDay, Year = _year, CompetitionId = _clubLeagueCompetitions.First(_ => _.ClubId == club.Id).CompetitionId };
            if (isFired)
                message.Message = $"{club.Name} fired its trainer, {firedTrainer.Name} {firedTrainer.LastName}.";
            else message.Message = $"{club.Name} lost its trainer, {firedTrainer.Name} {firedTrainer.LastName}.";

            var trainer = FindTrainerAtOtherClub(strength);

            if (trainer == null)
            {
                trainer = _trainers
                    .Where(_ => _.Id != firedTrainerId && _.ClubId == Guid.Empty && _.LastTeamStrength >= (strength - 5) && _.LastTeamStrength <= (strength + 5))
                    .OrderByDescending(_ => _.LastTeamStrength)
                    .FirstOrDefault();
                if (trainer != null)
                    message.Message += $"They found a new unemployed trainer, {trainer.Name} {trainer.LastName}";
            }

            if (trainer == null)
            {
                trainer = NewTrainer(club.Id, matchDay);
                message.Message += $"They found a new trainer, {trainer.Name} {trainer.LastName}";
            }
            else
            {
                if (trainer.ClubId != Guid.Empty)
                {
                    var oldClubName = _clubs.First(_ => _.Id == trainer.ClubId).Name;
                    message.Message += $"They took over trainer {trainer.Name} {trainer.LastName} from {oldClubName}";
                }
                AssignTrainer(club.Id, trainer, matchDay);

            }

            _newsMessages.Add(message);
            return trainer;
        }

        private Trainer? FindTrainerAtOtherClub(int strength)
        {
            var club = _clubs
                .Where(_ => _.Strength < strength && _.Momentum > 10 && _.HasTrainerSinceWeek > 5)
                .OrderByDescending(_ => _.Strength)
                .ToList()
                .FirstOrDefault();

            return _trainers.FirstOrDefault(_ => _.ClubId == club?.Id);
        }

        private Guid FireTrainer(Guid id, int strength)
        {
            var trainer = _trainers.First(_ => _.ClubId.Equals(id));
            trainer.ClubId = Guid.Empty;
            trainer.LastTeamStrength = strength;
            return trainer.Id;
        }

        public void UpdateWeekStats(Guid userClubId, int matchDay)
        {
            ClubsFireTrainer(userClubId, matchDay);
        }

        public Trainer UserTrainer(Guid userClubId)
        {
            return _trainers.First(_ => _.ClubId == userClubId);
        }

        public void SaveGame()
        {
            _clubService.SaveAll(_clubs);
            _competitionService.SaveAll(_competitions);
            _clubPerCompetitionService.SaveAll(_clubsPerCompetition);
            _trainerService.SaveAll(_trainers);
        }
    }
}
