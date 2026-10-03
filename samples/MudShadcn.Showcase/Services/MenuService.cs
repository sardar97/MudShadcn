#nullable enable

using MudBlazor;
using MudBlazor.Charts;

namespace MudShadcn.Showcase.Services;

/// <summary>A link in the components menu, or a group of them.</summary>
public sealed class DocsComponent
{
    public required string Name { get; init; }

    /// <summary>The route segment after <c>components/</c>, e.g. <c>buttonfab</c>.</summary>
    public string Link => Name.ToLowerInvariant().Replace(" ", "");

    public Type? Type { get; init; }

    public IReadOnlyList<Type> ChildTypes { get; init; } = [];

    public IReadOnlyList<DocsComponent> GroupComponents { get; init; } = [];

    public bool IsNavGroup => GroupComponents.Count > 0;

    public string Href => $"components/{Link}";
}

/// <summary>
/// The components menu of mudblazor.com, copied from MudBlazor.Docs' <c>MenuService</c> (9.10.0)
/// with the same entries, child types and sort: top-level entries and the entries of each group are
/// ordered by name, exactly as <c>DocsComponents.GetComponentsSortedByName</c> does.
/// </summary>
public sealed class MenuService
{
    public MenuService()
    {
        Components = new ComponentList()
            .Add("Container", typeof(MudContainer))
            .Add("Grid", typeof(MudGrid), typeof(MudItem))
            .Add("Hidden", typeof(MudHidden))
            .Add("Breakpoint Provider", typeof(MudBreakpointProvider))
            .Add("Chips", typeof(MudChip<>))
            .Add("Chip Set", typeof(MudChipSet<>))
            .Add("Badge", typeof(MudBadge))
            .Add("App Bar", typeof(MudAppBar))
            .Add("Drawer", typeof(MudDrawer), typeof(MudDrawerHeader), typeof(MudDrawerContainer))
            .Add("Drop Zone", typeof(MudDropZone<>), typeof(MudDropContainer<>), typeof(MudDynamicDropItem<>))
            .Add("Link", typeof(MudLink))
            .Add("Menu", typeof(MudMenu), typeof(MudMenuItem))
            .Add("Message Box", typeof(MudMessageBox))
            .Add("Nav Menu", typeof(MudNavMenu), typeof(MudNavLink), typeof(MudNavGroup))
            .Add("Tabs", typeof(MudTabs), typeof(MudTabPanel), typeof(MudDynamicTabs))
            .Add("Progress", typeof(MudProgressCircular), typeof(MudProgressLinear))
            .Add("Dialog", typeof(MudDialog), typeof(MudDialogContainer), typeof(MudDialogProvider))
            .Add("Snackbar", typeof(SnackbarService), typeof(MudSnackbarProvider), typeof(MudSnackbarElement))
            .Add("Avatar", typeof(MudAvatar), typeof(MudAvatarGroup))
            .Add("Alert", typeof(MudAlert))
            .Add("Card", typeof(MudCard), typeof(MudCardActions), typeof(MudCardContent), typeof(MudCardHeader), typeof(MudCardMedia))
            .Add("Divider", typeof(MudDivider))
            .Add("Expansion Panels", typeof(MudExpansionPanels), typeof(MudExpansionPanel))
            .Add("Image", typeof(MudImage))
            .Add("Icons", typeof(MudIcon))
            .Add("List", typeof(MudList<>), typeof(MudListItem<>), typeof(MudListSubheader))
            .Add("Paper", typeof(MudPaper))
            .Add("Rating", typeof(MudRating), typeof(MudRatingItem))
            .Add("Skeleton", typeof(MudSkeleton))
            .Add("Table", typeof(MudTable<>), typeof(MudTableBase), typeof(MudTablePager), typeof(MudTableGroupRow<>), typeof(MudTableSortLabel<>), typeof(MudTd), typeof(MudTh), typeof(MudTr), typeof(MudTFootRow), typeof(MudTHeadRow))
            .Add("Data Grid", typeof(MudDataGrid<>), typeof(Column<>), typeof(FilterHeaderCell<>), typeof(FooterCell<>), typeof(HeaderCell<>), typeof(HierarchyColumn<>), typeof(MudDataGridPager<>), typeof(TemplateColumn<>))
            .Add("Simple Table", typeof(MudSimpleTable))
            .Add("Tooltip", typeof(MudTooltip))
            .Add("Typography", typeof(MudText))
            .Add("Overlay", typeof(MudOverlay))
            .Add("Highlighter", typeof(MudHighlighter))
            .Add("Element", typeof(MudElement))
            .Add("Focus Trap", typeof(MudFocusTrap))
            .Add("Tree View", typeof(MudTreeView<>), typeof(MudTreeViewItem<>), typeof(MudTreeViewItemToggleButton))
            .Add("Breadcrumbs", typeof(MudBreadcrumbs))
            .Add("Scroll To Top", typeof(MudScrollToTop))
            .Add("Popover", typeof(MudPopover))
            .Add("Swipe Area", typeof(MudSwipeArea))
            .Add("Tool Bar", typeof(MudToolBar))
            .Add("Carousel", typeof(MudCarousel<>), typeof(MudCarouselItem))
            .Add("Timeline", typeof(MudTimeline), typeof(MudTimelineItem))
            .Add("Pagination", typeof(MudPagination))
            .Add("Stack", typeof(MudStack))
            .Add("Spacer", typeof(MudSpacer))
            .Add("Collapse", typeof(MudCollapse))
            .Add("Stepper", typeof(MudStepper), typeof(MudStep))
            .Add("Split Panel", typeof(MudSplitPanel))
            .Add("Exit Prompt", typeof(MudExitPrompt))
            .Add("Hotkey", typeof(MudHotkey))
            .AddGroup("Form & Inputs", new ComponentList()
                .Add("Radio", typeof(MudRadio<>), typeof(MudRadioGroup<>))
                .Add("Check Box", typeof(MudCheckBox<>))
                .Add("Select", typeof(MudSelect<>), typeof(MudSelectItem<>))
                .Add("Slider", typeof(MudSlider<>))
                .Add("Switch", typeof(MudSwitch<>))
                .Add("Text Field", typeof(MudTextField<>))
                .Add("Numeric Field", typeof(MudNumericField<>))
                .Add("Form", typeof(MudForm))
                .Add("Autocomplete", typeof(MudAutocomplete<>))
                .Add("Field", typeof(MudField))
                .Add("File Upload", typeof(MudFileUpload<>))
                .Add("Toggle Group", typeof(MudToggleGroup<>), typeof(MudToggleItem<>)))
            .AddGroup("Pickers", new ComponentList()
                .Add("Date Picker", typeof(MudDatePicker))
                .Add("Date Range Picker", typeof(MudDateRangePicker))
                .Add("Time Picker", typeof(MudTimePicker))
                .Add("Color Picker", typeof(MudColorPicker)))
            .AddGroup("Buttons", new ComponentList()
                .Add("Button", typeof(MudButton))
                .Add("Button Group", typeof(MudButtonGroup))
                .Add("Icon Button", typeof(MudIconButton))
                .Add("Toggle Icon Button", typeof(MudToggleIconButton))
                .Add("Button FAB", typeof(MudFab))
                .Add("Button FAB Menu", typeof(MudFabMenu)))
            .AddGroup("Charts", new ComponentList()
                .Add("Donut Chart", typeof(Donut<>), typeof(DonutChartOptions), typeof(Legend<>))
                .Add("Line Chart", typeof(Line<>), typeof(LineChartOptions), typeof(Legend<>))
                .Add("Pie Chart", typeof(Pie<>), typeof(PieChartOptions), typeof(Legend<>))
                .Add("Bar Chart", typeof(Bar<>), typeof(BarChartOptions), typeof(Legend<>))
                .Add("Heat Map Chart", typeof(HeatMap<>), typeof(HeatMapChartOptions), typeof(Legend<>))
                .Add("Stacked Bar Chart", typeof(StackedBar<>), typeof(StackedBarChartOptions), typeof(Legend<>))
                .Add("Time Series Chart", typeof(TimeSeries<>), typeof(TimeSeriesChartOptions), typeof(Legend<>))
                .Add("Radar Chart", typeof(Radar<>), typeof(RadarChartOptions), typeof(Legend<>))
                .Add("Rose Chart", typeof(Rose<>), typeof(RoseChartOptions), typeof(Legend<>))
                .Add("Sankey Chart", typeof(Sankey<>), typeof(SankeyChartOptions), typeof(Legend<>))
                .Add("Scatter Plot Chart", typeof(ScatterPlot<>), typeof(ScatterPlotChartOptions), typeof(Legend<>))
                .Add("Universal Chart", typeof(MudChart<>), typeof(MudAxisChartBase<,>), typeof(ChartOptions)))
            .SortedByName();

        Pages = Components.SelectMany(c => c.IsNavGroup ? c.GroupComponents : [c]).ToList();
    }

    /// <summary>The menu as mudblazor.com shows it.</summary>
    public IReadOnlyList<DocsComponent> Components { get; }

    /// <summary>Every page in menu order, groups flattened. Drives search and previous/next.</summary>
    public IReadOnlyList<DocsComponent> Pages { get; }

    /// <summary>Finds the menu entry whose component, or one of whose child types, has this name.</summary>
    public DocsComponent? FindByComponentName(string? componentName)
    {
        if (string.IsNullOrEmpty(componentName))
        {
            return null;
        }

        return Pages.FirstOrDefault(p => p.Type is not null && TypeName(p.Type) == componentName)
            ?? Pages.FirstOrDefault(p => p.ChildTypes.Any(t => TypeName(t) == componentName));
    }

    public DocsComponent? FindByLink(string? link) =>
        Pages.FirstOrDefault(p => string.Equals(p.Link, link, StringComparison.OrdinalIgnoreCase));

    /// <summary><c>MudChip`1</c> → <c>MudChip</c>, the name used in markup and API links.</summary>
    public static string TypeName(Type type)
    {
        var name = type.Name;
        var tick = name.IndexOf('`');
        return tick < 0 ? name : name[..tick];
    }

    private sealed class ComponentList
    {
        private readonly List<DocsComponent> _items = [];

        public ComponentList Add(string name, Type type, params Type[] childTypes)
        {
            _items.Add(new DocsComponent { Name = name, Type = type, ChildTypes = childTypes });
            return this;
        }

        public ComponentList AddGroup(string name, ComponentList items)
        {
            _items.Add(new DocsComponent { Name = name, GroupComponents = items.SortedByName() });
            return this;
        }

        // OrderBy with the default (culture-aware) comparer, as MudBlazor.Docs does.
        public List<DocsComponent> SortedByName() => _items.OrderBy(c => c.Name).ToList();
    }
}
