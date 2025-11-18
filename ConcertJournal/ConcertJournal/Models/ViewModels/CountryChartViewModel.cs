using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace ConcertJournal.Models.ViewModels;
public class CountryChartViewModel
{
    public ISeries[] CountrySeries { get; set; }
    public Axis[] XAxes { get; set; }
    public Axis[] YAxes { get; set; }

    public double ChartHeight { get; set; }

    public CountryChartViewModel(IEnumerable<(string Country, int Count)> data)
    {


        // Sort descending by Count
        var sortedData = data.OrderBy(d => d.Count).ToList();

        // reverse values and labels for top-to-bottom
        var values = sortedData.Select(d => d.Count).ToArray();
        var labels = sortedData.Select(d => d.Country).ToArray();

        // Horizontal bars
        CountrySeries = new ISeries[]
        {
            new RowSeries<int>
            {
                Values = values,
                Fill = new SolidColorPaint(SKColor.Parse("#3F51B5")),
                DataLabelsPaint = new SolidColorPaint(SKColors.White), // contrast text
                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle,
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
                Labels = labels,
                TextSize = 14,
                LabelsRotation = 0,
                MinStep = 1,
                ShowSeparatorLines = false
            }
        };

        // X-axis: concert counts
        XAxes = new Axis[]
        {
            new Axis
            {
                Labeler = value => value.ToString(),
                Name = "Concerts",
                TextSize = 14,
                MinStep = 1,
            }
        };

        // Dynamically calculate height for the chart
        var rowHeight = 50; // pixels per country
        var padding = 20;   // top+bottom
        ChartHeight = sortedData.Count * rowHeight + padding;
    }
}
