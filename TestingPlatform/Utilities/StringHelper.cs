using TestingPlatform.Domain.Interfaces;

namespace TestingPlatform.Utilities
{
    public static class StringHelper
    {
        public static bool ValidateEmail(string text, Label label, ILocalizationService localization)
        {
            if (string.IsNullOrEmpty(text))
            {
                label.Text = localization.GetString("EmailCannotBeEmpty", localization.CurrentCulture);
                label.IsVisible = true;
                return false;
            }

            if (text.Contains(' '))
            {
                label.Text = localization.GetString("EmailShouldNotContainSpaces", localization.CurrentCulture);
                label.IsVisible = true;
                return false;
            }

            if (!text.RegexIsMatch(RegexPatterns.Email))
            {
                label.Text = localization.GetString("EmailWasEnteredInAnIncorrectFormat", localization.CurrentCulture);
                label.IsVisible = true;
                return false;
            }

            return true;
        }

        public static bool ValidatePassword(string text, Label label, ILocalizationService localization)
        {
            if (string.IsNullOrEmpty(text))
            {
                label.Text = localization.GetString("PasswordCannotBeEmpty", localization.CurrentCulture);
                label.IsVisible = true;
                return false;
            }

            if (text.Length < 8)
            {
                label.Text = localization.GetString("PasswordMustBeAtLeast8CharactersLong", localization.CurrentCulture);
                label.IsVisible = true;
                return false;
            }

            if (!text.Any(char.IsUpper))
            {
                label.Text = localization.GetString("PasswordMustContainAtLeastOneCapitalLetter", localization.CurrentCulture);
                label.IsVisible = true;
                return false;
            }

            if (!text.Any(char.IsLower))
            {
                label.Text = localization.GetString("PasswordMustContainAtLeastOneLowercaseLetter", localization.CurrentCulture);
                label.IsVisible = true;
                return false;
            }

            if (!text.Any(char.IsDigit))
            {
                label.Text = localization.GetString("PasswordMustContainAtLeastOneNumber", localization.CurrentCulture);
                label.IsVisible = true;
                return false;
            }

            if (text.Contains(' '))
            {
                label.Text = localization.GetString("PasswordMustNotContainSpaces", localization.CurrentCulture);
                label.IsVisible = true;
                return false;
            }

            if (!text.RegexIsMatch(RegexPatterns.Password))
            {
                label.Text = localization.GetString("PasswordMustContainAtLeastOneSpecialCharacter", localization.CurrentCulture);
                label.IsVisible = true;
                return false;
            }

            return true;
        }
    }
}