using System.Globalization;
using System.Text.RegularExpressions;

namespace TalukdarSales.Web.Infrastructure
{
    /// <summary>
    /// Reads quantity inputs named <c>Qty[productId]</c> straight from the form. The built-in dictionary binder falls back
    /// to binding the whole form when no such keys exist and then throws on keys like "UserId".
    /// </summary>
    public static class QtyForm
    {
        private static readonly Regex Key = new(@"^Qty\[(\d+)\]$", RegexOptions.Compiled);

        public static Dictionary<int, double?> Read(IFormCollection form)
        {
            var result = new Dictionary<int, double?>();
            if (form == null)
                return result;
            foreach (var key in form.Keys)
            {
                var m = Key.Match(key);
                if (!m.Success)
                    continue;
                var raw = form[key].ToString();
                result[int.Parse(m.Groups[1].Value)] =
                    double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : null;
            }
            return result;
        }
    }
}
