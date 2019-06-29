using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly FailedCountAdapter _failedCountAdapter;
        private readonly OtpService _otpService;
        private readonly SlackAdapter _slackAdapter;
        private readonly NLogAdapter _nLogAdapter;

        public AuthenticationService(ProfileDao profileDao, Sha256Adapter sha256Adapter, FailedCountAdapter failedCountAdapter, OtpService otpService, SlackAdapter slackAdapter, NLogAdapter nLogAdapter)
        {
            _profileDao = profileDao;
            _sha256Adapter = sha256Adapter;
            _failedCountAdapter = failedCountAdapter;
            _otpService = otpService;
            _slackAdapter = slackAdapter;
            _nLogAdapter = nLogAdapter;
        }

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _failedCountAdapter = new FailedCountAdapter();
            _otpService = new OtpService();
            _slackAdapter = new SlackAdapter();
            _nLogAdapter = new NLogAdapter();
        }

        public bool Verify(string account, string password, string otp)
        {
            var failedCountAdapter = _failedCountAdapter;
            var isLocked = failedCountAdapter.IsAccountLocked(account);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }

            var verifyPasswordFromDb = _profileDao.GetPassword(account);

            var verifyPasswordFromHash = _sha256Adapter.GetHash(password);

            var verifyOtp = _otpService.GetCurrentOtp(account);

            #region 驗證成功
            if (otp.Equals(verifyOtp) && verifyPasswordFromDb.Equals(verifyPasswordFromHash))
            {
                failedCountAdapter.ResetFailedCount(account);
                return true;
            }
            #endregion

            _slackAdapter.PushMessage();

            failedCountAdapter.AddFailedCount(account);

            var failedCount = failedCountAdapter.GetFailedCount(account);
            _nLogAdapter.Info($"accountId:{account} failed times:{failedCount}");

            return false;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}