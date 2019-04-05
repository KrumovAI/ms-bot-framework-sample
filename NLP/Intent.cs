namespace BasicBot.NLP
{
    using System.Collections.Generic;

    public class Intent
    {
        public string Id { get; set; }

        public IEnumerable<Property> Properties { get; set; }

        public string DialogId { get; set; }
    }
}
