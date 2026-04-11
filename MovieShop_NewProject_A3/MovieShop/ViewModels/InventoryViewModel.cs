using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MovieShop.Models;
using MovieShop.Services;

namespace MovieShop.ViewModels
{
    public class InventoryViewModel : INotifyPropertyChanged
    {
        private readonly IInventoryService inventoryService;

        public InventoryViewModel(IInventoryService inventoryService)
        {
            this.inventoryService = inventoryService;
            OwnedMovies = new ObservableCollection<Movie>();
            OwnedTickets = new ObservableCollection<MovieEvent>();
            OwnedEquipment = new ObservableCollection<Equipment>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<Movie> OwnedMovies { get; }

        public ObservableCollection<MovieEvent> OwnedTickets { get; }

        public ObservableCollection<Equipment> OwnedEquipment { get; }

        public void Load()
        {
            var userId = SessionManager.CurrentUserID;
            if (userId <= 0)
            {
                OwnedMovies.Clear();
                OwnedTickets.Clear();
                OwnedEquipment.Clear();
                return;
            }

            Replace(OwnedMovies, inventoryService.GetOwnedMovies(userId));
            Replace(OwnedTickets, inventoryService.GetOwnedTickets(userId));
            Replace(OwnedEquipment, inventoryService.GetOwnedEquipment(userId));
        }

        public void RemoveMovie(Movie movie)
        {
            if (movie == null)
            {
                return;
            }

            var updated = inventoryService.RemoveMovie(SessionManager.CurrentUserID, movie.ID);
            Replace(OwnedMovies, updated);
        }

        public void RemoveTicket(MovieEvent movieEvent)
        {
            if (movieEvent == null)
            {
                return;
            }

            var updated = inventoryService.RemoveTicket(SessionManager.CurrentUserID, movieEvent.ID);
            Replace(OwnedTickets, updated);
        }

        private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> source)
        {
            target.Clear();
            foreach (var item in source)
            {
                target.Add(item);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
