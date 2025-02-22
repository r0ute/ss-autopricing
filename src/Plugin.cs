﻿using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using MyBox;
using TMPro;
using UnityEngine;

namespace SS.src;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    internal static ConfigEntry<bool> AutoUpdatePricingData;

    internal static ConfigEntry<int> ProfitRateBoost;

    internal static ConfigEntry<KeyboardShortcut> ForcepdatePricingDataKey;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;

        AutoUpdatePricingData = Config.Bind("General", "AutoUpdatePricingData", true, "Enable to update store prices on day cycle change and licence purchase");

        ProfitRateBoost = Config.Bind("General", "ProfitRateBoost,%", 10, new ConfigDescription(
            @"Adjust the profit rate coefficient:
            At 0%, you make no profit, but customers are happy;
            at 10%, customers always buy products, and sales remain profitable;
            at 11%, customers start complaining about product prices;
            at 100%, customers will never buy products.", new AcceptableValueRange<int>(0, 100)));

        ForcepdatePricingDataKey = Config.Bind("Key Bindings", "ForceUpdatePricingDataKey",
                new KeyboardShortcut(KeyCode.E, KeyCode.LeftControl));

        Harmony harmony = new(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll(typeof(Patches));

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }


    class Patches
    {

        [HarmonyPatch(typeof(ProductLicenseManager), nameof(ProductLicenseManager.PurchaseLicense))]
        [HarmonyPostfix]
        static void OnPurchaseLicense(int licenseID)
        {
            UpdatePrices();
        }

        [HarmonyPatch(typeof(DayCycleManager), nameof(DayCycleManager.StartNextDay))]
        [HarmonyPostfix]
        static void OnNextDay()
        {
            UpdatePrices();
        }

        [HarmonyPatch(typeof(DayCycleManager), "Start")]
        [HarmonyPostfix]
        static void OnDayStart()
        {
            UpdatePrices();
        }

        [HarmonyPatch(typeof(DayCycleManager), "Update")]
        [HarmonyPostfix]
        static void OnDayUpdate()
        {
            if (ForcepdatePricingDataKey.Value.IsDown())
            {
                UpdatePrices(false);
            }
        }


        [HarmonyPatch(typeof(PricingItem), nameof(PricingItem.Setup))]
        [HarmonyPostfix]
        static void OnPricingItemSetup(Pricing data, ref PricingItem __instance)
        {
            var avgCostText = Traverse.Create(__instance).Field("m_AvgCostText").GetValue() as TMP_Text;
            avgCostText.text = string.Format("{0}</size> / {1}",
                data.AvgCost.ToMoneyText(avgCostText.fontSize),
                Singleton<PriceEvaluationManager>.Instance.PurchaseChance(data.ProductID)
                    .ToString("0.##\\%"));


            var marketPriceText = Traverse.Create(__instance).Field("m_MarketPriceText").GetValue() as TMP_Text;
            var currentCost = Singleton<PriceManager>.Instance.CurrentCost(data.ProductID);
            var product = Singleton<IDManager>.Instance.ProductSO(data.ProductID);
            var maxCost = RoundPriceDown(currentCost + currentCost * product.MaxProfitRate / 100);
            marketPriceText.text = string.Format("{0}</size>..{1}",
                data.MarketPrice.ToMoneyText(marketPriceText.fontSize),
                maxCost.ToMoneyText(marketPriceText.fontSize));

        }

        private static void UpdatePrices(bool auto = true)
        {
            if (auto && !AutoUpdatePricingData.Value)
            {
                return;
            }

            Singleton<PriceManager>.Instance.pricingDatas.ForEach((data) =>
            {
                if (!Singleton<ProductLicenseManager>.Instance.UnlockedProducts.Contains(data.ProductID))
                {
                    return;
                }

                var product = Singleton<IDManager>.Instance.ProductSO(data.ProductID);
                var currentCost = Singleton<PriceManager>.Instance.CurrentCost(data.ProductID);

                var newPrice = RoundPriceDown((((product.MaxProfitRate - product.OptimumProfitRate)
                    * ProfitRateBoost.Value / 100 + product.OptimumProfitRate) / 100 + 1) * currentCost);
                var oldPrice = Singleton<PriceManager>.Instance.SellingPrice(data.ProductID);

                if (!Mathf.Approximately(oldPrice, newPrice))
                {
                    Singleton<PriceManager>.Instance.PriceSet(new Pricing(data.ProductID, newPrice));
                    Logger.LogDebug($"Update price: product={product.name},oldPrice={oldPrice},newPrice={newPrice},profitRateBoost={ProfitRateBoost.Value},purchaseChance={Singleton<PriceEvaluationManager>.Instance.PurchaseChance(data.ProductID)}");
                }

            });

            if (!auto)
            {
                Singleton<SFXManager>.Instance.PlayCoinSFX();
            }

            Logger.LogInfo($"Pricing data update finished: auto={auto}");

        }

        private static float RoundPriceDown(float price)
        {
            var priceStr = price.ToString("0.000");
            var result = float.Parse(priceStr[..^1]);
            return result;
        }

    }
}
