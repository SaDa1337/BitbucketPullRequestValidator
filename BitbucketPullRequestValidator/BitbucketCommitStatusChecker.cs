using BitbucketPullRequestValidator.Dto;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BitbucketPullRequestValidator
{
    internal class BitbucketCommitStatusChecker
    {
        private HttpClient _httpClient;
        private readonly long _fromDate;
        private static Regex _squashedCommitRegex = new Regex("^(commit )(.{40})$", RegexOptions.Multiline);

        public BitbucketCommitStatusChecker(HttpClient httpClient, long fromDate)
        {
            _httpClient = httpClient;
            _fromDate = fromDate;
        }

        public async Task<Status> CheckStatus(IReadOnlyCollection<BitbucketRepo> repos)
        {
            var result = new Status();
            var bag = new ConcurrentBag<StatusEntry>();
            await
                Task.WhenAll(
                from partition in Partitioner.Create(repos).GetPartitions(16)
                select Task.Run(async delegate
                {
                    using (partition)
                    {
                        while (partition.MoveNext())
                            bag.Add(await HandleRepo(partition.Current).ConfigureAwait(false));
                    }
                }));

            result.FailedRepos = bag;
            return result;
        }

        public async Task<StatusEntry> HandleRepo(BitbucketRepo repo)
        {
            Console.WriteLine($"Starting: {repo.Name}");
            var commitsPossiblyWithoutPR = await new BitbucketCommitProvider(_httpClient, repo.Slug, repo.Project.Key, "?merges=exclude", _fromDate).Get().ConfigureAwait(false);
            var commitsPossiblyWithoutPRUnSquashed = commitsPossiblyWithoutPR.SelectMany(x => GetUnSquashedCommits(x));

            Console.WriteLine($"Possible fails for repo {repo.Name}: {commitsPossiblyWithoutPRUnSquashed.Count()}");
            if (commitsPossiblyWithoutPR.Count == 0)
            {
                Console.WriteLine($"Finished: {repo.Name}");
                return new StatusEntry { Repo = repo, Commits = new List<BitbucketCommit>() };
            }
            var pullRequests = await new BitbucketPullRequestProvider(_httpClient, repo.Slug, repo.Project.Key, _fromDate).Get();
            var pullRequestCommits = new List<BitbucketCommit>();
            foreach (var pullRequest in pullRequests.Where(x => x.Reviewers.Any(r => r.Approved && r.User.Id != x.Author.User.Id)))
            {
                var currentPullRequestCommits = await new BitbucketPullRequestCommitProvider(_httpClient, repo.Slug, repo.Project.Key, pullRequest.Id).Get();
                pullRequestCommits.AddRange(currentPullRequestCommits);
            }
            var mergeCommits = await new BitbucketCommitProvider(_httpClient, repo.Slug, repo.Project.Key, "?merges=only", _fromDate).Get();
            pullRequestCommits.AddRange(mergeCommits);
            var commitsWithoutPullRequest = new List<BitbucketCommit>();
            foreach (var commit in commitsPossiblyWithoutPRUnSquashed)
            {
                if (!pullRequestCommits.Any(x => x.Id == commit.Id))
                {
                    commitsWithoutPullRequest.Add(commit);
                }
            }
            Console.WriteLine($"Finished: {repo.Name}");
            return new StatusEntry { Commits = commitsWithoutPullRequest, Repo = repo };

        }

        private IEnumerable<BitbucketCommit> GetUnSquashedCommits(BitbucketCommit commit)
        {
            var squashedCommits = _squashedCommitRegex.Matches(commit.Message);
            if (squashedCommits.Count > 0)
            {
                return squashedCommits.Select(x => new BitbucketCommit { Id = x.Groups[2].Value, Committer = commit.Committer });
            }
            return new List<BitbucketCommit> { commit };
        }
    }
}