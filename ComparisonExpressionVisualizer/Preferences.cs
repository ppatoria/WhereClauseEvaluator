using System;
using System.IO;
using System.Xml.Serialization;

namespace ComparisonExpressionVisualizer
{
    public class Preferences
    {
        public bool ExpandAll { get; set; }
    }
    public static class PreferencesSerializer
    {
        private const string fileName = "preferences.dat";

        public static  Preferences Deserialize()
        {
            if (!File.Exists(fileName)) return new Preferences { ExpandAll = true };

            using (var stream = File.OpenRead($"{fileName}"))
            {
                try
                {
                    return (Preferences)new XmlSerializer(typeof(Preferences)).Deserialize(stream);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error while serializing preferences data. Please delete dat files from bin directory.", ex);
                }
            }

        }

        public static void Serialize(this Preferences preferences)
        {
            using (var stream = File.OpenWrite(fileName))
            {
                new XmlSerializer(typeof(Preferences)).Serialize(stream, preferences);
            }
        }

    }
}
