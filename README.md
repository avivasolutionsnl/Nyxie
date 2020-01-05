# Promethium

## Prerequisites
- Windows 10 update 1809
- Docker for Windows version >= 18.09.1
- Visual Studio 15.5.3

## Getting started 
- Open a console en navigate to the repository root folder.

- Build the project in Visual Studio and publish the projects to the following folders
Promethium.Serialization --> \build\website
Sitecore.Commerce.Engine --> \build\commerce

- Copy your Sitecore license file (license.xml) to the .\license folder

- Login in to the Docker repository using your Aviva credentials:
```
PS> az acr login --name avivasolutionsnl
```

- Spin up the environment, make sure you are using windows and not linux containers:
```
PS> docker-compose up
```

To set the Docker container service names as DNS names on your host edit your `hosts` file. 
A convenient tool to automatically do this is [whales-names](https://github.com/gregolsky/whales-names).

Synchronize the development content by running Unicorn: [http://sitecore/unicorn.aspx?verb=sync](http://sitecore/unicorn.aspx?verb=sync).

Fix indexes by:

- Opening the content editor
- Goto the commerce tab
- Delete Data Templates
- Update Data Templates
- Goto control panel and rebuild the `sitecore_master_index` & `sitecore_sxa_master_index`

> If you get an error saying: 'field _indexname' not found: remove files in host \cores folder. Restart containers and populate schema.

## Build Promethium docker images
- Run the docker script `.\Build-docker-images.ps1 ` 
- Publish the docker images, for example:

```
docker push avivasolutionsnl.azurecr.io/promethium-sitecore:9.1.0-20190528
docker push avivasolutionsnl.azurecr.io/promethium-solr:9.1.0-20190528
docker push avivasolutionsnl.azurecr.io/promethium-mssql:9.1.0-20190528
```

## How to use

### Qualifications

#### Cart contains products in specific category
Will apply the given benefit when the cart contains a product in a specific category.

When a qualification is added to a promotion, the following conditions will be available in the list:

`Cart contains [compares] [specific value] products in the [specific category] category`

| Variable              | Type        | Default value  |Description                     |
| -------------         |-------------| -----          | --------                       |
| compares              | operator    |                |standard operators|
| specific value        | integer     |                |indicates the subtotal of products compared using the configured operator|
| specific category     | category    |                |The name of the category. This is a search box. |
|Include sub categories |bool         |true            |indicates whether sub categories are included|

`Cart contains products in the [specific category] category for a total [compares] $ [specific value]`

| Variable              | Type        | Default value  |Description                     |
| -------------         |-------------| -----          | --------                       |
| compares              | operator    |                |standard operators|
| specific value        | integer     |                |indicates the numer of products compared using the configured operator|
| specific category     | category    |                |The name of the category. This is a search box. |
|Include sub categories |bool         |true            |indicates whether sub categories are included|

> If nothing happens when entering the category name and no categories can be found: make sure the indexes have been build and see the fix indexes paragraph above.

#### Cart has specific fulfillment
Will apply the given benefit when the cart contains a specific fulfillment method.

When a qualification is added to a promotion, the following condition will be available in the list:

`Cart fulfillment is [operator] [specific fulfillment]`

| Variable              | Type        | Default value  |Description                     |
| -------------         |-------------| -----          | --------                       |
|Operator               |operator     | equals         | equals/does not equal          |
|Fulfillment method     |fulfillment method||indicates the fulfillment method compared using the configured operator|

> The qualification will not apply if no fulfillment has been chosen.

#### Cart has specific payment
Will apply the given benefit when the cart contains a specific payment method.

When a qualification is added to a promotion, the following condition will be available in the list:

`Cart payment method is [operator] [specific payment]`

| Variable              | Type        | Default value  |Description                     |
| -------------         |-------------| -----          | --------                       |
|Operator               |operator     | equals         | equals/does not equal          |
|Payment method         |payment method|               |indicates the payment method compared using the configured operator|

> The qualification will not apply if no payment has been chosen.

#### Products in a specific category in order history
Will apply the given benefit when the order history of the customer contains products in a specific category.

When a qualification is added to a promotion, the following conditions will be available in the list:

`Order history contains [compares] [specific value] products in the [specific category] category`

| Variable              | Type        | Default value  |Description                     |
| -------------         |-------------| -----          | --------                       |
|compares               |operator     |                |standard operators              |
|specific value         |integer      |                |indicates the number of products compared using the configured operator|
|specific               |category     |                |a fully qualified category path |
|Include sub categories |bool         | true           |indicates whether sub categories are included|

`Order history contains products in the [specific category] category for a total [compares] $ [specific value]`

| Variable              | Type        | Default value  |Description                     |
| -------------         |-------------| -----          | --------                       |
|compares               |operator     |                |standard operators              |
|specific value         |integer      |                |indicates the subtotal of products compared using the configured operator|
|specific               |category     |                |a fully qualified category path |
|Include sub categories |bool         | true           |indicates whether sub categories are included|

### Benefits

#### Get a free gift
Will add a free gift to the cart when the given qualification has been met.

When a benefit is added to a promotion, the following action will be available in the list:

`Get [quantity] free [gift]`

| Variable              | Type        | Default value  |Description                     |
| -------------         |-------------| -----          | --------                       |
|Product                | product     |                |the product to add as free gift |
|Quantity               | integer     |1               |the number of products to add   |

> The gift will be removed from the cart when the qualification is no longer met.

#### % discount on every N-th qualifying product
Will apply a percentage discount to a certain number of products when a cartain number of those products have been added to the cart.

When a benefit is added to a promotion, the following action will be available in the list:

`For every [Items to award] of [Items to purchase] products in [Category] you get [Percentage Off] on the [Apply Award To] with a limit of [Award Limit]`

| Variable              | Type        | Default value  |Description                     |
| -------------         |-------------| -----          | --------                       |
|Category               |category     |                |a fully qualified category path |
|Include sub categories |bool         |true            |indicates whether sub categories are included|
|Items to purchase      |integer      |                |indicates the number of items (N) to be puchased for the discount to be applied|
|Items to award         |integer      |                |indicates the number of items (X) the discount will be applied to|
|Percentage Off         |decimal      |                |the percentage to deduct from the item price|
|Award Limit            |integer      |                |the maximum number of times the benefit will be applied|
|Apply Award To         |option       |Least Expensive Items First |indicates whether the most of least expensive items will be awarded first: Most Expensive Items First/Least Expensive Items First|

The action will do the following:

1. Select the eligable items
2. Sort the items by most/least expensive
3. Calculate the number of times the discount should be applied 
4. Apply the discount to the most/least expensive items

> Cart line quantity is taken into account, meaning that a cart line with a quantity of 10 could have the discount applied twice, resulting in 2 discounted products and 8 at full price.

> Uses the same rounding algorithm as Sitecore uses in its benefits.

#### $ discount on every N-th qualifying product

Will apply a fixed amount discount to a number of products, when the specified minimum amount of products are added to the cart.

When a benefit is added to a promotion, the following action will be available in the list:

`For every [Items to award] of [Items to purchase] products in [Category] you get [Amount Off] on the [Apply Award To] with a limit of [Award Limit]`

| Variable              | Type        | Default value  |Description                     |
| -------------         |-------------| -----          | --------                       |
|Category               |category     |                |a fully qualified category path |
|Include sub categories |bool         |true            |indicates whether sub categories are included|
|Items to purchase      |integer      |                |indicates the number of items (N) to be puchased for the discount to be applied|
|Items to award         |integer      |                |indicates the number of items (X) the discount will be applied to|
|Amount Off             |decimal      |                |the amount to deduct from the item price|
|Award Limit            |integer      |                |the maximum number of times the benefit will be applied|
|Apply Award To         |option       |Least Expensive Items First |indicates whether the most of least expensive items will be awarded first: Most Expensive Items First/Least Expensive Items First|

The action will do the following:

1. Select the eligable items
2. Sort the items by most/least expensive
3. Calculate the number of times the discount should be applied 
4. Apply the discount to the most/least expensive items

> Cart line quantity is taken into account, meaning that a cart line with a quantity of 10 could have the discount applied twice, resulting in 2 discounted products and 8 at full price.

> Uses the same rounding algorithm as Sitecore uses in its benefits.

#### Get $ discount on shipping
Will deduct a fixed amount from the shipping costs when the given qualification is met.

When a benefit is added to a promotion, the following action will be available in the list:

`Get [specific amount] off the shipping cost`

| Variable              | Type        | Default value  |Description                     |
| -------------         |-------------| -----          | --------                       |
|Amount Off             |decimal      |                |the amount to deduct from the shipping cost|

#### $ discount on products in a specific category
Will apply a fixed amount discount to a number of products in a category, when a specified amount of products from that category have been added to the cart.

When a benefit is added to a promotion, the following action will be available in the list:

`When you buy [Operator] [Product count] products in [Category] you get [Amount off] per product (ordered by [apply award to]) with a maximum of [award limit] products`

| Variable              | Type        | Default value  |Description                     |
| -------------         |-------------| -----          | --------                       |
|Operator               |operator     |                |standard operators              |
|Product count          |integer      |                |indicates the numer of products compared using the configured operator|
|Category               |category     |                |a fully qualified category path|
|Include sub categories |bool         | true           |indicates whether sub categories are included|
|Amount Off             |decimal      |                |the amount to deduct from the product price|
|Award Limit            |integer      |                |the maximum number of products the benefit will be applied to|
|Apply Award To         |option       |Least Expensive Items First |indicates whether the most of least expensive items will be awarded first: Most Expensive Items First/Least Expensive Items First|

The action will do the following:

1. Select the eligable items
2. Sort the items by most/least expensive
3. Calculate the number of times the discount should be applied 
4. Apply the discount to the most/least expensive items

> Cart line quantity is taken into account, meaning that a cart line with a quantity of 10 could have the discount applied twice, resulting in 2 discounted products and 8 at full price.

> Uses the same rounding algorithm as Sitecore uses in its benefits.

#### % discount on products in a specific category
Will deduct a certain percentage of a certain number of products in a certain group, when a certain amount of products in that group have been added to the cart.

When a benefit is added to a promotion, the following action will be available in the list:

`When you buy [Operator] [specific value] products in [specific category] you get [Percentage off] per product (ordered by [apply award to]) with a maximum of [award limit] products`

| Variable              | Type        | Default value  |Description                     |
| -------------         |-------------| -----          | --------                       |
|Operator               |operator     |                |standard operators              |
|Product count          |integer      |                |indicates the numer of products compared using the configured operator|
|Category               |category     |                |a fully qualified category path|
|Include sub categories |bool         | true           |indicates whether sub categories are included|
|Percentage Off         |decimal      |                |the percentage to deduct from the product price|
|Award Limit            |integer      |                |the maximum number of products the benefit will be applied to|
|Apply Award To         |option       |Least Expensive Items First |indicates whether the most of least expensive items will be awarded first: Most Expensive Items First/Least Expensive Items First|

The action will do the following:

1. Select the eligable items
2. Sort the items by most/least expensive
3. Calculate the number of times the discount should be applied 
4. Apply the discount to the most/least expensive items

> Cart line quantity is taken into account, meaning that a cart line with a quantity of 10 could have the discount applied twice, resulting in 2 discounted products and 8 at full price.

> Uses the same rounding algorithm as Sitecore uses in its benefits.

## Resources

https://sitecoresmurf.wordpress.com/2019/07/18/known-issues-limitations-and-extending-promotion-plugin-in-sitecore-commerce-9/
Out of the box, sitecore doesn't allow boolean values in Conditions.
To change this behavior we made our own version of the BuildRuleSetBlock in which we called our own SitecoreExtensions.ConvertToConditionExtended and SitecoreExtensions.ConvertToActionExtended.
