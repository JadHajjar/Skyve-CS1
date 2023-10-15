using Extensions;

using Skyve.Domain;
using Skyve.Domain.Systems;
using Skyve.Systems.Compatibility;
using Skyve.Systems.CS1.Utilities;

using System;

namespace Skyve.Systems.CS1.Systems;
internal class UserService : IUserService
{
	private KnownUser _user;

	public IKnownUser User => _user;

	public event Action? UserInfoUpdated;

	public UserService()
	{
		_user = new();

		new BackgroundAction(RefreshUserState).RunEvery(60000, true);
	}

	private async void RefreshUserState()
	{
		var steamId = SteamUtil.GetLoggedInSteamId();

		if (_user.Id?.Equals(steamId) ?? false)
		{
			return;
		}

		if (steamId == 0)
		{
			_user = new();
		}
		else
		{
			_user = new() { Id = steamId };

			var steamUser = SteamUtil.GetUser(steamId);

			if (steamUser != null)
			{
				_user.Name = steamUser.Name;
				_user.ProfileUrl = steamUser.ProfileUrl;
				_user.AvatarUrl = steamUser.AvatarUrl;
			}
		}

		var skyveUser = ServiceCenter.Get<ICompatibilityManager, CompatibilityManager>().CompatibilityData.Authors.TryGet(steamId);

		if (skyveUser is not null)
		{
			_user.Name ??= skyveUser.Name ?? string.Empty;
			_user.Verified = skyveUser.Verified;
			_user.Retired = skyveUser.Retired;
		}

		try
		{
			_user.Manager = await ServiceCenter.Get<SkyveApiUtil>().IsCommunityManager();
		}
		catch
		{
			_user.Manager = false;
		}

		UserInfoUpdated?.Invoke();
	}

	private class KnownUser : IKnownUser
	{
		public bool Retired { get; set; }
		public bool Verified { get; set; }
		public bool Malicious { get; set; }
		public bool Manager { get; set; }
		public string Name { get; set; } = "";
		public string ProfileUrl { get; set; } = "";
		public string AvatarUrl { get; set; } = "";
		public object? Id { get; set; }


		public override bool Equals(object? obj)
		{
			return obj is IUser user && (Id?.Equals(user.Id) ?? false);
		}

		public override int GetHashCode()
		{
			return 2139390487 + Id?.GetHashCode() ?? 0;
		}
	}
}
