using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ChromeStealer.Extensions;
using ChromeStealer.Models;
using ChromeStealer.Stealer;
using Newtonsoft.Json;

namespace ChromeStealer
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;

            var doExport =
                ConsoleHelper.GetInput("Do you want to export Chrome passwords? (Y/n): ", true, "y", "n", "");
            if (doExport == "n") return;

            var allLoginData = BaseDataStealer.Stealers
                .Select(x => x.GetLoginData())
                .SelectMany(x => x)
                .Where(x => !string.IsNullOrEmpty(x.UsernameValue) && !string.IsNullOrEmpty(x.PasswordValue))
                .ToList();

            var originalLoginData = new List<LoginData>();
            foreach (var loginData in allLoginData)
            {
                if (!originalLoginData.Any(x => x.OriginUrl == loginData.OriginUrl
                                               && x.ActionUrl == loginData.ActionUrl
                                               //&& x.UsernameElement != loginData.UsernameElement
                                               && x.UsernameValue == loginData.UsernameValue
                                               //&& x.PasswordElement != loginData.PasswordElement
                                               && x.PasswordValue == loginData.PasswordValue))
                {
                    originalLoginData.Add(loginData);
                }
            }

            var loginDataText = JsonConvert.SerializeObject(originalLoginData, Formatting.Indented);
            const string filePath = "login_data.json";
            File.WriteAllText(filePath, loginDataText);
            Console.WriteLine($"Written to {filePath}, items: {originalLoginData.Count}");
        }

        private static void CurrentDomainOnProcessExit(object sender, EventArgs e)
        {
            Directory.Delete(BaseDataStealer.TempFolderPath, true);
        }
    }
}