namespace AspireLearning.ServiceDefaults.GlobalConstant;

public static class CountryConstants 
{
    public static class Turkey
    {
        public readonly static Guid Id = new("91ecd1fa-4e8c-4923-8244-2fe289cd36a9");
        
        public static class Texts
        {
            public static class TR
            {
                public readonly static Guid Id = new("221961e1-f387-42c8-b9bd-81008fcaf12e");
                public const string Name = "Türkiye";
                public const string Code = "TR";
            }
            
            public static class EN
            {
                public readonly static Guid Id = new("9c22d74f-b485-4e58-8598-d62333a91225");
                public const string Name = "Turkey";
                public const string Code = "TR";
            }
        }
    }

    public static class USA
    {
        public readonly static Guid Id = new("a99f35bb-a02c-4709-8496-b87ce14b4864");
        
        public static class Texts
        {
            public static class EN
            {
                public readonly static Guid Id = new("bc2b9297-8428-4989-bea2-efa845c1cf8e");
                public const string Name = "United States of America";
                public const string Code = "US";
            }
            
            public static class TR
            {
                public readonly static Guid Id = new("c101df90-1eb9-4695-8aa6-f1df3eca265c");
                public const string Name = "Amerika Birleşik Devletleri";
                public const string Code = "US";
            }
        }
    }
}
