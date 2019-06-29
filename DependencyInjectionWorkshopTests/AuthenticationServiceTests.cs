using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [Test]
        public void is_valid()
        {
            var profile = Substitute.For<IProfile>();
            var hash = Substitute.For<IHash>();
            var otpService = Substitute.For<IOtpService>();

            var failedCounter = Substitute.For<IFailedCounter>();
            var notification = Substitute.For<INotification>();
            var logger = Substitute.For<ILogger>();

            var authenticationService = new AuthenticationService(profile, hash, failedCounter, otpService, notification, logger);

            profile.GetPassword("shih_jie").ReturnsForAnyArgs("6543215");
            hash.GetHash("123456").ReturnsForAnyArgs("6543215");

            otpService.GetCurrentOtp("shih_jie").ReturnsForAnyArgs("9527");

            var isValid = authenticationService.Verify("shih_jie", "123456", "9527");

            Assert.IsTrue(isValid);
        }
    }
}