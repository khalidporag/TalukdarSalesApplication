using System.Globalization;
using System.Net;
using System.Text;

namespace TalukdarSales.Web.Infrastructure
{
    public record LineSeries(string Name, string Color, double[] Values, bool Area = false);

    /// <summary>A two-or-more series line chart drawn in a 720x260 viewBox, with a hover layer.</summary>
    public record LineChart(string Id, string Aria, List<DateTime> Dates, List<LineSeries> Series)
    {
        public double YMax => Charts.NiceMax(Series.SelectMany(s => s.Values).DefaultIfEmpty(0).Max());
        public double X(int i) => 52 + (Dates.Count <= 1 ? 0 : i * 660.0 / (Dates.Count - 1));
        public double Y(double v) => 224 - v / YMax * 208;

        public string Path(LineSeries s) =>
            string.Join(' ', s.Values.Select((v, i) => (i == 0 ? "M" : "L") + Charts.N(X(i)) + " " + Charts.N(Y(v))));

        public string AreaPath(LineSeries s) => Path(s) + $" L{Charts.N(X(Dates.Count - 1))} 224 L52 224 Z";

        /// <summary>Tooltip markup for day i, escaped, placed in a data attribute.</summary>
        public string Tip(int i)
        {
            var sb = new StringBuilder();
            sb.Append("<b>").Append(WebUtility.HtmlEncode(Dates[i].ToString("ddd, d MMM", CultureInfo.InvariantCulture))).Append("</b>");
            foreach (var s in Series)
                sb.Append("<div class=\"r2\"><span><i class=\"dot round\" style=\"background:").Append(s.Color).Append("\"></i> ")
                  .Append(WebUtility.HtmlEncode(s.Name)).Append("</span><span class=\"num\">৳ ").Append(Fmt.Money(s.Values[i])).Append("</span></div>");
            return sb.ToString();
        }

        public string Ys(int i) => string.Join(';', Series.Select(s => Charts.N(Y(s.Values[i]))));

        public IEnumerable<(int Index, string Label)> XLabels()
        {
            var n = Dates.Count;
            var step = Math.Max(1, (int)Math.Round((n - 1) / 4.0));
            for (var i = 0; i < n; i += step) yield return (i, Dates[i].ToString("d MMM", CultureInfo.InvariantCulture));
        }
    }

    public static class Charts
    {
        public static string N(double v) => v.ToString("0.#", CultureInfo.InvariantCulture);

        public static double NiceMax(double max)
        {
            if (max <= 0) return 1000;
            var pow = Math.Pow(10, Math.Floor(Math.Log10(max)));
            foreach (var m in new[] { 1, 1.2, 1.5, 2, 2.5, 3, 4, 5, 6, 8, 10 })
                if (m * pow >= max) return m * pow;
            return 10 * pow;
        }

        /// <summary>Axis tick text: 0, 20k, 1.5L (lakh), 2cr.</summary>
        public static string Short(double v)
        {
            if (v >= 1_00_00_000) return N(v / 1_00_00_000) + "cr";
            if (v >= 1_00_000) return N(v / 1_00_000) + "L";
            if (v >= 1000) return N(v / 1000) + "k";
            return N(v);
        }

        public static string Sparkline(IReadOnlyList<double> v, double w, double h, double pad)
        {
            if (v.Count < 2) return "";
            var lo = v.Min(); var hi = v.Max(); var span = hi - lo == 0 ? 1 : hi - lo;
            return string.Join(' ', v.Select((x, i) => (i == 0 ? "M" : "L") + N(i * w / (v.Count - 1)) + " " + N(h - pad - (x - lo) / span * (h - 2 * pad))));
        }

        /// <summary>Delta text such as ▲ 8.2% for the change from previous to current; null when there is nothing to compare.</summary>
        public static (string Text, bool Up, bool Flat)? Change(double current, double previous)
        {
            if (previous <= 0) return current > 0 ? ("new", true, false) : null;
            var pct = (current - previous) / previous * 100;
            if (Math.Abs(pct) < 0.05) return ("0%", true, true);
            return ((pct > 0 ? "▲ " : "▼ ") + Math.Abs(pct).ToString("0.#", CultureInfo.InvariantCulture) + "%", pct > 0, false);
        }

        public static readonly string[] CategoryColors = { "#E5412D", "#7B4BB8", "#C98A12", "#1F9E96", "#B6A9BD" };
        public static readonly string[] AgeColors = { "#F4B6AE", "#EC7F70", "#D7261F", "#8E1B17" };

        /// <summary>stroke-dasharray / offsets for donut arcs with 3px gaps (r=58).</summary>
        public static List<(string Dash, string Offset)> DonutArcs(IReadOnlyList<double> shares)
        {
            const double c = 2 * Math.PI * 58; const double gap = 3;
            var total = shares.Sum(); var acc = 0.0; var list = new List<(string, string)>();
            foreach (var s in shares)
            {
                var len = total <= 0 ? 0 : s / total * c;
                list.Add(((Math.Max(0, len - gap)).ToString("0.0", CultureInfo.InvariantCulture) + " " + (c - len + gap).ToString("0.0", CultureInfo.InvariantCulture),
                    (-acc).ToString("0.0", CultureInfo.InvariantCulture)));
                acc += len;
            }
            return list;
        }
    }
}
