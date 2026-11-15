using System.Collections.Generic;

namespace Vek.Examples.Data.Models;

public class ElementGroup
{
    public string? Wiki { get; set; }

    public IReadOnlyCollection<Element>? Elements { get; set; }
}
