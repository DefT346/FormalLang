using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FormalLang
{
    internal static class Alphabet
    {
        static string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static char GetRandom(string exceptions)
        {
            var localchars = chars.ToCharArray().ToList();
            foreach(var el in exceptions)
            {
                localchars.Remove(el);
            }

            Random r = new Random();
            var index = r.Next(0, localchars.Count - 1);
            return localchars[index];
        }
    }
}
