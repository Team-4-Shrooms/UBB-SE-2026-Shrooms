using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using MovieShop.Models;
using MovieShop.Services;

namespace MovieShop.ViewModels
{
    public class MovieReviewsViewModel : INotifyPropertyChanged
    {
        private const int MinRating = 1;
        private const int MaxRating = 10;

        private readonly IMovieReviewService reviewService;
        private Movie? movie;
        private string title = "Reviews";
        private bool canAddReview;

        public MovieReviewsViewModel(IMovieReviewService reviewService)
        {
            this.reviewService = reviewService;
            Reviews = new ObservableCollection<MovieReview>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<MovieReview> Reviews { get; }

        public string Title
        {
            get => title;
            private set
            {
                title = value;
                OnPropertyChanged();
            }
        }

        public bool CanAddReview
        {
            get => canAddReview;
            private set
            {
                canAddReview = value;
                OnPropertyChanged();
            }
        }

        public void Initialize(Movie? selectedMovie)
        {
            movie = selectedMovie;
            Title = movie == null ? "Reviews" : $"Reviews - {movie.Title}";
            CanAddReview = SessionManager.IsLoggedIn && movie != null;
            LoadReviews();
        }

        public bool TryAddReview(string? ratingText, string? comment, out string error)
        {
            error = string.Empty;

            if (movie == null)
            {
                error = "No movie selected.";
                return false;
            }

            if (!TryParseRating(ratingText, out var rating, out error))
            {
                return false;
            }

            reviewService.AddReview(movie.ID, SessionManager.CurrentUserID, rating, comment);
            LoadReviews();
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void LoadReviews()
        {
            Reviews.Clear();
            if (movie == null)
            {
                return;
            }

            foreach (var review in reviewService.GetReviewsForMovie(movie.ID))
            {
                Reviews.Add(review);
            }
        }

        private static bool TryParseRating(string? text, out int rating, out string error)
        {
            rating = 0;
            error = string.Empty;

            var trimmedText = (text ?? string.Empty).Trim();
            if (!int.TryParse(trimmedText, NumberStyles.Integer, CultureInfo.InvariantCulture, out rating)
                && !int.TryParse(trimmedText, NumberStyles.Integer, CultureInfo.CurrentCulture, out rating))
            {
                error = $"Please enter a rating between {MinRating} and {MaxRating}.";
                return false;
            }

            if (rating < MinRating || rating > MaxRating)
            {
                error = $"Rating must be between {MinRating} and {MaxRating}.";
                return false;
            }

            return true;
        }
    }
}
