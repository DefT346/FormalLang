using System;
using System.Collections.Generic;
using System.Text;

namespace FormalLang
{
    internal class TypeDetector
    {
        public static string terminals;
        public static string nonTerminals;

        public static void LoadFields(string _terminals, string _nonTerminals)
        {
            terminals = _terminals;
            nonTerminals = _nonTerminals;
        }

        // Функция определения типа грамматики с вводом всех правил сразу
        public static int DetectType(List<(string L, string R)> rules)
        {
            if (terminals == "" || nonTerminals == "") throw new Exception("Терминалы или нетерминалы не были заданы");

            bool reg = false;
            bool cf = false;
            bool cs = false;

            foreach (var rule in rules)
            {
                reg = reg || isRegular(rule.L, rule.R); // 3
                cf = cf || isContextFree(rule.L); // 2
                cs = cs || isContextSensitive(rule.L, rule.R); // 1
            }

            int res;
            if (reg)
                res = 3; // регулярная
            else if (cf)
                res = 2; // контекстно свободная
            else if (cs)
                res = 1; // контекстно зависимая
            else
                res = 0; // типа ноль

            return res;
        }

        // Функция проверки заданной левой части и правой на 3 тип (Регулярные грамматики)
        private static bool isRegular(string alpha, string beta)
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

        // Функция проверки заданной левой части на 2 тип (Контекстно свободные)
        private static bool isContextFree(string alpha)
        {
            if (CountNonTerminals(alpha) == 1 && CountTerminals(alpha) == 0)
            {
                return true;
            }
            return false;
        }

        // Функция проверки заданной левой части и правой на 2 тип (Контекстно зависимые грамматики)
        private static bool isContextSensitive(string alpha, string beta)
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

        public static int CountNonTerminals(string chainElement)
        {
            int count = 0;
            foreach (var el in chainElement)
            {
                if (nonTerminals.Contains(el)) count++;
            }
            return count;
        }

        public static int CountTerminals(string chainElement)
        {
            int count = 0;
            foreach(var el in chainElement)
            {
                if (terminals.Contains(el)) count++;
            }
            return count;
        }

        private static int FindNonTerminal(string chainElement)
        {
            for (int i = 0; i < chainElement.Length; i++)
            {
                if (nonTerminals.Contains(chainElement[i])) return i;
            }
            return -1;
        }

    }
}
