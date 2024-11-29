# RackAutoFill

## BoxSize

| Name | Dimensions | Cubic Volume | Description             |
| ---- | ---------- | -----------: | ----------------------- |
| S-C  | 8x8x8      |          512 | Small Cube              |
| S-R  | 20x10x7    |         1400 | Small Rectangular       |
| M-CM | 15x15x15   |         3375 | Medium Compact Cube     |
| M-FR | 20x20x10   |         4000 | Medium Flat Rectangular |
| M-C  | 20x20x20   |         8000 | Medium Cube             |
| L-R  | 30x20x20   |        12000 | Large Rectangular       |
| XL-R | 40x26x26   |        26080 | Extra Large Rectangular |

## DisplayType

| Abbreviation | Display Type | Color (HTML Code)                                         |
| ------------ | ------------ | --------------------------------------------------------- |
| F            | FREEZER      | <span style="color:#A3D8FF">Icy Blue (#A3D8FF)</span>     |
| R            | FRIDGE       | <span style="color:#B3E0E5">Pale Cyan (#B3E0E5)</span>    |
| C            | CRATE        | <span style="color:#D1A15A">Wooden Brown (#D1A15A)</span> |
| S            | SHELF        | <span style="color:#D3D3D3">Neutral Gray (#D3D3D3)</span> |

## ProductCategory

| Abbreviation | Product Category | Color (HTML Code)                                            |
| ------------ | ---------------- | ------------------------------------------------------------ |
| E            | EDIBLE           | <span style="color:#A3D77A">Leafy Green (#A3D77A)</span>     |
| D            | DRINK            | <span style="color:#67BFFF">Refreshing Blue (#67BFFF)</span> |
| C            | CLEANING         | <span style="color:#F0F8FF">Clean White (#F0F8FF)</span>     |
| B            | BOOK             | <span style="color:#C2B29A">Calming Brown (#C2B29A)</span>   |

## What's the Chance of a Customer Buying a Product?

The game calculates the customer's chance of purchasing a product using the following method:

1. **Selling at a Loss:**  
   If you sell a product at a loss, the customer purchases it with a **200% chance** (guaranteed).

2. **Selling Below Market Price:**  
   If you sell a product for less than the Market Price, the chance of purchase varies between **100% and 200%**, following the curve:

   ### `m_PurchaseChanceCurveForCheapPrice` (AnimationCurve):

   - `[length=2]`
   - `value=200, time=0`
   - `value=100, time=1`

3. **Selling Above Market Price but Below Maximum Price:**  
   If you sell a product for more than the Market Price but less than the Maximum Price (as shown in the Pricing Viewer screen), the purchase chance ranges between **0% and 100%**, based on the curve:

   ### `m_PurchaseChanceCurveForExpensivePrice` (AnimationCurve):

   - `[length=3]`
   - `value=100, time=0`
   - `value=100, time=0.1`
   - `value=0, time=1`

   **Note:** Based on the curve above, if the _ProfitRateBoost_ mod setting is between **0% and 10%**, customers always purchase the product.

4. **Selling Above Maximum Price:**  
   Any price set above the Maximum Price results in **no sales**.

## Buy Chance

| Profit Ratio Boost, % | Product Purchase Chance, % |
| --------------------: | -------------------------: |
|                  0-10 |                        100 |
|                    25 |                        ~90 |
|                    55 |                        ~66 |
|                   100 |                          0 |