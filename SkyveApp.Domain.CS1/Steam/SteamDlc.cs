using Newtonsoft.Json;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities;

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
	[JsonIgnore] public Bitmap? Thumbnail => Program.Services.GetService<IImageService>().GetImage(ThumbnailUrl, true, $"{Id}.png").Result;
	[JsonIgnore] public bool IsIncluded { get => !Program.Services.GetService<IAssetUtil>().IsDlcExcluded(Id); set => Program.Services.GetService<IAssetUtil>().SetDlcExcluded(Id, !value); }
}
