using ColossalFramework.UI;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace KianCommons.UI;
internal class UIUtils
{
	// Token: 0x17000006 RID: 6
	// (get) Token: 0x06000038 RID: 56 RVA: 0x00004CF0 File Offset: 0x00002EF0
	public static UIUtils Instance
	{
		get
		{
			var flag = UIUtils.instance == null;
			if (flag)
			{
				UIUtils.instance = new UIUtils();
			}
			return UIUtils.instance;
		}
	}

	// Token: 0x06000039 RID: 57 RVA: 0x00004D20 File Offset: 0x00002F20
	private void FindUIRoot()
	{
		this.uiRoot = null;
		foreach (var uiview in UnityEngine.Object.FindObjectsOfType<UIView>())
		{
			var flag = uiview.transform.parent == null && uiview.name == "UIView";
			if (flag)
			{
				this.uiRoot = uiview;
				break;
			}
		}
	}

	// Token: 0x0600003A RID: 58 RVA: 0x00004D84 File Offset: 0x00002F84
	public string GetTransformPath(Transform transform)
	{
		var text = transform.name;
		var parent = transform.parent;
		while (parent != null)
		{
			text = parent.name + "/" + text;
			parent = parent.parent;
		}
		return text;
	}

	// Token: 0x0600003B RID: 59 RVA: 0x00004DD0 File Offset: 0x00002FD0
	public T FindComponent<T>(string name, UIComponent parent = null, UIUtils.FindOptions options = UIUtils.FindOptions.None) where T : UIComponent
	{
		var flag = this.uiRoot == null;
		if (flag)
		{
			this.FindUIRoot();
			var flag2 = this.uiRoot == null;
			if (flag2)
			{
				return default;
			}
		}
		foreach (var t in UnityEngine.Object.FindObjectsOfType<T>())
		{
			var flag3 = (options & UIUtils.FindOptions.NameContains) > UIUtils.FindOptions.None;
			bool flag4;
			if (flag3)
			{
				flag4 = t.name.Contains(name);
			}
			else
			{
				flag4 = t.name == name;
			}
			var flag5 = !flag4;
			if (!flag5)
			{
				var flag6 = parent != null;
				Transform transform;
				if (flag6)
				{
					transform = parent.transform;
				}
				else
				{
					transform = this.uiRoot.transform;
				}
				var parent2 = t.transform.parent;
				while (parent2 != null && parent2 != transform)
				{
					parent2 = parent2.parent;
				}
				var flag7 = parent2 == null;
				if (!flag7)
				{
					return t;
				}
			}
		}
		return default;
	}

	public static IEnumerable<T> GetCompenentsWithName<T>(string name) where T : UIComponent
	{
		var components = GameObject.FindObjectsOfType<T>();
		foreach (var component in components)
		{
			if (component.name == name)
			{
				yield return component;
			}
		}
	}

	// Token: 0x04000024 RID: 36
	private static UIUtils instance = null;

	// Token: 0x04000025 RID: 37
	private UIView uiRoot = null;

	// Token: 0x02000010 RID: 16
	[Flags]
	public enum FindOptions
	{
		// Token: 0x04000034 RID: 52
		None = 0,
		// Token: 0x04000035 RID: 53
		NameContains = 1
	}
}
