using System.Collections.Generic;
using System.Xml.Serialization;

namespace LoadOrderToolTwo.Domain.CompatibilityReport;

#nullable disable
// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
/// <remarks/>
[System.Serializable()]
[XmlRoot("CompatibilityReportAssetCatalog")]
[System.ComponentModel.DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public partial class AssetCatalog
{
	internal Dictionary<ulong, CompatibilityReportAssetCatalogAsset> AssetCatalogDictionary;
	private CompatibilityReportAssetCatalogAsset[] assetsField;

	/// <remarks/>
	[XmlArrayItem("Asset", IsNullable = false)]
	public CompatibilityReportAssetCatalogAsset[] Assets
	{
		get => this.assetsField;
		set => this.assetsField = value;
	}

	internal void CreateDictionary()
	{
		AssetCatalogDictionary = new();
		foreach (var item in this.Assets)
		{
			AssetCatalogDictionary[item.SteamID] = item;
		}
	}
}

/// <remarks/>
[System.Serializable()]
[System.ComponentModel.DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public partial class CompatibilityReportAssetCatalogAsset
{

	private uint steamIDField;

	private string nameField;

	private string stabilityField;

	/// <remarks/>
	public uint SteamID
	{
		get => this.steamIDField;
		set => this.steamIDField = value;
	}

	/// <remarks/>
	public string Name
	{
		get => this.nameField;
		set => this.nameField = value;
	}

	/// <remarks/>
	public string Stability
	{
		get => this.stabilityField;
		set => this.stabilityField = value;
	}
}
#nullable enable
