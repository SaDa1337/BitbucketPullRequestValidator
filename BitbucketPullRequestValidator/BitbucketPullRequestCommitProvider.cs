using BitbucketPullRequestValidator.Dto;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BitbucketPullRequestValidator
{
    public class BitbucketPullRequestCommitProvider : PagedResponseProvider<BitbucketCommit>
    {
        private readonly string _repoSlug;
        private readonly string _projectKey;
        private readonly int _pullRequestId;

        public BitbucketPullRequestCommitProvider(HttpClient httpClient, string repoSlug, string projectKey, int pullRequestId) : base(httpClient)
        {
            _repoSlug = repoSlug;
            _projectKey = projectKey;
            _pullRequestId = pullRequestId;
        }

        public Task<IReadOnlyCollection<BitbucketCommit>> Get()
        {
            return Get($"/rest/api/1.0/projects/{_projectKey}/repos/{_repoSlug}/pull-requests/{_pullRequestId}/commits", 100);
        }
    }
}