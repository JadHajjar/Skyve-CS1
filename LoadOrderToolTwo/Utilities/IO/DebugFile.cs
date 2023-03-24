using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.IO;

namespace LoadOrderToolTwo.Utilities.IO;

public class MonoFile : DebugFile
{
	public static MonoFile Instance = new MonoFile();

	public override string FilePath => Path.Combine(LocationManager.MonoPath, "mono.dll");
	public override string ReleaseFilePath => Path.Combine(LocationManager.MonoPath, "mono-orig.dll");
	public override string DebugFilePath => Path.Combine(LocationManager.MonoPath, "mono-debug.dll");
	public override string ResourceFileName => "mono-debug._dll";
}

public class CitiesFile : DebugFile
{
	public static CitiesFile Instance = new CitiesFile();

	public override string FilePath => Path.Combine(LocationManager.GamePath, LocationManager.CitiesExe);
	public override string ReleaseFilePath => FilePath + ".Orig";
	public override string DebugFilePath => FilePath + ".Profiler";
	public override string ResourceFileName => "Cities.exe.Profiler";
}

public abstract class DebugFile
{
	public abstract string ResourceFileName { get; }
	public string ResourceFilePath => Path.Combine(LocationManager.CurrentDirectory, ResourceFileName);

	public abstract string ReleaseFilePath { get; }
	public abstract string DebugFilePath { get; }
	public abstract string FilePath { get; }

	public static bool FilesEqual(string path1, string path2)
	{
		return new FileInfo(path1).Length == new FileInfo(path2).Length;
	}
	public static void CopyFile(string source, string dest)
	{
		if (FilesEqual(source, dest))
		{
			return; // already the same
		}

		File.Delete(dest);
		File.Copy(sourceFileName: source, destFileName: dest);
	}

	public void EnsureDebugWritten()
	{
		if (!File.Exists(DebugFilePath))
		{
			File.Copy(ResourceFilePath, DebugFilePath);
		}
	}

	public void EnsureBReleaseBackedup()
	{
		if (File.Exists(ReleaseFilePath))
		{
			return;
		}

		File.Copy(FilePath, ReleaseFilePath);
	}

	public virtual bool UseDebug()
	{
		try
		{
			EnsureBReleaseBackedup();
			EnsureDebugWritten();
			CopyFile(source: DebugFilePath, dest: FilePath);
			return true;
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "can't move files if CS is running");
		}
		return false;
	}
	public virtual bool UseRelease()
	{
		try
		{
			if (File.Exists(ReleaseFilePath))
			{
				CopyFile(source: ReleaseFilePath, dest: FilePath);
			}
			return true;
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "can't move files if CS is running");
		}
		return false;
	}

	/// <returns>
	/// true if release version is used,
	/// false if debug version is used,
	/// null if cannot determine.
	/// </returns>
	public bool? ReleaseIsUsed()
	{
		static bool Equal(string path1, string path2) =>
			File.Exists(path1) && File.Exists(path2) && FilesEqual(path1, path2);
		if (Equal(ReleaseFilePath, FilePath))
		{
			return true;
		}

		if (Equal(DebugFilePath, FilePath))
		{
			return false;
		}

		if (Equal(ResourceFilePath, FilePath))
		{
			return false;
		}

		if (!File.Exists(ReleaseFilePath))
		{
			return true;
		}

		return null;
	}
}