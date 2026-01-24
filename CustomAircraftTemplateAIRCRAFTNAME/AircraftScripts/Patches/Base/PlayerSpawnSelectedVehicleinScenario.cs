using CustomAircraftTemplateAIRCRAFTNAME.AircraftLoading;
using HarmonyLib;

namespace CustomAircraftTemplateAIRCRAFTNAME.AircraftScripts.Patches.Base;

[HarmonyPatch(typeof(PlayerSpawn), "PlayerSpawnRoutine")]
public class PlayerSpawnSelectedVehicleinScenario
{
	private static bool Prefix(PlayerSpawn __instance)
	{
		if (PilotSaveManager.currentVehicle.vehicleName != AircraftAPI.AircraftName)
		{
			return true;
		}
		VTScenario.current.vehicle = PilotSaveManager.currentVehicle;
		return true;
	}
}
