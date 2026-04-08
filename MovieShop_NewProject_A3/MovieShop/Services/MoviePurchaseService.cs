using System.Collections.Generic;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;

public class MoviePurchaseService : IMoviePurchaseService
{
    private readonly IMovieRepository movieRepo;
    private readonly IActiveSalesRepository activeSalesRepo;

    public MoviePurchaseService(
        IMovieRepository movieRepo,
        IActiveSalesRepository activeSalesRepo)
    {
        this.movieRepo = movieRepo;
        this.activeSalesRepo = activeSalesRepo;
    }

    public BuyButtonProps GetBuyButtonProps(
        Movie movie,
        int userId,
        bool isLoggedIn,
        decimal balance)
    {
        var owned = movieRepo.UserOwnsMovie(userId, movie.ID);

        if (owned)
        {
            return new BuyButtonProps("Owned", false, null, 1.0);
        }

        if (!isLoggedIn)
        {
            return new BuyButtonProps(
                "Buy movie",
                false,
                "You must be logged in to make a purchase.",
                0.55);
        }

        if (balance < movie.GetEffectivePrice())
        {
            return new BuyButtonProps(
                "Buy movie",
                false,
                "Your balance is too low to purchase this movie.",
                0.55);
        }

        return new BuyButtonProps("Buy movie", true, null, 1.0);
    }

    public void PurchaseMovie(int userId, Movie movie)
    {
        movieRepo.PurchaseMovie(userId, movie.ID, movie.GetEffectivePrice());
    }
}