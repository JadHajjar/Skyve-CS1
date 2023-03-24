using Extensions;

using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using System.IO;
using System.Linq;

using IoPath = System.IO.Path;

namespace LoadOrderToolTwo.Domain.Utilities;
internal class PackageWatcher : FileSystemWatcher
{
	private readonly DelayedAction<string> _delayedUpdate = new(1500);

	private PackageWatcher(string folder, bool builtIn, bool workshop)
	{
		this.Path = folder;
		NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
		Filter = "*.*";
		IncludeSubdirectories = true;

		Changed += new FileSystemEventHandler(FileChanged);
		Created += new FileSystemEventHandler(FileChanged);
		Deleted += new FileSystemEventHandler(FileChanged);

		EnableRaisingEvents = true;

		BuiltIn = builtIn;
		Workshop = workshop;
	}

	public bool BuiltIn { get; }
	public bool Workshop { get; }

	private void FileChanged(object sender, FileSystemEventArgs e)
	{
		if (IoPath.GetFileName(e.FullPath) == ContentUtil.EXCLUDED_FILE_NAME)
		{
			return;
		}

		var path = GetFirstFolderOrFileName(e.FullPath, Path);

		if (path != Path)
		{
			_delayedUpdate.Run(path, TriggerUpdate);
		}
	}

	private void TriggerUpdate(string path)
	{
		ContentUtil.ContentUpdated(path, BuiltIn, Workshop);
	}

	public string GetFirstFolderOrFileName(string filePath, string sourceFolder)
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
		if (parts.Length > 0 && string.IsNullOrEmpty(IoPath.GetExtension(parts[0])))
		{
			return IoPath.Combine(Path, parts[0]);
		}

		return Path;
	}

	public static void Create(string folder, bool builtIn, bool workshop)
	{
		if (Directory.Exists(folder))
		{
			new PackageWatcher(folder, builtIn, workshop);
		}
	}
}
