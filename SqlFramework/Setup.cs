namespace SqlFramework
{
    public static class Setup
    {
        public static string ConnectionString;
        public static char Pchar = '@';

        public static string BeforeInitMessage =
            "This SqlLine object has not been initialized yet. It will initialize on the first query";
    }
}
