using Newtonsoft.Json;

using System;

namespace Skyve.Domain.CS1.Steam;
public class SteamDlc : IDlcInfo
{
	public uint Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public DateTime ReleaseDate { get; set; }
	public string? Price { get; set; }
	public string? OriginalPrice { get; set; }
	public float Discount { get; set; }
    public DateTime Timestamp { get; set; }
    [JsonIgnore] public string ThumbnailUrl => $"https://cdn.akamai.steamstatic.com/steam/apps/{Id}/header.jpg";
}
