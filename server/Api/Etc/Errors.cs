namespace Api.Etc;

public abstract class AppError(string Message) : Exception(Message) { }

public class NotFoundError(string type, object Properties)
    : AppError($"{type}({Properties}) was not found!")
{
    public object Properties { get; } = Properties;
}

public class UnauthorizedError(string type) : AppError($"Operation not authorized for {type}!") { }

public class ForbiddenError() : AppError("Forbiddden!") { }

public class AuthenticationError() : AppError("Unable to authenticate!") { }
