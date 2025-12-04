namespace OlavFramework
{
    public static class Configuration
    {
        public static int MaxStrength = 20;
        public static int MinStrength = 1;
        public static int Weeks = 52;

        public static string DataRoot { get; private set; }

        // Je basisfolder (de standaard data)
        private static readonly string BaseDataPath =
            @"C:\Users\olavh\source\repos\FootballFull\data";

        public static bool Initialize(string? saveName = null)
        {
            // Bepaal save-path
            if (!string.IsNullOrEmpty(saveName))
            {
                DataRoot = Path.Combine(BaseDataPath, "Saves", saveName);

                // Bestaat de save nog niet? → kopieer alles
                if (!Directory.Exists(DataRoot))
                {
                    Directory.CreateDirectory(DataRoot);
                    CopyBaseDataToSave(DataRoot);
                    return true;
                }
            }
            else
            {
                // Geen save → werk rechtstreeks op de basisdata
                DataRoot = BaseDataPath;
            }
            return false;
        }

        private static void CopyBaseDataToSave(string destRoot)
        {
            // Enkel de JSON-bestanden uit de basisfolder kopiëren
            foreach (var file in Directory.GetFiles(BaseDataPath, "*.json"))
            {
                var destFile = Path.Combine(destRoot, Path.GetFileName(file));
                File.Copy(file, destFile, overwrite: false);
            }
        }
    }

}
