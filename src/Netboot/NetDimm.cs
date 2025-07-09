using System.Net;
using Netboot.Networking;

namespace Netboot;

public class NetDimm : IDisposable
{
    private readonly NetDimmConnection _connection;

    public NetDimm(IPAddress ipAddress)
    {
        _connection = new NetDimmConnection(ipAddress);
    }

    public ValueTask InitialiseAsync(CancellationToken cancellationToken) => _connection.ConnectAsync(cancellationToken);

    public async Task<NetDimmInfo> GetInfoAsync(CancellationToken cancellationToken)
    {
        await _connection.WritePacketAsync(NetDimmPacketFactory.GetInfo(), cancellationToken);

        var response = await _connection.ReadPacketAsync(cancellationToken);

        if (response.Id != 0x18)
        {
            throw new Exception("Invalid packet ID received");
        }

        if (response.Length != 12)
        {
            throw new Exception("Unexpected length of info packet");
        }

        (int unknown, int versionDigits, int gameMemory, int dimmMemory, int crc) = StructPacker.Unpack<Tuple<int, int, int, int, int>>("<HHHHI", response.Data);

        int versionHigh = (versionDigits >> 8) & 0xff;
        int versionLow = versionDigits & 0xff;

        var versionHighHex = versionHigh.ToString("x2");
        var versionLowHex = versionLow.ToString("x2");

        while (versionLowHex.Length < 2)
        {
            versionLowHex = "0" + versionLowHex;
        }

        Enum.TryParse<ENetDimmVersion>($"{versionHighHex}.{versionLowHex}", out var versionEnum);
        
        // __get_crc_information
        var crcStatusDigit = await GetCrcInformationAsync(cancellationToken);
        ECrcStatus crcStatus;

        if (crcStatusDigit is 0 or 1)
        {
            crcStatus = ECrcStatus.Checking;
        }
        else if (crcStatusDigit == 2)
        {
            crcStatus = ECrcStatus.Valid;
        }
        else if (crcStatusDigit == 3)
        {
            crcStatus = ECrcStatus.Invalid;
        }
        else if (crcStatusDigit == 4)
        {
            crcStatus = ECrcStatus.BadMemory;
        }
        else if (crcStatusDigit == 5)
        {
            crcStatus = ECrcStatus.Disabled;
        }
        else
        {
            throw new Exception("Unable to determine CRC status");
        }
        
        // __get_game_size
        var gameSize = await GetGameSizeAsync(cancellationToken);

        if (gameSize == 0 && crc == 0 && crcStatus == ECrcStatus.Valid)
        {
            crcStatus = ECrcStatus.Invalid;
        }
        
        // __host_control_read
        var control = await ReadHostControlAsync(cancellationToken);

        return new NetDimmInfo(
            crc,
            gameSize,
            crcStatus,
            dimmMemory,
            versionEnum,
            gameMemory << 20,
            control);
    }

    public async Task<int> GetCrcInformationAsync(CancellationToken cancellationToken)
    {
        var data = await DownloadAsync(0xfffeffe0, 4, cancellationToken);
        
        return StructPacker.UnpackSingle<int>("<I", data);
    }
    
    public async Task<int> GetGameSizeAsync(CancellationToken cancellationToken)
    {
        var data = await DownloadAsync(0xffff0004, 4, cancellationToken);
        
        return StructPacker.UnpackSingle<int>("<I", data);
    }

    public async Task<int> ReadHostControlAsync(CancellationToken cancellationToken)
    {
        await _connection.WritePacketAsync(NetDimmPacketFactory.ReadControlData(), cancellationToken);
        
        var packet = await _connection.ReadPacketAsync(cancellationToken);

        if (packet.Id != 0x10)
        {
            throw new Exception("Invalid packet ID received");
        }

        if (packet.Length != 8)
        {
            throw new  Exception("Unexpected length of control packet");
        }
        
        var unpacked = StructPacker.Unpack<Tuple<int, int>>("<II", packet.Data);

        return unpacked.Item2;
    }

    public async Task<byte[]> DownloadAsync(uint address, int size, CancellationToken cancellationToken)
    {
        var memoryAddress = StructPacker.Pack("<II", address, size);
        
        await _connection.WritePacketAsync(NetDimmPacketFactory.ReadMemoryAddress(memoryAddress.Build()), cancellationToken);
        
        var data = new List<byte>();

        while (true)
        {
            var packet = await _connection.ReadPacketAsync(cancellationToken);

            if (packet.Id != 0x04)
            {
                throw new Exception("Invalid packet ID received");
            }

            if (packet.Length <= 10)
            {
                throw new Exception("Unexpected data length");
            }
            
            data.AddRange(packet.Data[10..]);

            if ((packet.Flags & 0x1) != 0)
            {
                return data.ToArray();
            }
        }
    }
    
    public void Dispose()
    {
        _connection.Dispose();
    }
}