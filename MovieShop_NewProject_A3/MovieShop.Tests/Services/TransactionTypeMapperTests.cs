using MovieShop.Services;

namespace MovieShop.Tests.Services
{
    public class TransactionTypeMapperTests
    {
        [Theory]
        [InlineData("MoviePurchase", TransactionTypeMapper.MoviePurchaseDisplay)]
        [InlineData("TicketPurchase", TransactionTypeMapper.TicketPurchaseDisplay)]
        [InlineData("EquipmentPurchase", TransactionTypeMapper.EquipmentPurchaseDisplay)]
        [InlineData("TopUp", TransactionTypeMapper.TopUpDisplay)]
        [InlineData("EquipmentSale", TransactionTypeMapper.EquipmentSaleDisplay)]
        public void ToDisplayString_KnownType_ReturnsMappedDisplay(string type, string expected)
        {
            Assert.Equal(expected, TransactionTypeMapper.ToDisplayString(type));
        }

        [Fact]
        public void ToDisplayString_UnknownType_ReturnsUnknownDisplay()
        {
            Assert.Equal(TransactionTypeMapper.UnknownTransactionDisplay, TransactionTypeMapper.ToDisplayString("NotARealType"));
        }

        [Fact]
        public void ToDisplayString_EmptyString_ReturnsUnknownDisplay()
        {
            Assert.Equal(TransactionTypeMapper.UnknownTransactionDisplay, TransactionTypeMapper.ToDisplayString(string.Empty));
        }

        [Theory]
        [InlineData("Pending", TransactionTypeMapper.StatusPendingDisplay)]
        [InlineData("Completed", TransactionTypeMapper.StatusCompletedDisplay)]
        [InlineData("Failed", TransactionTypeMapper.StatusFailedDisplay)]
        public void StatusToDisplayString_KnownStatus_ReturnsMappedDisplay(string status, string expected)
        {
            Assert.Equal(expected, TransactionTypeMapper.StatusToDisplayString(status));
        }

        [Fact]
        public void StatusToDisplayString_UnknownStatus_ReturnsUnknownDisplay()
        {
            Assert.Equal(TransactionTypeMapper.UnknownStatusDisplay, TransactionTypeMapper.StatusToDisplayString("Bogus"));
        }

        [Fact]
        public void FormatAmount_PositiveAmount_PrependsPlusSign()
        {
            Assert.Equal("+$12.50", TransactionTypeMapper.FormatAmount(12.50m));
        }

        [Fact]
        public void FormatAmount_Zero_PrependsPlusSign()
        {
            Assert.Equal("+$0.00", TransactionTypeMapper.FormatAmount(0m));
        }

        [Fact]
        public void FormatAmount_NegativeAmount_PrependsMinusSignAndUsesAbsoluteValue()
        {
            Assert.Equal("-$7.25", TransactionTypeMapper.FormatAmount(-7.25m));
        }

        [Fact]
        public void FormatAmount_PositiveAmount_FormatsToTwoDecimalPlaces()
        {
            Assert.Equal("+$3.00", TransactionTypeMapper.FormatAmount(3m));
        }
    }
}
