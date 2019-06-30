using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    class Program
    {
        private static IContainer _container;

        static void Main(string[] args)
        {
            /*
            //var profileDao = new ProfileDao();
            var profileDao = new FakeProfile();
            //var sha256Adapter = new Sha256Adapter();
            var sha256Adapter = new FakeHash();
            //var otpService = new OtpService();
            var otpService = new FakeOtp();

            //var notification = new Notification();
            var notification = new FakeSlack();
            //var failedCounter = new FailedCounter();
            var failedCounter = new FakeFailedCounter();
            //var nLogAdapter = new NLogAdapter();
            var nLogAdapter = new ConsoleAdapter();

            var authentication = new AuthenticationService(profileDao, sha256Adapter, otpService);

            var notificationDecorator = new NotificationDecorator(authentication, notification);
            var failedCounterDecorator = new FailedCounterDecorator(notificationDecorator, failedCounter);
            var logFailedCountDecorator = new LogFailedCountDecorator(failedCounterDecorator, failedCounter, nLogAdapter);
            */
            RegisterContainer();

            var authentication = _container.Resolve<IAuthentication>();

            var isValid = authentication.Verify("ac", "pw", "123456");
            Console.WriteLine(isValid);
        }

        private static void RegisterContainer()
        {
             var containerBuilder = new ContainerBuilder();

             containerBuilder.RegisterType<FakeHash>().As<IHash>();
             containerBuilder.RegisterType<FakeProfile>().As<IProfile>();
             containerBuilder.RegisterType<FakeOtp>().As<IOtpService>();

             containerBuilder.RegisterType<AuthenticationService>().As<IAuthentication>();

             _container = containerBuilder.Build();
        }
    }

    internal class ConsoleAdapter : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine(message);
        }
    }

    internal class FakeSlack : INotification
    {
        public void PushMessage(string message)
        {
            Console.WriteLine($"{nameof(FakeSlack)}.{nameof(PushMessage)}({message})");
        }
    }

    internal class FakeFailedCounter : IFailedCounter
    {
        public void ResetFailedCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(ResetFailedCount)}({accountId})");
        }

        public void AddFailedCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(AddFailedCount)}({accountId})");
        }

        public string GetFailedCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(GetFailedCount)}({accountId})");
            return "91";
        }

        public bool IsAccountLocked(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(IsAccountLocked)}({accountId})");
            return false;
        }
    }

    internal class FakeOtp : IOtpService
    {
        public string GetCurrentOtp(string accountId)
        {
            Console.WriteLine($"{nameof(FakeOtp)}.{nameof(GetCurrentOtp)}({accountId})");
            return "123456";
        }
    }

    internal class FakeHash : IHash
    {
        public string GetHash(string plainText)
        {
            Console.WriteLine($"{nameof(FakeHash)}.{nameof(GetHash)}({plainText})");
            return "my hashed password";
        }
    }

    internal class FakeProfile : IProfile
    {
        public string GetPassword(string accountId)
        {
            Console.WriteLine($"{nameof(FakeProfile)}.{nameof(GetPassword)}({accountId})");
            return "my hashed password";
        }
    }

}
