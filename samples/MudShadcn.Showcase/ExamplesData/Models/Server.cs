// Ported from MudBlazor.Examples.Data (MudBlazor 9.10.0, MIT). See THIRD-PARTY-NOTICES.md.
#nullable enable

using System;

namespace MudBlazor.Examples.Data.Models;

public class Server
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Location { get; set; }

    public string? IpAddress { get; set; }
}
