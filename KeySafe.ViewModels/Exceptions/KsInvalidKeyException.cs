namespace KeySafe.ViewModels.Exceptions;

public class KsInvalidKeyException : Exception
{
    public KsInvalidKeyException(Exception ex) : base("invalid key", ex)
    {
        
    }
}
