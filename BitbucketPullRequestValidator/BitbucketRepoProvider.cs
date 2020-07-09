using BitbucketPullRequestValidator.Dto;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BitbucketPullRequestValidator
{
    public class BitbucketRepoProvider : PagedResponseProvider<BitbucketRepo>
    {
        private readonly IReadOnlyCollection<string> _repoSlugBlacklist = ConfigurationManager.AppSettings["RepoSlugBlacklist"].Split(";");
        public BitbucketRepoProvider(HttpClient httpClient) : base(httpClient)
        {
        }

        public Task<IReadOnlyCollection<BitbucketRepo>> Get()
        {
            return Get("/rest/api/latest/repos", 100);
        }

        protected override IReadOnlyCollection<BitbucketRepo> FilterResults(PagedBitbucketResponse<BitbucketRepo> pagedResponse)
        {
            return pagedResponse.Values.Where(x => !_repoSlugBlacklist.Contains(x.Slug)).ToList();
        }
    }
}