using ArkTracker.Cli.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace ArkTracker.Cli.UI;

public static class Display
{
    public static void ShowWelcomeScreen()
    {
        AnsiConsole.Clear();
        
        var figlet = new FigletText("ARK TRACKER").Centered().Color(Color.Cyan1);
        
        var content = new Rows(
            new Rule().RuleStyle("cyan").Centered(),
            figlet,
            new Rule("[bold magenta]P O R T F O L I O   A N A L Y T I C S[/]").RuleStyle("magenta").Centered(),
            new Text("\n\n"),
            new Align(new Text("WELCOME TO THE FUTURE OF ARK INVESTING", new Style(Color.Grey, decoration: Decoration.Italic)), HorizontalAlignment.Center),
            new Text("\n"),
            new Align(new Markup("[[[bold cyan] PRESS ENTER TO INITIALIZE [/]]]"), HorizontalAlignment.Center),
            new Text("\n\n\n"),
            new Align(new Text("SECURE AUTHENTICATION SYSTEM", new Style(Color.Grey27)).Centered(), HorizontalAlignment.Center),
            new Align(new Text("[AUTHENTICATION COMING SOON]", new Style(Color.Yellow)).Centered(), HorizontalAlignment.Center),
            new Text("\n"),
            new Rule().RuleStyle("cyan").Centered()
        );

        var panel = new Panel(content).Border(BoxBorder.None).Expand();
        AnsiConsole.Write(new Align(panel, HorizontalAlignment.Center, VerticalAlignment.Middle));
        
        Console.ReadLine();
    }

    public static async Task<T?> ShowSplashScreen<T>(Func<Task<T?>> fetchData)
    {
        AnsiConsole.Clear();
        T? result = default;
        Exception? fetchError = null;

        var figlet = new FigletText("ARK TRACKER").Centered().Color(Color.Cyan1);
        
        await AnsiConsole.Live(new Text("")) 
            .StartAsync(async ctx =>
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                var fetchTask = Task.Run(async () => {
                    try { return await fetchData(); }
                    catch (Exception ex) { fetchError = ex; return default; }
                });

                int i = 0;
                while (i <= 100)
                {
                    if (i == 96 && !fetchTask.IsCompleted)
                    {
                        await Task.Delay(50);
                        continue;
                    }

                    int barWidth = 30;
                    int filledWidth = (int)(i / 100.0 * barWidth);
                    string bar = new string('█', filledWidth) + new string('░', barWidth - filledWidth);

                    int totalFrames = 16;
                    int currentFrame = (int)(i / 100.0 * totalFrames);
                    var spinner = GetMultiLineSpinnerFrame(currentFrame);

                    var content = new Rows(
                        new Rule().RuleStyle("cyan").Centered(),
                        figlet,
                        new Rule("[bold magenta]P O R T F O L I O   A N A L Y T I C S[/]").RuleStyle("magenta").Centered(),
                        new Text("\n"),
                        new Align(spinner, HorizontalAlignment.Center),
                        new Text("\n"),
                        new Align(new Text("SYNCHRONIZING DATA", new Style(Color.Magenta1, decoration: Decoration.Bold)), HorizontalAlignment.Center),
                        new Align(new Text(bar, new Style(Color.Cyan1)), HorizontalAlignment.Center),
                        new Align(new Text($"{i}%", new Style(Color.Grey)), HorizontalAlignment.Center),
                        new Text("\n"),
                        new Rule().RuleStyle("cyan").Centered()
                    );

                    var container = new Panel(content).Border(BoxBorder.None).Expand();
                    ctx.UpdateTarget(new Align(container, HorizontalAlignment.Center, VerticalAlignment.Middle));
                    
                    i += 2;
                    await Task.Delay(6); 
                }

                result = await fetchTask;
                
                var remaining = 300 - stopwatch.ElapsedMilliseconds;
                if (remaining > 0) await Task.Delay((int)remaining);
            });

        AnsiConsole.Clear();
        if (fetchError != null) throw fetchError;
        return result;
    }

    private static IRenderable GetMultiLineSpinnerFrame(int step)
    {
        var grid = new Grid();
        for (int c = 0; c < 5; c++) grid.AddColumn(new GridColumn().NoWrap().Centered());

        List<(int r, int c)> ring = new List<(int r, int c)>();
        for (int c = 0; c < 5; c++) ring.Add((0, c));
        for (int r = 1; r < 5; r++) ring.Add((r, 4));
        for (int c = 3; c >= 0; c--) ring.Add((4, c));
        for (int r = 3; r >= 1; r--) ring.Add((r, 0));

        var colors = new[] { "cyan1", "springgreen3", "yellow1", "orange1", "red1", "magenta1", "purple3", "dodgerblue1" };
        
        int activeIdx = step % ring.Count;
        var active = ring[activeIdx];
        string activeColor = colors[step % colors.Length];

        for (int r = 0; r < 5; r++)
        {
            var row = new List<string>();
            for (int c = 0; c < 5; c++)
            {
                if (r == active.r && c == active.c)
                {
                    row.Add($"[{activeColor}]●[/]");
                }
                else if (ring.Contains((r, c)))
                {
                    row.Add("[grey]●[/]");
                }
                else if (r > 0 && r < 4 && c > 0 && c < 4)
                {
                    row.Add("[grey27]·[/]");
                }
                else
                {
                    row.Add("[grey15]·[/]");
                }
            }
            grid.AddRow(row.ToArray());
        }

        return grid;
    }

    public static void RenderHeader()
    {
        AnsiConsole.Clear();
        var figlet = new FigletText("ARK TRACKER").Centered();
        
        AnsiConsole.Write(new Rule().RuleStyle("cyan").Centered());
        AnsiConsole.Write(figlet.Color(Color.Cyan1));
        AnsiConsole.Write(new Rule("[bold magenta]P O R T F O L I O   A N A L Y T I C S[/]").RuleStyle("magenta").Centered());
        AnsiConsole.WriteLine();
    }

    public static void RenderComparisonTable(ComparisonResult result, string title, string sortBy = "Ticker")
    {
        var items = new List<DisplayRow>();

        foreach (var h in result.NewPositions) 
            items.Add(new DisplayRow("NEW", h.Ticker, h.Company, h.Shares, h.Weight, "teal"));
        
        foreach (var h in result.Increased) 
            items.Add(new DisplayRow("UP", h.Ticker, h.Company, h.NewShares - h.OldShares, h.NewWeight, "green"));
        
        foreach (var h in result.Reduced)
        {
            var isClosed = h.IsClosed;
            items.Add(new DisplayRow(
                isClosed ? "CLOSED" : "DOWN", 
                h.Ticker, h.Company, h.NewShares - h.OldShares, h.NewWeight, 
                isClosed ? "red" : "orange1"));
        }

        items = sortBy switch
        {
            "Ticker" => items.OrderBy(x => x.Ticker).ToList(),
            "Company" => items.OrderBy(x => x.Company).ToList(),
            "Weight" => items.OrderByDescending(x => x.Weight).ToList(),
            "Change Amount" => items.OrderByDescending(x => Math.Abs(x.ChangeAmount)).ToList(),
            _ => items.OrderBy(x => x.Ticker).ToList()
        };

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title($"[bold white]{title}[/]")
            .Caption($"[grey]Sorted by: {sortBy} | Colors: [teal]New[/] | [green]Up[/] | [orange1]Down[/] | [red]Closed[/][/]")
            .Expand();

        table.AddColumn("[bold]Change[/]");
        table.AddColumn("[bold]Ticker[/]");
        table.AddColumn("[bold]Company[/]");
        table.AddColumn(new TableColumn("[bold]Shares Change[/]").RightAligned());
        table.AddColumn(new TableColumn("[bold]Weight[/]").RightAligned());

        foreach (var item in items)
        {
            table.AddRow(
                $"[{item.Color}]{item.Status}[/]",
                $"[bold white]{Markup.Escape(item.Ticker)}[/]",
                Markup.Escape(item.Company),
                $"[{item.Color}]{(item.ChangeAmount >= 0 ? "+" : "")}{item.ChangeAmount:N0}[/]",
                $"[white]{item.Weight:N2}%[/]"
            );
        }

        if (items.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No changes found between these dates.[/]");
        }
        else
        {
            AnsiConsole.Write(table);
        }
        
        AnsiConsole.WriteLine();
    }

    private record DisplayRow(string Status, string Ticker, string Company, long ChangeAmount, decimal Weight, string Color);

    public static (DateTime from, DateTime to)? PromptForDates(List<DateTime> availableDates)
    {
        if (availableDates.Count < 2)
        {
            AnsiConsole.MarkupLine("[red]Not enough historical data to compare.[/]");
            return null;
        }

        var fromDates = availableDates.OrderBy(d => d).ToList();
        
        var fromDate = AnsiConsole.Prompt(
            new SelectionPrompt<DateTime>()
                .Title("Select the [bold teal]Start Date[/]:")
                .PageSize(10)
                .EnableSearch()
                .MoreChoicesText("[grey](Move up and down to reveal more dates)[/]")
                .UseConverter(d => d.ToString("dd. MM. yyyy"))
                .AddChoices(fromDates));

        var toDates = availableDates
            .Where(d => d > fromDate)
            .OrderByDescending(d => d)
            .ToList();

        if (toDates.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No available dates after the selected start date.[/]");
            return null;
        }

        var toDate = AnsiConsole.Prompt(
            new SelectionPrompt<DateTime>()
                .Title("Select the [bold teal]End Date[/]:")
                .PageSize(10)
                .EnableSearch()
                .MoreChoicesText("[grey](Move up and down to reveal more dates)[/]")
                .UseConverter(d => d.ToString("dd. MM. yyyy"))
                .AddChoices(toDates));

        return (fromDate, toDate);
    }

    public static void ShowError(string message)
    {
        var panel = new Panel($"[red]{Markup.Escape(message)}[/]")
            .Header("[bold red] ERROR [/]")
            .BorderColor(Color.Red)
            .Padding(1, 1, 1, 1);
        
        AnsiConsole.Write(panel);
    }
}
