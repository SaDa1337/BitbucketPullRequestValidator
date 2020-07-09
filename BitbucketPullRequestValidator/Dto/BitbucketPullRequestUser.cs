namespace BitbucketPullRequestValidator.Dto
{
    public class BitbucketPullRequestUser
    {
        public BitbucketUser User { get; set; }
        public string Role { get; set; }
        public bool Approved { get; set; }
    }
}