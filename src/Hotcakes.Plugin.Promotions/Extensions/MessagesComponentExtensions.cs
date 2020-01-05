using Sitecore.Commerce.Core;

namespace Hotcakes.Plugin.Promotions.Extensions
{
    public static class MessagesComponentExtensions
    {
        public static void AddPromotionApplied(this MessagesComponent messageComponent, CommerceContext commerceContext,
            string awardingBlock)
        {
            var propertiesModel = commerceContext.GetObject<PropertiesModel>();
            object promotionName = propertiesModel?.GetPropertyValue("PromotionId") ?? awardingBlock;
            messageComponent.AddMessage(commerceContext.GetPolicy<KnownMessageCodePolicy>().Promotions,
                $"PromotionApplied: {promotionName}");
        }
    }
}
