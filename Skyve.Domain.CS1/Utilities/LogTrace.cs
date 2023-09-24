using Extensions;

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Skyve.Domain.CS1.Utilities;
public class LogTrace : ILogTrace
{
	public LogTrace(List<string> lines, int index, bool crash)
	{
		Trace = new();
		Crash = crash;

		for (var i = 1; i <= 3; i++)
		{
			var timestampRegex = Regex.Match(lines[index - i].Trim(), @"^([\d,\.]+ms) \| ");

			if (timestampRegex.Success)
			{
				Timestamp = "@" + timestampRegex.Groups[1].Value;
				Title = lines[index - i].Trim().Substring(timestampRegex.Value.Length);

				while (i > 1)
				{
					Trace.Insert(0, lines[index - --i].Trim());
				}

				break;
			}
		}

		Title ??= lines[index - 1].Trim();
		Timestamp ??= "N/A";
	}

	public string Title { get; }
	public string Timestamp { get; }
	public bool Crash { get; }
	public List<string> Trace { get; }

	public void AddTrace(string trace)
	{
		Trace.Add(trace.RegexRemove(@"\[0x00.+\>\:0").Trim());
	}

	public override string ToString()
	{
		return $"{(Crash ? "Crash" : "Error")} @{Timestamp}\t-\t{Title}\r\n\r\n{Trace.ListStrings(x => $"\t{x}", "\r\n")}";
	}
}
