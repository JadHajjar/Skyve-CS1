using Extensions;

using Skyve.Domain;
using Skyve.Domain.Systems;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skyve.Systems.CS1.Utilities;
internal class SteamImageProcessor : PeriodicProcessor<SteamImageProcessor.ImgRequest, SteamImageProcessor.Stub>
{
	public SteamImageProcessor() : base(100, 500, null)
	{

	}

	protected override bool CanProcess()
	{
		return ConnectionHandler.IsConnected;
	}


	protected override async Task<(Dictionary<ImgRequest, Stub>, bool)> ProcessItems(List<ImgRequest> entities)
	{
		foreach (var img in entities)
		{
			if (!string.IsNullOrWhiteSpace(img.Url))
			{
				await ServiceCenter.Get<IImageService>().Ensure(img.Url, false, img.FileName, img.Square);
			}
		}

		return (new(), false);
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
