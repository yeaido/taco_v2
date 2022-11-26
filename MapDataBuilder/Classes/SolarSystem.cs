using System.Collections.Generic;

namespace MapDataBuilder
{
    class SolarSystem
    {
        public int Id { get; set; }        
        public int NativeId { get; set; }        
        public string Name { get; set; }        
        public string RegionName { get; set; }
        public double X { get; set; }        
        public double Y { get; set; }        
        public double Z { get; set; }
        public List<SolarSystemConnection> ConnectedTo { get; set; } = new List<SolarSystemConnection>();
        public List<int> Stargates { get; set; } = new List<int>();
    }
}
