// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SampleComponent.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Commerce.Plugin.Sample
{
    using Sitecore.Commerce.Core;

    /// <inheritdoc />
    /// <summary>
    /// The SampleComponent.
    /// </summary>
    public class CategoryComponent : Component
    {
        public string ParentCategoryList { get; set; }
    }
}