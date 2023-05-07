using System.Collections.Generic;

namespace LoadOrderToolTwo.Domain.Compatibility;
public class CompatibilityData
{
	public List<Package>? Packages { get; set; }
	public List<Author>? Authors { get; set; }
	public List<ulong>? BlackListedIds { get; set; }
	public List<string>? BlackListedNames { get; set; }

	//internal static readonly ulong[] BlackList = new ulong[]
	//{
	//	2620852727,
	//	2448824112,
	//};

	//internal static readonly string[] BlackListNames = new string[]
	//{
	//	"ali213_mod_01"
	//};
}
