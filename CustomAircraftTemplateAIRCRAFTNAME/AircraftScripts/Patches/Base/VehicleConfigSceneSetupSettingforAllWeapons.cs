using CustomAircraftTemplateAIRCRAFTNAME.AircraftLoading;
using HarmonyLib;

namespace CustomAircraftTemplateAIRCRAFTNAME.AircraftScripts.Patches.Base;

[HarmonyPatch(typeof(VehicleConfigSceneSetup), "Start")]
public class VehicleConfigSceneSetupSettingforAllWeapons
{
	private static bool Prefix()
	{
		if (PilotSaveManager.currentVehicle.vehicleName != AircraftAPI.AircraftName)
		{
			return true;
		}
		PilotSaveManager.currentCampaign.isStandaloneScenarios = false;
		return true;
	}
}
