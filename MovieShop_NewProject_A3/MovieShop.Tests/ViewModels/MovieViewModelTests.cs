using System;
using System.ComponentModel;
using Moq;
using MovieShop.ViewModels;
using Xunit;

namespace MovieShop.Tests.ViewModels
{
    public class MovieViewModelTests
    {
        [Fact]
        public void RevertToOriginalPrice_ActiveSale_RemovesSale()
        {
            // Arrange
            var viewModel = new MovieViewModel();
            viewModel.ApplyDatabaseSale(50, DateTime.Now.AddDays(1));

            // Act
            viewModel.RevertToOriginalPrice();

            // Assert
            Assert.Equal(0, viewModel.SaleID);
            Assert.Null(viewModel.SaleTimer);
        }

        [Fact]
        public void ApplyDatabaseSale_ValidSale_AppliesDiscount()
        {
            // Arrange
            var viewModel = new MovieViewModel();

            // Act
            viewModel.ApplyDatabaseSale(50, DateTime.Now.AddDays(1));

            // Assert
            Assert.Equal(50, viewModel.DiscountPercent);
            Assert.Equal(1, viewModel.SaleID);
            Assert.NotNull(viewModel.SaleTimer);
        }

        [Fact]
        public void DisplayPrice_WithActiveSale_ReturnsSalePrice()
        {
            // Arrange
            var viewModel = new MovieViewModel { BasePrice = 100 };
            viewModel.ApplyDatabaseSale(25, DateTime.Now.AddDays(1));

            // Act
            var displayPrice = viewModel.DisplayPrice;

            // Assert
            Assert.Equal(75, displayPrice);
        }

        [Fact]
        public void DisplayPrice_NoActiveSale_ReturnsBasePrice()
        {
            // Arrange
            var viewModel = new MovieViewModel { BasePrice = 100 };

            // Act
            var displayPrice = viewModel.DisplayPrice;

            // Assert
            Assert.Equal(100, displayPrice);
        }

        [Fact]
        public void SalePrice_CalculatesCorrectly()
        {
            // Arrange
            var viewModel = new MovieViewModel { BasePrice = 200, DiscountPercent = 10 };

            // Act
            var salePrice = viewModel.SalePrice;

            // Assert
            Assert.Equal(180, salePrice);
        }

        [Fact]
        public void IsSaleActive_NoSale_ReturnsFalse()
        {
            // Arrange
            var viewModel = new MovieViewModel { SaleID = 0 };

            // Act/Assert
            Assert.False(viewModel.IsSaleActive);
            Assert.False(viewModel.ShowStrike);
            Assert.Equal(string.Empty, viewModel.SaleBadgeText);
        }

        [Fact]
        public void SaleBadgeText_WithActiveSale_ReturnsFormattedString()
        {
            // Arrange
            var viewModel = new MovieViewModel { SaleID = 1, DiscountPercent = 25 };

            // Act/Assert
            Assert.True(viewModel.IsSaleActive);
            Assert.True(viewModel.ShowStrike);
            Assert.Equal("25% OFF", viewModel.SaleBadgeText);
        }
    }
}
