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

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;

        Harmony harmony = new(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll(typeof(Patches));

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    class Patches
    {


        [HarmonyPatch(typeof(PricingItem), nameof(PricingItem.Setup))]
        [HarmonyPostfix]
        static void OnPricingItemSetup(Pricing data, ref PricingItem __instance)
        {
            var avgCostText = Traverse.Create(__instance).Field("m_AvgCostText").GetValue() as TMP_Text;
            avgCostText.text = string.Format("{0} / {1}",
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


    }
}
