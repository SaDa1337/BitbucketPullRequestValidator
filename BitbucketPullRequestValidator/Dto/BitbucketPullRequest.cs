using System.Collections.Generic;

namespace BitbucketPullRequestValidator.Dto
{
    public class BitbucketPullRequest
    {
        public int Id { get; set; }
        public BitbucketPullRequestUser Author { get; set; }
        public IReadOnlyCollection<BitbucketPullRequestUser> Reviewers { get; set; } = new List<BitbucketPullRequestUser>();
        public long ClosedDate { get; set; }
    }
}
