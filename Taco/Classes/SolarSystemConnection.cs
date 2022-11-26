namespace Taco.Classes
{
    class SolarSystemConnection
    {
        public SolarSystemConnection(int toSystemNativeId, bool isRegional)
        {
            ToSystemNativeId = toSystemNativeId;
            IsRegional = isRegional;
        }
        
        public int ToSystemNativeId { get; set; }
        public bool IsRegional { get; set; }
    }
}
