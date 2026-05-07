namespace HouseBudget.Domain.Exceptions;

public class UnauthorizedDomainException : DomainException
{
    public UnauthorizedDomainException() : base("You are not authorized to perform this action.") { }
    public UnauthorizedDomainException(string message) : base(message) { }
}
