/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using Meta.XR.MRUtilityKit;

/// <summary>Last MRUK device load outcome for UI / logs.</summary>
public static class MRSceneLoadState
{
    public static MRUK.LoadDeviceResult LastLoadResult { get; internal set; } = MRUK.LoadDeviceResult.NoRoomsFound;
    public static string LastLoadMessage { get; internal set; } = "";
    public static string LastFaultDetail { get; internal set; } = "";

    public static void Reset()
    {
        LastLoadResult = MRUK.LoadDeviceResult.NoRoomsFound;
        LastLoadMessage = "";
        LastFaultDetail = "";
    }

    public static string Describe(MRUK.LoadDeviceResult result)
    {
        string message;
        switch (result)
        {
            case MRUK.LoadDeviceResult.Success:
                message = "sala carregada";
                break;
            case MRUK.LoadDeviceResult.NoScenePermission:
                message = "sem permissão USE_SCENE";
                break;
            case MRUK.LoadDeviceResult.NoRoomsFound:
                message = "nenhuma sala no Quest — conclua o Space Setup";
                break;
            case MRUK.LoadDeviceResult.FailureDataIsInvalid:
                message = "falha ao processar sala";
                break;
            case MRUK.LoadDeviceResult.FailureInsufficientResources:
                message = "memória insuficiente — tente de novo";
                break;
            case MRUK.LoadDeviceResult.FailureInsufficientView:
                message = "olhe ao redor para mapear a sala";
                break;
            case MRUK.LoadDeviceResult.FailurePermissionInsufficient:
                message = "permissão insuficiente (USE_SCENE / USE_ANCHOR_API)";
                break;
            default:
                message = result.ToString();
                break;
        }

        if (!string.IsNullOrEmpty(LastFaultDetail))
            message += "\n" + LastFaultDetail;

        return message;
    }
}
