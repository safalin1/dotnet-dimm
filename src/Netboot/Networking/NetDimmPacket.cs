namespace Netboot.Networking;

public class NetDimmPacket
{
    public int Id { get; }
    
    public int Flags { get; }
    
    public byte[] Data { get; set; }
    
    public int Length => Data.Length;

    public NetDimmPacket(int id, int flags, byte[] data)
    {
        Id = id;
        Flags = flags;
        Data = data;
    }
}