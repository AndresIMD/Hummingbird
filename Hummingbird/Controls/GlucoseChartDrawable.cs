namespace Hummingbird.Controls;

public class GlucoseChartDrawable : IDrawable
{
    public record ChartPoint(DateTime DateTime, int Glucose);

    public List<ChartPoint> Points { get; set; } = [];
    public int RangeLow { get; set; } = 70;
    public int RangeHigh { get; set; } = 140;
    public int RangeVeryHigh { get; set; } = 250;
    public int GlucoseTarget { get; set; } = 120;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Points.Count == 0) return;

        var w = dirtyRect.Width;
        var h = dirtyRect.Height;
        const float pL = 40, pR = 12, pT = 12, pB = 28;
        var cW = w - pL - pR;
        var cH = h - pT - pB;
        if (cW <= 0 || cH <= 0) return;

        var sorted = Points.OrderBy(p => p.DateTime).ToList();
        var tMin = sorted[0].DateTime;
        var tMax = sorted[^1].DateTime;
        var tSpan = Math.Max((tMax - tMin).TotalMinutes, 1);

        var gMin = Math.Max(Math.Min(sorted.Min(p => p.Glucose) - 20, RangeLow - 20), 30);
        var gMax = Math.Max(sorted.Max(p => p.Glucose) + 20, RangeVeryHigh + 20);
        var gRange = (float)(gMax - gMin);
        if (gRange <= 0) gRange = 1;

        float MapX(DateTime dt) => sorted.Count == 1
            ? pL + cW / 2
            : pL + (float)(dt - tMin).TotalMinutes / (float)tSpan * cW;
        float MapY(int g) => pT + cH - (g - gMin) / gRange * cH;

        // Range bands
        FillBand(canvas, pL, cW, MapY(gMax), MapY(RangeVeryHigh), Color.FromArgb("#EF4444").WithAlpha(0.06f));
        FillBand(canvas, pL, cW, MapY(RangeVeryHigh), MapY(RangeHigh), Color.FromArgb("#F59E0B").WithAlpha(0.06f));
        FillBand(canvas, pL, cW, MapY(RangeHigh), MapY(RangeLow), Color.FromArgb("#10B981").WithAlpha(0.08f));
        FillBand(canvas, pL, cW, MapY(RangeLow), MapY(gMin), Color.FromArgb("#3B82F6").WithAlpha(0.06f));

        // Target dashed line
        canvas.StrokeColor = Color.FromArgb("#10B981").WithAlpha(0.4f);
        canvas.StrokeDashPattern = [5, 5];
        canvas.StrokeSize = 1;
        canvas.DrawLine(pL, MapY(GlucoseTarget), w - pR, MapY(GlucoseTarget));
        canvas.StrokeDashPattern = null;

        // Connecting lines
        for (int i = 0; i < sorted.Count - 1; i++)
        {
            canvas.StrokeColor = PointColor(sorted[i].Glucose).WithAlpha(0.3f);
            canvas.StrokeSize = 2;
            canvas.DrawLine(
                MapX(sorted[i].DateTime), MapY(sorted[i].Glucose),
                MapX(sorted[i + 1].DateTime), MapY(sorted[i + 1].Glucose));
        }

        // Data points
        foreach (var p in sorted)
        {
            var x = MapX(p.DateTime);
            var y = MapY(p.Glucose);
            canvas.FillColor = PointColor(p.Glucose);
            canvas.FillCircle(x, y, 5);
            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 1.5f;
            canvas.DrawCircle(x, y, 5);
        }

        // Y axis labels
        canvas.FontSize = 9;
        canvas.FontColor = Colors.Gray;
        foreach (var g in new HashSet<int> { RangeLow, GlucoseTarget, RangeHigh })
        {
            canvas.DrawString(g.ToString(), 0, MapY(g) - 6, pL - 4, 12,
                HorizontalAlignment.Right, VerticalAlignment.Center);
        }

        // X axis date labels
        var dates = sorted.Select(p => p.DateTime.Date).Distinct().ToList();
        foreach (var date in dates)
        {
            var dayPoints = sorted.Where(p => p.DateTime.Date == date).ToList();
            var avgTicks = dayPoints.Average(p => (double)p.DateTime.Ticks);
            var x = MapX(new DateTime((long)avgTicks));
            canvas.DrawString(date.ToString("dd/MM"), x - 18, h - pB + 4, 36, 16,
                HorizontalAlignment.Center, VerticalAlignment.Top);
        }
    }

    private static void FillBand(ICanvas canvas, float x, float w, float y1, float y2, Color color)
    {
        if (y2 <= y1) return;
        canvas.FillColor = color;
        canvas.FillRectangle(x, y1, w, y2 - y1);
    }

    private Color PointColor(int glucose)
    {
        if (glucose < RangeLow) return Color.FromArgb("#3B82F6");
        if (glucose <= RangeHigh) return Color.FromArgb("#10B981");
        if (glucose <= RangeVeryHigh) return Color.FromArgb("#F59E0B");
        return Color.FromArgb("#EF4444");
    }
}
