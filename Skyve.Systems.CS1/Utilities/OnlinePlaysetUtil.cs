using Extensions;

using Skyve.Domain;
using Skyve.Domain.CS1;
using Skyve.Domain.Systems;

using SlickControls;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Skyve.Systems.CS1.Utilities;
internal class OnlinePlaysetUtil : IOnlinePlaysetUtil
{
	private readonly ILogger _logger;
	private readonly SkyveApiUtil _skyveApiUtil;
	private readonly IPlaysetManager _playsetManager;

	public OnlinePlaysetUtil(ILogger logger, SkyveApiUtil skyveApiUtil, IPlaysetManager playsetManager)
	{
		_logger = logger;
		_skyveApiUtil = skyveApiUtil;
		_playsetManager = playsetManager;
	}

	public async Task Share(IPlayset item)
	{
		try
		{
			var profile = (item as Playset)!;
			var result = await _skyveApiUtil.SaveUserProfile(new()
			{
				Author = SteamUtil.GetLoggedInSteamId(),
				Banner = profile.BannerBytes,
				Color = profile.Color?.ToArgb(),
				Name = item.Name,
				ProfileUsage = (int)item.Usage,
				ProfileId = profile.ProfileId,
				Contents = profile.Assets.Concat(profile.Mods).Select(x => x.AsProfileContent()).ToArray()
			});

			if (result.Success)
			{
				profile.ProfileId = (int)Convert.ChangeType(result.Data, typeof(int));
				profile.Author = SteamUtil.GetLoggedInSteamId();

				_playsetManager.Save(item);
			}
			else
			{
				SystemsProgram.MainForm.TryInvoke(() => MessagePrompt.Show((profile.ProfileId == 0 ? Locale.FailedToUploadPlayset : Locale.FailedToUpdatePlayset) + "\r\n\r\n" + LocaleHelper.GetGlobalText(result.Message), PromptButtons.OK, PromptIcons.Error, form: SystemsProgram.MainForm as SlickForm));
			}
		}
		catch (Exception ex)
		{
			SystemsProgram.MainForm.TryInvoke(() => MessagePrompt.Show(ex, (item as Playset)!.ProfileId == 0 ? Locale.FailedToUploadPlayset : Locale.FailedToUpdatePlayset, form: SystemsProgram.MainForm as SlickForm));
		}
	}

	public async Task<bool> DownloadPlayset(ICustomPlayset item)
	{
		try
		{
			var profile = await _skyveApiUtil.GetUserProfileContents(item.ProfileId);
			if (profile == null)
			{
				return false;
			}

			var generatedProfile = (_playsetManager.Playsets.FirstOrDefault(x => x.Name?.Equals(item.Name, StringComparison.InvariantCultureIgnoreCase) == true) ?? profile.CloneTo<IPlayset, Playset>()) as Playset;

			generatedProfile!.Color = ((ICustomPlayset)profile).Color;
			generatedProfile.Author = profile.Author;
			generatedProfile.ProfileId = profile.ProfileId;
			generatedProfile.Usage = profile.Usage;
			generatedProfile.BannerBytes = profile.Banner;
			generatedProfile.Assets = profile.Contents.Where(x => !x.IsMod).ToList(x => new Playset.Asset(x));
			generatedProfile.Mods = profile.Contents.Where(x => x.IsMod).ToList(x => new Playset.Mod(x));

			_playsetManager.AddPlayset(generatedProfile);

			return _playsetManager.Save(generatedProfile);
		}
		catch (Exception ex)
		{
			_logger.Exception(ex, "Failed to download profile");

			return false;
		}
	}

	public async Task<bool> DownloadPlayset(string link)
	{
		try
		{
			var profile = await _skyveApiUtil.GetUserProfileByLink(link);

			if (profile == null)
			{
				return false;
			}

			var generatedProfile = profile.CloneTo<IPlayset, Playset>();

			generatedProfile.Assets = profile.Contents.Where(x => !x.IsMod).ToList(x => new Playset.Asset(x));
			generatedProfile.Mods = profile.Contents.Where(x => x.IsMod).ToList(x => new Playset.Mod(x));

			return _playsetManager.Save(generatedProfile);
		}
		catch (Exception ex)
		{
			_logger.Exception(ex, "Failed to download profile");

			return false;
		}
	}

	public async Task<bool> SetVisibility(ICustomPlayset profile, bool @public)
	{
		try
		{
			var result = await _skyveApiUtil.SetProfileVisibility(profile.ProfileId, @public);

			if (!result.Success)
			{
				SystemsProgram.MainForm.TryInvoke(() => MessagePrompt.Show(Locale.FailedToUpdatePlayset + "\r\n\r\n" + LocaleHelper.GetGlobalText(result.Message), PromptButtons.OK, PromptIcons.Error, form: SystemsProgram.MainForm as SlickForm));
			}
			else
			{
				profile.Public = @public;
				return _playsetManager.Save(profile);
			}

			return result.Success;
		}
		catch (Exception ex)
		{
			SystemsProgram.MainForm.TryInvoke(() => MessagePrompt.Show(ex, Locale.FailedToUpdatePlayset, form: SystemsProgram.MainForm as SlickForm));
			return false;
		}
	}

	public async Task<bool> DeleteOnlinePlayset(IOnlinePlayset profile)
	{
		try
		{
			var result = await _skyveApiUtil.DeleteUserProfile(profile.ProfileId);

			if (!result.Success)
			{
				SystemsProgram.MainForm.TryInvoke(() => MessagePrompt.Show(Locale.FailedToDeletePlayset + "\r\n\r\n" + LocaleHelper.GetGlobalText(result.Message), PromptButtons.OK, PromptIcons.Error, form: SystemsProgram.MainForm as SlickForm));
			}

			return result.Success;
		}
		catch (Exception ex)
		{
			SystemsProgram.MainForm.TryInvoke(() => MessagePrompt.Show(ex, Locale.FailedToDeletePlayset, form: SystemsProgram.MainForm as SlickForm));
			return false;
		}
	}
}
