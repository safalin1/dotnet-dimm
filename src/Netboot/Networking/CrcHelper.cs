using System.IO.Hashing;

namespace Netboot.Networking;

public static class CrcHelper
{
    public static uint Calculate(byte[] data) => Crc32.HashToUInt32(data);
}