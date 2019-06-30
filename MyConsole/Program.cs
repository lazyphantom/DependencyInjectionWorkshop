using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var authentication = new Authentication(new ProfileDao(), new Sha256Adapter(), new OtpService());

            var notificationDecorator = new NotificationDecorator(authentication, new Notification());
            var failedCounterDecorator = new FailedCounterDecorator(notificationDecorator, new FailedCounter());
            var logFailedCountDecorator = new LogFailedCountDecorator(failedCounterDecorator, new FailedCounter(), new NLogAdapter());

            var isValid = logFailedCountDecorator.Verify("shih_jie", "", "");
            Console.WriteLine(isValid);
        }
    }
}
