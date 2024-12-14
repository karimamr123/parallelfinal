using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SocialMediaFetcherWPF;
using static SocialMediaFetcherWPF.SocialMediaFetcher;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void FetchPostsButton_Click(object sender, RoutedEventArgs e)
        {
            string facebookPageId = facebookPageIdTextBox.Text;
            string instagramUserId = instagramUserIdTextBox.Text;

            int threadCount = int.TryParse(threadCountTextBox.Text, out var count) ? count : 1;

            // Disable the button to avoid multiple clicks
            FetchPostsButton.IsEnabled = false;

            // Create and start threads
            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                Thread thread = new Thread(() =>
                {
                    FetchPostsWindow fetchPostsWindow = new FetchPostsWindow(facebookPageId, instagramUserId, threadIndex + 1);
                    fetchPostsWindow.Show();
                    System.Windows.Threading.Dispatcher.Run();
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }

            // Re-enable the button after starting threads
            FetchPostsButton.IsEnabled = true;
        }
    }

    public class FetchPostsWindow : Window
    {
        private SocialMediaFetcher fetcher;
        private TextBox facebookPostsTextBox;
        private StackPanel instagramPostsPanel;
        private string facebookPageId;
        private string instagramUserId;
        private const string accessToken = "EAASDR6ICHTcBOzoMFbyheIgF0YbEHz7GM4TdZCCARskW0wnzkfdQvsPpYWwKovuqZBYRWWcrsEDD7DHWTeE6CILHM0BFnMcNabSbglXmPyM4BeppefJdm622oL24h64ZALN8oQ4lkdOfzXnTPIgr5ZA4jRYURFtVTYlr3gZBWBpSkTufwqwZAOn0ov2NWwPy1Q";
        private int threadIndex;

        public FetchPostsWindow(string facebookPageId, string instagramUserId, int threadIndex)
        {
            this.facebookPageId = facebookPageId;
            this.instagramUserId = instagramUserId;
            this.threadIndex = threadIndex;

            fetcher = new SocialMediaFetcher();

            Title = $"Fetch Posts - Thread {threadIndex}";
            Width = 400;
            Height = 600;

            StackPanel stackPanel = new StackPanel();

            facebookPostsTextBox = new TextBox { Margin = new Thickness(10), Height = 200, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            instagramPostsPanel = new StackPanel { Margin = new Thickness(10) };

            stackPanel.Children.Add(new TextBlock { Text = $"Facebook Posts (Thread {threadIndex})", Margin = new Thickness(10) });
            stackPanel.Children.Add(facebookPostsTextBox);
            stackPanel.Children.Add(new TextBlock { Text = $"Instagram Posts (Thread {threadIndex})", Margin = new Thickness(10) });
            stackPanel.Children.Add(instagramPostsPanel);

            Content = stackPanel;

            Loaded += FetchPostsWindow_Loaded;
        }

        private async void FetchPostsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Fetch posts asynchronously
            var facebookPosts = await fetcher.FetchFacebookPostsAsync(facebookPageId, accessToken);
            var instagramPosts = await fetcher.FetchInstagramPostsAsync(instagramUserId, accessToken);

            // Display the results
            DisplayPosts(facebookPosts, facebookPostsTextBox);
            DisplayInstagramPosts(instagramPosts, instagramPostsPanel);
        }

        private void DisplayPosts(List<Post> posts, TextBox textBox)
        {
            textBox.Clear();
            if (posts.Count == 0)
            {
                textBox.AppendText("No posts found.\n");
                return;
            }

            foreach (var post in posts)
            {
                textBox.AppendText($"[{post.CreatedTime}] {post.Content}\n");
            }
        }

        private void DisplayInstagramPosts(List<Post> posts, StackPanel panel)
        {
            panel.Children.Clear();
            if (posts.Count == 0)
            {
                panel.Children.Add(new TextBlock { Text = "No posts found.", Margin = new Thickness(10) });
                return;
            }

            foreach (var post in posts)
            {
                if (!string.IsNullOrEmpty(post.MediaUrl))
                {
                    Image image = new Image
                    {
                        Source = new BitmapImage(new Uri(post.MediaUrl)),
                        Width = 200,
                        Height = 200,
                        Margin = new Thickness(10)
                    };
                    panel.Children.Add(image);
                }
            }
        }
    }
}