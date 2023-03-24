using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace LoadOrderToolTwo.ColossalOrder;

public enum DLC
{
	None = 0,

	[DLCInfo("After Dark", DLCType.Main)]
	AfterDarkDLC = 369150,

	[DLCInfo("Snow Fall", DLCType.Main)]
	SnowFallDLC = 420610,

	[DLCInfo("Natural Disasters", DLCType.Main)]
	NaturalDisastersDLC = 515191,

	[DLCInfo("Mass Transit", DLCType.Main)]
	InMotionDLC = 547502,

	[DLCInfo("Green Cities", DLCType.Main)]
	GreenCitiesDLC = 614580,

	[DLCInfo("Parklife", DLCType.Main)]
	ParksDLC = 715191,

	[DLCInfo("Industries", DLCType.Main)]
	IndustryDLC = 715194,

	[DLCInfo("Campus", DLCType.Main)]
	CampusDLC = 944071,

	[DLCInfo("Sunset Harbor", DLCType.Main)]
	UrbanDLC = 1146930,

	[DLCInfo("Airports", DLCType.Main)]
	AirportDLC = 1726380,

	[DLCInfo("Plazas & Promenades", DLCType.Main)]
	PlazasAndPromenadesDLC = 2008400,

	[DLCInfo("Financial Districts", DLCType.Main)]
	FinancialDistrictsDLC = 2148901,

	[DLCInfo("Pearls From the East", DLCType.Misc)]
	OrientalBuildings = 563850,

	[DLCInfo("Match Day (Football)", DLCType.Misc)]
	Football = 456200,

	[DLCInfo("Concerts (music festivals)", DLCType.Misc)]
	MusicFestival = 614581,

	[DLCInfo("Carols, Candles and Candy (Christmas)", DLCType.Misc)]
	Christmas = 715192,

	[DLCInfo("Deluxe Pack", DLCType.Misc)]
	DeluxeDLC = 346791,

	[DLCInfo("Music DLCs", DLCType.Misc)]
	MusicDLCs = 547501,

	[DLCInfo("CCP: Art Deco", DLCType.ContentCreator)]
	ModderPack1 = 515190,

	[DLCInfo("CCP: High-Tech", DLCType.ContentCreator)]
	ModderPack2 = 547500,

	[DLCInfo("CCP: European Suburbia", DLCType.ContentCreator)]
	ModderPack3 = 715190,

	[DLCInfo("CCP: University City", DLCType.ContentCreator)]
	ModderPack4 = 1059820,

	[DLCInfo("CCP: Modern City Center", DLCType.ContentCreator)]
	ModderPack5 = 1148020,

	[DLCInfo("CCP: Modern Japan", DLCType.ContentCreator)]
	ModderPack6 = 1148022,

	[DLCInfo("CCP: Train Stations", DLCType.ContentCreator)]
	ModderPack7 = 1531470,

	[DLCInfo("CCP: Bridges & Piers", DLCType.ContentCreator)]
	ModderPack8 = 1531471,

	[DLCInfo("CCP: Map Pack", DLCType.ContentCreator)]
	ModderPack9 = 1726382,

	[DLCInfo("CCP: Vehicles of the World", DLCType.ContentCreator)]
	ModderPack10 = 1726381,

	[DLCInfo("CCP: Mid-Century Modern", DLCType.ContentCreator)]
	ModderPack11 = 1992290,

	[DLCInfo("CCP: Seaside Resorts", DLCType.ContentCreator)]
	ModderPack12 = 1992291,

	[DLCInfo("CCP: Skyscrapers", DLCType.ContentCreator)]
	ModderPack13 = 2144481,

	[DLCInfo("CCP: Heart of Korea", DLCType.ContentCreator)]
	ModderPack14 = 2144480,

	[DLCInfo("CCP: Map Pack 2", DLCType.ContentCreator)]
	ModderPack15 = 2148903,
}

#if TOOL || TOOL2
public static class Extensions
{
	public static DLCInfoAttribute GetDLCInfo(this DLC dlc)
	{
		var member = typeof(DLC).GetMember(dlc.ToString())[0];
		return member.GetCustomAttribute<DLCInfoAttribute>();
	}
}
#endif

[Flags]
public enum DLCType
{
	None = 0,
	Main = 1,
	ContentCreator = 2,
	Misc = 4,
}

public class DLCInfoAttribute : Attribute
{
	public string Text;
	public DLCType Type;
	public DLCInfoAttribute(string text, DLCType type)
	{
		Text = text;
		Type = type;
	}
}
