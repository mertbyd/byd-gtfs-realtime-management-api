namespace Byd.GtfsRealtimeManagement.Consts;

// islevi: Kaynak liste filtresinin dis alan adlarini tanimlar.
// sistemdeki gorevi: Dinamik sorgu motoruna sabit alan yollarini verir.
public static class RealtimeSourceFilterFields
{
    public const string Name = "name";
    public const string IsActive = "isActive";
    public const string CreationTime = "creationTime";

    public static class PropertyPaths
    {
        public const string Name = "Name";
        public const string IsActive = "IsActive";
        public const string CreationTime = "CreationTime";
    }
}
