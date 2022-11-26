using System;
using System.Collections.Generic;
using System.IO;

namespace MapDataBuilder
{
    class Program
    {
        static string kOutputFileName = "systemdata.bin";
        static SystemFileParser _parser = new SystemFileParser();

        static private bool ValidateDirectory()
        {
            var subDirectories = Directory.GetDirectories(Directory.GetCurrentDirectory());
            List<String> lFolders = new List<string>();
            foreach (var folder in subDirectories)
            {
                string sFolderName = Path.GetFileName(folder);
                lFolders.Add(sFolderName); //strip folder name from full path
                Console.WriteLine("Found folder: {0}", sFolderName);
            }
            System.Diagnostics.Debug.Assert(lFolders.Contains("eve"), "needs to be run from the folder that contains the 'eve' and 'wormhole' folder in the eve static sde");
            System.Diagnostics.Debug.Assert(lFolders.Contains("wormhole"), "needs to be run from the folder that contains the 'eve' and 'wormhole' folder in the eve static sde");
            return true;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Eve to TACO map binary builder");
            ValidateDirectory();

            Console.WriteLine("Gathering Solarsystem Files...");
            string sCurrentDirectory = Directory.GetCurrentDirectory();
            //we only want the items in the eve folder because the other things aren't really part of the map
            var solarSystemFiles = Directory.GetFiles(sCurrentDirectory + "\\eve", "solarsystem.staticdata", SearchOption.AllDirectories);
            Console.WriteLine("Files Gathered");

            Console.WriteLine("Checking for SystemID Upgrade .bin (needed for upgrade from v0.8.0b and later)");
            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            var dictionaryFileInfo = new System.IO.FileInfo(@"systemID_upgrade_map.bin");
            Dictionary<int, int> oldSystemIdMapReadback = new Dictionary<int, int>();
            Dictionary<int, int> newSystemIdMapReadback = new Dictionary<int, int>();
            bool bDictionaryLoaded = false;
            try
            {
                using (var binaryFile = dictionaryFileInfo.OpenRead())
                {
                    Console.WriteLine("SystemID Upgrade .bin Found, translating IDs");
                    oldSystemIdMapReadback = (Dictionary<int, int>)binaryFormatter.Deserialize(binaryFile);
                    if (oldSystemIdMapReadback.Count != solarSystemFiles.Length)
                    {
                        System.Diagnostics.Debug.Assert(false, "SystemID dictionary and solarsystem mismatch error");
                        return;
                    }
                    bDictionaryLoaded = true;
                    foreach (var item in oldSystemIdMapReadback)
                    {  // flip it around for easier consumption (nativeid key to arbitrary id value)
                        newSystemIdMapReadback.Add(item.Value, item.Key);
                    }
                }
            }
            catch
            {
                Console.WriteLine("SystemID Upgrade .bin Not Found. Continuing Without...");
            }

            Console.WriteLine("Parsing Files...");
            List<SolarSystem> lSystems = new List<SolarSystem>();
            foreach (var file in solarSystemFiles)
            {
                SolarSystem tempSystem = new SolarSystem();
                string sSystemPath = Path.GetDirectoryName(file);
                tempSystem.Name = new DirectoryInfo(sSystemPath).Name; //folder name is system name
                tempSystem.RegionName = Directory.GetParent(sSystemPath).Parent.Name; //folder parent name is region name  
                _parser.ParseSystemFile(file, ref tempSystem);
                if (bDictionaryLoaded)
                {
                    if (newSystemIdMapReadback.ContainsKey(tempSystem.NativeId))
                    {
                        tempSystem.Id = newSystemIdMapReadback[tempSystem.NativeId];
                    }
                    else
                    {  // Odd...
                        System.Diagnostics.Debugger.Break();
                    }
                }
                lSystems.Add(tempSystem);
            }
            Console.WriteLine("Files Parsed");

            Console.WriteLine("Determining Connections...");
            _parser.DetermineSystemConnections(ref lSystems);
            Console.WriteLine("Connections Calculated");

            Console.WriteLine("Creating Dataset...");
            SolarSystemData[] systemData;
            _parser.CreateSystemData(lSystems, out systemData);
            Console.WriteLine("Dataset Created");

            Console.WriteLine("Writing to outputfile: {0}", kOutputFileName);
            try
            {
                using (var file = File.Create(kOutputFileName))
                {
                    ProtoBuf.Serializer.Serialize(file, systemData);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("File Created!");
            Console.WriteLine("Done!");
            Console.Read();
        }
    }
}
