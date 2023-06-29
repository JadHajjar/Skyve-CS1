using Extensions;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkyveApp.Utilities;
public class SteamImageProcessor : PeriodicProcessor<SteamImageProcessor.ImgRequest, SteamImageProcessor.Stub>
{
	public SteamImageProcessor() : base(100, 500, null)
	{

	}

	protected override bool CanProcess()
	{
		return ConnectionHandler.IsConnected;
	}


	protected override async Task<Dictionary<ImgRequest, Stub>> ProcessItems(List<ImgRequest> entities)
	{
		foreach (var img in entities)
		{
			if (!string.IsNullOrWhiteSpace(img.Url))
			{
				await ServiceCenter.Get<IImageService>().Ensure(img.Url, false, img.FileName, img.Square);
			}
		}

		return new();
	}

	protected override void CacheItems(Dictionary<ImgRequest, Stub> results)
	{ }

	public struct ImgRequest
	{
		public ImgRequest(string url, string? fileName, bool square)
		{
			Url = url;
			FileName = fileName;
			Square = square;
		}

		public string Url { get; }
		public string? FileName { get; }
		public bool Square { get; }
	}

	public struct Stub : ITimestamped
	{
		public DateTime Timestamp { get; }
	}
}
