namespace Transactions.Api.Exceptions;

// =====================================================================
// Excepciones que lanzo desde el Service o el cliente HTTP. El manejador
// global las convierte en el código HTTP que corresponde.
// =====================================================================

// → 404: la transacción o el producto no existen.
public class NotFoundException(string message) : Exception(message);

// → 400: los datos tienen buen formato pero rompen una regla (fecha futura, rango invertido...).
public class BusinessRuleException(string message) : Exception(message);

// → 409: Productos rechazó el ajuste porque el stock no alcanza.
// Reenvío el mensaje que me da Productos, que ya explica cuánto stock hay.
public class InsufficientStockException(string message) : Exception(message);

// → 503: no pude comunicarme con el servicio de Productos (caído o timeout).
public class ExternalServiceException(string message, Exception? innerException = null)
    : Exception(message, innerException);
