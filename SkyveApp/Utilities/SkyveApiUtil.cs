using Extensions;

using SkyveApp.Domain.Compatibility;
using SkyveApp.Domain.Compatibility.Api;
using SkyveApp.Domain.Steam;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkyveApp.Utilities;
internal class SkyveApiUtil
{
	private static async Task<T?> Get<T>(string url, params (string, object)[] queryParams)
	{
		return await ApiUtil.Get<T>(KEYS.API_URL + url
			, new[] { ("API_KEY", KEYS.API_KEY), ("USER_ID", Encryption.Encrypt(SteamUtil.GetLoggedInSteamId().ToString(), KEYS.SALT)) }
			, queryParams);
	}

	private static async Task<T?> Delete<T>(string url, params (string, object)[] queryParams)
	{
		return await ApiUtil.Delete<T>(KEYS.API_URL + url
			, new[] { ("API_KEY", KEYS.API_KEY), ("USER_ID", Encryption.Encrypt(SteamUtil.GetLoggedInSteamId().ToString(), KEYS.SALT)) }
			, queryParams);
	}

	private static async Task<T?> Post<TBody, T>(string url, TBody body, params (string, object)[] queryParams)
	{
		return await ApiUtil.Post<TBody, T>(KEYS.API_URL + url
			, body
			, new[] { ("API_KEY", KEYS.API_KEY), ("USER_ID", Encryption.Encrypt(SteamUtil.GetLoggedInSteamId().ToString(), KEYS.SALT)) }
			, queryParams);
	}

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

	internal static async Task<Dictionary<string, string>?> Translations()
	{
		return await Get<Dictionary<string, string>>("/Translations");
	}

	internal static async Task<ApiResponse> SendReviewRequest(ReviewRequest request)
	{
		return await Post<ReviewRequest, ApiResponse>("/RequestReview", request);
	}

	internal static async Task<ApiResponse> ProcessReviewRequest(ReviewRequest request)
	{
		return await Post<ReviewRequest, ApiResponse>("/ProcessReviewRequest", request);
	}

	internal static async Task<ReviewRequest[]?> GetReviewRequests()
	{
		return await Get<ReviewRequest[]>("/GetReviewRequests");
	}

	internal static async Task<ReviewRequest?> GetReviewRequest(ulong userId, ulong packageId)
	{
		return await Get<ReviewRequest>("/GetReviewRequest", (nameof(userId), userId), (nameof(packageId), packageId));
	}

	internal static async Task<UserProfile[]?> GetUserProfiles(ulong userId)
	{
		return await Get<UserProfile[]>("/GetUserProfiles", (nameof(userId), userId));
	}

	internal static async Task<UserProfile?> GetUserProfileContents(int profileId)
	{
		return await Get<UserProfile>("/GetUserProfileContents", (nameof(profileId), profileId));
	}

	internal static async Task<ApiResponse> DeleteUserProfile(int profileId)
	{
		return await Delete<ApiResponse>("/DeleteUserProfile", (nameof(profileId), profileId));
	}

	internal static async Task<ApiResponse> SaveUserProfile(UserProfile profile)
	{
		return await Post<UserProfile, ApiResponse>("/SaveUserProfile", profile);
	}

	internal static async Task<ApiResponse> UpdateUserProfile(UserProfile profile)
	{
		return await Post<UserProfile, ApiResponse>("/UpdateUserProfile", profile);
	}

	internal static async Task<UserProfile[]?> GetPublicProfiles()
	{
		return await Get<UserProfile[]>("/GetPublicProfiles");
	}
}
