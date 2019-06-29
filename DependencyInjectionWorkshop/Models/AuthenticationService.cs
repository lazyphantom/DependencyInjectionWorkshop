using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IFailedCounter _failedCounter;
        private readonly IOtpService _otpService;
        private readonly INotification _notification;
        private readonly ILogger _logger;

        public AuthenticationService(IProfile profile, IHash hash,
            IFailedCounter failedCounter, IOtpService otpService, INotification notification,
            ILogger logger)
        {
            _profile = profile;
            _hash = hash;
            _failedCounter = failedCounter;
            _otpService = otpService;
            _notification = notification;
            _logger = logger;
        }

        public AuthenticationService()
        {
            _profile = new Profile();
            _hash = new Hash();
            _failedCounter = new FailedCounter();
            _otpService = new OtpService();
            _notification = new Notification();
            _logger = new NLogAdapter();
        }

        public bool Verify(string account, string password, string otp)
        {
            var isLocked = _failedCounter.IsAccountLocked(account);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }

            var verifyPasswordFromDb = _profile.GetPassword(account);

            var verifyPasswordFromHash = _hash.GetHash(password);

            var verifyOtp = _otpService.GetCurrentOtp(account);

            #region 驗證成功

            if (otp.Equals(verifyOtp) && verifyPasswordFromDb.Equals(verifyPasswordFromHash))
            {
                _failedCounter.ResetFailedCount(account);
                return true;
            }

            #endregion

            _notification.PushMessage();

            _failedCounter.AddFailedCount(account);

            var failedCount = _failedCounter.GetFailedCount(account);
            _logger.Info($"accountId:{account} failed times:{failedCount}");

            return false;
        }
    }
}