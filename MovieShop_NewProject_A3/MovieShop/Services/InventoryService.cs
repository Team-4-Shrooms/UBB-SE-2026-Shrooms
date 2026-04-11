using System;
using System.Collections.Generic;
using MovieShop.Models;
using MovieShop.Repositories;

namespace MovieShop.Services
{
	public class InventoryService : IInventoryService
	{
		private readonly IInventoryRepository inventoryRepo;

		public InventoryService(IInventoryRepository inventoryRepo)
		{
			this.inventoryRepo = inventoryRepo;
		}

		public List<Movie> GetOwnedMovies(int userId) => inventoryRepo.GetOwnedMovies(userId);

		public List<MovieEvent> GetOwnedTickets(int userId) => inventoryRepo.GetOwnedTickets(userId);

		public List<Equipment> GetOwnedEquipment(int userId) => inventoryRepo.GetOwnedEquipment(userId);

		public IEnumerable<Movie> RemoveMovie(int userId, int movieId)
		{
			if (userId <= 0)
            {
                throw new ArgumentException("Invalid user ID.", nameof(userId));
            }

            if (movieId <= 0)
            {
                throw new ArgumentException("Invalid movie ID.", nameof(movieId));
            }

            inventoryRepo.RemoveOwnedMovie(userId, movieId);
			return inventoryRepo.GetOwnedMovies(userId);
		}

		public IEnumerable<MovieEvent> RemoveTicket(int userId, int eventId)
		{
			if (userId <= 0)
            {
                throw new ArgumentException("Invalid user ID.", nameof(userId));
            }

            if (eventId <= 0)
            {
                throw new ArgumentException("Invalid event ID.", nameof(eventId));
            }

            inventoryRepo.RemoveOwnedTicket(userId, eventId);
			return inventoryRepo.GetOwnedTickets(userId);
		}
	}
}