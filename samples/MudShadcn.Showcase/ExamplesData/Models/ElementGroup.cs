// Ported from MudBlazor.Examples.Data (MudBlazor 9.10.0, MIT). See THIRD-PARTY-NOTICES.md.
#nullable enable

using System.Collections.Generic;

namespace MudBlazor.Examples.Data.Models;

public class ElementGroup
{
    public string? Wiki { get; set; }

    public IReadOnlyCollection<Element>? Elements { get; set; }
}
