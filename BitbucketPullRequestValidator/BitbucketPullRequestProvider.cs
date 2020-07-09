using BitbucketPullRequestValidator.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BitbucketPullRequestValidator
{
    public class BitbucketPullRequestProvider : PagedResponseProvider<BitbucketPullRequest>
    {
        private readonly string _repoSlug;
        private readonly string _projectKey;
        private readonly long _fromDate;

        public BitbucketPullRequestProvider(HttpClient httpClient, string repoSlug, string projectKey, long fromDate) : base(httpClient)
        {
            _repoSlug = repoSlug;
            _projectKey = projectKey;
            _fromDate = fromDate;
        }

        public Task<IReadOnlyCollection<BitbucketPullRequest>> Get()
        {
            return Get($"/rest/api/1.0/projects/{_projectKey}/repos/{_repoSlug}/pull-requests?state=MERGED", 500);
        }
        protected override bool ShouldContinue(PagedBitbucketResponse<BitbucketPullRequest> lastResponse)
        {
            return !lastResponse.Values.Any(x => x.ClosedDate < _fromDate);
        }

        protected override IReadOnlyCollection<BitbucketPullRequest> FilterResults(PagedBitbucketResponse<BitbucketPullRequest> pagedResponse)
        {
            return pagedResponse.Values.Where(x => x.ClosedDate > _fromDate).ToList();
        }
    }
}