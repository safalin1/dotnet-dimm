namespace Netboot.Networking;

public record NetDimmInfo(
    int LoadedGameCrc,
    int LoadedGameSize,
    ECrcStatus LoadedGameCrcStatus,
    int MemorySize,
    ENetDimmVersion FirmwareVersion,
    int AvailableGameMemory,
    int ControlAddress
);