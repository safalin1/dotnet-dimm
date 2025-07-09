namespace Netboot.Networking;

public class ByteArrayBuilder
{
    private readonly MemoryStream _memoryStream;

    public ByteArrayBuilder()
    {
        _memoryStream = new MemoryStream();
    }

    public ByteArrayBuilder(byte[] buffer)
    {
        _memoryStream = new MemoryStream(buffer);
    }

    public void AppendByte(byte value) => _memoryStream.WriteByte(value);

    public void AppendBytes(byte[] bytes) => _memoryStream.Write(bytes);

    public void AppendValues(string format, params object[] values)
    {
        AppendBytes(StructPacker.Pack(format, values).Build());
    }
    
    public byte[] Build() => _memoryStream.ToArray();
}