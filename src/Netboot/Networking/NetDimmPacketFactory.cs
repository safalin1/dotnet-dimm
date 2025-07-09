namespace Netboot.Networking;

public static class NetDimmPacketFactory
{
    public static NetDimmPacket NoOp() => new(0x01, 0x00, []);

    public static NetDimmPacket GetInfo() => new(0x18, 0x00, []);

    public static NetDimmPacket ReadMemoryAddress(byte[] data) => new(0x05, 0x00, data);

    public static NetDimmPacket ReadControlData() => new(0x16, 0x00, []);
}