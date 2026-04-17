namespace JigaMotor.SharePoint.Api.Exceptions
{
    /// <summary>
    /// Usada quando uma de negócio da API do SharePoint é violada, como por exemplo, tentar cadastrar um dispositivo com um DevEui que já existe.
    /// </summary>
    public class SharePointBusinessException(string message): Exception(message)
    {
    }
}
