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

	public static List<TargetIdentity> aircraftIdentities = new List<TargetIdentity>(); 

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

		foreach (var targetIdentity in aircraftIdentities)
		{
			TargetIdentityManager.indexedIdentities.Remove(targetIdentity);
			TargetIdentityManager.identityDict.Remove(targetIdentity.targetId);
		}
		
		SortIdentities();
		
		aircraftBundle.Unload(true);
	}

	public static void RegisterIdentity()
	{
		var aircraftIdentity = TargetIdentityManager.RegisterNonSpawnIdentity(
			pvAircraft.vehicleName, pvAircraft.vehicleName,
			Actor.Roles.Air);
		
		if (!TargetIdentityManager.indexedIdentities.Contains(aircraftIdentity))
		{
			aircraftIdentity.index = TargetIdentityManager.indexedIdentities.Count;
			TargetIdentityManager.indexedIdentities.Add(aircraftIdentity);
		}
		
		aircraftIdentities.Add(aircraftIdentity);
		
		SortIdentities();
	}

	private static void SortIdentities()
	{
		TargetIdentityManager.indexedIdentities.Sort(TargetIdentityManager.IdentSorter);
		for (int j = 0; j < TargetIdentityManager.indexedIdentities.Count; j++)
		{
			TargetIdentityManager.indexedIdentities[j].index = j;
		}
	}
}
