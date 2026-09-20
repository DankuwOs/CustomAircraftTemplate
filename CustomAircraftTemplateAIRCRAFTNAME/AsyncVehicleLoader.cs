using System;
using System.Collections;
using CheeseMods.VTOLTaskProgressUI;
using UnityEngine;
using VTNetworking;

namespace CustomAircraftTemplateAIRCRAFTNAME.AircraftLoading;

public class AsyncVehicleLoader : MonoBehaviour
{
	public void LoadVehicle(string pathToBundle, string vehicleName)
	{
		StartCoroutine(LoadVehicleAsync(pathToBundle, vehicleName));
	}

	private IEnumerator LoadVehicleAsync(string pathToBundle, string vehicleName)
	{
		// This is all VTOLTaskProgressUI is used for, you can remove this if you want but its cool.
		var taskProgress = VTOLTaskProgressManager.RegisterTask(Main.instance, $"Loading {AircraftAPI.AircraftName}");

		var asyncOpStatus = VTResources.FinallyLoadExtVehicleAsync(pathToBundle, vehicleName);
		while (!asyncOpStatus.isDone)
		{
			taskProgress.SetProgress(asyncOpStatus.progress);
			yield return null;
		}

		taskProgress.FinishTask();

		foreach (var assetBundle in AssetBundle.GetAllLoadedAssetBundles())
		{
			if (assetBundle.name == "aircraftassetbundlefile") // rename! or remove all the code this is related to. 
			{
				AircraftAPI.aircraftBundle = assetBundle;
				break;
			}
		}

		AircraftAPI.pvAircraft = VTResources.GetPlayerVehicle(AircraftAPI.AircraftName);
		
		// This is all VTOLAPI is used for, you can remove this if you want but your shaders might be broken or slightly off.
		VTAPI.FixShaders(AircraftAPI.pvAircraft.vehiclePrefab, false, true, true);
		
		VTResources.LoadStandaloneLODInfos(false, false);
		var lodCampaign = VTResources.GenerateStandaloneCustomScenarios(AircraftAPI.AircraftName);

		AircraftAPI.pvAircraft.standaloneCustomScenarios = lodCampaign;

		var wm = AircraftAPI.pvAircraft.vehiclePrefab.GetComponent<WeaponManager>();
		try
		{
			foreach (var equipPrefab in AircraftAPI.pvAircraft.allEquipPrefabs)
			{
				//VTResources.RegisterOverriddenResource($"{wm.resourcePath}/{equipPrefab.name}", equipPrefab);
				//VTNetworkManager.RegisterOverrideResource($"{wm.resourcePath}/{equipPrefab.name}", equipPrefab);
				
				AircraftAPI.ResourcesToRemove.Add($"{wm.resourcePath}/{equipPrefab.name}");
				var equipMl = equipPrefab.GetComponent<HPEquipMissileLauncher>();

				if (equipMl && equipMl.ml?.missilePrefab != null)
				{
					var obj = equipMl.ml.missilePrefab;

					BuiltMixerFixer.FixAudioSourcesInChildren(obj);
					VTResources.RegisterOverriddenResource(equipMl.missileResourcePath, obj);
					VTNetworkManager.RegisterOverrideResource(equipMl.missileResourcePath, obj);

					var missileUnitID = obj.GetComponent<UnitIDIdentifier>();
					if (!missileUnitID)
					{
						missileUnitID = obj.AddComponent<UnitIDIdentifier>();
						missileUnitID.targetName = obj.name;
						missileUnitID.unitID = $"AircraftID.{obj.name}"; // Rename! just needs to be unique.
						missileUnitID.role = Actor.Roles.Missile;
						
					}
					// only register identities on new or modified missiles. UnitIDIdentifier on the missile probably needs to be changed
					if (missileUnitID.unitID.ToLower().Contains("aircraftid.")) // Rename!
					{
						TargetIdentity targetIdentity = TargetIdentityManager.RegisterNonSpawnIdentity($"AircraftID.{obj.name}", // Rename!
							missileUnitID.targetName,
							missileUnitID.role);
						if (!TargetIdentityManager.indexedIdentities.Contains(targetIdentity))
							TargetIdentityManager.indexedIdentities.Add(targetIdentity);
						
						AircraftAPI.aircraftIdentities.Add(targetIdentity);
					}


					AircraftAPI.ResourcesToRemove.Add(equipMl.missileResourcePath);
				}
			}
			
			AircraftAPI.RegisterIdentity();
		}
		catch (Exception e)
		{
			Debug.Log($"[AircraftAPI]: OOpsie ie exceptioned.. {e}");
			throw;
		}

		Debug.Log($"[AircraftAPI]: Got pvAircraft {AircraftAPI.pvAircraft}");
		
		if (AircraftAPI.aircraftBundle != null)
			AircraftAPI.aircraftBundle.Unload(false);
		
		Destroy(this);
	}
}