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

        private bool ValidGrammatic(string rules)
        {
            string data = rules
                .Replace("\r", String.Empty)
                .Replace("\n", String.Empty)
                .Replace(" ", String.Empty)
                .Replace("=", String.Empty)
                .Replace("|", String.Empty);

            var nonTerminals = TypeDetector.nonTerminals;
            var terminals = TypeDetector.terminals;

            foreach (var el in data)
            {
                if (!nonTerminals.Contains(el) && !terminals.Contains(el)) 
                    return false;
            }
            return true;
        }
        
        private bool ValidLanguageM(List<(string L, string R)> rules)
        {
            SortedSet<char> terminals = new SortedSet<char>(TypeDetector.terminals);
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

            if (N.Contains(RulesConverter.startNonTerminal)) return true;
            else return false;    
        }

        private string GetResult(int type)
        {
            if (type == 3)
            {
                return "регулярная";
            }
            else if (type == 2)
            {
                return "контекстно свободная";
            }
            else if (type == 1)
            {
                return "контекстно зависимая";
            }
            else
            {
                return "типа ноль";
            }
        }

        List<(string L, string R)> updatableRules = new List<(string L, string R)>(); // обновляемый список правил
        bool hasTremsChain = false;
        bool autoDetectSymbols = true;
        private void ParseRules()
        {
            TypeDetector.LoadFields(terminalsInput.Text + RulesConverter.eps, notterminalsInput.Text);

            try
            {
                hasTremsChain = false;

                if (autoDetectSymbols) ClearSymbolInputs();

                updatableRules.Clear();

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

                    if (autoDetectSymbols)
                    {
                        DectectAndSetSymbols(alpha);
                        foreach(var right in beta)
                            DectectAndSetSymbols(right);
                    }

                    foreach (var right in beta)
                    {
                        updatableRules.Add((alpha, right));
                        if (TypeDetector.CountTerminals(right) > 0 && TypeDetector.CountNonTerminals(right) == 0)
                        {
                            hasTremsChain = true;
                        }
                    }
                }

                if (!ValidGrammatic(rulesText))
                    throw new Exception("ошибка, проверьте символы");

                var type = TypeDetector.DetectType(updatableRules);

                resultBox.Text = GetResult(type);

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
                    if (!terminalsInput.Text.Contains(c) && c != RulesConverter.eps)
                        terminalsInput.Text += c;
                }
            }
        }

        private void LeftFactorizationRules(object sender, RoutedEventArgs e)
        {
            UpdateRulsInput(RulesConverter.LeftFactorizationRules(updatableRules));
        }

        /// <summary>
        /// Удаляет бесполезные символы из введённого правила и выводит новые
        /// </summary>
        private void DeleteUselessUnreachableCharacters(object sender, RoutedEventArgs e)
        {
            UpdateRulsInput(RulesConverter.DeleteUselessUnreachableCharacters(updatableRules));
        }

        private void EliminateChainRules(object sender, RoutedEventArgs e)
        {
            UpdateRulsInput(RulesConverter.EliminateChainRules(updatableRules));

        }

        /// <summary>
        /// Удаление эпсилон правил
        /// </summary>
        private void DeleteEpsRules(object sender, RoutedEventArgs e)
        {
            UpdateRulsInput(RulesConverter.DeleteEpsRules(updatableRules));
        }

        /// <summary>
        /// Выводит списко указанных правил в поле ввода правил
        /// </summary>
        private void UpdateRulsInput(List<(string L, string R)> newrules)
        {
            var groupedRules = RulesConverter.GroupRules(newrules);

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
