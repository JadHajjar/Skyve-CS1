using Extensions;

using SkyveApp.Domain.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;

namespace SkyveApp.Utilities;
internal class DataCollectionUtil
{
	private static RequestData? _requestData;
	private static DateTime uptimeStart;

	internal static async void Start(List<Domain.Package> packages)
	{
		try
		{
			_requestData = ISave.Load<RequestData>("RequestData.json");

			if (_requestData?.UserId == null)
			{
				try
				{
					var id = (await SkyveApiUtil.GetUserGuid()).Data?.ToString();

					if (!string.IsNullOrEmpty(id))
					{
						_requestData = new RequestData { UserId = id!, Logs = new() };

						_requestData.Save("RequestData.json");
					}
				}
				catch { }
			}

			if (_requestData?.UserId != null)
			{
				_requestData.TotalPackages = packages.Count;
				_requestData.TotalMods = packages.Count(x => x.IsMod);
				_requestData.TotalAssets = packages.Sum(x => x.Assets?.Length ?? 0);
				_requestData.IncludedMods = packages.Select(x => x.Mod).Count(x => x?.IsIncluded ?? false);
				_requestData.IncludedAssets = packages.SelectMany(x => x.Assets).Count(x => x?.IsIncluded ?? false);
				_requestData.TotalIncludedModSizes = packages.Where(x => x.IsMod && x.Mod!.IsIncluded).Sum(x => x.FileSize) / 8 / 1024;
				_requestData.TotalIncludedAssetSizes = packages.Sum(x => x.Assets?.Sum(x => x.IsIncluded ? x.FileSize : default) ?? 0) / 8 / 1024;
				_requestData.TotalModSizes = packages.Where(x => x.IsMod).Sum(x => x.FileSize) / 8 / 1024;
				_requestData.TotalAssetSizes = packages.Sum(x => x.Assets?.Sum(x => x.FileSize) ?? 0) / 8 / 1024;

				_requestData.Save("RequestData.json");
			}

			uptimeStart = DateTime.Now;

			var timer = new System.Timers.Timer(5 * 60 * 1000);
			timer.Elapsed += (s, e) => Send();
			timer.Start();
		}
		catch { }
	}

	internal static void Log(string url, long contentLength)
	{
		try
		{
			if (_requestData?.UserId == null)
			{
				return;
			}

			lock (_requestData)
			{
				var current = _requestData.Logs.FirstOrDefault(x => x.BaseUrl == url) ?? new() { BaseUrl = url };

				current.RequestCounts++;
				current.TotalRequestSizesInKb += contentLength / 1024;

				_requestData.Logs!.Remove(current);
				_requestData.Logs.Add(current);
				_requestData.TotalUpTime += (long)(DateTime.Now - uptimeStart).TotalSeconds;

				uptimeStart = DateTime.Now;

				_requestData.Save("RequestData.json");
			}
		}
		catch { }
	}

	private static async void Send()
	{
		try
		{
			if (_requestData?.UserId != null)
			{
				lock (_requestData)
				{
					_requestData.TotalUpTime += (long)(DateTime.Now - uptimeStart).TotalSeconds;

					uptimeStart = DateTime.Now;
				}

				await SkyveApiUtil.SendRequestsData(_requestData);
			}
		}
		catch { }
	}
}
