using Newtonsoft.Json;

using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using System;
using System.Drawing;

namespace SkyveApp.Domain.Steam;
public class SteamDlc
{
	public uint Id { get; set; }
	public string? Name { get; set; }
	public string? Description { get; set; }
	public DateTime ReleaseDate { get; set; }
	[JsonIgnore] public string ThumbnailUrl => $"https://cdn.akamai.steamstatic.com/steam/apps/{Id}/header.jpg";
	[JsonIgnore] public Bitmap? Thumbnail => ImageManager.GetImage(ThumbnailUrl, true, $"{Id}.png").Result;
	[JsonIgnore] public bool IsIncluded { get => !AssetsUtil.IsDlcExcluded(Id); set => AssetsUtil.SetDlcExcluded(Id, !value); }
}
