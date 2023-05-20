using Extensions;
using SkyveApp.Domain.Compatibility;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Utilities;
internal class CompatibilityApiUtil
{
	internal static async Task<bool> IsCommunityManager()
	{
		return await Get<bool>("/IsCommunityManager");
	}

	internal static async Task<CompatibilityData?> Catalogue()
	{
		return await Get<CompatibilityData>("/Catalogue");
	}

	internal static async Task<CompatibilityData?> Catalogue(object steamId)
	{
		return await Get<CompatibilityData>("/Package", ("steamId", steamId));
	}

	internal static async Task<ApiResponse> SaveEntry(PostPackage package)
	{
		return await Post<PostPackage, ApiResponse>("/SaveEntry", package);
	}

	private static async Task<T?> Get<T>(string url, params (string, object)[] queryParams)
	{
		using var httpClient = new HttpClient();

		httpClient.DefaultRequestHeaders.Add("API_KEY", KEYS.API_KEY);
		httpClient.DefaultRequestHeaders.Add("USER_ID", Encryption.Encrypt(SteamUtil.GetLoggedInSteamId().ToString(), KEYS.SALT));

		if (queryParams.Length > 0)
		{
			var query = queryParams.Select(x => $"{Uri.EscapeDataString(x.Item1)}={Uri.EscapeDataString(x.Item2.ToString())}");

			url += "?" + string.Join("&", query);
		}

		var httpResponse = await httpClient.GetAsync(KEYS.API_URL + url);

		if (httpResponse.IsSuccessStatusCode)
		{
			var response = await httpResponse.Content.ReadAsStringAsync();

			return JsonConvert.DeserializeObject<T>(response);
		}

		return default;
	}

	private static async Task<T?> Post<TBody, T>(string url, TBody body, params (string, object)[] queryParams)
	{
		using var httpClient = new HttpClient();

		httpClient.DefaultRequestHeaders.Add("API_KEY", KEYS.API_KEY);
		httpClient.DefaultRequestHeaders.Add("USER_ID", Encryption.Encrypt(SteamUtil.GetLoggedInSteamId().ToString(), KEYS.SALT));

		if (queryParams.Length > 0)
		{
			var query = queryParams.Select(x => $"{Uri.EscapeDataString(x.Item1)}={Uri.EscapeDataString(x.Item2.ToString())}");

			url += "?" + string.Join("&", query);
		}

		var json = JsonConvert.SerializeObject(body);
		var content = new StringContent(json, Encoding.UTF8, "application/json");
		var httpResponse = await httpClient.PostAsync(KEYS.API_URL + url, content);

		if (httpResponse.IsSuccessStatusCode)
		{
			var response = await httpResponse.Content.ReadAsStringAsync();

			return JsonConvert.DeserializeObject<T>(response);
		}

		return default;
	}

	internal static async Task<Dictionary<string,string>?> Translations()
	{
		return await Get<Dictionary<string, string>>("/Translations");
	}
}
