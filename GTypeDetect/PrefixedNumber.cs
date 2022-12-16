using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace GTypeDetect
{
    public class PrefixedNumber
    {
        private static Regex parser = new Regex(@"^(\p{L}+)(\d+)$");

        public PrefixedNumber(string source) // you may want a static Parse method.
        {
            Match parsed = parser.Match(source); // think about an error here when it doesn't match
            Prefix = parsed.Groups[1].Value;
            Index = parsed.Groups[2].Value;
        }

        public string Prefix { get; set; }
        public string Index { get; set; }
    }
}


////List<string> data = new List<string> { "abc001", "abc002", "abc003", "cdef001",
////                           "cdef002", "cdef004", "ghi002", "ghi001" };
////var groups = data.Select(str => new PrefixedNumber(str))
////                 .GroupBy(prefixed => prefixed.Index);

////foreach(var el in groups)
////{
////    Console.WriteLine(el.Key);
////}




//List<string> data = new List<string> { "kssl", "kssm", "kssn"};





//string pattern = @"^(\p{L}+)(\d+)$";
////string pattern = @"^(\D*)\d+$";
////  \D* any non digit characters, and \d+ means followed by at least one digit,
//// Note if you want also to capture string like "abc" alone without followed by numbers
//// then the pattern will be "^(\D*)$"

//Regex regex = new Regex(pattern);

//Dictionary<string, List<string>> matchesStrings = new Dictionary<string, List<string>>();

//foreach (string item in data)
//{
//    var match = regex.Match(item);

//    if (match.Groups.Count > 1)
//    {
//        var key = match.Groups[1].Value;

//        var value = match.Groups[2].Value;

//        if (matchesStrings.ContainsKey(key))
//        {
//            matchesStrings[key].Add(value);
//        }
//        else
//        {
//            matchesStrings.Add(key, new List<string>(new string[] { value }));
//        }
//    }
//}

//foreach (var el in matchesStrings)
//{
//    Console.WriteLine($"{el.Key}");
//    foreach(var va in el.Value)
//    {
//        Console.WriteLine($"    {va}");
//    }
//}

//var data = new List<string> { "kSl", "kSm", "n" };
//List<string> data = new List<string> { "abc001", "abc002", "abc003", "cdef001", "cdef002", "cdef004", "ghi002", "ghi001" };


//var samples = new[] { "kSl", "kSm", "n" };

//var commonPrefix = new string(
//    samples.First().Substring(0, samples.Min(s => s.Length))
//    .TakeWhile((c, i) => samples.All(s => s[i] == c))
//    .ToArray());
