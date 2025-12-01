using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace ConcertJournal.Models.ViewModels;
public class CountryChartViewModel
{
    public ISeries[] CountrySeries { get; set; }
    public Axis[] XAxes { get; set; }
    public Axis[] YAxes { get; set; }

    public double ChartHeight { get; set; }

    private readonly int maxLabelLength = 12;
    private string[] originalLabels;

    public CountryChartViewModel(
        IEnumerable<(string Country, int ConcertCount, int PerformerCount)> data)
    {
        // Sort descending by ConcertCount
        var sortedData = data.OrderBy(d => d.ConcertCount).ToList();

        // reverse values and labels for top-to-bottom
        var concertValues = sortedData.Select(d => d.ConcertCount).ToArray();
        var performerValues = sortedData.Select(d => d.PerformerCount).ToArray();
        originalLabels = sortedData.Select(d => d.Country).ToArray();

        // create trimmed labels for display
        var trimmedLabels = originalLabels
            .Select(l => TrimLabel(l))
            .ToArray();

        var textColor = (Color)Application.Current.Resources["TextColor"];
        var skTextColor = textColor.ToSKColor();

        // Horizontal bars with two series
        CountrySeries = new ISeries[]
        {
            new RowSeries<int>
            {
                Name = "Concerts",
                Values = concertValues,
                Fill = new SolidColorPaint(SKColor.Parse("#00BCF5")),
                DataLabelsPaint = new SolidColorPaint(skTextColor),
                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Left,
                DataLabelsSize = 14,
                MaxBarWidth = 40,
                Stroke = null,
                Padding = 4,
            },
            new RowSeries<int>
            {
                Name = "Unique Performers Seen",
                Values = performerValues,
                Fill = new SolidColorPaint(SKColor.Parse("#FFB400")),
                DataLabelsPaint = new SolidColorPaint(skTextColor),
                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Left,
                DataLabelsSize = 14,
                MaxBarWidth = 40,
                Stroke = null,
                Padding = 4,
            }
        };

        // Y-axis: country names
        YAxes = new Axis[]
        {
            new Axis
            {
                Labels = trimmedLabels,
                LabelsPaint = new SolidColorPaint(skTextColor),
                TextSize = 20,
                MinStep = 1,
                ShowSeparatorLines = false,
            }
        };

        // X-axis: concert counts
        XAxes = new Axis[]
        {
            new Axis
            {
                Labeler = value => value.ToString(),
                LabelsPaint = new SolidColorPaint(skTextColor),
                TextSize = 14,
                MinStep = 1,
            }
        };

        // Dynamically calculate height for the chart
        var rowHeight = 60; // pixels per country
        var padding = 20;   // top+bottom
        ChartHeight = sortedData.Count * rowHeight + padding;
    }

    private string TrimLabel(string text)
    {
        if (text.Length <= maxLabelLength)
            return text;

        return text.Substring(0, maxLabelLength) + "…";
    }
}

