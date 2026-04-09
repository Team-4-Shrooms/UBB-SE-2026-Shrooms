using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MovieShop.Models;

namespace MovieShop.Services
{
    #pragma warning disable SA1313 // Parameter names should begin with lower-case letter
    public record BuyButtonProps(string Content, bool IsEnabled, string? ToolTip, double Opacity);
    #pragma warning restore SA1313
    public interface IMoviePurchaseService
    {
        BuyButtonProps GetBuyButtonProps(
            Movie movie,
            int userId,
            bool isLoggedIn,
            decimal balance);

        void PurchaseMovie(int userId, Movie movie);
    }
}