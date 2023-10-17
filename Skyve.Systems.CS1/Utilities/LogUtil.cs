using Extensions;

using Skyve.Domain;
using Skyve.Domain.CS1;
using Skyve.Domain.CS1.Utilities;
using Skyve.Domain.Enums;
using Skyve.Domain.Systems;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Skyve.Systems.CS1.Utilities;
internal class LogUtil : ILogUtil
{
	private readonly ICompatibilityManager _compatibilityManager;
	private readonly ILocationManager _locationManager;
	private readonly IPackageManager _contentManager;
	private readonly IPlaysetManager _profileManager;
	private readonly ILogger _logger;

	public LogUtil(ILocationManager locationManager, IPackageManager contentManager, IPlaysetManager profileManager, ILogger logger, ICompatibilityManager compatibilityManager)
	{
		_compatibilityManager = compatibilityManager;
		_locationManager = locationManager;
		_contentManager = contentManager;
		_profileManager = profileManager;
		_logger = logger;

		try
		{
			foreach (var item in Directory.GetFiles(CrossIO.Combine(_locationManager.SkyveAppDataPath, "Support Logs")))
			{
				if (DateTime.Now - File.GetLastWriteTime(item) > TimeSpan.FromDays(15))
				{
					CrossIO.DeleteFile(item);
				}
			}
		}
		catch { }
	}

	public string GameLogFile => CrossIO.CurrentPlatform switch
	{
		Platform.MacOSX => $"/Users/{Environment.UserName}/Library/Logs/Unity/Player.log",
		Platform.Linux => $"/home/{Environment.UserName}/.config/unity3d/Colossal Order/Cities_ Skylines/Player.log",
		_ => CrossIO.Combine(_locationManager.GamePath, "Cities_Data", "output_log.txt")
	};

	public string GameDataPath => CrossIO.CurrentPlatform switch
	{
		Platform.MacOSX => CrossIO.Combine(_locationManager.GamePath, "Cities.app", "Contents"),
		_ => CrossIO.Combine(_locationManager.GamePath, "Cities_Data")
	};

	public string CreateZipFileAndSetToClipboard(string? folder = null)
	{
		var file = CrossIO.Combine(folder ?? Path.GetTempPath(), $"LogReport_{DateTime.Now:yy-MM-dd_HH-mm}.zip");

		using (var fileStream = File.Create(file))
		{
			CreateZipToStream(fileStream);
		}

		PlatformUtil.SetFileInClipboard(file);

		return file;
	}

	public void CreateZipToStream(Stream fileStream)
	{
		using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create, true);

		AddMainFilesToZip(zipArchive);

		foreach (var filePath in GetFilesForZip())
		{
			if (CrossIO.FileExists(filePath))
			{
				var tempFile = Path.GetTempFileName();

				CrossIO.CopyFile(filePath, tempFile, true);

				try
				{
					zipArchive.CreateEntryFromFile(tempFile, $"Other Files\\{Path.GetFileName(filePath)}");
				}
				catch { }
			}
		}
	}

	private IEnumerable<string> GetFilesForZip()
	{
		yield return GetLastCrashLog();
		yield return GetLastLSMReport();

		if (!Directory.Exists(GameDataPath))
		{
			yield break;
		}

		foreach (var item in new DirectoryInfo(CrossIO.Combine(GameDataPath, "Logs")).GetFiles("*.log"))
		{
			if (DateTime.Now - item.LastWriteTime < TimeSpan.FromDays(1))
			{
				yield return item.FullName;
			}
		}

		foreach (var item in new DirectoryInfo(GameDataPath).GetFiles("*.log"))
		{
			if (DateTime.Now - item.LastWriteTime < TimeSpan.FromDays(1) && Path.GetFileName(GameLogFile) != item.Name)
			{
				yield return item.FullName;
			}
		}
	}

	private void AddMainFilesToZip(ZipArchive zipArchive)
	{
		if (CrossIO.FileExists(GameLogFile))
		{
			var tempLogFile = Path.GetTempFileName();
			CrossIO.CopyFile(GameLogFile, tempLogFile, true);
			zipArchive.CreateEntryFromFile(tempLogFile, "log.txt");

			var logTrace = SimplifyLog(tempLogFile, out var simpleLogText);

			AddSimpleLog(zipArchive, simpleLogText);

			AddErrors(zipArchive, logTrace);
		}

		if (CrossIO.FileExists(_logger.LogFilePath))
		{
			var tempSkyveLogFile = Path.GetTempFileName();
			CrossIO.CopyFile(_logger.LogFilePath, tempSkyveLogFile, true);
			zipArchive.CreateEntryFromFile(tempSkyveLogFile, "Skyve\\SkyveLog.log");
		}

		if (CrossIO.FileExists(_logger.PreviousLogFilePath))
		{
			var tempPrevSkyveLogFile = Path.GetTempFileName();
			CrossIO.CopyFile(_logger.PreviousLogFilePath, tempPrevSkyveLogFile, true);
			zipArchive.CreateEntryFromFile(tempPrevSkyveLogFile, "Skyve\\SkyveLog_Previous.log");
		}

		AddCompatibilityReport(zipArchive);

		AddProfile(zipArchive);
	}

	private void AddCompatibilityReport(ZipArchive zipArchive)
	{
		//var culture = LocaleHelper.CurrentCulture;
		//LocaleHelper.CurrentCulture = new System.Globalization.CultureInfo("en-US");

		var profileEntry = zipArchive.CreateEntry("Skyve\\CompatibilityReport.json");
		using var writer = new StreamWriter(profileEntry.Open());

		//_compatibilityManager.CacheReport();
		var reports = _contentManager.Packages.ToList(x => x.GetCompatibilityInfo());
		reports.RemoveAll(x => x.GetNotification() < NotificationType.Warning && !(x.Package!.IsIncluded(out var partial) || partial));

		writer.Write(Newtonsoft.Json.JsonConvert.SerializeObject(reports, Newtonsoft.Json.Formatting.Indented));

		//LocaleHelper.CurrentCulture = culture;
		//_compatibilityManager.CacheReport();
	}

	private void AddProfile(ZipArchive zipArchive)
	{
		var profileEntry = zipArchive.CreateEntry("Skyve\\LogProfile.json");
		using var writer = new StreamWriter(profileEntry.Open());
		var profile = new Playset("LogProfile");
		_profileManager.GatherInformation(profile);
		profile.Temporary = true;

		writer.Write(Newtonsoft.Json.JsonConvert.SerializeObject(profile, Newtonsoft.Json.Formatting.Indented));
	}

	private static void AddErrors(ZipArchive zipArchive, List<ILogTrace> logTrace)
	{
		if (logTrace.Count == 0)
		{
			return;
		}

		var errorsEntry = zipArchive.CreateEntry("log_errors.txt");
		using var writer = new StreamWriter(errorsEntry.Open());
		var errors = logTrace.Select(e => e.ToString()).ListStrings("\r\n*********************************************\r\n");

		writer.Write(errors);
	}

	private static void AddSimpleLog(ZipArchive zipArchive, string simpleLogText)
	{
		var simpleLogEntry = zipArchive.CreateEntry("log_simple.txt");
		using var writer = new StreamWriter(simpleLogEntry.Open());
		writer.Write(simpleLogText);
	}

	private string GetLastCrashLog()
	{
		if (CrossIO.CurrentPlatform is not Platform.Windows)
		{
			return string.Empty;
		}

		try
		{
			var mainGameDir = new DirectoryInfo(GameDataPath).Parent;
			var directories = mainGameDir.GetDirectories($"*-*-*");
			var latest = directories
				.Where(s => DateTime.Now - s.LastWriteTime < TimeSpan.FromDays(1))
				.OrderByDescending(s => s.CreationTime)
				.FirstOrDefault();

			if (latest != null)
			{
				return CrossIO.Combine(latest.FullName, "error.log");
			}
		}
		catch (Exception ex)
		{
			_logger.Exception(ex, "Failed to load the previous crash dump log");
		}

		return string.Empty;
	}

	private string GetLastLSMReport()
	{
		try
		{
			var path = LsmUtil.GetReportFolder();

			var reports = Directory.GetFiles(path, "*Assets Report*.htm");

			return reports.OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
		}
		catch (Exception e)
		{
			_logger.Exception(e, "Failed to get the last LSM report");
		}

		return string.Empty;
	}

	public List<ILogTrace> SimplifyLog(string log, out string simpleLog)
	{
		var lines = File.ReadAllLines(log).ToList();

		// decruft the log file
		for (var i = lines.Count - 1; i > 0; i--)
		{
			var current = lines[i];
			if (current.IndexOf("DebugBindings.gen.cpp Line: 51") != -1 ||
				current.StartsWith("Fallback handler") ||
				current.Contains("[PlatformService, Native - public]") ||
				current.Contains("m_SteamUGCRequestMap error") ||
				current.IndexOf("(this message is harmless)") != -1 ||
				current.IndexOf("PopsApi:") != -1 ||
				current.IndexOf("GfxDevice") != -1 ||
				current.StartsWith("Assembly ") ||
				current.StartsWith("No source files found:") ||
				current.StartsWith("d3d11: failed") ||
				current.StartsWith("(Filename:  Line: ") ||
				current.Contains("SteamHelper+DLC_BitMask") ||
				current.EndsWith(" [Packer - public]") ||
				current.EndsWith(" [Mods - public]"))
			{
				lines.RemoveAt(i);

				if (i < lines.Count && string.IsNullOrWhiteSpace(lines[i]))
				{
					lines.RemoveAt(i);
				}
			}
		}

		// clear excess blank lines

		var blank = false;

		for (var i = lines.Count - 1; i > 0; i--)
		{
			if (blank)
			{
				if (string.IsNullOrWhiteSpace(lines[i]))
				{
					lines.RemoveAt(i);
				}
				else
				{
					blank = false;
				}
			}
			else
			{
				blank = string.IsNullOrWhiteSpace(lines[i]);
			}
		}

		simpleLog = string.Join("\r\n", lines);

		// now split out errors

		LogTrace? currentTrace = null;
		var traces = new List<ILogTrace>();

		for (var i = 0; i < lines.Count; i++)
		{
			var current = lines[i];

			if (!current.StartsWith("Crash!!!") && !current.TrimStart().StartsWith("at ") && !(current.TrimStart().StartsWith("--") && currentTrace is not null))
			{
				if (currentTrace is not null)
				{
					if (!currentTrace.Title.Contains("System.Environment.get_StackTrace()"))
					{
						traces.Add(currentTrace);
					}

					currentTrace = null;
				}

				if (current.Contains("[Warning]") || current.Contains("[Error]"))
				{
					traces.Add(new LogTrace(lines, i + 1, false));
				}

				continue;
			}

			currentTrace ??= new(lines, i, current.StartsWith("Crash!!!"));

			currentTrace.AddTrace(current);
		}

		if (currentTrace is not null)
		{
			traces.Add(currentTrace);
		}

		return traces;
	}
}
