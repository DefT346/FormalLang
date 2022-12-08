using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace GTypeDetect
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
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            FindTypeButton_Click(null, null);
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


        string GetNonTerminals()
        {
            //S->ccAd
            //A->ccd | E
            return notterminalsInput.Text + "E";
        }

        private int CountTerminals(string chainElement)
        {
            var terminals = terminalsInput.Text;
            int count = 0;
            foreach(var el in chainElement)
            {
                if (terminals.Contains(el)) count++;
            }
            return count;
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
            var terminals = terminalsInput.Text;
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

        private void DetectType(string alpha, string[] beta)
        {
            reg = reg || isRegular(alpha, beta); // 3
            cf = cf || isContextFree(alpha, beta); // 2
            cs = cs || isContextSensitive(alpha, beta); // 1
        }

        private bool isRegular(string alpha, string[] beta)
        {
            if (CountNonTerminals(alpha) == 1 && CountTerminals(alpha) == 0)
            {
                foreach (var b in beta) {
                    if (CountNonTerminals(b) == 1)
                    {
                        var index = FindNonTerminal(b);
                        if (index == b.Length - 1 || index == 0) return true;
                        else return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool isContextFree(string alpha, string[] beta)
        {
            if (CountNonTerminals(alpha) == 1 && CountTerminals(alpha) == 0)
            {
                return true;
            }
            return false;
        }

        private bool isContextSensitive(string alpha, string[] beta)
        {
            if (CountNonTerminals(alpha) == 1)
            {
                var index = FindNonTerminal(alpha);
                if (index != 0 && index != alpha.Length - 1)
                {
                    foreach (var b in beta)
                        if (CountNonTerminals(b) == 1) 
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
                .Replace("->", String.Empty)
                .Replace("|", String.Empty);

            var nonTerminals = GetNonTerminals();
            var terminals = terminalsInput.Text;

            foreach (var el in data)
            {
                if (!nonTerminals.Contains(el) && !terminals.Contains(el)) return false;
            }
            return true;
        }

        private bool ValidLanguage(List<string> alphas, List<string> startChains)
        {
            foreach (var startChaine in startChains) {
                alphas.Remove(startNInput.Text); // Удаляем нетерминальный символ
                foreach (var leftN in alphas)
                {
                    if (startChaine.Contains(leftN)) return true;
                }
            }
            return false;
        }
        
        private bool ValidLanguageM(List<(string L, string R)> rules)
        {
            SortedSet<char> terminals = new SortedSet<char>(terminalsInput.Text);
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

            if (N.Contains(startNInput.Text[0])) return true;
            else return false;    
        }

        private void FindTypeButton_Click(object sender, RoutedEventArgs e)
        {
            //var s = rules.Items[0];
            //var t = s as TextBox;

            try
            {
                bool hasTremsChain = false;

                List<string> alphas = new List<string>();
                List<string> startChains = new List<string>();

                var rules = new List<(string L, string R)>();

                Reset();
                var rulesText = rulesInput.Text;

                if (!ValidGrammatic(rulesText)) throw new Exception("ошибка, проверьте символы");

                var lines = rulesText.Split("\n");

                foreach (var line in lines)
                {
                    if (line.Length == 0) continue;
                    var rule = line.Replace("\r", String.Empty).Replace(" ", String.Empty).Split("->");
                    var alpha = rule[0];
                    string[] beta;
                    if (rule[1].Contains("|")) beta = rule[1].Split("|");
                    else beta = new string[] { rule[1] };


                    if (alpha == startNInput.Text) startChains.AddRange(beta);
                    alphas.Add(alpha);

                    
                    DetectType(alpha, beta);

                    foreach(var right in beta)
                    {
                        rules.Add((alpha, right));
                        if (CountTerminals(right) > 0 && CountNonTerminals(right) == 0)
                        {
                            hasTremsChain = true;
                        }
                    }
                }
                var type = GetResult();
                if (type == 2 || type == 3)
                {
                    if (hasTremsChain && ValidLanguageM(rules))
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
    }
}
