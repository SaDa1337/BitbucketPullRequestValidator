using System.Configuration;
using System.Text;

namespace BitbucketPullRequestValidator
{
    public class BitbucketAuthenticationProvider
    {
        private string _user;
        private string _password;

        public BitbucketAuthenticationProvider()
        {
            _user = ConfigurationManager.AppSettings["AuthUser"];
            _password = ConfigurationManager.AppSettings["AuthPassword"];
        }

        public string GetAuthValue()
        {
            return System.Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_user}:{_password}"));
        }
    }
}