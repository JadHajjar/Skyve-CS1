﻿using Extensions;
using Extensions.Sql;
#if !API
using Newtonsoft.Json;
using SkyveApp.Domain.Systems;
#endif
using System;
using System.Drawing;

namespace SkyveApp.Domain.CS1.Steam;

#if !API
public class SteamUser : IUser, ITimestamped
#else
[DynamicSqlClass("SteamUsers")]
public class SteamUser : IDynamicSql
#endif
{
	public SteamUser(SteamUserEntry entry)
	{
		SteamId = ulong.Parse(entry.steamid);
		Name = entry.personaname;
		ProfileUrl = entry.profileurl;
		AvatarUrl = entry.avatarfull;
		Timestamp = DateTime.Now;
	}

	public SteamUser()
	{
		Name = string.Empty;
		ProfileUrl = string.Empty;
		AvatarUrl = string.Empty;
	}

	[DynamicSqlProperty(PrimaryKey = true)]
	public ulong SteamId { get; set; }
	[DynamicSqlProperty]
	public string Name { get; set; }
	[DynamicSqlProperty]
	public string ProfileUrl { get; set; }
	[DynamicSqlProperty]
	public string AvatarUrl { get; set; }

#if API
	[DynamicSqlProperty, System.Text.Json.Serialization.JsonIgnore]
	public DateTime Timestamp { get; set; }
#else
	[DynamicSqlProperty]
	public DateTime Timestamp { get; set; }
	[JsonIgnore] public object? Id => SteamId;
	[JsonIgnore] public Bitmap? AvatarImage => ServiceCenter.Get<IImageService>().GetImage(AvatarUrl, true).Result;

	public override string ToString()
	{
		return Name ?? LocaleHelper.GetGlobalText("UnknownUser");
	}

	public override bool Equals(object? obj)
	{
		return obj is IUser user && SteamId.Equals(user.Id);
	}

	public override int GetHashCode()
	{
		return 2139390487 + SteamId.GetHashCode();
	}
#endif
}
