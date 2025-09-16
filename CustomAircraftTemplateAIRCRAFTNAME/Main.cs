using System.IO;
using System.Reflection;
using ModLoader.Framework;
using ModLoader.Framework.Attributes;

namespace CustomAircraftTemplateAIRCRAFTNAME;

[ItemId("yourname-aircraftname")] // Rename!
public class Main : VtolMod
{
	public static Main Instance { get; private set; }

	public static string PathToBundle { get; private set; }

	public string ModFolder { get; private set; } 

	public void Start()
	{
		Instance = this;
		ModFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		PathToBundle = Path.Combine(ModFolder, "aircraftassetbundlefile.whatever"); // Rename! doesnt need an extension.
		
		VTResources.OnLoadingPlayerVehicles += AircraftAPI.VehicleAdd;
		AircraftAPI.VehicleAdd();
	}

	public override void UnLoad()
	{
		AircraftAPI.VehicleRemove();
	}
}