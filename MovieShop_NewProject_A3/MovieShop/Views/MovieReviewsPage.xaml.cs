using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MovieShop.Models;
using MovieShop.ViewModels;

namespace MovieShop.Views;

public sealed partial class MovieReviewsPage : Page
{
    public MovieReviewsViewModel ViewModel { get; } = App.Services.GetRequiredService<MovieReviewsViewModel>();

    public MovieReviewsPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.Initialize((e.Parameter as MovieReviewsNavArgs)?.Movie);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame?.CanGoBack == true)
        {
            Frame.GoBack();
        }
    }

    private async void AddReviewButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.CanAddReview)
        {
            return;
        }

        while (true)
        {
            var ratingBox = new TextBox
            {
                PlaceholderText = "1 - 10",
                Width = UIConstants.ReviewDialogInputWidth,
            };
            var commentBox = new TextBox
            {
                PlaceholderText = "Comment (optional)",
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Height = UIConstants.ReviewDialogInputHeight,
            };

            var content = new StackPanel { Spacing = UIConstants.StackPanelDefaultSpacing };
            content.Children.Add(new TextBlock
            {
                Text = "Rating",
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
            });
            content.Children.Add(ratingBox);
            content.Children.Add(new TextBlock
            {
                Text = "Comment",
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
                Margin = new Thickness(0, UIConstants.ReviewDialogLabelTopMargin, 0, 0),
            });
            content.Children.Add(commentBox);

            var dialog = new ContentDialog
            {
                Title = "Add review",
                Content = content,
                PrimaryButtonText = "Submit",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot,
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            if (ViewModel.TryAddReview(ratingBox.Text, commentBox.Text, out var error))
            {
                return;
            }

            var errorDialog = new ContentDialog
            {
                Title = "Invalid rating",
                Content = error,
                PrimaryButtonText = "OK",
                XamlRoot = XamlRoot,
            };
            _ = await errorDialog.ShowAsync();
        }
    }
}
