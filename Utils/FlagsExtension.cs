using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dubeg.Sw.ExportTools.Utils;

public static class FlagsExtension {
    public static IEnumerable<T> SplitFlags<T>(this int value) {
        foreach (object cur in Enum.GetValues(typeof(T))) {
            var number = (int)(object)(T)cur;
            if (0 != (number & value)) {
                yield return (T)cur;
            }
        }
    }
}
