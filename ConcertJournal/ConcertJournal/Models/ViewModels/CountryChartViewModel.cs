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
    public CountryChartViewModel(IEnumerable<(string Country, int Count)> data)
    {


        // Sort descending by Count
        var sortedData = data.OrderBy(d => d.Count).ToList();

        // reverse values and labels for top-to-bottom
        var values = sortedData.Select(d => d.Count).ToArray();
        originalLabels = sortedData.Select(d => d.Country).ToArray();

        // create trimmed labels for display
        var trimmedLabels = originalLabels
            .Select(l => TrimLabel(l))
            .ToArray();

        var textColor = (Color)Application.Current.Resources["TextColor"];
        var skTextColor = textColor.ToSKColor();

        // Horizontal bars
        CountrySeries = new ISeries[]
        {
            new RowSeries<int>
            {
                Values = values,
                Fill = new SolidColorPaint(SKColor.Parse("#00BCF5")),
                DataLabelsPaint = new SolidColorPaint(skTextColor), // contrast text
                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Left,
                DataLabelsSize = 14,
                MaxBarWidth = 40,
                Stroke = null
            }
        };

        // Y-axis: country names
        YAxes = new Axis[]
        {
            new Axis
            {
                Labels = trimmedLabels,
                LabelsPaint = new SolidColorPaint(skTextColor),
                TextSize = 16,
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
        var rowHeight = 50; // pixels per country
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

