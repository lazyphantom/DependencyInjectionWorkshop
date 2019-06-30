namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator: BaseAuthenticationDecorator
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authentication, IFailedCounter failedCounter) : base(authentication)
        {
            _failedCounter = failedCounter;
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

        public override bool Verify(string account, string password, string otp)
        {
            CheckAccountIsLocked(account);
            var isValid = base.Verify(account, password, otp);
            if (isValid)
            {
                ResetFailedCount(account);
            }
            else
            {
                AddFailedCount(account);
            }

            return isValid;
        }

        private void AddFailedCount(string account)
        {
            _failedCounter.AddFailedCount(account);
        }
    }
}