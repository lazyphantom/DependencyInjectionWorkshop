using System;
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
        private readonly IOtpService _otpService;

        public Authentication(IProfile profile, IHash hash, IOtpService otpService)
        {
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
        }

        public Authentication()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
        }

        public bool Verify(string account, string password, string otp)
        {
            //BaseAuthenticationDecorator.CheckAccountIsLocked(account, _failedCounter);

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

            //_failedCounter.AddFailedCount(account);

            //LogFailedCountDecorator.LogFailedCount(account, _failedCounter, _logger);

            return false;
        }

        public static implicit operator Authentication(NotificationDecorator v)
        {
            throw new NotImplementedException();
        }
    }
}