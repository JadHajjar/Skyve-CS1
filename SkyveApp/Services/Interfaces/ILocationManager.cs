using Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Services.Interfaces;
public interface ILocationManager
{
	string AddonsPath { get; }
	string AppDataPath { get; set; }
	string AssetsPath { get; }
	string CitiesPathWithExe { get; }
	string DataPath { get; }
	string GameContentPath { get; }
	string GamePath { get; set; }
	string ManagedDLL { get; }
	string MapsPath { get; }
	string MapThemesPath { get; }
	string ModsPath { get; }
	string MonoPath { get; }
	string SkyveAppDataPath { get; }
	string SkyveProfilesAppDataPath { get; }
	string SteamPath { get; set; }
	string SteamPathWithExe { get; }
	string StylesPath { get; }
	string WorkshopContentPath { get; }

	void RunFirstTimeSetup();
}
