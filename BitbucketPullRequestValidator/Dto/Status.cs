using System.Collections.Generic;

namespace BitbucketPullRequestValidator.Dto
{
    public class Status
    {
        public IReadOnlyCollection<StatusEntry> FailedRepos { get; set; }
    }
}
