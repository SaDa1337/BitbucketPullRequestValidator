using BitbucketPullRequestValidator.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BitbucketPullRequestValidator
{
    public class BitbucketCommitProvider : PagedResponseProvider<BitbucketCommit>
    {
        private readonly string _repoSlug;
        private readonly string _projectKey;
        private readonly string _query;
        private readonly long _fromDate;

        public BitbucketCommitProvider(HttpClient httpClient, string repoSlug, string projectKey,
            string query, long fromDate) : base(httpClient)
        {
            _repoSlug = repoSlug;
            _projectKey = projectKey;
            _query = query;
            _fromDate = fromDate;
        }

        public Task<IReadOnlyCollection<BitbucketCommit>> Get()
        {
            return Get($"/rest/api/1.0/projects/{_projectKey}/repos/{_repoSlug}/commits{_query}", 1000);
        }

        protected override bool ShouldContinue(PagedBitbucketResponse<BitbucketCommit> lastResponse)
        {
            return !lastResponse.Values.Any(x => x.AuthorTimestamp < _fromDate);
        }

        protected override IReadOnlyCollection<BitbucketCommit> FilterResults(PagedBitbucketResponse<BitbucketCommit> pagedResponse)
        {
            return pagedResponse.Values.Where(x => x.AuthorTimestamp > _fromDate).ToList();
        }
    }
}