namespace ChromaCore.Code.Utils
{
    public static class DataManager
    {
        public static float masterVolume = 1;
        public static float soundEffectVolume = 1;
        public static float musicVolume = 1;

        public static string rootDirectory;
        public static string saveDataDirectory;

        public static List<ControlProfile> controllerProfiles = new List<ControlProfile>();
        public static Dictionary<string, int> keyBindNames = new Dictionary<string, int>()
        {
            { "Up", Controller.Key_Up },
            { "Down", Controller.Key_Down },
            { "Left", Controller.Key_Left },
            { "Right", Controller.Key_Right },
            { "Light", Controller.Key_Light },
            { "Medium", Controller.Key_Medium },
            { "Heavy", Controller.Key_Heavy },
            { "Grab", Controller.Key_Grab },
            { "Dash", Controller.Key_Dash },
        };

        public static void LoadControlProfiles()
        {
            Dictionary<string, IEnumerable<byte>> profiles = ReadAllSections("ControllerProfiles");
            if (profiles.Count == 0) controllerProfiles.Add(new ControlProfile() { name = "Default" });
            foreach (KeyValuePair<string, IEnumerable<byte>> p in profiles) controllerProfiles.Add(ControlProfile.LoadFromBytes(p.Value.ToArray(), p.Key.Substring(0, p.Key.Length - 1)));
        }

        public static void SaveControlProfiles()
        {
            Dictionary<string, IEnumerable<byte>> profiles = new Dictionary<string, IEnumerable<byte>>();
            foreach (ControlProfile c in controllerProfiles) profiles.Add(c.name + controllerProfiles.IndexOf(c), c.ToBytes());
            WriteAllSections("ControllerProfiles", profiles);
        }
        public static int[] LoadLastUsedProfiles()
        {
            List<byte> bytes = ReadFileSection("Preferences", "LastUsedProfiles");
            if (bytes == null) return new int[] { 0, 0 };
            else return new int[] { bytes[0], bytes[1] };
        }

        public static void SetLastUsedProfiles(byte p1, byte p2) => WriteFileSection("Preferences", "LastUsedProfiles", new List<byte>() { p1, p2 });

        public static void WriteFileSection(string fileName, string sectionTitle, IEnumerable<byte> data)
        {
            Dictionary<string, IEnumerable<byte>> sections = ReadAllSections(fileName);

            if (sections.ContainsKey(sectionTitle)) sections[sectionTitle] = data;
            else sections.Add(sectionTitle, data);

            WriteAllSections(fileName, sections);
        }

        public static List<byte> ReadFileSection(string fileName, string sectionTitle)
        {
            Dictionary<string, IEnumerable<byte>> sections = ReadAllSections(fileName);

            if (sections.ContainsKey(sectionTitle)) return sections[sectionTitle].ToList();
            else return null;
        }

        public static void WriteAllSections(string fileName, Dictionary<string, IEnumerable<byte>> sections)
        {
            if (!File.Exists(rootDirectory + "/" + fileName)) File.Create(rootDirectory + "/" + fileName).Close();

            FileStream fs = File.OpenWrite(rootDirectory + "/" + fileName);
            fs.SetLength(0);
            foreach (KeyValuePair<string, IEnumerable<byte>> section in sections)
            {
                fs.Write(Encoding.ASCII.GetBytes("{" + section.Key + ":"));
                fs.Write(section.Value.ToArray());
                fs.WriteByte((byte)'}');
            }
            fs.Close();
        }

        public static Dictionary<string, IEnumerable<byte>> ReadAllSections(string fileName)
        {
            Dictionary<string, IEnumerable<byte>> sections = new Dictionary<string, IEnumerable<byte>>();
            if (!File.Exists(rootDirectory + "/" + fileName)) return sections;

            FileStream fs = File.OpenRead(rootDirectory + "/" + fileName);
            byte[] data = new byte[fs.Length];
            fs.Read(data);
            fs.Close();

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == '{')
                {
                    i++;
                    List<byte> name = new List<byte>();
                    while (i < data.Length && data[i] != ':')
                    {
                        name.Add(data[i]);
                        i++;
                    }
                    i++;

                    List<byte> bytes = new List<byte>();
                    while (i < data.Length && data[i] != '}')
                    {
                        bytes.Add(data[i]);
                        i++;
                    }

                    sections.Add(Encoding.ASCII.GetString(name.ToArray()), bytes);
                }
                else break;
            }

            return sections;
        }
    }
}
