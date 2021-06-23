using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ChromeStealer.Extensions;
using Newtonsoft.Json;

namespace ChromeStealer
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var doExport = ConsoleHelper.GetInput("Do you want to export Chrome passwords? (Y/n): ", true, "y", "n", "");
            if (doExport == "n") return;

            var loginData = ChromeDataRepository.GetLoginData()
                .Where(x => !string.IsNullOrEmpty(x.UsernameValue) && !string.IsNullOrEmpty(x.PasswordValue));
            var loginDataText = JsonConvert.SerializeObject(loginData, Formatting.Indented);
            const string filePath = "login_data.json";
            File.WriteAllText(filePath, loginDataText);
            Console.WriteLine($"Written to {filePath}");
        }
    }
}
