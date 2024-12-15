using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace SocialMediaFetcherWPF
{
    public class SocialMediaFetcher
    {
        private static readonly HttpClient client = new HttpClient();

        // Fetch Facebook Posts
        //hello peter
        public async Task<List<Post>> FetchFacebookPostsAsync(string facebookPageId, string accessToken)
        {
            try
            {
                string url = $"https://graph.facebook.com/v17.0/{facebookPageId}/posts?access_token={accessToken}";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Failed to fetch posts. Status code: {response.StatusCode}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return new List<Post>();
                }

                var jsonData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(jsonData))
                {
                    MessageBox.Show("No data received from the Facebook API.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return new List<Post>();
                }

                return ParseFacebookPosts(jsonData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching Facebook posts: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Post>();
            }
        }

        private List<Post> ParseFacebookPosts(string jsonData)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<FacebookResponse>(jsonData, options);

                if (data?.Data == null)
                {
                    MessageBox.Show("Error: No posts data in the response.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return new List<Post>();
                }

                return data.Data.Select(item => new Post
                {
                    Id = item.Id,
                    Content = item.Message ?? "No message available",
                    CreatedTime = DateTime.TryParse(item.CreatedTime, out DateTime createdTime) ? createdTime : DateTime.MinValue
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error parsing Facebook posts: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Post>();
            }
        }

        // Fetch Instagram Posts
        public async Task<List<Post>> FetchInstagramPostsAsync(string userId, string accessToken)
        {
            try
            {
                string url = $"https://graph.facebook.com/v12.0/{userId}/media?fields=id,media_type,media_url,thumbnail_url,caption&access_token={accessToken}";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Failed to fetch Instagram posts. Status code: {response.StatusCode}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return new List<Post>();
                }

                var jsonData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(jsonData))
                {
                    MessageBox.Show("No data received from Instagram API.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return new List<Post>();
                }

                return ParseInstagramPosts(jsonData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching Instagram posts: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Post>();
            }
        }

        private List<Post> ParseInstagramPosts(string jsonData)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<InstagramResponse>(jsonData, options);

                if (data?.Data == null)
                {
                    MessageBox.Show("Error: No posts data in the response.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return new List<Post>();
                }

                return data.Data.Select(item => new Post
                {
                    Id = item.Id,
                    Content = item.Caption ?? "No caption available",
                    CreatedTime = DateTime.TryParse(item.Timestamp, out DateTime createdTime) ? createdTime : DateTime.MinValue,
                    MediaUrl = item.MediaUrl
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error parsing Instagram posts: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Post>();
            }
        }

        public class FacebookResponse
        {
            public List<FacebookPost> Data { get; set; }
        }

        public class FacebookPost
        {
            public string Id { get; set; }
            public string Message { get; set; }
            public string CreatedTime { get; set; }
        }

        public class Post
        {
            public string Id { get; set; }
            public string Content { get; set; }
            public DateTime CreatedTime { get; set; }


            [JsonPropertyName("media_url")]
            public string MediaUrl { get; set; }  // Optional, if you need it for Instagram posts
        }

        public class InstagramResponse
        {
            public List<InstagramPost> Data { get; set; }
        }

        public class InstagramPost
        {
            public string Id { get; set; }
            public string Caption { get; set; }
            public string Timestamp { get; set; }
            public string MediaUrl { get; set; }
        }
    }
}