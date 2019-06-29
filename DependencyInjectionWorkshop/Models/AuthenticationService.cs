using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IFailedCountAdapter _failedCountAdapter;
        private readonly IOtpService _otpService;
        private readonly INnotification _notification;
        private readonly ILogger _logger;

        public AuthenticationService(IProfile profile, IHash hash,
            IFailedCountAdapter failedCountAdapter, IOtpService otpService, INnotification notification,
            ILogger logger)
        {
            _profile = profile;
            _hash = hash;
            _failedCountAdapter = failedCountAdapter;
            _otpService = otpService;
            _notification = notification;
            _logger = logger;
        }

        public AuthenticationService()
        {
            _profile = new Profile();
            _hash = new Hash();
            _failedCountAdapter = new FailedCountAdapter();
            _otpService = new OtpService();
            _notification = new Nnotification();
            _logger = new NLogAdapter();
        }

        public bool Verify(string account, string password, string otp)
        {
            var isLocked = _failedCountAdapter.IsAccountLocked(account);
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
                _failedCountAdapter.ResetFailedCount(account);
                return true;
            }

            #endregion

            _notification.PushMessage();

            _failedCountAdapter.AddFailedCount(account);

            var failedCount = _failedCountAdapter.GetFailedCount(account);
            _logger.Info($"accountId:{account} failed times:{failedCount}");

            return false;
        }
    }
}