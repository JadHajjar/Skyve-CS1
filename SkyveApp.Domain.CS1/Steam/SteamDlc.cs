using Newtonsoft.Json;

using SkyveApp.Domain.Systems;

using System;
using System.Drawing;

namespace SkyveApp.Domain.CS1.Steam;
public class SteamDlc : IDlcInfo
{
	public uint Id { get; set; }
	public string? Name { get; set; }
	public string? Description { get; set; }
	public DateTime ReleaseDate { get; set; }
	[JsonIgnore] public string ThumbnailUrl => $"https://cdn.akamai.steamstatic.com/steam/apps/{Id}/header.jpg";
	[JsonIgnore] public Bitmap? Thumbnail => ServiceCenter.Get<IImageService>().GetImage(ThumbnailUrl, true, $"{Id}.png").Result;
	[JsonIgnore]
	public bool IsIncluded
	{
		get => !ServiceCenter.Get<IDlcManager>().IsDlcExcluded(Id);
		set => ServiceCenter.Get<IAssetUtil>().SetDlcExcluded(Id, !value);
	}
}
