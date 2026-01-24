using System.Collections;
using System.Diagnostics;
using CustomAircraftTemplateAIRCRAFTNAME.AircraftLoading;
using HarmonyLib;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CustomAircraftTemplateAIRCRAFTNAME.AircraftScripts.Patches.Base;

[HarmonyPatch(typeof(CampaignSelectorUI), nameof(CampaignSelectorUI.SetupCampaignScreenRoutine))]
public class CSUIPatch_SetupCampaignScreen
{
    public static bool Prefix(CampaignSelectorUI __instance, ref IEnumerator __result)
    {
        if (PilotSaveManager.currentVehicle.vehicleName != AircraftAPI.AircraftName)
            return true;

        __result = SetupCampaignScreenRoutine_Patch(__instance);
        
        return false;
    }

    public static IEnumerator SetupCampaignScreenRoutine_Patch(CampaignSelectorUI __instance)
    {
	    __instance.loadingCampaignScreenObj.SetActive(true);
	    
	    var wasInputEnabled = !ControllerEventHandler.eventsPaused;
	    
	    ControllerEventHandler.PauseEvents();
	    
	    VTScenarioEditor.returnToEditor = false;
	    VTMapManager.nextLaunchMode = VTMapManager.MapLaunchModes.Scenario;
	    PlayerVehicleSetup.godMode = false;
	    
	    __instance.campaignDisplayObject.SetActive(true);
	    __instance.scenarioDisplayObject.SetActive(false);
	    
	    if (__instance.campaignsParent)
	    {
		    Object.Destroy(__instance.campaignsParent.gameObject);
	    }

	    __instance.campaignsParent = new GameObject("campaigns").transform;
	    __instance.campaignsParent.parent = __instance.campaignTemplate.transform.parent;
	    __instance.campaignsParent.localPosition = __instance.campaignTemplate.transform.localPosition;
	    __instance.campaignsParent.localRotation = Quaternion.identity;
	    __instance.campaignsParent.localScale = Vector3.one;
	    __instance.campaignWidth = ((RectTransform)__instance.campaignTemplate.transform).rect.width;
	    
	    var stopwatch = new Stopwatch();
	    stopwatch.Start();
	    __instance.campaigns.Clear();
	    
	    foreach (var builtInCampaign in VTResources.builtInCampaignsLOD.campaigns)
	    {
		    var pv = VTResources.GetPlayerVehicle(builtInCampaign.vehicle);
		    if (pv.dlc && !pv.IsDLCOwned())
			    continue;
		    
		    if (!builtInCampaign.hideFromMenu)
		    {
			    __instance.campaigns.Add(builtInCampaign);
		    }
	    }

	    stopwatch.Stop();
	    Debug.Log("Time loading BuiltInCampaigns: " + stopwatch.ElapsedMilliseconds);
	    stopwatch.Reset();
	    
	    __instance.campaigns.Add(PilotSaveManager.currentVehicle.standaloneCustomScenarios);
	    
	    VTResources.LoadAllCustomCampaignsLOD();
	    VTResources.GetCustomCampaignsLOD(__instance.customsCampaigns, PilotSaveManager.currentVehicle.vehicleName);
	    
	    foreach (var customCampaign in __instance.customsCampaigns)
	    {
		    __instance.campaigns.Add(customCampaign);
	    }

	    for (int i = 0; i < __instance.campaigns.Count; i++)
	    {
		    var gameObject = Object.Instantiate(__instance.campaignTemplate, __instance.campaignsParent);
		    gameObject.transform.localPosition += __instance.campaignWidth * i * Vector3.right;
		    
		    var component = gameObject.GetComponent<CampaignInfoUI>();
		    component.campaignImage.texture = __instance.noImage;
		    component.UpdateDisplay(__instance.campaigns[i], PilotSaveManager.currentVehicle.vehicleName);
		    
		    gameObject.SetActive(true);
		    
		    yield return null;
	    }

	    __instance.campaignIdx = Mathf.Clamp(__instance.campaignIdx, 0, __instance.campaigns.Count - 1);
	    __instance.campaignTemplate.SetActive(false);
	    
	    __instance.SetupCampaignList();
	    
	    __instance.loadingCampaignScreenObj.SetActive(false);
	    
	    if (wasInputEnabled)
	    {
		    ControllerEventHandler.UnpauseEvents();
	    }
    }
}