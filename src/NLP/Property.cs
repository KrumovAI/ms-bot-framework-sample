namespace BasicBot.NLP
{
    using System;

    public class Property
    {
        public Property(string name, Type type, string entityType)
        {
            this.Name = name;
            this.Type = type;
            this.EntityType = entityType;
        }

        public string Name { get; set; }

        public Type Type { get; set; }

        public string EntityType { get; set; }
    }
}
