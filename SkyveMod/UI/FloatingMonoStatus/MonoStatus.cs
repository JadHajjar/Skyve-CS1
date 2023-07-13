using ColossalFramework.IO;
using ColossalFramework.UI;

using KianCommons;
using KianCommons.IImplict;

using SkyveMod.Settings.Tabs;

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace SkyveMod.UI;
internal class MonoStatus : MonoBehaviour, IStartingObject
{
	#region LifeCycle

	public static MonoStatus Instance => FindObjectOfType<MonoStatus>();
	public static void Ensure()
	{
		_ = Instance ?? Create();
	}

	static MonoStatus Create()
	{
		return UIView.GetAView()?.gameObject.AddComponent<MonoStatus>();
	}

	public static void Release()
	{
		DecreaseRefCount(GetStatuslabel());

		//if (Instance != null && Instance)
		//    Destroy(Instance.gameObject);
	}

	public void Start()
	{
		try
		{
			CreateOrIncreaseRefCount();
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
	}

	static UILabel CreateOrIncreaseRefCount()
	{
		if (GetStatuslabel() is UILabel label)
		{
			label.objectUserData ??= 1; // recover from failure.
			label.objectUserData = (int)label.objectUserData + 1;
			return label;
		}
		else
		{
			return CreateLabel();
		}
	}

	static UILabel CreateLabel()
	{
		var statusLabel = (FloatingMonoStatus)UIView.GetAView().AddUIComponent(typeof(FloatingMonoStatus));
		
		if (statusLabel.debug = IsDebugMono())
		{
			Log.Warning("using DEBUG MONO is slow! use Skyve to launch game in release mode!", true);
		}

		statusLabel.name = LABEL_NAME;
		statusLabel.text = GetText();
		statusLabel.textScale = 1.3F;

		return statusLabel;
	}

	static void DecreaseRefCount(UILabel label)
	{
		if (label == null || !label)
		{
			return;
		}

		label.objectUserData ??= 1; // recover from failure.
		label.objectUserData = (int)label.objectUserData - 1;

		if ((int)label.objectUserData <= 0)
		{
			Destroy(label.gameObject);
		}
	}
	#endregion

	const string LABEL_NAME = "MonoDebugStatusLabel";

	static UILabel GetStatuslabel()
	{
		return UIView.GetAView()?.FindUIComponent<UILabel>(LABEL_NAME);
	}

	public void ModLoaded()
	{
		ShowText("Mod Loaded");
	}

	public void ModUnloaded()
	{
		ShowText("Mod Unloaded");
	}

	public Coroutine ShowText(string text, float sec = 4)
	{
		return StartCoroutine(ShowTextCoroutine(text, sec));
	}

	private IEnumerator ShowTextCoroutine(string text, float sec)
	{
		ShowText(text, true);
		yield return new WaitForSeconds(sec);
		ShowText(text, false);
		yield return null;
	}

	public void ShowText(string text, bool visible)
	{
		var lbl = GetStatuslabel();
		if (!lbl)
		{
			return;
		}

		if (visible)
		{
			if (Helpers.InStartupMenu)
			{
				lbl.text = text + "\n" + lbl.text;
			}
			else
			{
				lbl.text = lbl.text + "\n" + text;
			}
		}
		else
		{
			var index = lbl.text.IndexOf(text);
			lbl.text = lbl.text.Remove(startIndex: index, count: text.Length);
			lbl.text = lbl.text.RemoveEmptyLines().Trim();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static bool IsDebugMono()
	{
		try
		{
			var file = new StackFrame(true).GetFileName();
			return file?.EndsWith(".cs") ?? false;
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
			return false;
		}
	}

	static string GetText()
	{
		var playset = string.Empty;

		if (!StartupTab.HidePlaysetName.value && File.Exists(Path.Combine(Path.Combine(DataLocation.localApplicationData, "Skyve"), "CurrentPlayset")))
		{
			playset = File.ReadAllText(Path.Combine(Path.Combine(DataLocation.localApplicationData, "Skyve"), "CurrentPlayset"));
		}

		if (IsDebugMono())
		{
			return $"Debug Mono\r\n{playset}";
		}
		else
		{
			return playset;
		}
	}
}
