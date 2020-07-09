using BitbucketPullRequestValidator.Dto;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BitbucketPullRequestValidator
{
    public class PagedResponseProvider<T>
    {
        private HttpClient _httpClient;

        public PagedResponseProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        protected async Task<IReadOnlyCollection<T>> Get(string url, int pageSize)
        {
            PagedBitbucketResponse<T> pagedResponse = null;
            var results = new List<T>();
            do
            {
                var pagedUrl = BuildQuery(url, pageSize, pagedResponse?.NextPageStart ?? 0);
                var response = await _httpClient.GetAsync(pagedUrl);
                Console.WriteLine($"Requesting: {pagedUrl}");
                if (response.IsSuccessStatusCode)
                {
                    pagedResponse = JsonConvert.DeserializeObject<PagedBitbucketResponse<T>>(await response.Content.ReadAsStringAsync());
                    results.AddRange(FilterResults(pagedResponse));
                }
                else
                {
                    Console.WriteLine($"*** FAILED getting url {pagedUrl}");
                    break;
                }
            } while (!pagedResponse.IsLastPage && ShouldContinue(pagedResponse));

            return results;
        }

        protected virtual IReadOnlyCollection<T> FilterResults(PagedBitbucketResponse<T> pagedResponse)
        {
            return pagedResponse.Values;
        }

        protected virtual bool ShouldContinue(PagedBitbucketResponse<T> lastResponse)
        {
            return true;
        }

        private string BuildQuery(string url, int pageSize, int start)
        {
            var fullUri = new Uri(_httpClient.BaseAddress, url).ToString();
            fullUri = QueryHelpers.AddQueryString(fullUri, "limit", pageSize.ToString());
            fullUri = QueryHelpers.AddQueryString(fullUri, "start", start.ToString());
            return fullUri;
        }
    }
}