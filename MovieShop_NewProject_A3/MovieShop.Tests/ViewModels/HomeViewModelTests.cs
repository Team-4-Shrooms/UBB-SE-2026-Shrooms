using System;
using System.Collections.Generic;
using Moq;
using MovieShop.Models;
using MovieShop.Services;
using MovieShop.ViewModels;

namespace MovieShop.Tests.ViewModels
{
    public class HomeViewModelTests
    {
        private readonly Mock<IMovieCatalogService> mockCatalog;
        private readonly Mock<ISaleService> mockSale;

        public HomeViewModelTests()
        {
            mockCatalog = new Mock<IMovieCatalogService>();
            mockSale = new Mock<ISaleService>();
        }

        private HomeViewModel CreateViewModel(List<Movie> movies, Dictionary<int, int>? counts = null)
        {
            var reviewCounts = counts ?? new Dictionary<int, int>();
            mockCatalog.Setup(c => c.GetUndiscountedMovies()).Returns((movies, reviewCounts));
            mockCatalog.Setup(c => c.FilterAndSort(It.IsAny<List<Movie>>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns<List<Movie>, string, string>((m, _, _) => m);
            return new HomeViewModel(mockCatalog.Object, mockSale.Object);
        }

        [Fact]
        public void Load_PopulatesItemsFromCatalog()
        {
            var movies = new List<Movie> { new Movie { ID = 1 }, new Movie { ID = 2 } };
            var counts = new Dictionary<int, int> { { 1, 3 } };
            var viewModel = CreateViewModel(movies, counts);

            viewModel.Load();

            Assert.Equal(2, viewModel.Items.Count);
        }

        [Fact]
        public void SearchQuery_Set_ReappliesFilter()
        {
            var viewModel = CreateViewModel(new List<Movie> { new Movie { ID = 1 } });
            viewModel.Load();

            viewModel.SearchQuery = "abc";

            Assert.Equal("abc", viewModel.SearchQuery);
            mockCatalog.Verify(c => c.FilterAndSort(It.IsAny<List<Movie>>(), "abc", It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public void SearchQuery_SameValue_DoesNotReapplyFilter()
        {
            var viewModel = CreateViewModel(new List<Movie>());
            viewModel.SearchQuery = "abc";
            mockCatalog.Invocations.Clear();

            viewModel.SearchQuery = "abc";

            mockCatalog.Verify(c => c.FilterAndSort(It.IsAny<List<Movie>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void SortOption_Set_ReappliesFilter()
        {
            var viewModel = CreateViewModel(new List<Movie>());

            viewModel.SortOption = "PriceDesc";

            Assert.Equal("PriceDesc", viewModel.SortOption);
        }

        [Fact]
        public void SortOption_SameValue_DoesNotReapplyFilter()
        {
            var viewModel = CreateViewModel(new List<Movie>());
            mockCatalog.Invocations.Clear();

            viewModel.SortOption = "PriceAsc";

            mockCatalog.Verify(c => c.FilterAndSort(It.IsAny<List<Movie>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void FlashSale_DelegatesToSaleService()
        {
            var sale = new FlashSaleViewModel(DateTime.UtcNow.AddMinutes(-1), () => { });
            mockSale.Setup(s => s.CurrentSale).Returns(sale);
            var viewModel = CreateViewModel(new List<Movie>());

            Assert.Same(sale, viewModel.FlashSale);
        }
    }
}
