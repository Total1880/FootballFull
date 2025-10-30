using FootballFull.Models;
using FootballFull.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services
{

    public class ClubService
    {
        public List<Club> GetClubs()
        {
            var clubrepository = new ClubRepository();
            return clubrepository.Load();
        }
    }
}
