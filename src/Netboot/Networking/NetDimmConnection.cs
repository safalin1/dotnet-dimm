using System.Net;
using System.Net.Sockets;

namespace Netboot.Networking;

public class NetDimmConnection : IDisposable 
{
    private readonly IPEndPoint _endpoint;
    private readonly Socket _socket;

    public NetDimmConnection(IPAddress endpoint, int port = 10703)
    {
        _endpoint = new IPEndPoint(endpoint, port);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public async ValueTask ConnectAsync(CancellationToken cancellationToken)
    {
        await _socket.ConnectAsync(_endpoint, cancellationToken);
        
        // Test by sending noop
        await WritePacketAsync(NetDimmPacketFactory.NoOp(), cancellationToken);
    }

    public async Task WritePacketAsync(NetDimmPacket packet, CancellationToken cancellationToken)
    {
        if (!_socket.Connected)
        {
            throw new InvalidOperationException("Connection to netdimm is not open");
        }

        var buffer = StructPacker.Pack("<I", ((packet.Id & 0xff) << 24) | ((packet.Flags & 0xff) << 16) | (packet.Length & 0xffff));
    
        buffer.AppendBytes(packet.Data);
    
        await _socket.SendAsync(buffer.Build(), cancellationToken);
    }

    public async Task<NetDimmPacket> ReadPacketAsync(CancellationToken cancellationToken)
    {
        const int headerLength = 4;
        
        if (!_socket.Connected)
        {
            throw new InvalidOperationException("Connection to netdimm is not open");
        }
        
        var headerBytes = await ReadBytes(headerLength, cancellationToken);
        
        var header = StructPacker.UnpackSingle<int>("<I", headerBytes);
        var dataLength = header & 0xffff;
        
        var packet = new NetDimmPacket((header >> 24) & 0xff, (header >> 16) & 0xff, []);

        if (dataLength > 0)
        {
            packet.Data = await ReadBytes(dataLength, cancellationToken);
        }

        return packet;
    }
    
    public void Dispose()
    {
        if (_socket.Connected)
        {
            _socket.Close();
        }

        _socket.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<byte[]> ReadBytes(int length, CancellationToken cancellationToken)
    {
        var buffer = new byte[length];

        var received = await _socket.ReceiveAsync(buffer, cancellationToken);

        if (received != length)
        {
            throw new SocketException((int)SocketError.Interrupted);
        }
        
        return buffer;
    }
}