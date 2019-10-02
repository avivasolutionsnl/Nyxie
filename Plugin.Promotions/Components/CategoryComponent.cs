using System.Collections.Generic;
using System.Linq;
using Sitecore.Commerce.Core;
using System.Collections.Generic;
using System.Linq;

namespace Promethium.Plugin.Promotions.Components
{
    public class CategoryComponent : Component
    {
        public CategoryComponent()
        {
            ParentCategoryList = new List<string>();
        }

        /// <summary>
        /// A list of all the paths the product is in.
        /// Each path is a / seperated string of Guids starting from top root category to the most specific category
        ///
        /// Example:
        /// In the habitat store the product  6042185 (Mira 17.3” Laptop—16GB Memory, 1TB Hard drive) is in 2 categories
        /// - /Computers and Tablets
        /// - /Computers and Tablets/Laptops
        /// The list will contain the values
        /// - /b893c063-6238-ed57-700d-72862c56038d/994ec8cb-fe90-5071-a631-4458b1e83db3
        /// - /b893c063-6238-ed57-700d-72862c56038d/994ec8cb-fe90-5071-a631-4458b1e83db3/d7f45dfb-f7c0-ddca-a689-b402eacdddcd
        /// </summary>
        public List<string> ParentCategoryList { get; set; }

        public bool IsMatch(string specificCategory, bool includeSubCategories)
        {
            return ParentCategoryList.Any(x => includeSubCategories ? x.Contains(specificCategory) : x.EndsWith(specificCategory));
        }
    }
}