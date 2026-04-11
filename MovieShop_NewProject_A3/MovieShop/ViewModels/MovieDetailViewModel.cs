using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MovieShop.Models;
using MovieShop.Services;

namespace MovieShop.ViewModels
{
    public class MovieDetailViewModel : INotifyPropertyChanged
    {
        private const double MaxRating = 10.0;

        private readonly IMoviePurchaseService purchaseService;
        private readonly IMovieReviewService reviewService;
        private readonly IMovieCatalogService catalogService;

        private Movie? movie;
        private MainViewModel? mainViewModel;
        private string buyButtonContent = "Buy movie";
        private bool canBuyMovie;
        private double buyButtonOpacity = UIConstants.EnabledButtonOpacity;
        private string? buyButtonTooltip;
        private string? starDistributionTooltip;

        public MovieDetailViewModel(
            IMoviePurchaseService purchaseService,
            IMovieReviewService reviewService,
            IMovieCatalogService catalogService)
        {
            this.purchaseService = purchaseService;
            this.reviewService = reviewService;
            this.catalogService = catalogService;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Movie? Movie
        {
            get => movie;
            private set
            {
                movie = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(RatingText));
                OnPropertyChanged(nameof(PriceText));
                OnPropertyChanged(nameof(OriginalPriceText));
                OnPropertyChanged(nameof(HasActiveSale));
                OnPropertyChanged(nameof(PosterUrl));
            }
        }

        public MainViewModel? MainViewModel => mainViewModel;

        public string Title => movie?.Title ?? string.Empty;

        public string Description => string.IsNullOrEmpty(movie?.Description) ? "—" : movie!.Description;

        public string RatingText => movie == null ? string.Empty : $"Rating: {movie.Rating:0.0} / {MaxRating}";

        public string PriceText => movie == null ? string.Empty : $"${movie.DiscountedPriceText}";

        public string OriginalPriceText => movie == null ? string.Empty : $"${movie.OriginalPriceText}";

        public bool HasActiveSale => movie?.HasActiveSale == true;

        public string? PosterUrl => movie?.ImageUrl;

        public string BuyButtonContent
        {
            get => buyButtonContent;
            private set
            {
                buyButtonContent = value;
                OnPropertyChanged();
            }
        }

        public bool CanBuyMovie
        {
            get => canBuyMovie;
            private set
            {
                canBuyMovie = value;
                OnPropertyChanged();
            }
        }

        public double BuyButtonOpacity
        {
            get => buyButtonOpacity;
            private set
            {
                buyButtonOpacity = value;
                OnPropertyChanged();
            }
        }

        public string? BuyButtonTooltip
        {
            get => buyButtonTooltip;
            private set
            {
                buyButtonTooltip = value;
                OnPropertyChanged();
            }
        }

        public string? StarDistributionTooltip
        {
            get => starDistributionTooltip;
            private set
            {
                starDistributionTooltip = value;
                OnPropertyChanged();
            }
        }

        public void Initialize(MovieDetailNavArgs? args)
        {
            if (args?.Movie == null)
            {
                Movie = null;
                mainViewModel = null;
                ResetBuyButton();
                StarDistributionTooltip = null;
                return;
            }

            mainViewModel = args.MainViewModel;
            catalogService.ApplyDiscount(args.Movie);
            Movie = args.Movie;

            StarDistributionTooltip = reviewService.BuildStarDistributionTooltip(args.Movie.ID);
            RefreshBuyButtonState();
        }

        public void RefreshBuyButtonState()
        {
            if (movie == null)
            {
                ResetBuyButton();
                return;
            }

            mainViewModel?.RefreshBalanceFromDatabase();
            var balance = mainViewModel?.Balance ?? SessionManager.CurrentUserBalance;
            var props = purchaseService.GetBuyButtonProps(
                movie,
                SessionManager.CurrentUserID,
                SessionManager.IsLoggedIn,
                balance);

            BuyButtonContent = props.Content;
            CanBuyMovie = props.IsEnabled;
            BuyButtonOpacity = props.Opacity;
            BuyButtonTooltip = props.ToolTip;
        }

        public bool TryPurchase(out string error)
        {
            error = string.Empty;

            if (movie == null)
            {
                error = "No movie selected.";
                return false;
            }

            if (!SessionManager.IsLoggedIn)
            {
                error = "You must be logged in to make a purchase.";
                return false;
            }

            RefreshBuyButtonState();
            if (!CanBuyMovie)
            {
                error = BuyButtonTooltip ?? "Cannot purchase this movie.";
                return false;
            }

            try
            {
                purchaseService.PurchaseMovie(SessionManager.CurrentUserID, movie);
                if (mainViewModel != null)
                {
                    mainViewModel.RefreshWallet();
                    SessionManager.CurrentUserBalance = mainViewModel.Balance;
                }

                RefreshBuyButtonState();
                return true;
            }
            catch (InvalidOperationException ex)
            {
                error = ex.Message;
                return false;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void ResetBuyButton()
        {
            BuyButtonContent = "Buy movie";
            CanBuyMovie = false;
            BuyButtonOpacity = UIConstants.DisabledButtonOpacity;
            BuyButtonTooltip = null;
        }
    }
}
