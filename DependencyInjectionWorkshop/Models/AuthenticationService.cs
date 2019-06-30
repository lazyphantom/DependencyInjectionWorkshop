using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool Verify(string account, string password, string otp);
    }


    public class Authentication : IAuthentication
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IFailedCounter _failedCounter;
        private readonly IOtpService _otpService;
        
        private readonly ILogger _logger;

        public Authentication(IProfile profile, IHash hash,
            IFailedCounter failedCounter, IOtpService otpService,
            ILogger logger)
        {
            _profile = profile;
            _hash = hash;
            _failedCounter = failedCounter;
            _otpService = otpService;
            _logger = logger;
        }

        public Authentication()
        {
            _profile = new Profile();
            _hash = new Hash();
            _failedCounter = new FailedCounter();
            _otpService = new OtpService();
            _logger = new NLogAdapter();
        }

        public bool Verify(string account, string password, string otp)
        {
            //FailedCounterDecorator.CheckAccountIsLocked(account, _failedCounter);

            var verifyPasswordFromDb = _profile.GetPassword(account);

            var verifyPasswordFromHash = _hash.GetHash(password);

            var verifyOtp = _otpService.GetCurrentOtp(account);

            #region 驗證成功

            if (otp.Equals(verifyOtp) && verifyPasswordFromDb.Equals(verifyPasswordFromHash))
            {
                //ResetFailedCount(account, _failedCounter);
                return true;
            }

            #endregion

            //NotificationDecorator.VerifyWithNotification(account, _notification);

            _failedCounter.AddFailedCount(account);

            var failedCount = _failedCounter.GetFailedCount(account);
            _logger.Info($"accountId:{account} failed times:{failedCount}");

            return false;
        }

    }
}