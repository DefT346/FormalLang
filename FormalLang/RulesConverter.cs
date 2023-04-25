using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FormalLang
{
    internal class RulesConverter
    {

        public const char eps = 'e';
        public static char startNonTerminal = 'S';

        /// <summary>
        /// Устанение левой факторизации правил
        /// </summary>
        public static List<(string L, string R)> LeftFactorizationRules(List<(string L, string R)> updatableRules)
        {

            //var data = new List<string> { "kSl", "kSm", "n" };

            var modRules = new List<(string L, string R)>();

            var grouped = GroupRules(updatableRules);

            foreach (var rule in grouped)
            {
                var L = rule.Key;
                var matchesStrings = PrefixDetector.Detect(rule.Value, true);
                foreach (var math in matchesStrings)
                {
                    //формируем старое правило без новых штук

                    if (math.Value.Count < 2)
                    {
                        var newR = math.Key;
                        modRules.Add((L, newR));
                        continue;
                    }

                    // формируем отдельное правило 
                    var newL = Alphabet.GetRandom(TypeDetector.nonTerminals).ToString();
                    foreach (var part in math.Value)
                    {
                        var newR = part.Replace(math.Key, String.Empty);
                        //if (newR == "") 
                        //    newR = eps.ToString();
                        if (newR != "")
                            modRules.Add((newL, newR));
                    }

                    var subR = math.Key;
                    modRules.Add((L, subR + newL));
                }

            }


            return modRules;
        }

        /// <summary>
        /// Устранение цепных правил
        /// </summary>
        public static List<(string L, string R)> EliminateChainRules(List<(string L, string R)> updatableRules)
        {

            // Шаг 1
            var OPNTs = new Dictionary<char, SortedSet<char>>();

            foreach (var rule in updatableRules)
            {
                var data = OutputNonTerminals(rule.L[0], updatableRules);
                if (!OPNTs.ContainsKey(rule.L[0]))
                    OPNTs.Add(rule.L[0], data);
                else
                {
                    OPNTs[rule.L[0]].UnionWith(data);
                }
            }

            // Шаг 2
            var nonterminals = new SortedSet<char>(TypeDetector.nonTerminals);
            var modRules = new List<(string L, string R)>();
            foreach (var rule in updatableRules)
            {
                var symbols = new SortedSet<char>(rule.R);
                if (!symbols.IsSubsetOf(nonterminals))
                {
                    foreach (var nonTerm in OPNTs)
                    {
                        if (nonTerm.Value.Contains(rule.L[0]))
                        {
                            modRules.Add((nonTerm.Key.ToString(), rule.R));
                        }
                    }
                }
            }

            return modRules;

        }

        /// <summary>
        /// Удаляет бесполезные символы из введённого правила и выводит новые
        /// </summary>
        public static List<(string L, string R)> DeleteUselessUnreachableCharacters(List<(string L, string R)> updatableRules)
        {
            //var rightN = new SortedSet<int> { 1,2,3,4 };
            //var N = new SortedSet<int> { 1,2,3 };
            //var r = rightN.IsSubsetOf(N);
            var NGNT = GetNonGeneratingNonTerminals(updatableRules);
            var UNT = GetUnattainableNonTerminals(updatableRules);

            var newrules = DeleteEquals(updatableRules, NGNT);
            newrules = DeleteEquals(newrules, UNT);

            return newrules;
        }

        /// <summary>
        /// Удаление эпсилон правил
        /// </summary>
        public static List<(string L, string R)> DeleteEpsRules(List<(string L, string R)> updatableRules)
        {
            var EPR = GetEpsGeneratingNonTerminals(updatableRules);


            var newRules = new List<(string L, string R)>();
            foreach (var rule in updatableRules)
                if (!rule.R.Contains(eps))
                    newRules.Add(rule);

            // Шаг 3
            var modRules = new List<(string L, string R)>(newRules);
            foreach (var rule in newRules)
            {
                var rightNonTerminals = SelectNonTerminals(rule.R);

                if (Contains(rightNonTerminals, EPR))
                {
                    //modRules.Add(rule);
                    var pattern = new Pattern(rule.R, EPR);
                    var combRules = pattern.Combinate();
                    int parentIndex = modRules.IndexOf(rule);
                    modRules.Remove(rule);

                    foreach (var r in combRules)
                    {
                        if (r != "")
                            modRules.Insert(parentIndex, (rule.L, r));
                        //modRules.Remove(rule);
                    }
                }
            }

            if (EPR.Contains(startNonTerminal))
            {
                var oldStartNonTerminal = startNonTerminal;
                var exceptions = TypeDetector.nonTerminals + startNonTerminal;
                var newStartNonTerminal = Alphabet.GetRandom(exceptions);
                TypeDetector.nonTerminals += newStartNonTerminal;
                startNonTerminal = newStartNonTerminal;

                modRules.Insert(0, (newStartNonTerminal.ToString(), $"{oldStartNonTerminal} | {eps}"));
            }

            return modRules;

        }

        private static SortedSet<char> GetEpsGeneratingNonTerminals(List<(string L, string R)> rules)
        {
            var N = new SortedSet<char>();
            //var nonterminals = new SortedSet<char>(TypeDetector.nonTerminals);
            var tempN = new SortedSet<char>();

            foreach (var rule in rules)
                if (rule.R.Contains(eps))
                    N.Add(rule.L[0]);

            while (true)
            {
                foreach (var rule in rules)
                {
                    var rightNonTerminals = SelectNonTerminals(rule.R);
                    if (rightNonTerminals.Count > 0)
                        if (rightNonTerminals.IsSubsetOf(N))
                            N.Add(rule.L[0]);

                }

                if (tempN == N)
                    break;

                tempN = N;
            }

            //nonterminals.ExceptWith(N);

            return N;
        }

        /// <summary>
        /// Находит все нетерминалы, которые по цепочкам следуют из указанного nonTerminal
        /// </summary>
        private static SortedSet<char> OutputNonTerminals(char nonTerminal, List<(string L, string R)> rules)
        {
            var N = new SortedSet<char>(new char[] { nonTerminal });
            var nonterminals = new SortedSet<char>(TypeDetector.nonTerminals);
            var tempN = new SortedSet<char>();

            while (true)
            {
                foreach (var rule in rules)
                {
                    var rightNonTerminals = SelectNonTerminals(rule.R);
                    if (rightNonTerminals.Count > 0)
                        if (N.Contains(rule.L[0]) && rightNonTerminals.IsSubsetOf(nonterminals))
                            N.Add(rightNonTerminals.ToList()[0]);

                }

                if (tempN == N)
                    break;

                tempN = N;
            }

            //nonterminals.ExceptWith(N);

            return N;
        }

        /// <summary>
        /// Удаляет правила содержащие указанные нетерминалы
        /// </summary>
        private static List<(string L, string R)> DeleteEquals(List<(string L, string R)> rules, SortedSet<char> deletingnonterminals)
        {

            var newRules = new List<(string L, string R)>();
            foreach (var rule in rules)
            {
                var ruleNonTerminals = SelectNonTerminals(rule.L);
                ruleNonTerminals.UnionWith(SelectNonTerminals(rule.R));

                if (!Contains(ruleNonTerminals, deletingnonterminals))
                    newRules.Add(rule);
            }
            return newRules;
        }

        /// <summary>
        /// Проверяет, содержится ли в множестве root хотя бы один элемент из множества elements
        /// </summary>
        private static bool Contains(SortedSet<char> root, SortedSet<char> elements)
        {
            foreach (var el in elements)
                if (root.Contains(el)) return true;

            return false;
        }

        /// <summary>
        /// Находит недостижимые нетерминалы в указанном списке правил
        /// </summary>
        private static SortedSet<char> GetUnattainableNonTerminals(List<(string L, string R)> rules)
        {
            var N = new SortedSet<char> { startNonTerminal };
            var nonterminals = new SortedSet<char>(TypeDetector.nonTerminals);
            var tempN = new SortedSet<char>();

            while (true)
            {
                foreach (var rule in rules)
                {
                    var leftNonTerminal = new SortedSet<char> { rule.L[0] };
                    if (leftNonTerminal.IsSubsetOf(N))
                        N.UnionWith(SelectNonTerminals(rule.R));
                }

                if (tempN == N)
                    break;

                tempN = N;
            }

            nonterminals.ExceptWith(N);

            return nonterminals;
        }

        /// <summary>
        /// Находит непорождающие нетерминалы в указанном списке правил
        /// </summary>
        private static SortedSet<char> GetNonGeneratingNonTerminals(List<(string L, string R)> rules)
        {
            var N = new SortedSet<char>();
            var nonterminals = new SortedSet<char>(TypeDetector.nonTerminals);
            var tempN = new SortedSet<char>();

            foreach (var rule in rules)
                if (TypeDetector.CountNonTerminals(rule.R) == 0)
                    N.Add(rule.L[0]);

            while (true)
            {
                foreach (var rule in rules)
                {
                    var rightNonTerminals = SelectNonTerminals(rule.R);
                    if (rightNonTerminals.Count > 0)
                        if (rightNonTerminals.IsSubsetOf(N))
                            N.Add(rule.L[0]);
                }

                if (tempN == N)
                    break;

                tempN = N;
            }

            nonterminals.ExceptWith(N);

            return nonterminals;

        }

        public static Dictionary<string, List<string>> GroupRules(List<(string L, string R)> rules)
        {
            var groupedRules = new Dictionary<string, List<string>>();
            foreach (var r in rules)
            {
                if (groupedRules.ContainsKey(r.L))
                {
                    groupedRules[r.L].Add(r.R);
                }
                else
                {
                    groupedRules.Add(r.L, new List<string>(new string[] { r.R }));
                }
            }
            return groupedRules;
        }

        private static SortedSet<char> SelectNonTerminals(string R)
        {
            var N = new SortedSet<char>();
            var nonTerminals = TypeDetector.nonTerminals;
            foreach (var symbol in R)
            {
                if (nonTerminals.Contains(symbol)) N.Add(symbol);
            }
            return N;
        }

    }
}
