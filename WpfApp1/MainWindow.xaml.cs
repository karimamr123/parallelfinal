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

            // Create and show the new window for logs
            FetchPostsLogWindow logWindow = new FetchPostsLogWindow();
            logWindow.Show();

            // Log the start of the process
            logWindow.AppendLog("Fetching social media posts in parallel...\n");

            // Create and start threads
            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                Thread thread = new Thread(() =>
                {
                    // Log that the thread has started
                    logWindow.AppendLog($"Thread {threadIndex + 1} started.\n");

                    // Create and show a new window for each thread
                    FetchPostsWindow fetchPostsWindow = new FetchPostsWindow(facebookPageId, instagramUserId, threadIndex + 1, logWindow);
                    fetchPostsWindow.Show();

                    // Run the dispatcher loop to allow UI updates in the new window
                    System.Windows.Threading.Dispatcher.Run();

                    // Log that the thread has finished
                    logWindow.AppendLog($"Thread {threadIndex + 1} finished.\n");
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }

            // Re-enable the button after starting threads
            FetchPostsButton.IsEnabled = true;
        }
    }

    // New Window to display thread logs
    public class FetchPostsLogWindow : Window
    {
        private TextBox logTextBox;

        public FetchPostsLogWindow()
        {
            Title = "Fetch Posts Log";
            Width = 600;
            Height = 400;

            // Create a TextBox to display the log
            logTextBox = new TextBox
            {
                Margin = new Thickness(10),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                IsReadOnly = true,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            // Set the TextBox as the content of the window
            Content = logTextBox;
        }

        // Method to append log messages to the TextBox
        public void AppendLog(string message)
        {
            Dispatcher.Invoke(() =>
            {
                logTextBox.AppendText(message);
            });
        }
    }

    public class FetchPostsWindow : Window
    {
        private SocialMediaFetcher fetcher;
        private TextBox facebookPostsTextBox;
        private TextBox instagramPostsTextBox;
        private string facebookPageId;
        private string instagramUserId;
        private const string accessToken = "EAASDR6ICHTcBOzoMFbyheIgF0YbEHz7GM4TdZCCARskW0wnzkfdQvsPpYWwKovuqZBYRWWcrsEDD7DHWTeE6CILHM0BFnMcNabSbglXmPyM4BeppefJdm622oL24h64ZALN8oQ4lkdOfzXnTPIgr5ZA4jRYURFtVTYlr3gZBWBpSkTufwqwZAOn0ov2NWwPy1Q";
        private int threadIndex;
        private FetchPostsLogWindow logWindow; // Reference to the log window

        private StackPanel stackPanel; // Define StackPanel

        public FetchPostsWindow(string facebookPageId, string instagramUserId, int threadIndex, FetchPostsLogWindow logWindow)
        {
            this.facebookPageId = facebookPageId;
            this.instagramUserId = instagramUserId;
            this.threadIndex = threadIndex;
            this.logWindow = logWindow;

            fetcher = new SocialMediaFetcher();

            Title = $"Fetch Posts - Thread {threadIndex}";
            Width = 400;
            Height = 600;

            stackPanel = new StackPanel();  // Initialize StackPanel

            // Create and configure TextBoxes for Facebook and Instagram posts
            facebookPostsTextBox = new TextBox { Margin = new Thickness(10), Height = 200, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, IsReadOnly = true };
            instagramPostsTextBox = new TextBox { Margin = new Thickness(10), Height = 200, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, IsReadOnly = true };

            stackPanel.Children.Add(new TextBlock { Text = $"Facebook Posts (Thread {threadIndex})", Margin = new Thickness(10) });
            stackPanel.Children.Add(facebookPostsTextBox);
            stackPanel.Children.Add(new TextBlock { Text = $"Instagram Posts (Thread {threadIndex})", Margin = new Thickness(10) });
            stackPanel.Children.Add(instagramPostsTextBox);

            Content = stackPanel; // Set StackPanel as the window's content

            // Hook up the Loaded event
            Loaded += FetchPostsWindow_Loaded;
        }

        private async void FetchPostsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Fetch posts asynchronously using the fetcher
            var facebookPosts = await fetcher.FetchFacebookPostsAsync(facebookPageId, accessToken);
            var instagramPosts = await fetcher.FetchInstagramPostsAsync(instagramUserId, accessToken);

            // Display the fetched posts in the TextBoxes
            DisplayPosts(facebookPosts, facebookPostsTextBox);
            DisplayPosts(instagramPosts, instagramPostsTextBox);

            // Log that the thread has finished processing
            logWindow.AppendLog($"Thread {threadIndex} finished fetching posts.\n");
        }

        private void DisplayPosts(List<Post> posts, TextBox textBox)
        {
            // Clear existing content and display fetched posts
            textBox.Clear();
            if (posts.Count == 0)
            {
                textBox.AppendText("No posts found.\n");
                return;
            }

            // Append posts to the TextBox
            foreach (var post in posts)
            {
                // Display post creation time and content
                textBox.AppendText($"[{post.CreatedTime}] {post.Content}\n");

                if (!string.IsNullOrEmpty(post.MediaUrl))
                {
                    // Check if the media is an image (you can enhance this by checking the media type)
                    if (post.MediaUrl.EndsWith(".jpg") || post.MediaUrl.EndsWith(".jpeg") || post.MediaUrl.EndsWith(".png"))
                    {
                        try
                        {
                            // Create and display the image
                            var image = new Image
                            {
                                Source = new BitmapImage(new Uri(post.MediaUrl)),
                                Width = 200,  // Adjust the size as needed
                                Height = 200
                            };

                            stackPanel.Children.Add(image); // Add the image to the StackPanel
                        }
                        catch (Exception ex)
                        {
                            textBox.AppendText($"Error loading image: {ex.Message}\n");
                        }
                    }
                    else
                    {
                        // Display the media URL as a clickable link
                        textBox.AppendText($"Media URL: {post.MediaUrl}\n");
                    }
                }
                textBox.AppendText("\n");
            }
        }
    }

}
