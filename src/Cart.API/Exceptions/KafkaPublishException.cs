namespace Cart.API.Exceptions;

public class KafkaPublishException(string message, Exception innerException) : Exception(message, innerException);