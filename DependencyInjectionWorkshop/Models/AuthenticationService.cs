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
            
            #region 登入前檢查是否被鎖
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            if (isLockedResponse.Content.ReadAsAsync<bool>().Result)
            {
                throw new FailedTooManyTimesException();
            }
            #endregion

            string verifyPasswordFromDb;
            string verifyPasswordFromHash;
            string verifyOtp;

            #region 從DB內取hash過後的password
            using (var connection = new SqlConnection("my connection string"))
            {
                verifyPasswordFromDb = connection.Query<string>("spGetUserPassword", new { Id = account },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
            #endregion


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

            #region 驗證成功
            if (otp.Equals(verifyOtp) && verifyPasswordFromDb.Equals(verifyPasswordFromHash))
            {
                #region 登入成功後 重製失敗次數
                //成功
                var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", account).Result;
                resetResponse.EnsureSuccessStatusCode();
                #endregion

                return true;
            }
            #endregion

            #region 登入失敗發送slack通知
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(slackResponse => { }, "my channel", "my message", "my bot name");
            #endregion

            #region 登入失敗新增失敗紀錄
            //失敗
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", account).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
            #endregion

            #region 取得登入失敗次數並且寫log
            var failedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;
            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{account} failed times:{failedCount}");
            #endregion

            return false;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}