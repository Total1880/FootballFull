using FootballFull.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Repositories.Interfaces
{
    public interface IRepository<T>
    {
        IList<T> Create(IList<T> itemList, bool full = false);
        IList<T> Load();
        void Add(T item);
        void Update(T updateItem);
        void Delete(Guid id);
    }
}
