namespace Netboot.Networking;

public enum ECrcStatus
{
    Unknown = 0,
    Checking = 1,
    Valid = 2,
    Invalid = 3,
    BadMemory = 4,
    Disabled = 5
}