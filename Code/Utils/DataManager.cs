using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RCArena.Code.Utils
{
    public static class DataManager
    {
        public static float masterVolume = 1;
        public static float soundEffectVolume = 1;
        public static float musicVolume = 1;

        public static string rootDirectory;
        public static string saveDataDirectory;

        public static int[] lastUsedProfiles = new int[2];

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

        public static void SetupFileStructure()
        {
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "/SaveData/")) Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/SaveData/");
            saveDataDirectory = Directory.GetCurrentDirectory() + "/SaveData/";
            rootDirectory = Directory.GetCurrentDirectory() + "/";
        }

        public static void LoadControlProfiles()
        {
            if (!File.Exists(saveDataDirectory + "ControlProfiles"))
            {
                controllerProfiles = new List<ControlProfile>() { new ControlProfile() { name = "Default" } };
                return;
            }
            try
            {
                controllerProfiles = new List<ControlProfile>();
                JsonDocument json = JsonDocument.Parse(File.ReadAllText(saveDataDirectory + "ControlProfiles"));
                foreach (JsonElement e in json.RootElement.EnumerateArray())
                {
                    var c = ControlProfile.LoadFromBytes(Encoding.ASCII.GetBytes(e.GetProperty("data").GetString()), e.GetProperty("name").GetString());
                    controllerProfiles.Add(c);
                }
                if (controllerProfiles.Count == 0) controllerProfiles.Add(new ControlProfile() { name = "Default" });
            }
            catch
            {
                controllerProfiles = new List<ControlProfile>() { new ControlProfile() { name = "Default" } };
                return;
            }
        }

        public static void SaveControlProfiles()
        {
            try
            {
                JsonArray json = new JsonArray();
                foreach (ControlProfile c in controllerProfiles)
                {
                    json.Add(new JsonObject()
                    {
                        { "name", c.name },
                        { "data", Encoding.ASCII.GetString(c.ToBytes()) }
                    });
                }
                File.WriteAllText(saveDataDirectory + "ControlProfiles", json.ToString());
            }
            catch
            {
                return;
            }
        }
        public static int[] GetLastUsedProfiles()
        {
            return [lastUsedProfiles[0] < controllerProfiles.Count ? lastUsedProfiles[0] : 0, lastUsedProfiles[1] < controllerProfiles.Count ? lastUsedProfiles[1] : 0];
        }

        public static void SetLastUsedProfiles(byte p1, byte p2)
        {
            lastUsedProfiles[0] = p1;
            lastUsedProfiles[1] = p2;
            SaveSettings();
        }

        public static void LoadSettings()
        {
            try
            {
                JsonDocument json = JsonDocument.Parse(File.ReadAllText(saveDataDirectory + "Settings"));

                masterVolume = json.RootElement.GetProperty("MasterVolume").GetSingle();
                musicVolume = json.RootElement.GetProperty("MusicVolume").GetSingle();
                soundEffectVolume = json.RootElement.GetProperty("SoundVolume").GetSingle();
                lastUsedProfiles[0] = json.RootElement.GetProperty("P1Profile").GetInt32();
                lastUsedProfiles[1] = json.RootElement.GetProperty("P2Profile").GetInt32();
            }
            catch
            {
                return;
            }
        }
        public static void SaveSettings()
        {
            try
            {
                JsonObject json = new JsonObject()
                {
                    { "MasterVolume", masterVolume },
                    { "MusicVolume", musicVolume },
                    { "SoundVolume", soundEffectVolume },
                    { "P1Profile", lastUsedProfiles[0] },
                    { "P2Profile", lastUsedProfiles[1] },
                };
                File.WriteAllText(saveDataDirectory + "Settings", json.ToString());
            }
            catch
            {
                return;
            }
        }
    }
}
