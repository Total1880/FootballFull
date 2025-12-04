namespace OlavFramework
{
    public static class Configuration
    {
        public static int MaxStrength = 20;
        public static int MinStrength = 1;
        public static int Weeks = 52;
        //public static string DataRoot = @"C:\Users\olavh\source\repos\FootballFull\data";
        public static string DataRoot { get; private set; }

        public static void Initialize(string? basePath = null, string? saveName = null)
        {
            // Basisfolder (bv. Mijn Documenten\FootballFull)
            //var root = basePath ?? Path.Combine(
            //    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            //    "FootballFull");
            var root = @"C:\Users\olavh\source\repos\FootballFull\data";

            // Optioneel: subfolder per savegame
            if (!string.IsNullOrEmpty(saveName))
            {
                root = Path.Combine(root, "Saves", saveName);
            }

            Directory.CreateDirectory(root);
            DataRoot = root;
        }
    }
}
