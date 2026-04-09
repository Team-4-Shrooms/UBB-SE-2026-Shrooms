using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Windows.ApplicationModel.Background;

namespace MovieShop.ViewModels
{
    public class MovieViewModel : INotifyPropertyChanged
    {
        private int saleID;
        private decimal basePrice;
        private decimal discountPercent;

        private DateTime expiryDate;

        private FlashSaleViewModel saleTimer;
        public FlashSaleViewModel SaleTimer
        {
            get => saleTimer;
            set
            {
                saleTimer = value;
                OnPropertyChanged();
            }
        }

        public int SaleID
        {
            get => saleID;
            set
            {
                saleID = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSaleActive));
                OnPropertyChanged(nameof(DisplayPrice));
            }
        }

        public decimal BasePrice
        {
            get => basePrice;
            set
            {
                basePrice = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayPrice));
            }
        }

        public decimal SalePrice
        {
            get => BasePrice * (1 - (discountPercent / 100.0m));
        }

        public decimal DiscountPercent
        {
            get => discountPercent;
            set
            {
                discountPercent = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SalePrice));
            }
        }

        public bool IsSaleActive
        {
            get
            {
                if (saleID != 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public decimal DisplayPrice => IsSaleActive ? SalePrice : BasePrice;

        public void RevertToOriginalPrice()
        {
            this.SaleID = 0;
            this.SaleTimer = null;
        }

        public void ApplyDatabaseSale(decimal discountPercent, DateTime expiryDate)
        {
            this.DiscountPercent = discountPercent;
            this.SaleID = 1;

            this.expiryDate = expiryDate;

            this.SaleTimer = new FlashSaleViewModel(expiryDate, this.RevertToOriginalPrice);
        }

        public string SaleBadgeText => IsSaleActive ? $"{DiscountPercent:0}% OFF" : string.Empty;
        public bool ShowStrike => IsSaleActive;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
