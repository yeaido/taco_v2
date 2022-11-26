using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MapDataBuilder
{
    class SystemFileParser
    {
        /// <summary>
        /// Grabs all the static system data from the solarsystem.staticdata file passed to it from the eve static data files. Only pass files from the 'eve' folder since the other folder is full of wormholes and otherwise un-mappable systems.
        /// </summary>
        /// <param name="file">full path of system file. make sure it's from the eve folder - the other folder is wormholes</param>
        /// <param name="system">by ref will fill the nativeID, x, y, z, and connection source data and target connection if applicable</param>
        public void ParseSystemFile(string file, ref SolarSystem system)
        {
            try
            {
                using (StreamReader reader = File.OpenText(file))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("solarSystemID"))
                        { //get NativeId
                            var items = line.Split(':');
                            var number = items[1].Trim();
                            int nNativeId;
                            if (!int.TryParse(number, out nNativeId))
                            {
                                System.Diagnostics.Debug.Assert(false, "failed to convert NativId (solarSystemID)");
                            }
                            system.NativeId = nNativeId;
                        }
                        else if (line.StartsWith("center:"))
                        { //get x and y coords
                            line = reader.ReadLine();
                            string sX = line.Substring(2);
                            line = reader.ReadLine();
                            string sZ = "0.0"; //don't need this value - set it to 0
                            line = reader.ReadLine();
                            string sY = line.Substring(2);
                            double dX;
                            double.TryParse(sX, out dX);
                            //not sure why this offset was chosen - just had to reverse engineer it
                            var nOffset = 100000000000000;
                            dX /= nOffset;
                            double dZ;
                            double.TryParse(sZ, out dZ);
                            double dY;
                            double.TryParse(sY, out dY);
                            dY /= nOffset;
                            system.X = dX;
                            system.Y = dY;
                            system.Z = dZ;
                        }
                        else if (line.StartsWith("stargates:") && !line.EndsWith("{}"))
                        { //more than 0 stargates:
                            line = reader.ReadLine();
                            while (!line.StartsWith("sunTypeID:"))
                            {
                                string sStargateId = line.Trim().Trim(':');
                                int nStargateId;
                                int.TryParse(sStargateId, out nStargateId);
                                SolarSystemConnection connection = new SolarSystemConnection();
                                connection.FromStargateId = nStargateId;
                                system.Stargates.Add(nStargateId);
                                line = reader.ReadLine();
                                string sDestinationId = line.Split(':')[1].Trim();
                                int nDestinationId;
                                int.TryParse(sDestinationId, out nDestinationId);
                                connection.ToStargateId = nDestinationId;
                                system.ConnectedTo.Add(connection);
                                while (!line.Trim().StartsWith("typeID:"))
                                {
                                    line = reader.ReadLine();
                                }
                                line = reader.ReadLine();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Determines system connections after determining all static data in the eve static files. Matches target connection ids to systems with stargates containing those IDs to fill the remaining connection data in the list
        /// </summary>
        /// <param name="systems">List of solarsystems with all static data populated already</param>
        public void DetermineSystemConnections(ref List<SolarSystem> systems)
        {
            var nConnectionCount = 0;
            foreach(var system in systems)
            {
                if (system.ConnectedTo.Count == 0)
                {
                    continue;
                }
                foreach (var connection in system.ConnectedTo)
                {
                    var destinationStargate = connection.ToStargateId;
                    var destinationSystem = systems.Find(
                        delegate (SolarSystem s)
                        {
                            return s.Stargates.Contains(destinationStargate);
                        }
                        );
                    if (destinationSystem != null)
                    {
                        ++nConnectionCount;
                        connection.ToSystemNativeId = destinationSystem.NativeId;
                        connection.ToSystemId = destinationSystem.Id;
                        if (system.RegionName == destinationSystem.RegionName)
                        {
                            connection.IsRegional = false;
                        }
                        else
                        {
                            connection.IsRegional = true;
                        }
                    }
                }
            }
            Console.WriteLine("Connection hit count: {0}", nConnectionCount);
        }

        /// <summary>
        /// Creates the actual protobuf list of objects to eventually serialize out of the list of systems constructed above.
        /// </summary>
        /// <param name="systems">List of systems created earlier containing all the static data in the static eve files</param>
        /// <param name="data">uninitialized protobuf object array that will be duplicated from the 'systems' list passed in</param>
        public void CreateSystemData(List<SolarSystem> systems, out SolarSystemData[] data)
        {
            data = new SolarSystemData[systems.Count];
            var nSystemCount = 0;
            foreach (var system in systems)
            {
                SolarSystemData item = new SolarSystemData();
                item.Id = system.Id;
                item.NativeId = system.NativeId;
                item.Name = system.Name;
                item.X = system.X;
                item.Y = system.Y;
                item.Z = system.Z;
                item.ConnectedTo = new SolarSystemConnectionData[system.ConnectedTo.Count];
                var nCurrentConnection = 0;
                foreach (var connection in system.ConnectedTo)
                {
                    SolarSystemConnectionData connectionItem = new SolarSystemConnectionData();
                    connectionItem.ToSystemId = connection.ToSystemId;
                    connectionItem.ToSystemNativeId = connection.ToSystemNativeId;
                    connectionItem.IsRegional = connection.IsRegional;
                    item.ConnectedTo[nCurrentConnection] = connectionItem;
                    ++nCurrentConnection;
                }
                data[nSystemCount++] = item;
            }
        }
    }
}
