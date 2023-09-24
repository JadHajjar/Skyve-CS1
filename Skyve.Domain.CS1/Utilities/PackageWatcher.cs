using Extensions;

using Skyve.Domain.Systems;

using System.Collections.Generic;
using System.IO;

using IoPath = System.IO.Path;

namespace Skyve.Domain.CS1.Utilities;
public class PackageWatcher
{
	private readonly IContentManager _contentManager;
	private readonly ILogger _logger;
	private readonly DelayedAction<string> _delayedUpdate = new(5000);
	private static readonly List<PackageWatcher> _watchers = new();

	private FileWatcher? watcher;

	private PackageWatcher(string folder, bool allowSelf, bool workshop)
	{
		Path = folder;
		AllowSelf = allowSelf;
		Workshop = workshop;

		ServiceCenter.Get(out _contentManager, out _logger);

		CreateWatcher();
	}

	private void CreateWatcher()
	{
		watcher?.Dispose();

		watcher = new()
		{
			Path = Path,
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
			Filter = "*.*",
			IncludeSubdirectories = true
		};

		watcher.Changed += FileChanged;
		watcher.Created += FileChanged;
		watcher.Deleted += FileChanged;

		watcher.EnableRaisingEvents = true;
	}

	public bool AllowSelf { get; }
	public bool Workshop { get; }
	public string Path { get; }

	private void FileChanged(object sender, FileWatcherEventArgs e)
	{
		var path = GetFirstFolderOrFileName(e.FullPath, Path);

		if (path != Path)
		{
			_delayedUpdate.Run(path, TriggerUpdate);
		}
		else if (AllowSelf)
		{
			_delayedUpdate.Run(path, TriggerSelfUpdate);
		}
	}

	private void TriggerUpdate(string path)
	{
		_logger.Info($"[Auto] Package content update triggered from '{path}'");

		_contentManager.ContentUpdated(path, false, Workshop, false);
	}

	private void TriggerSelfUpdate(string path)
	{
		_logger.Info($"[Auto] Package content update triggered from '{path}'");

		_contentManager.ContentUpdated(path, false, Workshop, true);
	}

	private string GetFirstFolderOrFileName(string filePath, string sourceFolder)
	{
		// Normalize the file path and source folder to use the same directory separator and casing
		var normalizedFilePath = IoPath.GetFullPath(filePath).Replace(IoPath.AltDirectorySeparatorChar, IoPath.DirectorySeparatorChar).ToLower();
		var normalizedSourceFolder = IoPath.GetFullPath(sourceFolder).Replace(IoPath.AltDirectorySeparatorChar, IoPath.DirectorySeparatorChar).ToLower();

		// Ensure that the source folder ends with a directory separator
		if (!normalizedSourceFolder.EndsWith(IoPath.DirectorySeparatorChar.ToString()))
		{
			normalizedSourceFolder += IoPath.DirectorySeparatorChar;
		}

		// Get the substring of the file path that comes after the source folder
		var startIndex = normalizedFilePath.IndexOf(normalizedSourceFolder) + normalizedSourceFolder.Length;
		var relativePath = normalizedFilePath.Substring(startIndex);

		// Get the first folder or file name from the relative path
		var parts = relativePath.Split(IoPath.DirectorySeparatorChar, IoPath.AltDirectorySeparatorChar);
		return parts.Length > 0 && string.IsNullOrEmpty(IoPath.GetExtension(parts[0])) ? CrossIO.Combine(Path, parts[0]) : Path;
	}

	public static void Create(string folder, bool allowSelf, bool workshop)
	{
		if (Directory.Exists(folder))
		{
			_watchers.Add(new PackageWatcher(folder, allowSelf, workshop));
		}
	}

	public static void Pause()
	{
		foreach (var item in _watchers)
		{
			item.watcher?.Dispose();
		}
	}

	public static void Resume()
	{
		foreach (var item in _watchers)
		{
			item.CreateWatcher();
		}
	}

	public static void Dispose()
	{
		foreach (var item in _watchers)
		{
			item.watcher?.Dispose();
		}

		_watchers.Clear();
	}
}
