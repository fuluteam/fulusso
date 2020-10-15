namespace Fulu.Passport.Domain
{
    public class RegexConstance
    {
        public const string IsPhone = "^(1[3-9])\\d{9}$";

        public const string IsUserName = "^[a-zA-Z0-9]{1}[a-zA-Z0-9_]{5,17}$";

        public const string IsEmail = "[\\w!#$%&\'*+/=?^_`{|}~-]+(?:\\.[\\w!#$%&\'*+/=?^_`{|}~-]+)*@(?:[\\w](?:[\\w-]*[\\w])?\\.)+[\\w](?:[\\w-]*[\\w])?";

        public const string IsPassword = "^.{6,20}$";

        public const string IsSubAccount = @"^[\w-]+(\.[\w-]+)*@[\d]{5,13}$";
    }
}
