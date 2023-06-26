using Extensions;

using SkyveApp.Domain.Systems;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace SkyveApp.Systems;

public class LoggerSystem : ILogger
{
	private bool failed;
	private readonly INotifier _notifier;
	private readonly bool _disabled;
	private readonly Stopwatch? _stopwatch;

	public string LogFilePath { get; }

	public LoggerSystem(ISettings _, INotifier notifier)
	{
		var folder = CrossIO.Combine(ISave.CustomSaveDirectory, ISave.AppName, "Logs");
		var previousLog = CrossIO.Combine(folder, $"SkyveApp_Previous.log");

		LogFilePath = CrossIO.Combine(folder, $"SkyveApp.log");

		_stopwatch = Stopwatch.StartNew();
		_notifier = notifier;

		try
		{
			Directory.CreateDirectory(folder);

			if (CrossIO.FileExists(previousLog))
			{
				CrossIO.DeleteFile(previousLog);
			}

			if (CrossIO.FileExists(LogFilePath))
			{
				File.Move(LogFilePath, previousLog);
			}

			File.WriteAllBytes(LogFilePath, new byte[0]);

			_stopwatch = Stopwatch.StartNew();

			var assembly = Assembly.GetExecutingAssembly();
			var details = assembly.GetName();

			Info($"{details.Name} v{details.Version}");
			Info($"Now  = {DateTime.Now:yyyy-mm-dd hh:mm:ss tt}");
			Info($"Here = {assembly.Location}");
		}
		catch
		{
			_disabled = true;
		}

		_notifier = notifier;
	}

	public void Debug(object message)
	{
		ProcessLog("DEBUG", message);
	}

	public void Info(object message)
	{
		ProcessLog("INFO ", message);
	}

	public void Warning(object message)
	{
		ProcessLog("WARN ", message);
	}

	public void Error(object message)
	{
		ProcessLog("ERROR", message);
	}

	public void Exception(Exception exception, object message)
	{
		ProcessLog("FATAL", $"{message}\r\n{exception.ToString().Replace("\n", "\n\t\t")}\r\n");
	}

	private void ProcessLog(string type, object content)
	{
		if (_disabled)
		{
			return;
		}

		var ticks = _stopwatch!.ElapsedTicks;
		var secs = ticks / Stopwatch.Frequency;
		var fraction = ticks % Stopwatch.Frequency;
		var sb = new StringBuilder();
		var time = $"{secs:n0}.{fraction:D3}";

		_ = sb.AppendFormat("\r\n{0} ", type);

		if (time.Length < 10)
		{
			_ = sb.Append(' ', 10 - time.Length);
		}

		_ = sb.Append(time);

		_ = sb.Append(" | ");

		_ = sb.Append(content);

		lock (LogFilePath)
		{
			try
			{
				File.AppendAllText(LogFilePath, sb.ToString());
			}
			catch (Exception ex)
			{
				if (!failed)
				{
					failed = true;

					_notifier.OnLoggerFailed(ex);
				}
			}
		}
	}
}