using FootballFull.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services.Interfaces
{
    public interface IEndOfSeasonService
    {
        EndOfSeasonOptions GetOptions(
            Guid countryId,
            FootballAssociation footballAssociation);

        IList<Club> GetApplicantClubs(
            Guid countryId,
            FootballAssociation footballAssociation,
            int numberOfApplicants);

        void AdmitClub(
            Guid countryId,
            Guid clubId,
            FootballAssociation footballAssociation);

        void CreateLowerDivision(
            Guid countryId,
            int numberOfClubs,
            FootballAssociation footballAssociation);

        Club CreateApplicantClub(Guid countryId, string clubName);
    }
}
