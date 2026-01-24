using CustomAircraftTemplateAIRCRAFTNAME.AircraftLoading;
using HarmonyLib;

namespace CustomAircraftTemplateAIRCRAFTNAME.AircraftScripts.Patches.Base;

[HarmonyPatch(typeof(LoadoutConfigurator), "Initialize")]
public class LoadInAircraftsWeapons
{
	public static void Prefix(LoadoutConfigurator __instance)
	{
		if (PilotSaveManager.currentVehicle.vehicleName != AircraftAPI.AircraftName)
		{
			return;
		}
		__instance.availableEquipStrings.Clear();
		foreach (string equipNames in PilotSaveManager.currentVehicle.GetEquipNamesList())
		{
			__instance.availableEquipStrings.Add(equipNames);
		}
	}
}
