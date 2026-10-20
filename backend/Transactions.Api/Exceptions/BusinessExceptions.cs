namespace Transactions.Api.Exceptions;

public class NotFoundException(string message) : Exception(message);

public class BusinessRuleException(string message) : Exception(message);

public class InsufficientStockException(string message) : Exception(message);

public class ExternalServiceException(string message, Exception? innerException = null)
    : Exception(message, innerException);
