using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

using FluentAssertions;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Extensions;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Sample.Contexts;
using Sitecore.Commerce.ServiceProxy;

namespace Sitecore.Commerce.Sample.Console
{
    public static class ComposerUX
    {
        private const string EntityId = "Entity-SellableItem-AW140 13";
        private const string TemplateId = "Entity-ComposerTemplate-MyConsoleTemplate";
        private const string ComposerViewName = "MyConsoleView";
        private const string TemplateName = "MyConsoleTemplate";
        private static Sitecore.Commerce.Engine.Container _authoringContainer;

        public static void RunScenarios()
        {
            var watch = new Stopwatch();
            watch.Start();

            System.Console.WriteLine("Begin Composer");

            var context = new CsrSheila().Context;
            context.Environment = "AdventureWorksAuthoring";
            _authoringContainer = context.ShopsContainer();

            var composerEntityViewItemId = AddChildView();
            AddProperties(composerEntityViewItemId, EntityId);
            EditView(composerEntityViewItemId, EntityId);
            AddMinMaxPropertyConstraint(composerEntityViewItemId, EntityId);
            AddSelectionOptionPropertyConstraint(composerEntityViewItemId, EntityId);
            RemoveProperty(composerEntityViewItemId, EntityId);
            RemoveView(composerEntityViewItemId, EntityId);

            composerEntityViewItemId = AddChildView();
            MakeTemplate(composerEntityViewItemId);
            RemoveView(composerEntityViewItemId, EntityId);

            composerEntityViewItemId = AddChildViewFromTemplate();
            RemoveView(composerEntityViewItemId, EntityId);

            var composerTemplateViewItemId = GetTemplateViews();
            ManageTemplateTags();
            LinkTemplateToEntities();
            AddProperties(composerTemplateViewItemId, TemplateId);
            EditView(composerTemplateViewItemId, TemplateId);
            AddMinMaxPropertyConstraint(composerTemplateViewItemId, TemplateId);
            AddSelectionOptionPropertyConstraint(composerTemplateViewItemId, TemplateId);
            RemoveProperty(composerTemplateViewItemId, TemplateId);
            RemoveTemplate();

            watch.Stop();

            System.Console.WriteLine($"End Composer :{watch.ElapsedMilliseconds} ms");
        }

        private static string AddChildView()
        {
            System.Console.WriteLine("Begin AddChildView");

            var view = Proxy.GetValue(_authoringContainer.GetEntityView(EntityId, "AddChildView", "AddChildView", string.Empty));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.ChildViews.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();

            var version = view.Properties.FirstOrDefault(p => p.Name.Equals("Version"));
            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "Name", Value = string.Empty },
                new ViewProperty { Name = "DisplayName", Value = string.Empty },
                version
            };
            var result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            ConsoleExtensions.WriteColoredLine(ConsoleColor.Yellow, "Expected error");

            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "Name", Value = ComposerViewName },
                new ViewProperty { Name = "DisplayName", Value = "My Console View" },
                version
            };
            result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();

            var masterView = Proxy.GetValue(_authoringContainer.GetEntityView(EntityId, "Master", string.Empty, string.Empty));
            masterView.Should().NotBeNull();
            var composerView = masterView.ChildViews.OfType<EntityView>().FirstOrDefault(v => v.Name.Equals(ComposerViewName));
            composerView.Should().NotBeNull();
            return composerView.ItemId;
        }

        private static string AddChildViewFromTemplate()
        {
            System.Console.WriteLine("Begin AddChildViewFromTemplate");

            var view = Proxy.GetValue(_authoringContainer.GetEntityView(EntityId, "AddChildViewFromTemplate", "AddChildViewFromTemplate", string.Empty));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.ChildViews.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();

            var version = view.Properties.FirstOrDefault(p => p.Name.Equals("Version"));
            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "Template", Value = string.Empty },
                version
            };
            var result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            ConsoleExtensions.WriteColoredLine(ConsoleColor.Yellow, "Expected error");

            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "Template", Value = TemplateName },
                version
            };
            result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();

            var masterView = Proxy.GetValue(_authoringContainer.GetEntityView(EntityId, "Master", string.Empty, string.Empty));
            masterView.Should().NotBeNull();
            var composerView = masterView.ChildViews.OfType<EntityView>().FirstOrDefault(v => v.Name.Equals(ComposerViewName));
            composerView.Should().NotBeNull();
            return composerView.ItemId;
        }

        private static void AddProperties(string composerViewItemId, string entityId)
        {
            System.Console.WriteLine("Begin AddProperties");

            var view = Proxy.GetValue(_authoringContainer.GetEntityView(entityId, "AddProperty", "AddProperty", composerViewItemId));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.ChildViews.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();

            var version = view.Properties.FirstOrDefault(p => p.Name.Equals("Version"));
            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "Name", Value = string.Empty },
                new ViewProperty { Name = "DisplayName", Value = string.Empty },
                new ViewProperty { Name = "PropertyType", Value = string.Empty },
                version
            };
            var result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            ConsoleExtensions.WriteColoredLine(ConsoleColor.Yellow, "Expected error");

            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "Name", Value = "MyStringProperty" },
                new ViewProperty { Name = "DisplayName", Value = "My String Property" },
                new ViewProperty { Name = "PropertyType", Value = "System.String" },
                version
            };
            result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();

            view = Proxy.GetValue(_authoringContainer.GetEntityView(entityId, "AddProperty", "AddProperty", composerViewItemId));
            version = view.Properties.FirstOrDefault(p => p.Name.Equals("Version"));
            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "Name", Value = "MyDecimalProperty" },
                new ViewProperty { Name = "DisplayName", Value = "My Decimal Property" },
                new ViewProperty { Name = "PropertyType", Value = "System.Decimal" },
                version
            };
            result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();

            view = Proxy.GetValue(_authoringContainer.GetEntityView(entityId, "AddProperty", "AddProperty", composerViewItemId));
            version = view.Properties.FirstOrDefault(p => p.Name.Equals("Version"));
            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "Name", Value = "MyIntProperty" },
                new ViewProperty { Name = "DisplayName", Value = "My Int Property" },
                new ViewProperty { Name = "PropertyType", Value = "System.Int64" },
                version
            };
            result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();

            view = Proxy.GetValue(_authoringContainer.GetEntityView(entityId, "AddProperty", "AddProperty", composerViewItemId));
            version = view.Properties.FirstOrDefault(p => p.Name.Equals("Version"));
            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "Name", Value = "MyDateProperty" },
                new ViewProperty { Name = "DisplayName", Value = "My Date Property" },
                new ViewProperty { Name = "PropertyType", Value = "System.DateTimeOffset" },
                version
            };
            result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();

            view = Proxy.GetValue(_authoringContainer.GetEntityView(entityId, "AddProperty", "AddProperty", composerViewItemId));
            version = view.Properties.FirstOrDefault(p => p.Name.Equals("Version"));
            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "Name", Value = "MyBoolProperty" },
                new ViewProperty { Name = "DisplayName", Value = "My Bool Property" },
                new ViewProperty { Name = "PropertyType", Value = "System.Boolean" },
                version
            };
            result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();

            var masterView = Proxy.GetValue(_authoringContainer.GetEntityView(entityId, "Master", string.Empty, string.Empty));
            masterView.Should().NotBeNull();
            var composerView = masterView.ChildViews.OfType<EntityView>().FirstOrDefault(v => v.Name.Equals(ComposerViewName));
            composerView.Should().NotBeNull();
            composerView.Properties.Should().NotBeEmpty();
        }

        private static void EditView(string composerViewItemId, string entityId)
        {
            System.Console.WriteLine("Begin EditView");

            var view = Proxy.GetValue(_authoringContainer.GetEntityView(entityId, "EditView", "EditView", composerViewItemId));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.ChildViews.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();

            var version = view.Properties.FirstOrDefault(p => p.Name.Equals("Version"));
            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "MyStringProperty", Value = string.Empty },
                new ViewProperty { Name = "MyDecimalProperty", Value = string.Empty },
                new ViewProperty { Name = "MyIntProperty", Value = "asd" },
                new ViewProperty { Name = "MyBoolProperty", Value = null },
                new ViewProperty { Name = "MyDateProperty", Value = string.Empty },
                version
            };
            var result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            ConsoleExtensions.WriteColoredLine(ConsoleColor.Yellow, "Expected error");

            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "MyStringProperty", Value = "value" },
                new ViewProperty { Name = "MyDecimalProperty", Value = "3.5" },
                new ViewProperty { Name = "MyIntProperty", Value = "3" },
                new ViewProperty { Name = "MyBoolProperty", Value = "true"},
                new ViewProperty { Name = "MyDateProperty", Value = "2018-02-23T14:14:09.404Z" },
                version
            };
            result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();

            var masterView = Proxy.GetValue(_authoringContainer.GetEntityView(entityId, "Master", string.Empty, string.Empty));
            masterView.Should().NotBeNull();
            var composerView = masterView.ChildViews.OfType<EntityView>().FirstOrDefault(v => v.Name.Equals(ComposerViewName));
            composerView.Should().NotBeNull();
            composerView.Properties.Should().NotBeEmpty();
            composerView.Properties.All(p => p.Value != string.Empty).Should().BeTrue();
        }

        private static void AddMinMaxPropertyConstraint(string composerViewItemId, string entityId)
        {
            System.Console.WriteLine("Begin AddMinMaxPropertyConstraint");

            var view = Proxy.GetValue(_authoringContainer.GetEntityView(entityId, "AddMinMaxPropertyConstraint", "AddMinMaxPropertyConstraint", composerViewItemId));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.ChildViews.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();

            var version = view.Properties.FirstOrDefault(p => p.Name.Equals("Version"));
            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "Property", Value = string.Empty },
                new ViewProperty { Name = "Minimum", Value = string.Empty },
                new ViewProperty { Name = "Maximum", Value = "asd" },
                version
            };
            var result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            ConsoleExtensions.WriteColoredLine(ConsoleColor.Yellow, "Expected error");

            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "Property", Value = "MyDateProperty" },
                new ViewProperty { Name = "Minimum", Value = "0" },
                new ViewProperty { Name = "Maximum", Value = "20" },
                version
            };
            result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            ConsoleExtensions.WriteColoredLine(ConsoleColor.Yellow, "Expected error");

            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "Property", Value = "MyIntProperty" },
                new ViewProperty { Name = "Minimum", Value = "0" },
                new ViewProperty { Name = "Maximum", Value = "20" },
                version
            };
            result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
        }

        private static void AddSelectionOptionPropertyConstraint(string composerViewItemId, string entityId)
        {
            System.Console.WriteLine("Begin AddSelectionOptionPropertyConstraint");

            var view = Proxy.GetValue(_authoringContainer.GetEntityView(entityId, "AddSelectionOptionPropertyConstraint", "AddSelectionOptionPropertyConstraint", composerViewItemId));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.ChildViews.Should().NotBeEmpty();
            view.Properties.Should().NotBeEmpty();

            var version = view.Properties.FirstOrDefault(p => p.Name.Equals("Version"));
            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "Property", Value = string.Empty },
                version
            };
            var result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            ConsoleExtensions.WriteColoredLine(ConsoleColor.Yellow, "Expected error");

            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "Property", Value = "MyStringProperty" },
                version
            };
            view.ChildViews = new ObservableCollection<Model>
            {
                new EntityView {
                    Name = "Cell",
                    Properties = new ObservableCollection<ViewProperty>
                    {
                        new ViewProperty { Name = "OptionValue", Value = "Value1" },
                        new ViewProperty { Name = "OptionName", Value = "Value 1" }
                    }
                },
                new EntityView {
                    Name = "Cell",
                    Properties = new ObservableCollection<ViewProperty>
                    {
                        new ViewProperty { Name = "OptionValue", Value = "Value2" },
                        new ViewProperty { Name = "OptionName", Value = "Value 2" }
                    }
                }
            };
            result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
        }

        private static void RemoveProperty(string composerViewItemId, string entityId)
        {
            System.Console.WriteLine("Begin RemoveProperty");

            var view = Proxy.GetValue(_authoringContainer.GetEntityView(entityId, "RemoveProperty", "RemoveProperty", composerViewItemId));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.ChildViews.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();

            var version = view.Properties.FirstOrDefault(p => p.Name.Equals("Version"));
            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "Property", Value = string.Empty },
                version
            };
            var result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            ConsoleExtensions.WriteColoredLine(ConsoleColor.Yellow, "Expected error");

            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "Property", Value = "MyDecimalProperty" },
                version
            };
            result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();

            var masterView = Proxy.GetValue(_authoringContainer.GetEntityView(entityId, "Master", string.Empty, string.Empty));
            masterView.Should().NotBeNull();
            var composerView = masterView.ChildViews.OfType<EntityView>().FirstOrDefault(v => v.Name.Equals(ComposerViewName));
            composerView.Should().NotBeNull();
            composerView.Properties.Should().NotBeEmpty();
            composerView.Properties.Any(p => p.Name.Equals("MyDecimalProperty")).Should().BeFalse();
        }

        private static void MakeTemplate(string composerViewItemId)
        {
            System.Console.WriteLine("Begin MakeTemplate");

            var view = Proxy.GetValue(_authoringContainer.GetEntityView(EntityId, "MakeTemplate", "MakeTemplate", composerViewItemId));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.ChildViews.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();

            var version = view.Properties.FirstOrDefault(p => p.Name.Equals("Version"));
            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "Name", Value = string.Empty },
                new ViewProperty { Name = "DisplayName", Value = string.Empty },
                version
            };
            var result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            ConsoleExtensions.WriteColoredLine(ConsoleColor.Yellow, "Expected error");

            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "Name", Value = TemplateName },
                new ViewProperty { Name = "DisplayName", Value = "My Console Template" },
                version
            };
            result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();

            var templatesView = Proxy.GetValue(_authoringContainer.GetEntityView(string.Empty, "ComposerTemplates", string.Empty, string.Empty));
            templatesView.Should().NotBeNull();
            templatesView.Policies.Should().NotBeEmpty();
            templatesView.Properties.Should().BeEmpty();
            templatesView.ChildViews.Should().NotBeEmpty();
            templatesView.ChildViews.OfType<EntityView>().Any(v => v.EntityId.Equals(TemplateId)).Should().BeTrue();
        }

        private static void RemoveView(string composerViewItemId, string entityId)
        {
            System.Console.WriteLine("Begin RemoveView");

            var view = Proxy.GetValue(_authoringContainer.GetEntityView(entityId, string.Empty, "RemoveView", composerViewItemId));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.ChildViews.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();
            
            var result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();

            var masterView = Proxy.GetValue(_authoringContainer.GetEntityView(EntityId, "Master", string.Empty, string.Empty));
            masterView.Should().NotBeNull();
            masterView.ChildViews.OfType<EntityView>().Any(v => v.Name.Equals(ComposerViewName)).Should().BeFalse();
        }

        private static string GetTemplateViews()
        {
            System.Console.WriteLine("Begin GetTemplateViews");

            var view = Proxy.GetValue(_authoringContainer.GetEntityView(TemplateId, "Master", string.Empty, string.Empty));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();
            view.ChildViews.Should().NotBeEmpty();
            foreach (var childView in view.ChildViews.OfType<EntityView>())
            {
                childView.Name.Should().BeOneOf("Details", ComposerViewName);
                childView.EntityId.Should().Be(TemplateId);

                if (childView.Name.Equals("Details"))
                {
                    childView.ItemId.Should().BeNullOrEmpty();
                }

                if (childView.Name.Equals(ComposerViewName))
                {
                    childView.ItemId.Should().NotBeNullOrEmpty();
                }
                
                childView.Policies.Should().NotBeEmpty();
                childView.ChildViews.Should().BeEmpty();
            }

            view = Proxy.GetValue(_authoringContainer.GetEntityView(TemplateId, "Details", string.Empty, string.Empty));
            view.Should().NotBeNull();
            view.Policies.Should().NotBeEmpty();
            view.Properties.Should().NotBeEmpty();
            view.ChildViews.Should().NotBeEmpty();
            return view.ChildViews.OfType<EntityView>().FirstOrDefault()?.ItemId;
        }

        private static void ManageTemplateTags()
        {
            System.Console.WriteLine("Begin ManageTemplateTags");

            var view = Proxy.GetValue(_authoringContainer.GetEntityView(TemplateId, "ManageTemplateTags", "ManageTemplateTags", string.Empty));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.ChildViews.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();

            var version = view.Properties.FirstOrDefault(p => p.Name.Equals("Version"));
            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "Tags", Value = "['Tag1','Tag2','Tag3']" },
                version
            };
            var result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();

            view = Proxy.GetValue(_authoringContainer.GetEntityView(TemplateId, "ManageTemplateTags", "ManageTemplateTags", string.Empty));
            version = view.Properties.FirstOrDefault(p => p.Name.Equals("Version"));
            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty { Name = "Tags", Value = "['Tag1','Tag2']" },
                version
            };
            result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();

            view = Proxy.GetValue(_authoringContainer.GetEntityView(TemplateId, "Details", string.Empty, string.Empty));
            view.Should().NotBeNull();
            view.Properties.FirstOrDefault(p => p.Name.Equals("Tags"))?.Value?.Should().Be("[\r\n  \"Tag1\",\r\n  \"Tag2\"\r\n]");
        }

        private static void LinkTemplateToEntities()
        {
            System.Console.WriteLine("Begin LinkTemplateToEntities");

            var view = Proxy.GetValue(_authoringContainer.GetEntityView(TemplateId, "LinkTemplateToEntities", "LinkTemplateToEntities", string.Empty));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.ChildViews.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();
            
            var catalogProperty = view.Properties.FirstOrDefault(p => p.Name.Equals("Sitecore.Commerce.Plugin.Catalog.Catalog"));
            catalogProperty.Should().NotBeNull();
            catalogProperty.Value = "true";
            var result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();

            view = Proxy.GetValue(_authoringContainer.GetEntityView(TemplateId, "LinkTemplateToEntities", "LinkTemplateToEntities", string.Empty));
            catalogProperty = view.Properties.FirstOrDefault(p => p.Name.Equals("Sitecore.Commerce.Plugin.Catalog.Catalog"));
            catalogProperty.Should().NotBeNull();
            catalogProperty.Value = "false";
            result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();

            view = Proxy.GetValue(_authoringContainer.GetEntityView(TemplateId, "Details", string.Empty, string.Empty));
            view.Should().NotBeNull();
            view.Properties.FirstOrDefault(p => p.Name.Equals("LinkedEntities"))?.Value?.Should().Be("[]");
        }

        private static void RemoveTemplate()
        {
            System.Console.WriteLine("Begin RemoveTemplate");

            var view = Proxy.GetValue(_authoringContainer.GetEntityView(string.Empty, string.Empty, "RemoveTemplate", TemplateId));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.ChildViews.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();

            var result = Proxy.DoCommand(_authoringContainer.DoAction(view));
            result.Messages.Any(m => m.Code.Equals("validationerror", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();

            var templatesView = Proxy.GetValue(_authoringContainer.GetEntityView(string.Empty, "ComposerTemplates", string.Empty, string.Empty));
            templatesView.Should().NotBeNull();
            templatesView.ChildViews.OfType<EntityView>().Any(v => v.ItemId.Equals(TemplateId)).Should().BeFalse();
        }
    }
}
