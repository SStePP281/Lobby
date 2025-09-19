using Unity.Services.CloudSave.Models;
using Unity.Services.CloudSave.Models.Data.Player;

public class PrivateWriteAccessClassOptions : WriteAccessClassOptions
{
    public override AccessClass AccessClass => AccessClass.Private;
}

public class PrivateReadAccessClassOptions : ReadAccessClassOptions
{
    public override AccessClass AccessClass => AccessClass.Private;

    public override string PlayerId => AuthenticationServiceManager.Instance.GetUserId();
}