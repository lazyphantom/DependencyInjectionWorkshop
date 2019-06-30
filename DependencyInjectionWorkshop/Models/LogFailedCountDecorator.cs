﻿namespace DependencyInjectionWorkshop.Models
{
    public class LogFailedCountDecorator:BaseAuthenticationDecorator
    {
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;

        private void LogFailedCount(string account)
        {
            var failedCount = _failedCounter.GetFailedCount(account);
            _logger.Info($"accountId:{account} failed times:{failedCount}");
        }

        public LogFailedCountDecorator(IAuthentication authentication, IFailedCounter failedCounter, ILogger logger) : base(authentication)
        {
            _failedCounter = failedCounter;
            _logger = logger;
        }

        public override bool Verify(string account, string password, string otp)
        {
            var isValid = base.Verify(account, password, otp);
            if (!isValid)
            {
                LogFailedCount(account);
            }
            return isValid;
        }
    }
}