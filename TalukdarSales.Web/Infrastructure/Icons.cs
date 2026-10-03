using Microsoft.AspNetCore.Html;

namespace TalukdarSales.Web.Infrastructure
{
    /// <summary>Inline stroke icons (24px grid, currentColor) used across the UI.</summary>
    public static class Icons
    {
        private static readonly Dictionary<string, string> Paths = new()
        {
            ["dashboard"] = "M4 4h7v9H4zM13 4h7v5h-7zM13 11h7v9h-7zM4 15h7v5H4z",
            ["new-order"] = "M12 8v8M8 12h8M4 6a2 2 0 0 1 2-2h12a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2z",
            ["orders"] = "M9 4h6v3H9zM7 5H6a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-1M8 12h8M8 16h5",
            ["invoices"] = "M6 3h12v18l-3-2-3 2-3-2-3 2zM9 8h6M9 12h6",
            ["collections"] = "M4 7a2 2 0 0 1 2-2h12v4M4 7v10a2 2 0 0 0 2 2h14V9H6a2 2 0 0 1-2-2zM16 14h2",
            ["production"] = "M3 9l9-5 9 5-9 5zM3 14l9 5 9-5",
            ["reports"] = "M5 20V10M12 20V4M19 20v-7",
            ["products"] = "M3 8l9-5 9 5v8l-9 5-9-5zM3 8l9 5 9-5M12 13v8",
            ["customers"] = "M16 20v-1a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v1M10 11a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7zM20 20v-1a4 4 0 0 0-3-3.9M16 4.2a3.5 3.5 0 0 1 0 6.6",
            ["roles"] = "M12 3l7 3v6c0 4.5-3 8-7 9-4-1-7-4.5-7-9V6z",
            ["notices"] = "M6 9a6 6 0 1 1 12 0c0 6 2 7 2 7H4s2-1 2-7zM10 20a2 2 0 0 0 4 0",
            ["clock"] = "M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18zM12 7v5l3 2",
            ["plus"] = "M12 5v14M5 12h14",
            ["check"] = "M5 12l5 5 9-10",
            ["download"] = "M12 4v11M7 11l5 5 5-5M5 20h14",
            ["print"] = "M6 9V3h12v6M6 18H4v-7h16v7h-2M6 14h12v7H6z",
            ["logout"] = "M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9",
            ["x"] = "M6 6l12 12M18 6L6 18",
            ["chevron-right"] = "M9 6l6 6-6 6",
            ["chevron-left"] = "M15 6l-6 6 6 6",
            ["alert"] = "M12 9v4M12 17h.01M10.3 3.9L2.4 18a2 2 0 0 0 1.7 3h15.8a2 2 0 0 0 1.7-3L13.7 3.9a2 2 0 0 0-3.4 0z",
            ["lock"] = "M6 11V8a6 6 0 0 1 12 0v3M5 11h14v10H5z",
            ["image"] = "M4 16l4-4 4 4 3-3 5 5M4 4h16v16H4zM9 9h.01",
            ["calendar"] = "M8 3v4M16 3v4M4 9h16M5 5h14a1 1 0 0 1 1 1v13a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1V6a1 1 0 0 1 1-1z",
            ["tag"] = "M20 12l-8 8-9-9V4h7zM7.5 7.5h.01",
            ["shield-lock"] = "M12 3l7 3v6c0 4.5-3 8-7 9-4-1-7-4.5-7-9V6zM9.5 10.5V9a2.5 2.5 0 0 1 5 0v1.5M9 10.5h6v4H9z",
        };

        public static IHtmlContent Svg(string name, int size = 20, double stroke = 1.75, string cls = null)
        {
            var d = Paths.TryGetValue(name, out var p) ? p : "";
            var c = cls == null ? "" : $" class=\"{cls}\"";
            return new HtmlString($"<svg{c} width=\"{size}\" height=\"{size}\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"{stroke.ToString(System.Globalization.CultureInfo.InvariantCulture)}\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><path d=\"{d}\"></path></svg>");
        }

        /// <summary>Raw path data, for places that need to put it into their own &lt;svg&gt;.</summary>
        public static string Path(string name) => Paths.TryGetValue(name, out var p) ? p : "";
    }

    /// <summary>Indian digit grouping (12,48,500) for amounts and counts.</summary>
    public static class Fmt
    {
        public static string Int(double n)
        {
            var v = (long)Math.Round(n, MidpointRounding.AwayFromZero);
            var s = Math.Abs(v).ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (s.Length > 3)
            {
                var head = s[..^3];
                var sb = new System.Text.StringBuilder();
                for (var i = 0; i < head.Length; i++)
                {
                    if (i > 0 && (head.Length - i) % 2 == 0) sb.Append(',');
                    sb.Append(head[i]);
                }
                s = sb + "," + s[^3..];
            }
            return (v < 0 ? "-" : "") + s;
        }

        /// <summary>Whole taka unless there are paisa, e.g. 8,420 or 8,420.50.</summary>
        public static string Money(double n)
        {
            var r = Math.Round(n, 2, MidpointRounding.AwayFromZero);
            var whole = Math.Truncate(r);
            var frac = Math.Abs(r - whole);
            return frac < 0.005 ? Int(r) : Int(whole) + "." + ((int)Math.Round(frac * 100)).ToString("D2");
        }

        public static string Taka(double n) => "৳ " + Money(n);

        public static string Initials(string name)
        {
            var parts = (name ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Concat(parts.Take(2).Select(p => char.ToUpperInvariant(p[0])));
        }
    }
}
