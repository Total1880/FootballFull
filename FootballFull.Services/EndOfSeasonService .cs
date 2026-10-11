using FootballFull.Models;
using FootballFull.Services.Interfaces;
using OlavFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services
{
    public class EndOfSeasonService : IEndOfSeasonService
    {
        private const int MaximumNumberOfClubs = 100;

        private readonly ISeasonService _seasonService;
        private readonly IClubService _clubService;
        private readonly ITrainerService _trainerService;
        private readonly ICompetitionService _competitionService;
        private readonly ICompetitionRulesService _competitionRulesService;
        private readonly IClubPerCompetitionService _clubPerCompetitionService;
        private readonly IClubFinancialService _clubFinancialService;
        private readonly IHistoryService _historyService;
        private readonly IPrizeMoneyService _prizeMoneyService;
        private readonly IFootballAssociationsService _associations;

        public EndOfSeasonService(
            ISeasonService seasonService,
            IClubService clubService,
            ITrainerService trainerService,
            ICompetitionService competitionService,
            ICompetitionRulesService competitionRulesService,
            IClubPerCompetitionService clubPerCompetitionService,
            IClubFinancialService clubFinancialService,
            IHistoryService historyService,
            IPrizeMoneyService prizeMoneyService,
            IFootballAssociationsService associations)
        {
            _seasonService = seasonService;
            _clubService = clubService;
            _trainerService = trainerService;
            _competitionService = competitionService;
            _competitionRulesService = competitionRulesService;
            _clubPerCompetitionService = clubPerCompetitionService;
            _clubFinancialService = clubFinancialService;
            _historyService = historyService;
            _prizeMoneyService = prizeMoneyService;
            _associations = associations;
        }

        public EndOfSeasonOptions GetOptions(Guid countryId, FootballAssociation footballAssociation)
        {
            var clubCount = _clubPerCompetitionService
                .GetAllClubPerCompetitionForCountry(countryId)
                .Count;

            var competitions = _competitionService
                .GetCompetitionsForCountry(countryId);

            // Get lowest competition tier
            var lowestTier = competitions.Max(c => c.Tier);
            var lowestTierCompetitions = competitions.Where(_ => _.Tier == lowestTier);

            // count clubs in lowest tier
            var lowestTierClubCount = 0;
            foreach (var competition in lowestTierCompetitions)
                lowestTierClubCount += _clubPerCompetitionService.GetAllClubPerSpecificCompetitions(competition.Id).Count;

            // GetRequiredReputationForNextTier of zoeits maken, ook om max tier te controleren
            var requiredReputationForNextTier = GetRequiredReputationForNextTier(lowestTier);
            var requiredReputationForNextClub = GetRequiredReputationForNextClub(clubCount);

            var canAddClub =
                clubCount < MaximumNumberOfClubs &&
                footballAssociation.Balance >= Configuration.NewClubCost &&
                footballAssociation.Reputation >= requiredReputationForNextClub;

            var canCreateLowerDivision =
                lowestTierClubCount >= 8 &&
                footballAssociation.Balance >= Configuration.LowerDivisionCost &&
                footballAssociation.Reputation >= requiredReputationForNextTier;

            var canCreateNationalCup =
                footballAssociation.Balance >= Configuration.StartNationalCupCost &&
                footballAssociation.Reputation >= Configuration.ReputationForStartNationalCup &&
                !competitions.Any(_ => _.Type == Competition.CompetitionType.Cup);

            return new EndOfSeasonOptions
            {
                CurrentClubCount = clubCount,
                CanAddClub = canAddClub,
                CanCreateLowerDivision = canCreateLowerDivision,
                CanCreateNationalCup = canCreateNationalCup,
                CannotAddClubReason = canAddClub
                    ? null
                    : GetCannotAddClubReason(
                        clubCount,
                        requiredReputationForNextClub,
                        footballAssociation),
                CannotCreateLowerDivisionReason = canCreateLowerDivision
                    ? null
                    : "Je voldoet nog niet aan de voorwaarden voor een lagere divisie divisie.",
                CannotCreateNationalCupReason = canCreateNationalCup
                    ? null
                    : "Je voldoet niet aan de voorwaarden voor een nieuwe nationale beker."
            };
        }

        private static int GetRequiredReputationForNextTier(int lowestTier)
        {
            return lowestTier switch
            {
                >= 4 => 50,
                >= 3 => 40,
                >= 2 => 30,
                >= 1 => 20,
                _ => 0
            };
        }

        private static int GetRequiredReputationForNextClub(int clubCount)
        {
            return clubCount switch
            {
                >= 7 => 15,
                >= 6 => 10,
                _ => 0
            };
        }

        private static string GetCannotAddClubReason(int clubCount, int requiredReputation, FootballAssociation footballAssociation)
        {
            if (clubCount >= MaximumNumberOfClubs)
                return $"Er zijn al {MaximumNumberOfClubs} clubs.";

            if (footballAssociation.Balance < Configuration.NewClubCost)
                return "Je hebt onvoldoende saldo.";

            if (footballAssociation.Reputation < requiredReputation)
                return $"Je hebt minstens {requiredReputation} reputatie nodig.";

            return "Je kan momenteel geen club toelaten.";
        }

        public IList<Club> GetApplicantClubs(Guid countryId, FootballAssociation footballAssociation, int numberOfApplicants)
        {
            if (countryId == Guid.Empty)
                throw new ArgumentException(
                    "CountryId mag niet leeg zijn.",
                    nameof(countryId));

            if (footballAssociation == null)
                throw new ArgumentNullException(
                    nameof(footballAssociation));

            if (footballAssociation.CountryId != countryId)
                throw new ArgumentException(
                    "De voetbalbond behoort niet tot het opgegeven land.",
                    nameof(footballAssociation));

            if (numberOfApplicants <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(numberOfApplicants),
                    "Het aantal kandidaten moet groter zijn dan nul.");

            var options = GetOptions(countryId, footballAssociation);

            if (!options.CanAddClub)
            {
                throw new InvalidOperationException(
                    options.CannotAddClubReason ??
                    "Er kan momenteel geen extra club worden toegelaten.");
            }

            var existingClubIds = _clubPerCompetitionService
                .GetAllClubPerCompetitionForCountry(countryId)
                .Select(cpc => cpc.ClubId)
                .ToHashSet();

            return _clubService
                .GetEndOfSeasonRequestClubs(
                    countryId,
                    numberOfApplicants,
                    existingClubIds.ToList())
                .Take(numberOfApplicants)
                .ToList();
        }

        public void AdmitClub(Guid countryId, Guid clubId, FootballAssociation footballAssociation)
        {
            var options = GetOptions(countryId, footballAssociation);

            if (!options.CanAddClub)
                throw new InvalidOperationException(options.CannotAddClubReason);

            var lowestCompetition = _competitionService
                .GetCompetitionsForCountry(countryId)
                .OrderByDescending(c => c.Tier)
                .First();

            _trainerService.CreateRandomTrainer(clubId);

            _clubPerCompetitionService.AddClubToCompetition(
                clubId,
                lowestCompetition.Id);

            footballAssociation.Balance -= Configuration.NewClubCost;
        }

        public void CreateLowerDivision(Guid countryId, int numberOfClubs, FootballAssociation footballAssociation)
        {
            var options = GetOptions(countryId, footballAssociation);

            if (!options.CanCreateLowerDivision)
                throw new InvalidOperationException(
                    options.CannotCreateLowerDivisionReason);

            if (numberOfClubs <= 0 || numberOfClubs >= options.CurrentClubCount)
                throw new ArgumentOutOfRangeException(nameof(numberOfClubs));

            var competitions = _competitionService
                .GetCompetitionsForCountry(countryId);
            var lowestTier = competitions.Max(c => c.Tier);

            var lowestDivision = competitions.Single(c => c.Tier == lowestTier);

            var ranking = _seasonService.GetRanking(lowestDivision.Id);
            var clubsToMove = ranking.TakeLast(numberOfClubs).ToList();

            var newDivision = new Competition
            {
                Id = Guid.NewGuid(),
                Name = "Division " + (lowestTier + 1),
                CountryId = countryId,
                Tier = lowestTier + 1,
                Type = Competition.CompetitionType.League,
                Strength = Configuration.MinStrength
            };

            _competitionService.Add(newDivision);

            var oldLowestDivisionRules = _competitionRulesService.GetCompetitionRules(lowestDivision.Id);
            oldLowestDivisionRules.CompetitionRelegationToId = newDivision.Id;
            oldLowestDivisionRules.RelegationPlaces = 1;

            _competitionRulesService.Save(oldLowestDivisionRules);

            _competitionRulesService.Save(new CompetitionRules
            {
                CompetitionId = newDivision.Id,
                CompetitionPromotionToId = lowestDivision.Id,
                PromotionPlaces = 1
            });

            foreach (var club in clubsToMove)
            {
                _clubPerCompetitionService.RemoveClubFromCompetition(
                    club.ClubId,
                    club.CompetitionId);

                _clubPerCompetitionService.AddClubToCompetition(
                    club.ClubId,
                    newDivision.Id);
            }

            footballAssociation.Balance -= Configuration.LowerDivisionCost;
        }

        public Club CreateApplicantClub(Guid countryId, string clubName)
        {
            if (countryId == Guid.Empty)
                throw new ArgumentException(
                    "CountryId mag niet leeg zijn.",
                    nameof(countryId));

            if (string.IsNullOrWhiteSpace(clubName))
                throw new ArgumentException(
                    "De naam van de club mag niet leeg zijn.",
                    nameof(clubName));

            var normalizedName = clubName.Trim();

            var clubAlreadyExists = _clubService
                .GetClubs()
                .Any(club =>
                    club.CountryId == countryId &&
                    string.Equals(
                        club.Name,
                        normalizedName,
                        StringComparison.OrdinalIgnoreCase));

            if (clubAlreadyExists)
                throw new InvalidOperationException(
                    $"Er bestaat al een club met de naam '{normalizedName}'.");

            var club = new Club
            {
                Id = Guid.NewGuid(),
                Name = normalizedName,
                CountryId = countryId,
                Strength = Configuration.MinStrength
            };

            _clubService.Add(club);

            _trainerService.CreateRandomTrainer(club.Id);

            return club;
        }

        public void ProcessClubFinances(Guid countryId)
        {
            var clubsPerCompetition =
                _clubPerCompetitionService
                    .GetAllClubPerCompetitionForCountry(countryId);

            // A club can also appear in cups or split phases: book one domestic league season.
            var leagues = _competitionService.GetCompetitionsForCountry(countryId)
                .Where(c => c.Type == Competition.CompetitionType.League)
                .ToDictionary(c => c.Id);
            var memberships = clubsPerCompetition
                .Where(cpc => leagues.ContainsKey(cpc.CompetitionId))
                .OrderBy(cpc => leagues[cpc.CompetitionId].Tier)
                .GroupBy(cpc => cpc.ClubId)
                .Select(group => group.First());

            foreach (var cpc in memberships)
            {
                var club = _clubService.GetClubById(cpc.ClubId);
                var competition = leagues[cpc.CompetitionId];
                if (club == null)
                    continue;

                var result =
                    _clubFinancialService.CalculateSeasonResult(
                        club,
                        competition);

                result.SubsidyReceived =
    club.SubsidySeason == _seasonService.Year
        ? club.SeasonSubsidyReceived
        : 0m;

                result.PrizeMoneyReceived = _competitionService.GetCompetitions()
                    .SelectMany(c => c.PrizeMoneySeasons)
                    .Where(s => s.Year == _seasonService.Year)
                    .SelectMany(s => s.Payments)
                    .Where(p => p.ClubId == club.Id)
                    .Sum(p => p.Amount);

                // Subsidies and prizes have already been credited to the balance.
                club.Balance += result.NetResult - result.SubsidyReceived - result.PrizeMoneyReceived;
                club.LastSeasonFinancialResult = result;

                _clubService.Update(club);
            }
        }

        public void ReservePrizeMoney(Competition competition, CompetitionPrizeMoneySettings settings,
            FootballAssociation association, int year)
        {
            if (competition.CountryId != association.CountryId)
                throw new ArgumentException("De competitie behoort niet tot deze voetbalbond.");
            if (competition.PrizeMoneySeasons.Any(s => s.Year == year))
                throw new InvalidOperationException("De prijzenpot is al vastgelegd voor dit seizoen.");

            _prizeMoneyService.ValidateSettings(settings, competition.Type);
            if (settings.TotalBudget > 0 && competition.Type == Competition.CompetitionType.Cup &&
                settings.Distribution == PrizeMoneyDistribution.SemiFinalists)
            {
                var leagueIds = _competitionService.GetCompetitionsForCountry(association.CountryId)
                    .Where(c => c.Type == Competition.CompetitionType.League).Select(c => c.Id).ToHashSet();
                var clubCount = _clubPerCompetitionService.GetAllClubPerCompetitionForCountry(association.CountryId)
                    .Where(c => leagueIds.Contains(c.CompetitionId)).Select(c => c.ClubId).Distinct().Count();
                if (clubCount < 4)
                    throw new InvalidOperationException("Voor halvefinalepremies zijn minstens vier clubs nodig.");
            }


            if (settings.TotalBudget > 0 && settings.TotalBudget > association.Balance)
                throw new InvalidOperationException("Je hebt onvoldoende bondsbudget voor deze prijzenpot.");

            competition.PrizeMoneySettings = settings;
            competition.PrizeMoneySeasons.Add(new CompetitionPrizeMoneySeason
            {
                Year = year,
                Settings = new CompetitionPrizeMoneySettings
                {
                    TotalBudget = settings.TotalBudget,
                    Distribution = settings.Distribution
                }
            });
            association.Balance -= settings.TotalBudget;
            _associations.Update(association);
            _competitionService.Update(competition);
        }

        public void ProcessPrizeMoney(IList<Fixture> cupFixtures, IList<Fixture>? internationalFixtures)
        {
            foreach (var competition in _competitionService.GetCompetitions().ToList())
            {
                var season = competition.PrizeMoneySeasons
                    .SingleOrDefault(s => s.Year == _seasonService.Year);
                if (season == null || season.IsPaid)
                    continue;

                IReadOnlyList<ClubPrizeMoney> prizes;
                if (season.Settings.TotalBudget == 0)
                    prizes = Array.Empty<ClubPrizeMoney>();
                else if (competition.Type == Competition.CompetitionType.League)
                {
                    var ranking = _seasonService.GetRanking(competition.Id);
                    if (ranking.Count == 0)
                        throw new InvalidOperationException($"Geen eindstand voor {competition.Name}.");
                    prizes = _prizeMoneyService.CalculateLeaguePrizes(
                        season.Settings, ranking.Select(r => r.ClubId).ToList());
                }
                else
                {
                    var fixtures = competition.Type == Competition.CompetitionType.Cup
                        ? cupFixtures : internationalFixtures ?? new List<Fixture>();
                    var games = fixtures.Where(f => f.CompetitionId == competition.Id).ToList();
                    if (games.Count == 0)
                        throw new InvalidOperationException($"Geen bekerresultaten voor {competition.Name}.");
                    var finalRound = games.Max(f => f.RoundNo);
                    var finals = games.Where(f => f.RoundNo == finalRound).ToList();
                    if (finals.Count != 1 || finals[0].HomeScore == finals[0].AwayScore)
                        throw new InvalidOperationException($"Geen eenduidige bekerwinnaar voor {competition.Name}.");
                    var final = finals[0];
                    var winner = final.HomeScore > final.AwayScore ? final.HomeTeamId : final.AwayTeamId;
                    var runnerUp = final.HomeScore > final.AwayScore ? final.AwayTeamId : final.HomeTeamId;
                    var losers = new List<Guid>();
                    if (season.Settings.Distribution == PrizeMoneyDistribution.SemiFinalists)
                    {
                        var semis = games.Where(f => f.RoundNo == finalRound - 1).ToList();
                        if (semis.Count != 2 || semis.Any(f => f.HomeScore == f.AwayScore))
                            throw new InvalidOperationException($"Geen twee besliste halve finales voor {competition.Name}.");
                        losers = semis.Select(f => f.HomeScore > f.AwayScore ? f.AwayTeamId : f.HomeTeamId).ToList();
                        if (losers.Contains(winner) || losers.Contains(runnerUp))
                            throw new InvalidOperationException("Ongeldige halvefinaleresultaten.");
                    }
                    prizes = _prizeMoneyService.CalculateCupPrizes(season.Settings, winner, runnerUp, losers);
                }

                // Resolve every recipient before changing any balance.
                var recipients = prizes.Select(p => (Prize: p, Club: _clubService.GetClubById(p.ClubId)
                    ?? throw new InvalidOperationException($"Club {p.ClubId} ontbreekt."))).ToList();
                foreach (var recipient in recipients)
                {
                    // Payments are recorded per club so a retry does not book it twice.
                    if (!recipient.Club.PrizeMoneyPayments.Any(p =>
                        p.CompetitionId == competition.Id && p.Year == season.Year))
                    {
                        recipient.Club.Balance += recipient.Prize.Amount;
                        recipient.Club.PrizeMoneyPayments.Add(new ClubPrizeMoneyPayment
                        {
                            CompetitionId = competition.Id,
                            Year = season.Year,
                            Amount = recipient.Prize.Amount
                        });
                        _clubService.Update(recipient.Club);
                    }
                    if (!season.Payments.Any(p => p.ClubId == recipient.Prize.ClubId))
                        season.Payments.Add(recipient.Prize);
                    _competitionService.Update(competition);
                }
                season.IsPaid = true;
                _competitionService.Update(competition);
            }
        }

        public void ProcessSeasonResults(
            IList<Fixture> cupFixtures,
            IList<Fixture>? internationalFixtures)
        {
            _historyService.SaveSeasonHistory(
                cupFixtures,
                internationalFixtures);
        }

        public void CreateNationalCup(Guid countryId, FootballAssociation association)
        {
            var options = GetOptions(countryId, association);

            if (!options.CanCreateNationalCup)
                throw new InvalidOperationException(
                    options.CannotCreateLowerDivisionReason);

            var newCup = new Competition
            {
                CountryId = countryId,
                Type = Competition.CompetitionType.Cup,
                Name = "National Cup"
            };

            _competitionService.Add(newCup);

            association.Balance -= Configuration.StartNationalCupCost;
        }
    }
}
