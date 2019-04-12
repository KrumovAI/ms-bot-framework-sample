namespace BasicBot.Services
{
    using Microsoft.Bot.Builder;

    public interface ITurnContextResolverService
    {
        ITurnContext TurnContext { get; }

        void UpdateTurnContext(ITurnContext turnContext);
    }
}
