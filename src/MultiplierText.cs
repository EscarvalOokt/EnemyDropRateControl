using System;
using System.Globalization;

namespace EnemyDropRateControl;

internal static class MultiplierText
{
    internal static bool TryParse(ReadOnlySpan<char> text, out double value)
    {
        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) &&
               double.IsFinite(value) &&
               value >= 0;
    }

    internal static string Format(double value) => value.ToString("R", CultureInfo.InvariantCulture);
}