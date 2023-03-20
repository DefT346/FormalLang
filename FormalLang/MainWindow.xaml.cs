using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FormalLang
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            timer.Start();

            rulesInput.Text = Settings.Default.Rules;

        }

        private void timer_Tick(object sender, EventArgs e)
        {
            ParseRules();
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    rules.Items.Add("");
        //}

        //private void Button_Click_1(object sender, RoutedEventArgs e)
        //{
        //    if (rules.Items.Count > 0)
        //        rules.Items.RemoveAt(rules.Items.Count - 1);
        //}

        char GetStartNonTerminal()
        {
            return startNInput.Text[0];
        }

        const char eps = 'ε';

        // символы и буковки
        string GetTerminals()
        {
            return terminalsInput.Text + eps;
        }

        string GetNonTerminals()
        {
            //S=ccAd
            //A=ccd | E
            return notterminalsInput.Text;
        }

        private int CountTerminals(string chainElement)
        {
            var terminals = GetTerminals();
            int count = 0;
            foreach(var el in chainElement)
            {
                if (terminals.Contains(el)) count++;
            }
            return count;
        }

        private SortedSet<char> SelectNonTerminals(string R)
        {
            var N = new SortedSet<char>();
            var nonTerminals = GetNonTerminals();
            foreach (var symbol in R)
            {
                if (nonTerminals.Contains(symbol)) N.Add(symbol);
            }
            return N;
        }

        private int CountNonTerminals(string chainElement)
        {
            var nonTerminals = GetNonTerminals();
            int count = 0;
            foreach (var el in chainElement)
            {
                if (nonTerminals.Contains(el)) count++;
            }
            return count;
        }

        private int FindNonTerminal(string chainElement)
        {
            var nonTerminals = GetNonTerminals();
            for(int i =0; i < chainElement.Length; i++)
            {
                if (nonTerminals.Contains(chainElement[i])) return i;
            }
            return -1;
        }

        public bool isTerminal(char symb)
        {
            var terminals = GetTerminals();
            if (terminals.Contains(symb)) return true;
            else
                return false;
        }

        public bool isNonTerminal(char symb)
        {
            var nonTerminals = GetNonTerminals();
            if (nonTerminals.Contains(symb)) return true;
            else
                return false;
        }

        bool reg = false;
        bool cf = false;
        bool cs = false;

        private void Reset()
        {
            reg = false;
            cf = false;
            cs = false;
        }

        private int GetResult()
        {
            int res = 3;
            if (reg)
            {
                resultBox.Text = "регулярная";
                res = 3;
            }
            else if (cf)
            {
                resultBox.Text = "контекстно свободная";
                res = 2;
            }
            else if (cs)
            {
                resultBox.Text = "контекстно зависимая";
                res = 1;
            }
            else
            {
                resultBox.Text = "типа ноль";
                res = 0;
            }

            return res;
        }

        private void DetectType(string alpha, string beta)
        {
            reg = reg || isRegular(alpha, beta); // 3
            cf = cf || isContextFree(alpha); // 2
            cs = cs || isContextSensitive(alpha, beta); // 1
        }

        private void DetectType(List<(string L, string R)> rules)
        {
            foreach(var rule in rules)
            {
                DetectType(rule.L, rule.R);
            }
        }

        private bool isRegular(string alpha, string beta)
        {
            if (CountNonTerminals(alpha) == 1 && CountTerminals(alpha) == 0)
            {
                //foreach (var b in beta) {
                    if (CountNonTerminals(beta) == 1)
                    {
                        var index = FindNonTerminal(beta);
                        if (index == beta.Length - 1 || index == 0) return true;
                        else return false;
                    }
                    else
                    {
                        return true;
                    }
                //}
            }
            return false;
        }

        private bool isContextFree(string alpha)
        {
            if (CountNonTerminals(alpha) == 1 && CountTerminals(alpha) == 0)
            {
                return true;
            }
            return false;
        }

        private bool isContextSensitive(string alpha, string beta)
        {
            if (CountNonTerminals(alpha) == 1)
            {
                var index = FindNonTerminal(alpha);
                if (index != 0 && index != alpha.Length - 1)
                {
                    //foreach (var b in beta)
                        if (CountNonTerminals(beta) == 1) 
                            return false;

                    return true;
                }


            }
            return false;
        }

        private bool ValidGrammatic(string rules)
        {
            string data = rules
                .Replace("\r", String.Empty)
                .Replace("\n", String.Empty)
                .Replace(" ", String.Empty)
                .Replace("=", String.Empty)
                .Replace("|", String.Empty);

            var nonTerminals = GetNonTerminals();
            var terminals = GetTerminals();

            foreach (var el in data)
            {
                if (!nonTerminals.Contains(el) && !terminals.Contains(el)) return false;
            }
            return true;
        }

        private bool ValidLanguage(List<string> alphas, List<string> startChains)
        {
            foreach (var startChaine in startChains) {
                alphas.Remove(GetStartNonTerminal().ToString()); // Удаляем нетерминальный символ
                foreach (var leftN in alphas)
                {
                    if (startChaine.Contains(leftN)) return true;
                }
            }
            return false;
        }
        
        private bool ValidLanguageM(List<(string L, string R)> rules)
        {
            SortedSet<char> terminals = new SortedSet<char>(GetTerminals());
            SortedSet<char> N = new SortedSet<char>();
            SortedSet<char> tempN = new SortedSet<char>();

            while (true)
            {
                foreach (var rule in rules)
                {
                    var R = new SortedSet<char>(rule.R);

                    var exc = new SortedSet<char>(N); exc.UnionWith(terminals);

                    if (R.IsProperSubsetOf(exc))
                    {
                        N.Add(rule.L[0]);
                    }
                }

                if (tempN == N) 
                    break;

                tempN = N;
            }

            if (N.Contains(GetStartNonTerminal())) return true;
            else return false;    
        }

        List<(string L, string R)> updatableRules = new List<(string L, string R)>(); // обновляемый список правил
        bool hasTremsChain = false;
        bool autoDetectSymbols = true;
        private void ParseRules()
        {
            //var s = rules.Items[0];
            //var t = s as TextBox;

            

            try
            {
                hasTremsChain = false;

                //List<string> alphas = new List<string>();
                //List<string> startChains = new List<string>();

                if (autoDetectSymbols) ClearSymbolInputs();

                updatableRules.Clear();

                Reset();
                var rulesText = rulesInput.Text;


                var lines = rulesText.Split("\n");
                foreach (var line in lines)
                {
                    if (line.Length == 0) continue;
                    var rule = line.Replace("\r", String.Empty).Replace(" ", String.Empty).Split("=");
                    var alpha = rule[0];
                    string[] beta;
                    if (rule[1].Contains("|")) beta = rule[1].Split("|");
                    else beta = new string[] { rule[1] };

                    //if (alpha == GetStartNonTerminal().ToString()) startChains.AddRange(beta);
                    //alphas.Add(alpha);

                    if (autoDetectSymbols)
                    {
                        DectectAndSetSymbols(alpha);
                        foreach(var right in beta)
                            DectectAndSetSymbols(right);
                    }


                    //DetectType(alpha, beta);

                    foreach (var right in beta)
                    {
                        updatableRules.Add((alpha, right));
                        if (CountTerminals(right) > 0 && CountNonTerminals(right) == 0)
                        {
                            hasTremsChain = true;
                        }
                    }
                }

                if (!ValidGrammatic(rulesText)) throw new Exception("ошибка, проверьте символы");

                DetectType(updatableRules);

                var type = GetResult();

                if (type == 2 || type == 3)
                {
                    startNInput.Text = updatableRules[0].L[0].ToString();

                    if (hasTremsChain && ValidLanguageM(updatableRules))
                    {
                        LanguageResult.Text = "Язык грамматики существует";
                    }
                    else LanguageResult.Text = "Язык грамматики не существует";
                }
            }
            catch (Exception ex)
            {
                resultBox.Text = ex.Message;
            }
        }

        private void ClearSymbolInputs()
        {
            notterminalsInput.Text = "";
            terminalsInput.Text = "";
        }

        private void DectectAndSetSymbols(string part)
        {
            foreach (var c in part)
            {
                if (Char.IsLetter(c) && Char.IsUpper(c))
                {
                    if (!notterminalsInput.Text.Contains(c))
                        notterminalsInput.Text += c;
                }
                else
                {
                    if (!terminalsInput.Text.Contains(c) && c != eps)
                        terminalsInput.Text += c;
                }
            }
        }

        private void LeftFactorizationRules(object sender, RoutedEventArgs e)
        {


            //var data = new List<string> { "kSl", "kSm", "n" };

            var modRules = new List<(string L, string R)>();

            var grouped = GroupRules(updatableRules);

            foreach(var rule in grouped)
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
                    var newL = Alphabet.GetRandom(GetNonTerminals()).ToString();   
                    foreach(var part in math.Value)
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


            UpdateRulsInput(modRules);


        }

        /// <summary>
        /// Удаляет бесполезные символы из введённого правила и выводит новые
        /// </summary>
        private void DeleteUselessUnreachableCharacters(object sender, RoutedEventArgs e)
        {
            //var rightN = new SortedSet<int> { 1,2,3,4 };
            //var N = new SortedSet<int> { 1,2,3 };
            //var r = rightN.IsSubsetOf(N);
            var NGNT = GetNonGeneratingNonTerminals(updatableRules);
            var UNT = GetUnattainableNonTerminals(updatableRules);

            var newrules = DeleteEquals(updatableRules, NGNT);
            newrules = DeleteEquals(newrules, UNT);

            UpdateRulsInput(newrules);
        }

        private void EliminateChainRules(object sender, RoutedEventArgs e)
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
            var nonterminals = new SortedSet<char>(GetNonTerminals());
            var modRules = new List<(string L, string R)>();
            foreach (var rule in updatableRules)
            {
                var symbols = new SortedSet<char>(rule.R);
                if (!symbols.IsSubsetOf(nonterminals))
                {
                    foreach(var nonTerm in OPNTs)
                    {
                        if (nonTerm.Value.Contains(rule.L[0]))
                        {
                            modRules.Add((nonTerm.Key.ToString(), rule.R));
                        }
                    }
                }
            }

            UpdateRulsInput(modRules);

        }

        private SortedSet<char> OutputNonTerminals(char nonTerminal, List<(string L, string R)> rules)
        {
            var N = new SortedSet<char>(new char[] { nonTerminal });
            var nonterminals = new SortedSet<char>(GetNonTerminals());
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

        private void DeleteEpsRules(object sender, RoutedEventArgs e)
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

            if (EPR.Contains(GetStartNonTerminal()))
            {
                var oldStartNonTerminal = startNInput.Text[0];
                var exceptions = notterminalsInput.Text + startNInput.Text;
                var newStartNonTerminal = Alphabet.GetRandom(exceptions);
                notterminalsInput.Text += newStartNonTerminal;
                startNInput.Text = newStartNonTerminal.ToString();

                modRules.Insert(0, (newStartNonTerminal.ToString(), $"{oldStartNonTerminal} | {eps}"));
            }

            UpdateRulsInput(modRules);

        }


        private Dictionary<string, List<string>> GroupRules(List<(string L, string R)> rules)
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

        /// <summary>
        /// Выводит списко указанных правил в поле ввода правил
        /// </summary>
        private void UpdateRulsInput(List<(string L, string R)> newrules)
        {
            var groupedRules = GroupRules(newrules);

            rulesInput.Text = "";
            foreach (var rule in groupedRules)
            {
                var L = rule.Key;
                rulesInput.Text += $"{L} = {rule.Value[0]}";

                for (int i = 1; i < rule.Value.Count; i++)
                {
                    var R = rule.Value[i];
                    rulesInput.Text += $" | {R}";
                }

                rulesInput.Text += "\n";
            }


            //rulesInput.Text = "";
            //var tempL = '\0';
            //foreach (var r in newrules)
            //{
            //    if (tempL == r.L[0])
            //    {
            //        rulesInput.Text += $" | {r.R}";
            //    }
            //    else
            //    {
            //        if (tempL != '\0') rulesInput.Text += "\n";
            //        rulesInput.Text += $"{r.L} = {r.R}";
            //    }
            //    tempL = r.L[0];
            //}
        }

        /// <summary>
        /// Проверяет, содержится ли в множестве root хотя бы один элемент из множества elements
        /// </summary>
        private bool Contains(SortedSet<char> root, SortedSet<char> elements)
        {
            foreach(var el in elements)
                if (root.Contains(el)) return true;

            return false;
        }

        /// <summary>
        /// Удаляет правила содержащие указанные нетерминалы
        /// </summary>
        private List<(string L, string R)> DeleteEquals(List<(string L, string R)> rules, SortedSet<char> deletingnonterminals)
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


        private SortedSet<char> GetEpsGeneratingNonTerminals(List<(string L, string R)> rules)
        {
            var N = new SortedSet<char>();
            var nonterminals = new SortedSet<char>(GetNonTerminals());
            var tempN = new SortedSet<char>();

            foreach (var rule in rules)
                if (rule.R.Contains(eps))
                    N.Add(rule.L[0]);

            while (true)
            {
                foreach(var rule in rules)
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
        /// Находит недостижимые нетерминалы в указанном списке правил
        /// </summary>
        private SortedSet<char> GetUnattainableNonTerminals(List<(string L, string R)> rules)
        {
            var N = new SortedSet<char> { GetStartNonTerminal() };
            var nonterminals = new SortedSet<char>(GetNonTerminals());
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
        private SortedSet<char> GetNonGeneratingNonTerminals(List<(string L, string R)> rules)
        {
            var N = new SortedSet<char>();
            var nonterminals = new SortedSet<char>(GetNonTerminals());
            var tempN = new SortedSet<char>();

            foreach (var rule in rules)
                if (CountNonTerminals(rule.R) == 0)
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

        private void Rectangle_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                Application.Current.Shutdown();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            autoDetectSymbols = autoSumbCheck.IsChecked.Value;
        }

        private void rulesInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.Rules = rulesInput.Text;
            Settings.Default.Save();
        }
    }
}
