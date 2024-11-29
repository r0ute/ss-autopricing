# Supermarket Simulator Automatic Pricing BepInEx Plugin

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