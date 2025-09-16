using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using VTNetworking;
using CheeseMods.VTOLTaskProgressUI;

namespace CustomAircraftTemplateAIRCRAFTNAME;

internal class AircraftAPI 
{
	public const string AircraftName = "AircraftName";

	public static PlayerVehicle PvAircraft { get; private set; }

	public static AssetBundle AircraftBundle { get; private set; }

    private static List<string> ResourcesToRemove = new List<string>();

    public static void VehicleAdd()
	{
		var go = new GameObject("AsyncVehicleLoader");
		UnityEngine.Object.DontDestroyOnLoad(go);
		go.AddComponent<AsyncVehicleLoader>().LoadVehicle(Main.PathToBundle, "PrefabName"); // Rename!
	}

	// I do not know if this code works.
	public static void VehicleRemove()
	{
		foreach (var resourcePath in ResourcesToRemove)
		{
			VTResources.ResetOverriddenResource(resourcePath);
			VTNetworkManager.overriddenResources.Remove(resourcePath);
		}
		
		VTResources.ResetOverriddenResource(PvAircraft.resourcePath);
		VTNetworkManager.overriddenResources.Remove(PvAircraft.resourcePath);

		VTResources.finalPVList.Remove(PvAircraft);
		VTResources.pvDict.Remove(PvAircraft.vehicleName);
		VTResources.loadedExternalVehicles.Remove(PvAircraft.vehiclePrefab.GetComponent<ExternalVehicleInfo>());
		
		AircraftBundle.Unload(true);
	}

    public static void AddResourceToRemove(string resourcePath)
    {
        ResourcesToRemove.Add(resourcePath);
    }

    class AsyncVehicleLoader : MonoBehaviour
    {
        public void LoadVehicle(string pathToBundle, string vehicleName)
        {
            StartCoroutine(LoadVehicleAsync(pathToBundle, vehicleName));
        }

        private IEnumerator LoadVehicleAsync(string pathToBundle, string vehicleName)
        {
            // This is all VTOLTaskProgressUI is used for, you can remove this if you want but its cool.
            var taskProgress = VTOLTaskProgressManager.RegisterTask(Main.Instance, $"Loading {AircraftAPI.AircraftName}");

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
                    AircraftAPI.AircraftBundle = assetBundle;
                    break;
                }
            }

            AircraftAPI.PvAircraft = VTResources.GetPlayerVehicle(AircraftAPI.AircraftName);

            TargetIdentity aircraftIdentity = TargetIdentityManager.RegisterNonSpawnIdentity(
                AircraftAPI.PvAircraft.vehicleName, AircraftAPI.PvAircraft.vehicleName,
                Actor.Roles.Air);
            if (!TargetIdentityManager.indexedIdentities.Contains(aircraftIdentity))
                TargetIdentityManager.indexedIdentities.Add(aircraftIdentity);

            VTResources.LoadStandaloneLODInfos(false, false);
            var lodCampaign = VTResources.GenerateStandaloneCustomScenarios(AircraftAPI.AircraftName);

            AircraftAPI.PvAircraft.standaloneCustomScenarios = lodCampaign;

            var wm = AircraftAPI.PvAircraft.vehiclePrefab.GetComponent<WeaponManager>();
            try
            {
                foreach (var equipPrefab in AircraftAPI.PvAircraft.allEquipPrefabs)
                {
                    //VTResources.RegisterOverriddenResource($"{wm.resourcePath}/{equipPrefab.name}", equipPrefab);
                    //VTNetworkManager.RegisterOverrideResource($"{wm.resourcePath}/{equipPrefab.name}", equipPrefab);

                    AircraftAPI.AddResourceToRemove($"{wm.resourcePath}/{equipPrefab.name}");
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
                                missileUnitID.unitID,
                                missileUnitID.role);
                            if (!TargetIdentityManager.indexedIdentities.Contains(targetIdentity))
                                TargetIdentityManager.indexedIdentities.Add(targetIdentity);
                        }


                        AircraftAPI.AddResourceToRemove(equipMl.missileResourcePath);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log($"[AircraftAPI]: OOpsie ie exceptioned.. {e}");
                throw;
            }

            Debug.Log($"[AircraftAPI]: Got pvAircraft {AircraftAPI.PvAircraft}");

            if (AircraftAPI.AircraftBundle != null)
                AircraftAPI.AircraftBundle.Unload(false);

            Destroy(this);
        }
    }
}
