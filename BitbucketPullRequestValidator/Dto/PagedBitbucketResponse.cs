using System.Collections.Generic;

namespace BitbucketPullRequestValidator.Dto
{
    public class PagedBitbucketResponse<T>
    {
        public int Size { get; set; }
        public int Limit { get; set; }
        public bool IsLastPage { get; set; }
        public int Start { get; set; }
        public int? NextPageStart { get; set; }
        public IReadOnlyCollection<T> Values { get; set; }
    }
}