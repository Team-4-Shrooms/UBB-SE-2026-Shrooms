using System.ComponentModel;
using System.Runtime.CompilerServices;
using MovieShop.Models;

namespace MovieShop.ViewModels
{
    public class MovieEventListItem : INotifyPropertyChanged
    {
        private bool canBuy;

        public MovieEventListItem(MovieEvent movieEvent, bool canBuy)
        {
            Event = movieEvent;
            this.canBuy = canBuy;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MovieEvent Event { get; }

        public string Title => Event.Title;

        public string Description => Event.Description;

        public string Location => Event.Location;

        public string PosterUrl => Event.PosterUrl;

        public string DisplayDate => Event.DisplayDate;

        public string DisplayTicketPrice => Event.DisplayTicketPrice;

        public bool CanBuy
        {
            get => canBuy;
            set
            {
                if (canBuy == value)
                {
                    return;
                }

                canBuy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BuyButtonOpacity));
            }
        }

        public double BuyButtonOpacity => CanBuy ? UIConstants.EnabledButtonOpacity : UIConstants.DisabledButtonOpacity;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
