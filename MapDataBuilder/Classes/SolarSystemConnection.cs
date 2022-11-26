namespace MapDataBuilder
{
    class SolarSystemConnection
    {
        //id's found in solarsystem.staticdata files
        public int FromStargateId { get; set; }
        public int ToStargateId { get; set; }
        public int ToSystemNativeId { get; set; } //will have to determine this one

        //id's created in this program
        public int ToSystemId { get; set; }        
        public bool IsRegional { get; set; }
    }
}
