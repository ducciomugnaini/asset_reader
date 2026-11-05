namespace AssetReader.Infrastructure.BorsaItaliana;

public sealed class BorsaItalianaUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);

public sealed class BorsaItalianaPayloadException(string message, Exception? innerException = null)
    : Exception(message, innerException);
