namespace BitbucketPullRequestValidator.Dto
{
    public class BitbucketCommit
    {
        public string Id { get; set; }
        public long AuthorTimestamp { get; set; }
        public BitbucketUser Committer { get; set; }
        public string Message { get; set; }
    }
}
