using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao = new ProfileDao();
        private readonly Sha256Adapter _sha256Adapter = new Sha256Adapter();
        private readonly OtpService _otpService = new OtpService();
        private readonly NLogAdapter _nLogAdapter = new NLogAdapter();
        private readonly SlackAdapter _slackAdapter = new SlackAdapter();
        private readonly FailedCountAdapter _failedCountAdapter = new FailedCountAdapter();

        public bool Verify(string account, string password, string otp)
        {
            var isLocked = _failedCountAdapter.IsAccountLocked(account);
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
                _failedCountAdapter.ResetFailedCount(account);
                return true;
            }
            #endregion

            _slackAdapter.PushMessage();

            _failedCountAdapter.AddFailedCount(account);

            var failedCount = _failedCountAdapter.GetFailedCount(account);
            _nLogAdapter.Info($"accountId:{account} failed times:{failedCount}");

            return false;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}