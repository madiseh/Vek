using System.Collections.Generic;
using System.Threading.Tasks;
using Vek.Examples.Data.Models;

namespace Vek.Examples.Data;

public interface IPeriodicTableService
{
    Task<IEnumerable<Element>> GetElements();

    Task<IEnumerable<Element>> GetElements(string search);
}
