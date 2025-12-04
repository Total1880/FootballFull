namespace FootballFull.Models
{
    public class CountryCoefficientRanking
    {
        public Guid CountryId { get; set; }
        public Country Country { get; set; }

        /// <summary>
        /// Totale coefficient over de laatste 5 jaar,
        /// berekend als som van: (punten in jaar X / # deelnemende clubs in jaar X)
        /// </summary>
        public double FiveYearCoefficient { get; set; }

        /// <summary>
        /// Detail per jaar: (punten van dat jaar) / (# clubs van dat jaar)
        /// </summary>
        public IDictionary<int, double> CoefficientPerYear { get; set; }
            = new Dictionary<int, double>();

        /// <summary>
        /// Hoeveel clubs uit dit land hadden punten in dit jaar.
        /// </summary>
        public IDictionary<int, int> ClubsParticipatingPerYear { get; set; }
            = new Dictionary<int, int>();

        /// <summary>
        /// Ruwe som van punten per jaar (zonder deling).
        /// </summary>
        public IDictionary<int, int> RawPointsPerYear { get; set; }
            = new Dictionary<int, int>();
    }
}
