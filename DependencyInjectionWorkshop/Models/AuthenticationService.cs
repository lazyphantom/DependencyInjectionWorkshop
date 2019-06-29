using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string account, string password, string otp)
        {

            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            
            var isLocked = IsAccountLocked(account, httpClient);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }

            var verifyPasswordFromDb = GetCurrentPasswordFromDb(account);

            var verifyPasswordFromHash = GetCurrentHashPassword(password);

            var verifyOtp = GetCurrentOtp(account, httpClient);

            #region 驗證成功
            if (otp.Equals(verifyOtp) && verifyPasswordFromDb.Equals(verifyPasswordFromHash))
            {
                ResetFailedCount(account, httpClient);

                return true;
            }
            #endregion

            PushMessage();

            AddFailedCount(account, httpClient);

            LogFailed(account, httpClient);

            return false;
        }

        private static void LogFailed(string account, HttpClient httpClient)
        {
            #region 取得登入失敗次數並且寫log
            var failedCount = GetFailedCount(account, httpClient);
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{account} failed times:{failedCount}");

            #endregion
        }

        private static int GetFailedCount(string account, HttpClient httpClient)
        {
            var failedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;
            failedCountResponse.EnsureSuccessStatusCode();
            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        private static void AddFailedCount(string account, HttpClient httpClient)
        {
            #region 登入失敗新增失敗紀錄
            //失敗
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", account).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
            #endregion
        }

        private static void PushMessage()
        {
            #region 登入失敗發送slack通知
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(slackResponse => { }, "my channel", "my message", "my bot name");
            #endregion
        }

        private static bool IsAccountLocked(string account, HttpClient httpClient)
        {
            #region 登入前檢查是否被鎖

            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;

            #endregion

            return isLocked;
        }

        private static void ResetFailedCount(string account, HttpClient httpClient)
        {
            #region 登入成功後 重製失敗次數

            //成功
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", account).Result;
            resetResponse.EnsureSuccessStatusCode();

            #endregion
        }

        private static string GetCurrentOtp(string account, HttpClient httpClient)
        {
            string verifyOtp;

            #region 從service 拿account當下的otp

            var response = httpClient.PostAsJsonAsync("api/otps", account).Result;
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

        private static string GetCurrentHashPassword(string password)
        {
            string verifyPasswordFromHash;

            #region 把使用者輸入的password做hash

            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            verifyPasswordFromHash = hash.ToString();

            #endregion

            return verifyPasswordFromHash;
        }

        private static string GetCurrentPasswordFromDb(string account)
        {
            string verifyPasswordFromDb;

            #region 從DB內取hash過後的password

            using (var connection = new SqlConnection("my connection string"))
            {
                verifyPasswordFromDb = connection.Query<string>("spGetUserPassword", new {Id = account},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            #endregion

            return verifyPasswordFromDb;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}