using CustomAircraftTemplateAIRCRAFTNAME.AircraftLoading;
using HarmonyLib;

namespace CustomAircraftTemplateAIRCRAFTNAME.AircraftScripts.Patches.Base;

[HarmonyPatch(typeof(PlayerSpawn), "OnPreSpawnUnit")]
public class OnPreSpawnSelectedVehicleinScenario
{
	private static bool Prefix()
	{
		if (PilotSaveManager.currentVehicle.vehicleName != AircraftAPI.AircraftName)
		{
			return true;
		}
		VTScenario.current.vehicle = PilotSaveManager.currentVehicle;
		return true;
	}
}
