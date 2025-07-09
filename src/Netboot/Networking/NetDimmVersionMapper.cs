namespace Netboot.Networking;

public static class NetDimmVersionMapper
{
    public static string Map(ENetDimmVersion version) 
        => version switch
        {
            ENetDimmVersion.Unknown => "Unknown",
            ENetDimmVersion.Version1_02 => "1.02",
            ENetDimmVersion.Version2_03 => "2.03",
            ENetDimmVersion.Version2_06 => "2.06",
            ENetDimmVersion.Version2_13 => "2.13",
            ENetDimmVersion.Version2_17 => "2.17",
            ENetDimmVersion.Version3_01 => "3.01",
            ENetDimmVersion.Version3_03 => "3.03",
            ENetDimmVersion.Version3_12 => "3.12",
            ENetDimmVersion.Version3_17 => "3.17",
            ENetDimmVersion.Version4_01 => "4.01",
            ENetDimmVersion.Version4_02 => "4.02",
            _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
        };
}