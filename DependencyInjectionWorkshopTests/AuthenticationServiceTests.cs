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
        private const string defaultOtp = "9527";
        private const string defaultHashedPassword = "6543215";
        private IProfile _profile;
        private IHash _hash;
        private IOtpService _otpService;
        private IFailedCounter _failedCounter;
        private INotification _notification;
        private ILogger _logger;
        private AuthenticationService _authenticationService;

        [SetUp]
        public void SetUp()
        {
            _profile = Substitute.For<IProfile>();
            _hash = Substitute.For<IHash>();
            _otpService = Substitute.For<IOtpService>();

            _failedCounter = Substitute.For<IFailedCounter>();
            _notification = Substitute.For<INotification>();
            _logger = Substitute.For<ILogger>();

            _authenticationService = new AuthenticationService(_profile, _hash, _failedCounter, _otpService, _notification, _logger);
        }

        [Test]
        public void is_valid()
        {
            GivenPasswordFromDb(DefaultAccount, defaultHashedPassword);
            GivenHashPassword(DefaultPassword, defaultHashedPassword);

            GivenOtp(DefaultAccount, defaultOtp);

            var isValid = WhenVerify(DefaultAccount, DefaultPassword, defaultOtp);

            ShouldBeValid(isValid);
        }
        [Test]
        public void is_invalid_when_otp_is_wrong()
        {
            GivenPasswordFromDb(DefaultAccount, defaultHashedPassword);
            GivenHashPassword(DefaultPassword, defaultHashedPassword);

            GivenOtp(DefaultAccount, defaultOtp);

            var isValid = WhenVerify(DefaultAccount, DefaultPassword, "wrong otp");

            ShouldBeInvalid(isValid);
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