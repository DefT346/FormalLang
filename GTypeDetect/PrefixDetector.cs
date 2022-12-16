using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GTypeDetect
{
    internal static class PrefixDetector
    {
        public static Dictionary<string, List<string>> Detect(List<string> strings, bool printInConsole = false)
        {
            Dictionary<string, List<string>> matchesStrings = new Dictionary<string, List<string>>();

            List<string> buffer = new List<string>();
            //List<string> commonPrefixes = new List<string>();
            string tempPrefix = "";
            for (int i = 0; i < strings.Count; i++)
            {
                var d = strings[i];
                buffer.Add(d);

                var commonPrefix = new string(
                buffer.First().Substring(0, buffer.Min(s => s.Length))
                .TakeWhile((c, i) => buffer.All(s => s[i] == c))
                .ToArray());

                if (tempPrefix != "" && commonPrefix == "")
                {
                    addToMathes(tempPrefix);
                    i--;
                }
                if (i == strings.Count - 1)
                {
                    addToMathes(commonPrefix, 0);
                }

                tempPrefix = commonPrefix;
            }

            void addToMathes(string prefix, int cut = 1)
            {
                var element = buffer.GetRange(0, buffer.Count - cut);
                if (matchesStrings.ContainsKey(prefix))
                    matchesStrings[prefix].AddRange(element);
                else
                    matchesStrings.Add(prefix, new List<string>(element));

                buffer.Clear();
            }

            if (printInConsole)
                foreach (var el in matchesStrings)
                {
                    Console.WriteLine($"{el.Key}");
                    foreach (var va in el.Value)
                    {
                        Console.WriteLine($"    {va}");
                    }
                }

            return matchesStrings;
        }
    }
}
