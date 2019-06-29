using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string account, string password, string otp)
        {
            string verifyPasswordFromDb;
            string verifyPasswordFromHash;
            string verifyOtp;

            using (var connection = new SqlConnection("my connection string"))
            {
                verifyPasswordFromDb = connection.Query<string>("spGetUserPassword", new { Id = account },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }


            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            verifyPasswordFromHash = hash.ToString();


            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var response = httpClient.PostAsJsonAsync("api/otps", account).Result;
            if (response.IsSuccessStatusCode)
            {
                verifyOtp = response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api error, accountId:{account}");
            }


            return otp.Equals(verifyOtp) && verifyPasswordFromDb.Equals(verifyPasswordFromHash);
        }
    }
}