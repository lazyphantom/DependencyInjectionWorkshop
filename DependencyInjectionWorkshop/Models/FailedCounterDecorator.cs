namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator:IAuthentication
    {
        private readonly IFailedCounter _failedCounter;
        private readonly IAuthentication _authentication;

        public FailedCounterDecorator(IAuthentication authentication, IFailedCounter failedCounter)
        {
            _failedCounter = failedCounter;
            _authentication = authentication;
        }

        private void CheckAccountIsLocked(string account)
        {
            var isLocked = _failedCounter.IsAccountLocked(account);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }
        }

        private void ResetFailedCount(string account)
        {
            _failedCounter.ResetFailedCount(account);
        }

        public bool Verify(string account, string password, string otp)
        {
            CheckAccountIsLocked(account);
            var isValid = _authentication.Verify(account, password, otp);
            if (isValid)
            {
                ResetFailedCount(account);
            }
            return isValid;
        }
    }
}