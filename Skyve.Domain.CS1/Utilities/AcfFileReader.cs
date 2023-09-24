using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Skyve.Domain.CS1.Utilities;
internal class AcfFileReader
{
	public string FileLocation { get; }

	public AcfFileReader(string FileLocation)
	{
		this.FileLocation = File.Exists(FileLocation) ? FileLocation : throw new FileNotFoundException("Error", FileLocation);
	}

	public bool CheckIntegrity()
	{
		var Content = File.ReadAllText(FileLocation);
		var quote = Content.Count(x => x == '"');
		var braceleft = Content.Count(x => x == '{');
		var braceright = Content.Count(x => x == '}');

		return braceleft == braceright && quote % 2 == 0;
	}

	public ACF_Struct ACFFileToStruct()
	{
		return ACFFileToStruct(File.ReadAllText(FileLocation));
	}

	private ACF_Struct ACFFileToStruct(string RegionToReadIn)
	{
		var ACF = new ACF_Struct();
		var LengthOfRegion = RegionToReadIn.Length;
		var CurrentPos = 0;
		while (LengthOfRegion > CurrentPos)
		{
			var FirstItemStart = RegionToReadIn.IndexOf('"', CurrentPos);
			if (FirstItemStart == -1)
			{
				break;
			}

			var FirstItemEnd = RegionToReadIn.IndexOf('"', FirstItemStart + 1);
			CurrentPos = FirstItemEnd + 1;
			var FirstItem = RegionToReadIn.Substring(FirstItemStart + 1, FirstItemEnd - FirstItemStart - 1);

			var SecondItemStartQuote = RegionToReadIn.IndexOf('"', CurrentPos);
			var SecondItemStartBraceleft = RegionToReadIn.IndexOf('{', CurrentPos);

			if (SecondItemStartBraceleft == -1 || SecondItemStartQuote < SecondItemStartBraceleft)
			{
				var SecondItemEndQuote = RegionToReadIn.IndexOf('"', SecondItemStartQuote + 1);
				var SecondItem = RegionToReadIn.Substring(SecondItemStartQuote + 1, SecondItemEndQuote - SecondItemStartQuote - 1);
				CurrentPos = SecondItemEndQuote + 1;
				ACF.SubItems.Add(FirstItem, SecondItem);
			}
			else
			{
				var SecondItemEndBraceright = RegionToReadIn.NextEndOf('{', '}', SecondItemStartBraceleft + 1);
				var ACFS = ACFFileToStruct(RegionToReadIn.Substring(SecondItemStartBraceleft + 1, SecondItemEndBraceright - SecondItemStartBraceleft - 1));
				CurrentPos = SecondItemEndBraceright + 1;
				ACF.SubACF.Add(FirstItem, ACFS);
			}
		}

		return ACF;
	}
}

internal class ACF_Struct
{
	public Dictionary<string, ACF_Struct> SubACF { get; private set; }
	public Dictionary<string, string> SubItems { get; private set; }

	public ACF_Struct()
	{
		SubACF = new Dictionary<string, ACF_Struct>();
		SubItems = new Dictionary<string, string>();
	}

	public void WriteToFile(string File)
	{

	}

	public override string ToString()
	{
		return ToString(0);
	}

	private string ToString(int Depth)
	{
		var SB = new StringBuilder();
		foreach (var item in SubItems)
		{
			SB.Append('\t', Depth);
			SB.AppendFormat("\"{0}\"\t\t\"{1}\"\r\n", item.Key, item.Value);
		}

		foreach (var item in SubACF)
		{
			SB.Append('\t', Depth);
			SB.AppendFormat("\"{0}\"\n", item.Key);
			SB.Append('\t', Depth);
			SB.AppendLine("{");
			SB.Append(item.Value.ToString(Depth + 1));
			SB.Append('\t', Depth);
			SB.AppendLine("}");
		}

		return SB.ToString();
	}
}

internal static class Extension
{
	public static int NextEndOf(this string str, char Open, char Close, int startIndex)
	{
		if (Open == Close)
		{
			throw new Exception("\"Open\" and \"Close\" char are equivalent!");
		}

		var OpenItem = 0;
		var CloseItem = 0;
		for (var i = startIndex; i < str.Length; i++)
		{
			if (str[i] == Open)
			{
				OpenItem++;
			}

			if (str[i] == Close)
			{
				CloseItem++;
				if (CloseItem > OpenItem)
				{
					return i;
				}
			}
		}

		throw new Exception("Not enough closing characters!");
	}
}