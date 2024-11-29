# Supermarket Simulator Automatic Pricing BepInEx Plugin

![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/r0ute/ss-autopricing/dotnet.yml)
![GitHub last commit](https://img.shields.io/github/last-commit/r0ute/ss-autopricing)
![GitHub Release Date](https://img.shields.io/github/release-date/r0ute/ss-autopricing)

## Overview

This BepInEx plugin for _Supermarket Simulator_ simplifies pricing strategies by dynamically adjusting customer purchase chances.

### Main Features

- Automatically adjusts product pricing based on customer purchase chances.
- Dynamically calculates purchase likelihood based on pricing relative to Market Price.
- Configurable settings for fine-tuning pricing and maximizing profits.
- Works seamlessly with the in-game Pricing Viewer screen.

## Why?

This is the only plugin that takes into account Product Buy Chance. Other plugins simply mark up prices for all products by a certain amount or percentage, which has nothing to do with how the game decides when customers buy products.

## User Guide

### New Players Who Haven't Unlocked Cashier Yet

It's recommended to disable the `AutoUpdatePricingData` option. Use the `Ctrl+E` combination to initially set the prices, and then round them to the nearest dollar unless you're enjoying dealing with pennies.

### Once You Acquire Cashier or Self-Checkout

Setting `ProfitRateBoost` to 10% can be used at the start, and later increased to sacrifice store points over profit.

---

## What's the Chance of a Customer Buying a Product?

The game calculates the customer's chance of purchasing a product using the following method:

1. **Selling at a Loss:**  
   If you sell a product at a loss, the customer purchases it with a **200% chance** (guaranteed).

2. **Selling Below Market Price:**  
   If you sell a product for less than the Market Price, the chance of purchase varies between **100% and 200%**, following this curve:

   `m_PurchaseChanceCurveForCheapPrice` (AnimationCurve):

   - `[length=2]`
   - `value=200, time=0`
   - `value=100, time=1`

3. **Selling Above Market Price but Below Maximum Price:**  
   If you sell a product for more than the Market Price but less than the Maximum Price (as shown in the Pricing Viewer screen), the purchase chance ranges between **0% and 100%**, based on the curve:

   `m_PurchaseChanceCurveForExpensivePrice` (AnimationCurve):

   - `[length=3]`
   - `value=100, time=0`
   - `value=100, time=0.1`
   - `value=0, time=1`

   **Note:** Based on this curve, if the _ProfitRateBoost_ mod setting is between **0% and 10%**, customers always purchase the product.

4. **Selling Above Maximum Price:**  
   Any price set above the Maximum Price results in **no sales**.

---

### Correlation between Profit Ratio Boost and Product Purchase Chance

| Profit Ratio Boost, % | Product Purchase Chance, % |
| --------------------: | -------------------------: |
|                  0-10 |                        100 |
|                    25 |                        ~90 |
|                    55 |                        ~66 |
|                   100 |                          0 |