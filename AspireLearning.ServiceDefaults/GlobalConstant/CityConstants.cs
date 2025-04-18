namespace AspireLearning.ServiceDefaults.GlobalConstant;

public static class CityConstants 
{
    public static class Istanbul
    {
        public readonly static Guid Id = new("895f23d9-fb86-4846-b745-6b20d0eaa7ea");
        public readonly static Guid CountryId = CountryConstants.Turkey.Id;
        
        public static class Texts
        {
            public static class TR
            {
                public readonly static Guid Id = new("0bcb1039-21bc-44ec-b12b-db7280404220");
                public const string Name = "İstanbul";
            }
            
            public static class EN
            {
                public readonly static Guid Id = new("6bd15218-6055-4310-b0c6-7bd807cd2c8f");
                public const string Name = "Istanbul";
            }
        }
    }
    
    public static class LosAngeles
    {
        public readonly static Guid Id = new("c420f15b-e6c2-45c1-b96b-ada41b93e50a");
        public readonly static Guid CountryId = CountryConstants.USA.Id;
        
        public static class Texts
        {
            public static class EN
            {
                public readonly static Guid Id = new("31311302-b2e7-4355-8f9d-653339333600");
                public const string Name = "Los Angeles";
            }
            
            public static class TR
            {
                public readonly static Guid Id = new("833c583b-7a96-4c09-be98-7d9cc8a02dbf");
                public const string Name = "Los Angeles";
            }
        }
    }
}
