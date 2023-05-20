using Extensions;

using SkyveApp.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkyveApp.Utilities;
internal class SteamImageProcessor : PeriodicProcessor<SteamImageProcessor.ImgRequest, SteamImageProcessor.Stub>
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
				await ImageManager.Ensure(img.Url, false, img.FileName, img.Square);
			}
		}

		return new();
	}

	protected override void CacheItems(Dictionary<ImgRequest, Stub> results)
	{ }

	internal struct ImgRequest
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

	internal struct Stub : ITimestamped
	{
		public DateTime Timestamp { get; }
	}
}
