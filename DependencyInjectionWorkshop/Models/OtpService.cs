using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IOtpService
    {
        string GetCurrentOtp(string account);
    }

    public class OtpService : IOtpService
    {
        public string GetCurrentOtp(string account)
        {
            string verifyOtp;

            #region 從service 拿account當下的otp

            var response = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/otps", account).Result;
            if (response.IsSuccessStatusCode)
            {
                verifyOtp = response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api error, accountId:{account}");
            }

            #endregion

            return verifyOtp;
        }
    }
}