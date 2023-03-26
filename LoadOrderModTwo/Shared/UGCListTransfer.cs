using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LoadOrderShared;
public static class UGCListTransfer
{
	public const string FILE_NAME = "UGCListTransfer.txt";
	const string MISSING = "missing";
	public static string FilePath => Path.Combine(SharedUtil.LocalLOMData, FILE_NAME);

	public static void SendList(IEnumerable<ulong> ids, bool missing)
	{
		SendListToFile(ids, FilePath, missing);
	}

	public static void SendListToFile(IEnumerable<ulong> ids, string filePath, bool missing)
	{
		if (ids == null)
		{
			throw new ArgumentNullException("ids");
		}

		static bool IsValid(ulong id) => id is not 0 and not ulong.MaxValue;

		var ids2 = ids.Where(IsValid).Select(id => id.ToString()).ToList();
		if (missing)
		{
			ids2.Add(MISSING);
		}

		var text = string.Join(";", ids2.ToArray());

		File.WriteAllText(filePath, text);
	}


	public static List<ulong> GetList(out bool missing)
	{
		return GetListFromFile(FilePath, out missing);
	}

	public static List<ulong> GetListFromFile(string filePath, out bool missing)
	{
		var text = File.ReadAllText(filePath);
		var ids = text.Split(';', ',', ' ', '\t').Distinct().ToList();
		missing = ids.Remove(MISSING);
		return ToNumber(ids);
	}

	public static void DeleteFile()
	{
		var path = FilePath;
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}

	public static List<ulong> ToNumber(IEnumerable<string> ids)
	{
		var ret = new List<ulong>();
		foreach (var strId in ids.Distinct())
		{
			if (ulong.TryParse(strId, out var id))
			{
				ret.Add(id);
			}
		}
		return ret;
	}
}
