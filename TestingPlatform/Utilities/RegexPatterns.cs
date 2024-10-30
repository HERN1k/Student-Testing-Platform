namespace TestingPlatform.Utilities
{
    public class RegexPatterns
    {
        public const string Email = @"^[a-zA-Z0-9_]+(\.[a-zA-Z0-9_]+)*@[a-zA-Z0-9]+\.[a-zA-Z]{2,}(?:\.[a-zA-Z]{2,})?$";

        public const string Password = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*()\-_{}\/\\])(?!.*\s).{8,}$";
    }
}