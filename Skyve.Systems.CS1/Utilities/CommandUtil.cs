using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Skyve.Systems.CS1.Utilities;
public static class CommandUtil
{
	public static string? PreSelectedProfile { get; private set; }
	public static bool LaunchOnLoad { get; private set; }
	public static bool CloseWhenFinished { get; private set; }
	public static bool NoWindow { get; private set; }

	public static bool Parse(string[] args)
	{
		args = string.Join(" ", args).Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

		var exit = false;

		foreach (var arg in args)
		{
			exit |= Parse(arg);
		}

		return exit;
	}

	private static bool Parse(string arg)
	{
		if (isCommand("stub", out _))
		{
			Process.Start(Application.ExecutablePath);

			return true;
		}

		if (isCommand("profile", out var profileName))
		{
			PreSelectedProfile = profileName;
		}

		if (isCommand("launch", out _))
		{
			LaunchOnLoad = true;
		}

		if (isCommand("closeWhenFinished", out _))
		{
			CloseWhenFinished = true;
		}

		if (isCommand("noWindow", out _))
		{
			NoWindow = true;
		}

		return false;

		bool isCommand(string command, out string value)
		{
			if (arg.IndexOf(command, StringComparison.CurrentCultureIgnoreCase) == 0)
			{
				value = arg.Substring(command.Length).Trim();
				return true;
			}

			value = string.Empty;
			return false;
		}
	}
}
