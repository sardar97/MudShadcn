#nullable enable

using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Interfaces;

namespace MudShadcn.Showcase.Docs;

/// <summary>
/// The frame of a docs page: content, previous/next links and the "On this page" navigation on the
/// right, which <see cref="SectionHeader"/>s register themselves with. Mirrors MudBlazor.Docs' DocsPage.
/// </summary>
public partial class DocsPage
{
    private readonly Queue<(DocsSectionLink Link, DocsPageSection? Section)> _pending = new();
    private readonly Dictionary<DocsPageSection, MudPageContentSection> _sections = [];
    private MudPageContentNavigation? _contentNavigation;
    private bool _tocOpen = true;
    private string? _anchor;

    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>Accepted for source compatibility with MudBlazor's pages; the footer is always the same.</summary>
    [Parameter] public bool DisplayFooter { get; set; }

    protected override void OnInitialized()
    {
        var fragment = NavigationManager.ToAbsoluteUri(NavigationManager.Uri).Fragment.TrimStart('#');
        _anchor = string.IsNullOrEmpty(fragment) ? null : fragment;
    }

    protected override Task OnAfterRenderAsync(bool firstRender) =>
        firstRender ? FlushAsync() : Task.CompletedTask;

    public string GetParentTitle(DocsPageSection? section) =>
        section?.ParentSection is { } parent && _sections.TryGetValue(parent, out var info) ? info.Title : string.Empty;

    internal Task AddSectionAsync(DocsSectionLink link, DocsPageSection? section)
    {
        _pending.Enqueue((link, section));
        return FlushAsync();
    }

    private async Task FlushAsync()
    {
        if (_contentNavigation is null)
        {
            return;
        }

        var added = false;
        while (_pending.TryDequeue(out var item))
        {
            if (_contentNavigation.Sections.Any(s => s.Id == item.Link.Id))
            {
                continue;
            }

            MudPageContentSection? parent = null;
            if (item.Section?.ParentSection is { } parentSection)
            {
                _sections.TryGetValue(parentSection, out parent);
            }

            var info = new MudPageContentSection(item.Link.Title, item.Link.Id, item.Section?.Level ?? 0, parent);
            if (item.Section is not null)
            {
                _sections.TryAdd(item.Section, info);
            }

            _contentNavigation.AddSection(info, false);
            added = true;

            if (_anchor == item.Link.Id)
            {
                _anchor = null;
                await _contentNavigation.ScrollToSection(new Uri(NavigationManager.Uri));
            }
        }

        if (added)
        {
            ((IMudStateHasChanged)_contentNavigation).StateHasChanged();
        }
    }
}
