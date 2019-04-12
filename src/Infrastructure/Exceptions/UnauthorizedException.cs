namespace BasicBot.Infrastructure.Exceptions
{
    using BasicBot.Infrastructure.Constants;
    using System;

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message = MessageConstants.UnauthorizedRequest) 
            : base(message)
        {
        }
    }
}
