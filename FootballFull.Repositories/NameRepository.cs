using FootballFull.Models;
using FootballFull.Repositories.Interfaces;

namespace FootballFull.Repositories
{
    public class NameRepository : INameRepository
    {
        private IList<Name> _firstNames;
        private IList<Name> _lastNames;
        public string GetRandomFirstName(Guid CountryId)
        {
            _firstNames = new List<Name>();
            _firstNames.Add(new Name { TypeName = TypeName.FirstName, Value = "Noah" });
            _firstNames.Add(new Name { TypeName = TypeName.FirstName, Value = "Arthur" });
            _firstNames.Add(new Name { TypeName = TypeName.FirstName, Value = "Jules" });
            _firstNames.Add(new Name { TypeName = TypeName.FirstName, Value = "Louis" });
            _firstNames.Add(new Name { TypeName = TypeName.FirstName, Value = "Liam" });
            _firstNames.Add(new Name { TypeName = TypeName.FirstName, Value = "Rémus" });

            var value = Random.Shared.Next(0, _firstNames.Count);
            return _firstNames[value].Value;
        }

        public string GetRandomLastName(Guid CountryId)
        {
            _lastNames = new List<Name>();
            _lastNames.Add(new Name { TypeName = TypeName.LastName, Value = "Peeters" });
            _lastNames.Add(new Name { TypeName = TypeName.LastName, Value = "Janssens" });
            _lastNames.Add(new Name { TypeName = TypeName.LastName, Value = "Maes" });
            _lastNames.Add(new Name { TypeName = TypeName.LastName, Value = "Jacobs" });
            _lastNames.Add(new Name { TypeName = TypeName.LastName, Value = "Willems" });
            _lastNames.Add(new Name { TypeName = TypeName.LastName, Value = "Mertens" });
            _lastNames.Add(new Name { TypeName = TypeName.LastName, Value = "Hendrickx" });

            var value = Random.Shared.Next(0, _lastNames.Count);
            return _lastNames[value].Value;
        }
    }
}
