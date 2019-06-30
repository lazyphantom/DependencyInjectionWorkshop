using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public interface IProfile
    {
        string GetPassword(string account);
    }

    public class ProfileDao : IProfile
    {
        public string GetPassword(string account)
        {
            string verifyPasswordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                verifyPasswordFromDb = connection.Query<string>("spGetUserPassword", new {Id = account},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
            return verifyPasswordFromDb;
        }

    }
}