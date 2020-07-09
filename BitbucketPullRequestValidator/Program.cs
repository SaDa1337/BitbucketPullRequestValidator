using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BitbucketPullRequestValidator
{
    class Program
    {
        private static string BITBUCKET_URL = ConfigurationManager.AppSettings["RepoUrl"];

        static async Task Main(string[] args)
        {
            var fromDate = args.Length > 0
                ? DateTime.ParseExact(args[0], "yyyy-MM-dd", CultureInfo.InvariantCulture)
                : DateTime.Today.AddDays(-1);

            var auth = new BitbucketAuthenticationProvider().GetAuthValue();

            var httpClient = CreateHttpClient(auth);

            var repos = await new BitbucketRepoProvider(httpClient).Get();

            var commitStatus = await new BitbucketCommitStatusChecker(httpClient, ToUnixTime(fromDate)).CheckStatus(repos);
            var sb = new StringBuilder();
            sb.AppendLine($"Pull request check starting from {fromDate.ToShortDateString()}");
            sb.AppendLine();
            if (commitStatus.FailedRepos.Any(x => x.Commits.Count > 0))
            {
                foreach (var userGroup in commitStatus.FailedRepos
                    .Where(x => x.Commits.Count > 0)
                    .SelectMany(x => x.Commits)
                    .GroupBy(x => x.Committer.EmailAddress)
                    .OrderByDescending(x => x.Count()))
                {
                    sb.AppendLine($"Status for: {userGroup.Key}, failed count: {userGroup.Count()}");
                    foreach (var commit in userGroup)
                    {
                        var repo = commitStatus.FailedRepos.Single(x => x.Commits.Any(c => c.Id == commit.Id)).Repo;
                        sb.AppendLine($"{BITBUCKET_URL}/projects/{repo.Project.Key}/repos/{repo.Slug}/commits/{commit.Id}");
                    }
                    sb.AppendLine();
                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine("OK");
            }
            await SaveResults(sb, fromDate);
        }

        private static async Task SaveResults(StringBuilder sb, DateTime fromDate)
        {
            Console.WriteLine("Saving results");
            using (var sw = File.CreateText("results.txt"))
            {
                await sw.WriteAsync(sb.ToString());
                await sw.FlushAsync();
            };
        }

        private static HttpClient CreateHttpClient(string auth)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);
            httpClient.BaseAddress = new Uri(BITBUCKET_URL);
            return httpClient;
        }

        public static long ToUnixTime(DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date.ToUniversalTime() - epoch).TotalMilliseconds);
        }
    }
}
