using System;
using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const string DefaultAccount = "shih_jie";
        private const string DefaultPassword = "123456";
        private const string DefaultOtp = "9527";
        private const string DefaultHashedPassword = "6543215";
        private const string WrongOtp = "wrong otp";
        private const string DefaultFailedCount = "88";
        private IProfile _profile;
        private IHash _hash;
        private IOtpService _otpService;
        private IFailedCounter _failedCounter;
        private INotification _notification;
        private ILogger _logger;
        private IAuthenticationService _authenticationService;

        [SetUp]
        public void SetUp()
        {
            _profile = Substitute.For<IProfile>();
            _hash = Substitute.For<IHash>();
            _otpService = Substitute.For<IOtpService>();
            _notification = Substitute.For<INotification>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _notification = Substitute.For<INotification>();
            _logger = Substitute.For<ILogger>();

            _authenticationService = new AuthenticationService(_profile, _hash, _failedCounter, _otpService, _logger);

            _authenticationService = new NotificationDecorator(_authenticationService, _notification);
        }

        [Test]
        public void is_valid()
        {
            GivenPasswordFromDb(DefaultAccount, DefaultHashedPassword);
            GivenHashPassword(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccount, DefaultOtp);

            var isValid = WhenVerify(DefaultAccount, DefaultPassword, DefaultOtp);
            
            ShouldBeValid(isValid);
        }

        private bool WhenValid()
        {
            GivenPasswordFromDb(DefaultAccount, DefaultHashedPassword);
            GivenHashPassword(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccount, DefaultOtp);

            var isValid = WhenVerify(DefaultAccount, DefaultPassword, DefaultOtp);
            return isValid;
        }

        [Test]
        public void is_invalid_when_otp_is_wrong()
        {
            GivenPasswordFromDb(DefaultAccount, DefaultHashedPassword);
            GivenHashPassword(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccount, DefaultOtp);

            var isValid = WhenVerify(DefaultAccount, DefaultPassword, WrongOtp);

            ShouldBeInvalid(isValid);
        }
        [Test]
        public void should_notify_when_invalid()
        {
            WhenInvalid();
            ShouldNotify(DefaultAccount);
        }
        [Test]
        public void should_add_failed_count_when_invalid()
        {
            WhenInvalid();
            ShouldAddFailedCount(DefaultAccount);
        }
        [Test]
        public void should_reset_failed_count_when_valid()
        {
            WhenInvalid();
            ShouldResetFailedCount(DefaultAccount);
        }
        [Test]
        public void should_log_failed_count_when_invalid()
        {
            GivenFailedCount(DefaultFailedCount);
            WhenInvalid();
            _logger.Received().Info(Arg.Is<string>(m => m.Contains(DefaultAccount)&& m.Contains(DefaultFailedCount)));
        }

        [Test]
        public void account_is_locked()
        {
            GivenAccountIsLocked();
            ShouldThrow<FailedTooManyTimesException>();
        }
        
        private void ShouldThrow<TException>() where TException : Exception
        {
            TestDelegate action = () => WhenValid();

            Assert.Throws<TException>(action);
        }

        private void GivenAccountIsLocked()
        {
            _failedCounter.IsAccountLocked(DefaultAccount).ReturnsForAnyArgs(true);
        }

        private void GivenFailedCount(string failedCount)
        {
            _failedCounter.GetFailedCount(DefaultAccount).ReturnsForAnyArgs(failedCount);
        }

        private void ShouldResetFailedCount(string account)
        {
            _failedCounter.ResetFailedCount(account);
        }

        private void ShouldAddFailedCount(string account)
        {
            _failedCounter.Received().AddFailedCount(account);
        }

        private void ShouldNotify(string account)
        {
            _notification.Received().PushMessage(Arg.Is<string>(m => m.Contains(account)));
        }

        private bool WhenInvalid()
        {
            GivenPasswordFromDb(DefaultAccount, DefaultHashedPassword);
            GivenHashPassword(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccount, DefaultOtp);

            var isValid = WhenVerify(DefaultAccount, DefaultPassword, WrongOtp);
            return isValid;
        }

        private static void ShouldBeInvalid(bool isValid)
        {
            Assert.IsFalse(isValid);
        }

        private static void ShouldBeValid(bool isValid)
        {
            Assert.IsTrue(isValid);
        }

        private bool WhenVerify(string account, string password, string otp)
        {
            return _authenticationService.Verify(account, password, otp);
        }

        private void GivenOtp(string account, string otp)
        {
            _otpService.GetCurrentOtp(account).ReturnsForAnyArgs(otp);
        }

        private void GivenHashPassword(string password, string hashedPassword)
        {
            _hash.GetHash(password).ReturnsForAnyArgs(hashedPassword);
        }

        private void GivenPasswordFromDb(string account, string hashedPassword)
        {
            _profile.GetPassword(account).ReturnsForAnyArgs(hashedPassword);
        }
    }
}