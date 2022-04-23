// Copyright (c) 2021 Fabio Iotti
// The copyright holders license this file to you under the MIT license,
// available at https://github.com/bruce965/util/raw/master/LICENSE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Utility
{
    public static class EnumSourceInitializer
    {
        const string ListStart = "// Autogenerated START";
        const string ListEnd = "// Autogenerated END";

        [Conditional("UNITY_EDITOR")]
        public static void SyncSource<T>(Func<IEnumerable<(T Value, string HumanName)>> factory, [CallerFilePath] string filePath = null)
        {
            try
            {
                var source = File.ReadAllText(filePath);
                var sb = new StringBuilder(source, source.Length + 4096);

                var startIndex = source.LastIndexOf(ListStart, StringComparison.InvariantCulture) + ListStart.Length;
                var endIndex = source.IndexOf(ListEnd, startIndex, StringComparison.InvariantCulture);

                var indentStartCr = source.LastIndexOf('\r', startIndex - 1);
                var indentStartLf = source.LastIndexOf('\n', startIndex - 1);
                var indentStart = Math.Min(indentStartCr == -1 ? int.MaxValue : indentStartCr, indentStartLf == -1 ? int.MaxValue : indentStartLf);
                var indent = source.Substring(indentStart, startIndex - indentStart - ListStart.Length);
                var end = source.Substring(endIndex);

                sb.Length = startIndex;

                var isFlagsEnum = typeof(T).GetCustomAttribute<FlagsAttribute>() != null;

                foreach (var (value, humanName) in factory())
                {
                    var csharpName = StringUtility.ToCSharpName(humanName);

                    sb.Append(indent);

                    if (isFlagsEnum)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, "{0} = ", csharpName);

                        if (Convert.ToDecimal(value) == 0)
                        {
                            sb.Append('0');
                        }
                        else
                        {
                            var valueAsNumber = Convert.ToInt32(value);  // TODO: use a decimal.
                            var candidateBit = Mathf.NextPowerOfTwo(valueAsNumber);

                            var isFirstMatch = true;
                            do
                            {
                                if ((candidateBit & valueAsNumber) == 0)
                                    continue;

                                if (isFirstMatch)
                                    isFirstMatch = false;
                                else
                                    sb.Append(" | ");

                                // TODO: reuse other flag names if they exist.
                                sb.AppendFormat(CultureInfo.InvariantCulture, "1 << {0}", Convert.ToInt32(Math.Log(candidateBit, 2)));
                            }
                            while ((candidateBit >>= 1) > 0);
                        }

                        sb.Append(',');
                    }
                    else
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, "{0} = {1},", csharpName, Convert.ToDecimal(value));
                    }
                }

                sb.Append(indent);
                sb.Append(end);

                var updatedSource = sb.ToString();
                if (updatedSource != source)
                {
                    File.WriteAllText(filePath, updatedSource);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning(e);
            }
        }
    }
}
