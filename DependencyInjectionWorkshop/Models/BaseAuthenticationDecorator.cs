﻿namespace DependencyInjectionWorkshop.Models
{
    public abstract class BaseAuthenticationDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;

        protected BaseAuthenticationDecorator(IAuthentication authentication)
        {
            _authentication = authentication;
        }

        public virtual bool Verify(string account, string password, string otp)
        {
            return _authentication.Verify(account, password, otp);
        }
    }
}