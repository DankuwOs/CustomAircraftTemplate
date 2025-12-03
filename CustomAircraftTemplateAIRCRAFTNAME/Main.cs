using System.IO;
using System.Reflection;
using ModLoader.Framework;
using ModLoader.Framework.Attributes;

namespace CustomAircraftTemplateAIRCRAFTNAME;

[ItemId("yourname-aircraftname")] // Rename!
public class Main : VtolMod
{
	public static Main instance;

	public static string pathToBundle;

	public string ModFolder; 

	public void Start()
	{
		instance = this;
		ModFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		pathToBundle = Path.Combine(ModFolder, "aircraftassetbundlefilename"); // Rename! Should be the full file name including the extension if it has one.
		
		VTResources.OnLoadingPlayerVehicles += AircraftAPI.VehicleAdd;
		AircraftAPI.VehicleAdd();
	}

	public override void UnLoad()
	{
		AircraftAPI.VehicleRemove();
	}
}