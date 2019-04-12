namespace BasicBot.Infrastructure.Exceptions
{
    using System;
    using BasicBot.Infrastructure.Constants;

    public class BadRequestException : Exception
    {
        public BadRequestException(string message = MessageConstants.BadRequest)
            : base(message)
        {
        }
    }
}
