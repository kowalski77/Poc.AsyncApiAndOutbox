namespace Poc.FirmwareUploadOutbox.Outbox;

public enum EventState
{
    Pending = 0,
    Finalized = 1,
    Failed = 2
}
