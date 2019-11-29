using System;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Pricing;

namespace Promethium.Plugin.Promotions.Classes
{
    public class MoneyEx
    {
        private readonly GlobalPricingPolicy pricingPolicy;

        public Money Value { get; }

        public MoneyEx(GlobalPricingPolicy pricingPolicy, Money value)
        {
            this.pricingPolicy = pricingPolicy;
            Value = value;
        }

        public MoneyEx(CommerceContext commerceContext, Money value) : 
            this(commerceContext.GetPolicy<GlobalPricingPolicy>(), value)
        {
        }

        public MoneyEx(CommerceContext commerceContext, decimal value) :
            this(commerceContext.GetPolicy<GlobalPricingPolicy>(), new Money(value))
        {
        }

        public MoneyEx Round()
        {
            if (pricingPolicy.ShouldRoundPriceCalc)
            {
                return new MoneyEx(pricingPolicy, new Money(Value.CurrencyCode, Math.Round(Value.Amount,
                    pricingPolicy.RoundDigits,
                    pricingPolicy.MidPointRoundUp
                        ? MidpointRounding.AwayFromZero
                        : MidpointRounding.ToEven)));
            }

            return new MoneyEx(pricingPolicy, Value);
        }


        public MoneyEx CalculatePriceDiscount(decimal amountOff)
        {
            return new MoneyEx(pricingPolicy, new Money(Value.CurrencyCode, amountOff > Value.Amount ? Value.Amount : amountOff));
        }

        public MoneyEx CalculatePercentageDiscount(decimal percentage)
        {
            return new MoneyEx(pricingPolicy, new Money(Value.CurrencyCode, Value.Amount * (percentage / 100)));
        }
    }
}