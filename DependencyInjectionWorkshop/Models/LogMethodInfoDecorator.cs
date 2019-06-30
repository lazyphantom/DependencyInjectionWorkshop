namespace DependencyInjectionWorkshop.Models
{
    public class LogMethodInfoDecorator:BaseAuthenticationDecorator
    {
        private readonly ILogger _logger;


        public LogMethodInfoDecorator(IAuthentication authentication, ILogger logger) : base(authentication)
        {
            _logger = logger;
        }

        public override bool Verify(string account, string password, string otp)
        {
            _logger.Info($"Start : {nameof(AuthenticationService)}.{nameof(Verify)}({account}, {password}, {otp})");
            var isValid = base.Verify(account, password, otp);
            _logger.Info($"End : {nameof(AuthenticationService)}.{nameof(Verify)}({account}, {password}, {otp}) {isValid}");
            return isValid;
        }
    }
}