using System.Collections.Generic;

namespace BitbucketPullRequestValidator.Dto
{
    public class StatusEntry
    {
        public BitbucketRepo Repo { get; set; }
        public IReadOnlyCollection<BitbucketCommit> Commits { get; set; }
    }
}