using System.Collections.Generic;
using UnityEngine;
using VTNetworking;

namespace CustomAircraftTemplateAIRCRAFTNAME.AircraftLoading;

internal class AircraftAPI 
{
	public const string AircraftName = "AircraftName";

	public static PlayerVehicle pvAircraft;

	public static List<string> ResourcesToRemove = new List<string>();

	public static AssetBundle aircraftBundle;

	public static void VehicleAdd()
	{
		var go = new GameObject("AsyncVehicleLoader");
		UnityEngine.Object.DontDestroyOnLoad(go);
		go.AddComponent<AsyncVehicleLoader>().LoadVehicle(Main.pathToBundle, "PrefabName"); // Rename!
	}

	// I do not know if this code works.
	public static void VehicleRemove()
	{
		foreach (var resourcePath in ResourcesToRemove)
		{
			VTResources.ResetOverriddenResource(resourcePath);
			VTNetworkManager.overriddenResources.Remove(resourcePath);
		}
		
		VTResources.ResetOverriddenResource(pvAircraft.resourcePath);
		VTNetworkManager.overriddenResources.Remove(pvAircraft.resourcePath);

		VTResources.finalPVList.Remove(pvAircraft);
		VTResources.pvDict.Remove(pvAircraft.vehicleName);
		VTResources.loadedExternalVehicles.Remove(pvAircraft.vehiclePrefab.GetComponent<ExternalVehicleInfo>());
		
		aircraftBundle.Unload(true);
	}
}
