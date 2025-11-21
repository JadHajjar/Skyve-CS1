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

		return ([], false);
	}

	protected override void CacheItems(Dictionary<ImgRequest, Stub> results)
	{ }

    public readonly struct ImgRequest(string url, string? fileName, bool square)
    {
        public string Url { get; } = url;
        public string? FileName { get; } = fileName;
        public bool Square { get; } = square;
    }

	public readonly struct Stub : ITimestamped
	{
		public DateTime Timestamp { get; }
	}
}
