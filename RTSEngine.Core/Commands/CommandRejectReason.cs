namespace RTSEngine.Core.Commands;

public enum CommandRejectReason
{
    None = 0,
    Empty = 1,
    TooManyUnits = 2,
    MissingTarget = 3,
    UnknownCommand = 4,
    NotOwner = 5
}
