using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjectionWorkshop.Models
{
    public interface IFailedCounter
    {
        void AddFailedCount(string account);
        bool IsAccountLocked(string account);
        void ResetFailedCount(string account);
        string GetFailedCount(string account);
    }

    public class FailedCounter : IFailedCounter
    {

        public void AddFailedCount(string account)
        {
            #region 登入失敗新增失敗紀錄
            //失敗
            var addFailedCountResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Add", account).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
            #endregion
        }

        public bool IsAccountLocked(string account)
        {
            #region 登入前檢查是否被鎖

            var isLockedResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;

            #endregion

            return isLocked;
        }

        public void ResetFailedCount(string account)
        {
            #region 登入成功後 重製失敗次數

            //成功
            var resetResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Reset", account).Result;
            resetResponse.EnsureSuccessStatusCode();

            #endregion
        }

        public string GetFailedCount(string account)
        {
            var failedCountResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;
            failedCountResponse.EnsureSuccessStatusCode();
            var failedCount = failedCountResponse.Content.ReadAsAsync<string>().Result;
            return failedCount;
        }
    }
}
