using System;

namespace MovieShop.Services
{
    public static class TransactionTypeMapper
    {
        public const string MoviePurchaseDisplay = "🎬 Movie Purchase";
        public const string TicketPurchaseDisplay = "🎟️ Ticket Purchase";
        public const string EquipmentPurchaseDisplay = "🎥 Equipment Purchase";
        public const string TopUpDisplay = "💳 Top-Up";
        public const string EquipmentSaleDisplay = "💰 Equipment Sale";
        public const string UnknownTransactionDisplay = "Unknown Transaction";

        public const string StatusPendingDisplay = "⏳ Pending";
        public const string StatusCompletedDisplay = "✅ Completed";
        public const string StatusFailedDisplay = "❌ Failed";
        public const string UnknownStatusDisplay = "Unknown Status";

        public static string ToDisplayString(string type)
        {
            return type switch
            {
                "MoviePurchase" => MoviePurchaseDisplay,
                "TicketPurchase" => TicketPurchaseDisplay,
                "EquipmentPurchase" => EquipmentPurchaseDisplay,
                "TopUp" => TopUpDisplay,
                "EquipmentSale" => EquipmentSaleDisplay,
                _ => UnknownTransactionDisplay
            };
        }

        public static string StatusToDisplayString(string status)
        {
            return status switch
            {
                "Pending" => StatusPendingDisplay,
                "Completed" => StatusCompletedDisplay,
                "Failed" => StatusFailedDisplay,
                _ => UnknownStatusDisplay
            };
        }

        public static string FormatAmount(decimal amount)
        {
            return amount >= 0
                ? $"+${amount:0.00}"
                : $"-${Math.Abs(amount):0.00}";
        }
    }
}