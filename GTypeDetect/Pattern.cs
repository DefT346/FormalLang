using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GTypeDetect
{
    internal class Pattern
    {
        public List<int> positions = new List<int>();
        public string pattern = "";
        //public SortedSet<char> patternSymbols = new SortedSet<char>();

        public Pattern(string rule, SortedSet<char> symbols)
        {
            pattern = rule;
            for (int i = 0; i< rule.Length; i++)
            {
                var ch = rule[i];

                if (symbols.Contains(ch))
                    positions.Add(i);

                //if (symbols.Contains(ch))
                //{
                //    patternSymbols.Add(ch);
                //    positions.Add(i);
                //    mask += '_';
                //}
                //else
                //{
                //    mask += ch;
                //}
            }

            ////добавим пустой символ
            //patternSymbols.UnionWith(new char[] { '\0' });
        }

        public List<string> Combinate()
        {
            Console.WriteLine($"Комбинации маски: {pattern}");

            int size = positions.Count;
            bool[][] result =
                Enumerable.Range(0, 1 << size)
                .Select(i => new BitArray(new int[] { i }).Cast<bool>().Take(size).ToArray())
                .ToArray();


            var rows = new List<string>();
            foreach (var variant in result)
            {
                var chars = pattern.ToCharArray();
                for (int i = 0; i < variant.Length; i++)
                    if (variant[i])
                        chars[positions[i]] = '\0';
                var newRow = new string(chars).Replace("\0", String.Empty);
                rows.Add(newRow);
            }

            return rows;

            //var seq = GetAllPermutations(patternSymbols, positions.Count);
            //foreach (var s in seq)
            //{
            //    var newr = mask.ToCharArray();
            //    for(int i = 0; i < positions.Count; i++)
            //    {
            //        newr[positions[i]] = s[i];
            //    }
            //    rules.Add(new string(newr).Replace("\0", String.Empty));
            //}
            //return rules;

        }

        //static IEnumerable<string> GetAllPermutations(IEnumerable<char> chars, int n)
        //{
        //    HashSet<char> curr = new HashSet<char>(chars);
        //    foreach (var s in GetAllPermutationsRec(n, curr))
        //        yield return s;
        //}

        //static IEnumerable<string> GetAllPermutationsRec(int n, HashSet<char> curr) // not pure!
        //{
        //    if (n == 0)
        //        yield return "";
        //    foreach (var c in curr.ToList())
        //    {
        //        curr.Remove(c);
        //        foreach (var s in GetAllPermutationsRec(n - 1, curr))
        //            yield return c + s;
        //        curr.Add(c);
        //    }
        //}
    }
}
