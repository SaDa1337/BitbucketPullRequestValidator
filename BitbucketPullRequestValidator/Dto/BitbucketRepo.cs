namespace BitbucketPullRequestValidator.Dto
{
    public class BitbucketRepo
    {
        public int Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public BitbucketProject Project { get; set; }
    }
}