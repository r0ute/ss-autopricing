using System;
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

    internal static ConfigEntry<KeyboardShortcut> ForcepdatePricingDataKey;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;

        AutoUpdatePricingData = Config.Bind("General", "AutoUpdatePricingData", true, "Enable to update store prices on day cycle change and licence purchase");

        ForcepdatePricingDataKey = Config.Bind("Key Bindings", "ForcepdatePricingDataKey",
                new KeyboardShortcut(KeyCode.E, KeyCode.LeftControl));

        Harmony harmony = new(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll(typeof(Patches));

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }


    class Patches
    {

        [HarmonyPatch(typeof(PricingProductViewer), nameof(PricingProductViewer.UpdateUnlockedProducts))]
        [HarmonyPostfix]
        static void OnUpdateUnlockedProducts(int licenseID)
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
            var maxCost = (float)Math.Round(currentCost + currentCost * product.MaxProfitRate / 100f, 2);
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
                var currentCost = Singleton<PriceManager>.Instance.CurrentCost(data.ProductID);
                var product = Singleton<IDManager>.Instance.ProductSO(data.ProductID);
                var newPrice = (((product.MaxProfitRate - product.OptimumProfitRate) * 0.1f + product.OptimumProfitRate) / 100f + 1f) * currentCost;
                
                if (!Mathf.Approximately(data.Price, newPrice)) {
                    Logger.LogDebug($"Update price: product={Singleton<IDManager>.Instance.ProductSO(data.ProductID).name},oldPrice={data.Price},newPrice={newPrice}");
                    Singleton<PriceManager>.Instance.PriceSet(new Pricing(data.ProductID, newPrice));
                }
                
            });

            if (!auto)
            {
                Singleton<SFXManager>.Instance.PlayCoinSFX();
            }

            Logger.LogInfo($"Pricing data update finished: auto={auto}");

        }


    }
}
