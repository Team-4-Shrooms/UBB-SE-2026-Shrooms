using System;

namespace MovieShop.Models
{
    public sealed class MovieEvent
    {
        public int ID { get; set; }
        public int MovieID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal TicketPrice { get; set; }
        public string PosterUrl { get; set; } = string.Empty;

        public string DisplayDate => Date.ToString("yyyy-MM-dd HH:mm");
        public string DisplayTicketPrice => TicketPrice.ToString("C");
    }
}

