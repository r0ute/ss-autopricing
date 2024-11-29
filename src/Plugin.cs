using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MyBox;
using TMPro;

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
        static void OnSetup(Pricing data, ref PricingItem __instance, ref bool __runOriginal)
        {
            var product = Singleton<IDManager>.Instance.ProductSO(data.ProductID);
            var purchaseChance = Singleton<PriceEvaluationManager>.Instance.PurchaseChance(data.ProductID);

            var currentCost = Singleton<PriceManager>.Instance.CurrentCost(data.ProductID);
            var maxCost = (float)Math.Round(currentCost + currentCost * product.MaxProfitRate / 100f, 2);

            var avgCost = Traverse.Create(__instance).Field("m_AvgCostText").GetValue() as TMP_Text;
            avgCost.text = purchaseChance.ToString("0.##\\%");

            var mktPrice = Traverse.Create(__instance).Field("m_MarketPriceText").GetValue() as TMP_Text;
            mktPrice.text = string.Format("{0}</size>..{1}", data.MarketPrice.ToMoneyText(mktPrice.fontSize),
                maxCost.ToMoneyText(mktPrice.fontSize));

        }



    }
}
