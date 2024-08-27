using System;

public class CabinetValidationException : Exception
{
    public string CabinetName { get; private set; }

    public CabinetValidationException(string cabinetName)
        : base($"Validation failed for cabinet: {cabinetName}")
    {
        CabinetName = cabinetName;
    }

    public CabinetValidationException(string cabinetName, string message)
        : base($"Validation failed for cabinet: {cabinetName}. {message}")
    {
        CabinetName = cabinetName;
    }

    public CabinetValidationException(string cabinetName, string message, Exception innerException)
        : base($"Validation failed for cabinet: {cabinetName}. {message}", innerException)
    {
        CabinetName = cabinetName;
    }
}

public class CabinetPartException : CabinetValidationException
{
    public string PartName { get; private set; }
    public int PartPosition { get; private set; }

    public CabinetPartException(string cabinetName, string partName)
        : base(cabinetName, $"Operation failed for part: {partName}")
    {
        PartName = partName;
    }

    public CabinetPartException(string cabinetName, string partName, string message)
        : base(cabinetName, $"Operation failed for part: {partName}. {message}")
    {
        PartName = partName;
    }

    public CabinetPartException(string cabinetName, string partName, string message, Exception innerException)
        : base(cabinetName, $"Operation failed for part: {partName}. {message}", innerException)
    {
        PartName = partName;
    }
    public CabinetPartException(string cabinetName, int partPosition)
       : base(cabinetName, $"Operation failed for part: #{partPosition}")
    {
        PartPosition = partPosition;
    }

    public CabinetPartException(string cabinetName, int partPosition, string message)
        : base(cabinetName, $"Operation failed for part: #{partPosition}. {message}")
    {
        PartPosition = partPosition;
    }

    public CabinetPartException(string cabinetName, int partPosition, string message, Exception innerException)
        : base(cabinetName, $"Operation failed for part: #{partPosition}. {message}", innerException)
    {
        PartPosition = partPosition;
    }
}
