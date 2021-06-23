using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChromeStealer.Extensions
{
    public static class ConsoleHelper
    {
        public static string GetInput(string label, bool ignoreCase = true)
        {
            Console.Write(label);
            var input = Console.ReadLine();
            return (ignoreCase ? input?.ToLower() : input)?.Trim();
        }
        
        public static string GetInput(string label, Regex regex, bool ignoreCase = true)
        {
            string input = null;
            while (input == null || !regex.IsMatch(input))
            {
                Console.Write(label);
                input = Console.ReadLine();
            }

            return (ignoreCase ? input.ToLower() : input).Trim();
        }
        
        public static string GetInput(string label, bool ignoreCase, params string[] possibleOptions)
        {
            if (ignoreCase)
            {
                possibleOptions = possibleOptions.Select(option => option.ToLower()).ToArray();
            }

            string input = null;
            
            bool CheckMatch()
            {
                if (input == null) return false;
                var inp = (ignoreCase ? input.ToLower() : input).Trim();
                return possibleOptions.Any(option => option.Trim() == inp);
            }
            
            while (!CheckMatch())
            {
                Console.Write(label);
                input = Console.ReadLine();
            }

            return input;
        }
    }
}