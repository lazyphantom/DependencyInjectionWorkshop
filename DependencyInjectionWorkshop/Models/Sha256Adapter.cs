using System.Text;

namespace DependencyInjectionWorkshop.Models
{
    public class Sha256Adapter
    {
        public string GetHash(string password)
        {
            #region 把使用者輸入的password做hash

            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var verifyPasswordFromHash = hash.ToString();

            #endregion

            return verifyPasswordFromHash;
        }
    }
}