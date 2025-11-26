using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Models
{
    public class Name
    {
        public Guid CountryId {  get; set; }
        public string Value { get; set; }
        public TypeName TypeName { get; set; }

    }

    public enum TypeName
    {
        FirstName, LastName
    }
}
