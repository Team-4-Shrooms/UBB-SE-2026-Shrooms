using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace MovieShop.ViewModels
{
    public class FlashSaleViewModel : INotifyPropertyChanged
    {
        private DispatcherTimer timer;
        private string displayText;
        private string timerText;
        private DateTime expiryDate;
        private bool isActive;

        private Action onExpiredAction;

        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                OnPropertyChanged();
            }
        }

        public string TimerText
        {
            get => timerText;
            set
            {
                timerText = value;
                OnPropertyChanged();
            }
        }

        public string DisplayText
        {
            get => displayText;
            set
            {
                displayText = value;
                OnPropertyChanged();
            }
        }

        public FlashSaleViewModel(DateTime saleEndTime, Action onExpired)
        {
            onExpiredAction = onExpired;

            if (saleEndTime <= DateTime.Now)
            {
                IsActive = false;
                return;
            }

            expiryDate = saleEndTime;
            IsActive = true;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TimerTick;
            timer.Start();

            UpdateCountdown();
        }

        private void TimerTick(object sender, object e)
        {
            UpdateCountdown();
        }

        private void UpdateCountdown()
        {
            TimeSpan timeRemaining = expiryDate - DateTime.Now;

            if (timeRemaining.TotalSeconds > 0)
            {
                DisplayText = "Flash sale";
                TimerText = string.Format("{0:D2}:{1:D2}:{2:D2}",
                                                (int)timeRemaining.TotalHours,
                                                (int)timeRemaining.Minutes,
                                                (int)timeRemaining.Seconds);
            }
            else
            {
                timer.Stop();
                DisplayText = "Flash sale has expired!";
                TimerText = "00:00:00";

                IsActive = false;

                onExpiredAction?.Invoke();

                // HandleSaleExpiry();
            }
        }

        private void HandleSaleExpiry()
        {
            Debug.WriteLine("Flash Sale has expired!");
        }

        public Visibility BannerVisibility => IsActive ? Visibility.Visible : Visibility.Collapsed;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
