using Extensions;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;

using System;
using System.IO;

namespace SkyveApp.Utilities.IO;

public class MonoFile : DebugFile
{
	public static MonoFile Instance = new MonoFile();

	public override string FilePath => CrossIO.Combine(Program.Services.GetService<ILocationManager>().MonoPath, "mono.dll");
	public override string ReleaseFilePath => CrossIO.Combine(Program.Services.GetService<ILocationManager>().MonoPath, "mono-orig.dll");
	public override string DebugFilePath => CrossIO.Combine(Program.Services.GetService<ILocationManager>().MonoPath, "mono-debug.dll");
	public override string ResourceFileName => "mono-debug._dll";
}

public class CitiesFile : DebugFile
{
	public static CitiesFile Instance = new CitiesFile();

	public override string FilePath => Program.Services.GetService<ILocationManager>().CitiesPathWithExe;
	public override string ReleaseFilePath => FilePath + ".Orig";
	public override string DebugFilePath => FilePath + ".Profiler";
	public override string ResourceFileName => "Cities.exe.Profiler";
}

public abstract class DebugFile
{
	public abstract string ResourceFileName { get; }
	public string ResourceFilePath => CrossIO.Combine(Program.CurrentDirectory, ResourceFileName);

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

		CrossIO.DeleteFile(dest);
		CrossIO.CopyFile(source, dest, false);
	}

	public void EnsureDebugWritten()
	{
		if (!CrossIO.FileExists(DebugFilePath))
		{
			CrossIO.CopyFile(ResourceFilePath, DebugFilePath, false);
		}
	}

	public void EnsureBReleaseBackedup()
	{
		if (CrossIO.FileExists(ReleaseFilePath))
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
			Program.Services.GetService<ILogger>().Exception(ex, "can't move files if CS is running");
		}
		return false;
	}
	public virtual bool UseRelease()
	{
		try
		{
			if (CrossIO.FileExists(ReleaseFilePath))
			{
				CopyFile(source: ReleaseFilePath, dest: FilePath);
			}
			return true;
		}
		catch (Exception ex)
		{
			Program.Services.GetService<ILogger>().Exception(ex, "can't move files if CS is running");
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
			CrossIO.FileExists(path1) && CrossIO.FileExists(path2) && FilesEqual(path1, path2);
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

		if (!CrossIO.FileExists(ReleaseFilePath))
		{
			return true;
		}

		return null;
	}
}