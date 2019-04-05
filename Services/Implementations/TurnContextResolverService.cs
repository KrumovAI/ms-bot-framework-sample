namespace BasicBot.Services
{
    using Microsoft.Bot.Builder;

    public class TurnContextResolverService : ITurnContextResolverService
    {
        private ITurnContext turnContext;

        public ITurnContext TurnContext { get => this.turnContext; }

        public void UpdateTurnContext(ITurnContext turnContext)
        {
            this.turnContext = turnContext;
        }
    }
}
