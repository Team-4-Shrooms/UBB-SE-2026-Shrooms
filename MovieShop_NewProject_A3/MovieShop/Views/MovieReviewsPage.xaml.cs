using System;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;
using MovieShop.ViewModels;

namespace MovieShop.Views;

public sealed partial class MovieReviewsPage : Page
{
    private Movie? movie;
    private MainViewModel? mainVm;
    private readonly IMovieReviewService reviewService = App.Services.GetRequiredService<IMovieReviewService>();

    public MovieReviewsPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not MovieReviewsNavArgs args)
        {
            return;
        }

        movie = args.Movie;
        mainVm = args.MainViewModel;
        TitleBlock.Text = movie == null ? "Reviews" : $"Reviews - {movie.Title}";

        AddReviewButton.IsEnabled = SessionManager.IsLoggedIn;
        LoadReviews();
    }

    private void BackButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (Frame?.CanGoBack == true)
        {
            Frame.GoBack();
        }
    }

    private void LoadReviews()
    {
        if (movie == null)
        {
            return;
        }

        ReviewsList.ItemsSource = reviewService.GetReviewsForMovie(movie.ID);
    }

    private async void AddReviewButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (!SessionManager.IsLoggedIn || movie == null)
        {
            return;
        }

        while (true)
        {
            var ratingBox = new TextBox { PlaceholderText = "1.0 - 10.0 (max 1 decimal)", Width = UIConstants.ReviewDialogInputWidth };
            var commentBox = new TextBox { PlaceholderText = "Comment (optional)", AcceptsReturn = true, TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap, Height = UIConstants.ReviewDialogInputHeight };

            var content = new StackPanel { Spacing = UIConstants.StackPanelDefaultSpacing };
            content.Children.Add(new TextBlock { Text = "Rating", Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White) });
            content.Children.Add(ratingBox);
            content.Children.Add(new TextBlock { Text = "Comment", Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White), Margin = new Microsoft.UI.Xaml.Thickness(0, UIConstants.ReviewDialogLabelTopMargin, 0, 0) });
            content.Children.Add(commentBox);

            var dialog = new ContentDialog
            {
                Title = "Add review",
                Content = content,
                PrimaryButtonText = "Submit",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            if (!TryParseRating(ratingBox.Text, out var rating, out var error))
            {
                var err = new ContentDialog
                {
                    Title = "Invalid rating",
                    Content = error,
                    PrimaryButtonText = "OK",
                    XamlRoot = XamlRoot
                };
                _ = await err.ShowAsync();
                continue;
            }

            reviewService.AddReview(
                movie.ID,
                SessionManager.CurrentUserID,
                rating,
                commentBox.Text);
            LoadReviews();
            return;
        }
    }

    private static bool TryParseRating(string? text, out int rating, out string error)
    {
        rating = 0;
        error = string.Empty;

        var trimmedText = (text ?? string.Empty).Trim();
        if (!int.TryParse(trimmedText, NumberStyles.Number, CultureInfo.InvariantCulture, out rating))
        {
            if (!int.TryParse(trimmedText, NumberStyles.Number, CultureInfo.CurrentCulture, out rating))
            {
                error = "Please enter a rating between 1 and 10.";
                return false;
            }
        }

        if (rating < 1 || rating > 10)
        {
            error = "Rating must be between 1 and 10.";
            return false;
        }

        return true;
    }
}

