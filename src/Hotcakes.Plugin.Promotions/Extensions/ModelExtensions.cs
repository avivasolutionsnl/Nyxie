using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Sitecore.Commerce.Plugin.Rules;
using Sitecore.Framework.Rules;
using Sitecore.Framework.Rules.Registry;

namespace Hotcakes.Plugin.Promotions.Extensions
{
    internal static class ModelExtensions
    {
        /// <summary>
        /// Extension of Sitecore.Commerce.Plugin.Rules.ModelExtensions.ConvertToCondition to allow boolean values
        /// </summary>
        internal static ICondition ConvertToConditionExtended(
          this ConditionModel model,
          IEntityMetadata metaData,
          IEntityRegistry registry,
          IServiceProvider services)
        {
            return model.Properties.Convert<ICondition>(metaData, registry, services);
        }

        /// <summary>
        /// Extension of Sitecore.Commerce.Plugin.Rules.ModelExtensions.ConvertToAction to allow boolean values
        /// </summary>
        internal static IAction ConvertToActionExtended(
          this ActionModel model,
          IEntityMetadata metaData,
          IEntityRegistry registry,
          IServiceProvider services)
        {
            return model.Properties.Convert<IAction>(metaData, registry, services);
        }

        private static T Convert<T>(
            this IList<PropertyModel> modelProperties,
            IEntityMetadata metaData,
            IEntityRegistry registry,
            IServiceProvider services) where T : IMappableRuleEntity
        {
            if (metaData.Type.GetCustomAttributes(typeof(ObsoleteAttribute), false).Any())
            {
                return default;
            }

            if (!(ActivatorUtilities.CreateInstance(services, metaData.Type) is T instance1))
            {
                return default;
            }
            var properties = instance1
                .GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public);
            if (properties.Any(p => IsBinaryOperator(p.PropertyType)))
            {
                ProcessBinaryOperation(modelProperties, registry, services, properties, instance1);
            }
            foreach (var property in modelProperties)
            {
                ConvertToLiteralRuleValue(property, instance1);
            }
            return instance1;
        }

        private static void ProcessBinaryOperation(IEnumerable<PropertyModel> modelProperties, IEntityRegistry registry, IServiceProvider services, PropertyInfo[] properties, IMappableRuleEntity instance)
        {
            var operatorModelProperty = modelProperties.FirstOrDefault(x => x.IsOperator);
            if (operatorModelProperty == null)
            {
                return;
            }

            var entityMetadata = registry
                .GetOperators()
                .FirstOrDefault(m =>
                    m.Type.FullName != null &&
                    m.Type.FullName.EqualsOrdinalIgnoreCase(operatorModelProperty.Value));
            var instance2 = ActivatorUtilities.CreateInstance(services, entityMetadata?.Type);

            var propertyInfo = properties.FirstOrDefault(p => IsBinaryOperator(p.PropertyType));
            propertyInfo?.SetValue(instance, instance2);
        }

        private static void ConvertToLiteralRuleValue(PropertyModel property1, IMappableRuleEntity instance)
        {
            if (property1.IsOperator)
            {
                return;
            }

            var property2 = instance
                .GetType()
                .GetProperty(property1.Name, BindingFlags.Instance | BindingFlags.Public);
            if (property2 == null)
            { return; }

            var type = property2.PropertyType.IsGenericType &&
                           typeof(IRuleValue<>).IsAssignableFrom(property2.PropertyType.GetGenericTypeDefinition()) ?
                property2.PropertyType.GetGenericArguments().FirstOrDefault() :
                property2.PropertyType;
            if (type == null)
            {
                return;
            }

            object literalRuleValue = null;
            switch (type.FullName)
            {
                case "System.DateTime":
                    if (DateTime.TryParse(property1.Value, out var dateTimeResult))
                    {
                        literalRuleValue = new LiteralRuleValue<DateTime>() { Value = dateTimeResult };
                    }
                    break;
                case "System.DateTimeOffset":
                    if (DateTimeOffset.TryParse(property1.Value, out var dateTimeOffsetResult))
                    {
                        literalRuleValue = new LiteralRuleValue<DateTimeOffset>() { Value = dateTimeOffsetResult };
                    }
                    break;
                case "System.Int32":
                    if (int.TryParse(property1.Value, out var intResult))
                    {
                        literalRuleValue = new LiteralRuleValue<int>() { Value = intResult };
                    }
                    break;
                case "System.Decimal":
                    if (decimal.TryParse(property1.Value, out var decimalResult))
                    {
                        literalRuleValue = new LiteralRuleValue<decimal> { Value = decimalResult };
                    }
                    break;
                case "System.Boolean":
                    if (bool.TryParse(property1.Value, out var booleanResult))
                    {
                        literalRuleValue = new LiteralRuleValue<bool> { Value = booleanResult };
                    }
                    break;
                default:
                    literalRuleValue = new LiteralRuleValue<string>() { Value = property1.Value };
                    break;
            }

            property2.SetValue(instance, literalRuleValue, null);
        }

        private static bool IsBinaryOperator(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IBinaryOperator<,>);
        }
    }
}
