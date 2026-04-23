namespace JigaMotor.Everynet.Api.Exceptions
{
    public class EverynetDeviceAlreadyExistsException : Exception
    {
        public EverynetDeviceAlreadyExistsException(string devEui) 
            : base($"O dispositivo com DevEui {devEui} já existe na Everynet.")
        {
        }
    }
}
