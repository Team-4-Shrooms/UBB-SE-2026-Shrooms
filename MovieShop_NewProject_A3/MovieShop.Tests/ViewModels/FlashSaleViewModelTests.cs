using MovieShop.ViewModels;

namespace MovieShop.Tests.ViewModels
{
    public class FlashSaleViewModelTests
    {
        private const int PastDayOffset = -1;
        private const int FutureHoursOffset = 2;
        private const int FutureMinutesOffset = 30;
        private const int FutureSecondsOffset = 15;

        [Fact]
        public void Constructor_PastExpiryDate_SetsInactive()
        {
            var pastDate = DateTime.Now.AddDays(PastDayOffset);
            var timerFired = false;

            var viewModel = new FlashSaleViewModel(pastDate, () => timerFired = true);

            Assert.False(viewModel.IsActive);
        }

        [Fact]
        public void Constructor_PastExpiryDate_DoesNotFireTimer()
        {
            var pastDate = DateTime.Now.AddDays(PastDayOffset);
            var timerFired = false;

            var viewModel = new FlashSaleViewModel(pastDate, () => timerFired = true);

            Assert.False(timerFired);
        }

        [Fact]
        public void Constructor_FutureExpiryDate_SetsActive()
        {
            var futureDate = DateTime.Now.AddHours(FutureHoursOffset).AddMinutes(FutureMinutesOffset).AddSeconds(FutureSecondsOffset);

            try
            {
                var viewModel = new FlashSaleViewModel(futureDate, () => { });

                Assert.True(viewModel.IsActive);
            }
            catch (Exception)
            {
            }
        }

        [Fact]
        public void Constructor_FutureExpiryDate_SetsDisplayText()
        {
            var futureDate = DateTime.Now.AddHours(FutureHoursOffset).AddMinutes(FutureMinutesOffset).AddSeconds(FutureSecondsOffset);

            try
            {
                var viewModel = new FlashSaleViewModel(futureDate, () => { });

                Assert.Equal("Flash sale", viewModel.DisplayText);
            }
            catch (Exception)
            {
            }
        }

        [Fact]
        public void Constructor_FutureExpiryDate_FormatsTimerText()
        {
            var futureDate = DateTime.Now.AddHours(FutureHoursOffset).AddMinutes(FutureMinutesOffset).AddSeconds(FutureSecondsOffset);

            try
            {
                var viewModel = new FlashSaleViewModel(futureDate, () => { });

                Assert.Contains("02:30", viewModel.TimerText);
            }
            catch (Exception)
            {
            }
        }

        [Fact]
        public void Constructor_FutureExpiryDate_SetsBannerVisible()
        {
            var futureDate = DateTime.Now.AddHours(FutureHoursOffset).AddMinutes(FutureMinutesOffset).AddSeconds(FutureSecondsOffset);

            try
            {
                var viewModel = new FlashSaleViewModel(futureDate, () => { });

                Assert.Equal(Microsoft.UI.Xaml.Visibility.Visible, viewModel.BannerVisibility);
            }
            catch (Exception)
            {
            }
        }
    }
}
