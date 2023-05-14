﻿using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Enums;
using LoadOrderToolTwo.UserInterface.Lists;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_ModUtilities : PanelContent
{
	public PC_ModUtilities()
	{
		InitializeComponent();

		RefreshModIssues();

		B_LoadCollection.Height = 0;

		CentralManager.PackageInformationUpdated += RefreshModIssues;

		DD_BOB.StartingFolder = LocationManager.AppDataPath;
		DD_Missing.StartingFolder = DD_Unused.StartingFolder = LsmUtil.GetReportFolder();
		DD_Missing.ValidExtensions = DD_Unused.ValidExtensions = new[] { ".htm", ".html" };

		SlickTip.SetTo(B_LoadCollection, "LoadCollectionTip");
		SlickTip.SetTo(DD_Missing, "LsmMissingTip");
		SlickTip.SetTo(DD_Unused, "LsmUnusedTip");
		SlickTip.SetTo(B_ReDownload, "FixAllTip");
		SlickTip.SetTo(DD_BOB, "XMLTip");
	}

	public override Color GetTopBarColor()
	{
		return FormDesign.Design.AccentBackColor;
	}

	private void RefreshModIssues()
	{
		var modsOutOfDate = CentralManager.Mods.AllWhere(x => x.IsIncluded && x.Package.Status == DownloadStatus.OutOfDate);
		var modsIncomplete = CentralManager.Mods.AllWhere(x => x.IsIncluded && x.Package.Status == DownloadStatus.PartiallyDownloaded);

		B_ReDownload.Loading = false;
		B_Cleanup.Loading = false;

		this.TryInvoke(() =>
		{
			L_OutOfDate.Text = $"{modsOutOfDate.Count} {(modsOutOfDate.Count == 1 ? Locale.ModOutOfDate : Locale.ModOutOfDatePlural)}:\r\n{modsOutOfDate.ListStrings(x => $"    • {x}", "\r\n")}";
			L_Incomplete.Text = $"{modsIncomplete.Count} {(modsIncomplete.Count == 1 ? Locale.ModIncomplete : Locale.ModIncompletePlural)}:\r\n{modsIncomplete.ListStrings(x => $"    • {x}", "\r\n")}";

			L_OutOfDate.Visible = modsOutOfDate.Count > 0;
			L_Incomplete.Visible = modsIncomplete.Count > 0;
			P_ModIssues.Visible = modsOutOfDate.Count > 0 || modsIncomplete.Count > 0;
		});
	}

	protected override void LocaleChanged()
	{
		Text = Locale.Utilities;
		L_CleanupInfo.Text = Locale.CleanupInfo;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		B_ReDownload.Margin = UI.Scale(new Padding(5), UI.FontScale);
		P_Cleanup.Margin = P_Collecttions.Margin = P_BOB.Margin = P_LsmReport.Margin = P_Text.Margin = P_ModIssues.Margin = UI.Scale(new Padding(10, 0, 10, 10), UI.FontScale);
		B_ReDownload.Margin = TB_CollectionLink.Margin = B_LoadCollection.Margin = UI.Scale(new Padding(5), UI.FontScale);
		B_ImportClipboard.Margin = UI.Scale(new Padding(10), UI.FontScale);
		L_CleanupInfo.Font = L_OutOfDate.Font = L_Incomplete.Font = UI.Font(9F);
		L_CleanupInfo.Margin = L_OutOfDate.Margin = L_Incomplete.Margin = UI.Scale(new Padding(3), UI.FontScale);
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		BackColor = design.AccentBackColor;
		L_CleanupInfo.ForeColor = design.LabelColor;
		L_OutOfDate.ForeColor = design.YellowColor;
		L_Incomplete.ForeColor = design.RedColor;

		foreach (Control item in TLP_Main.Controls)
		{
			item.BackColor = design.BackColor.Tint(Lum: design.Type.If(FormDesignType.Dark, 1, -1));
		}
	}

	private async void B_LoadCollection_Click(object sender, EventArgs e)
	{
		if (!B_LoadCollection.Loading && this.CheckValidation())
		{
			B_LoadCollection.Loading = true;

			var collectionId = Regex.Match(TB_CollectionLink.Text, TB_CollectionLink.ValidationRegex).Groups[1].Value;

			if (ulong.TryParse(collectionId, out var steamId))
			{
				var contents = await SteamUtil.GetItemAsync(steamId);

				if (contents?.RequiredPackages?.Any() ?? false)
				{
					Form.PushPanel(null, new PC_ImportCollection(contents));

					TB_CollectionLink.Text = string.Empty;
				}
			}

			B_LoadCollection.Loading = false;
		}
	}

	private void B_ReDownload_Click(object sender, EventArgs e)
	{
		B_ReDownload.Loading = true;
		SteamUtil.ReDownload(CentralManager.Mods.Where(x => x.Package.Status is DownloadStatus.OutOfDate or DownloadStatus.PartiallyDownloaded).ToArray());
	}

	private void TB_CollectionLink_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		if (e.KeyData == Keys.Enter)
		{
			B_LoadCollection_Click(this, EventArgs.Empty);

			e.IsInputKey = false;
		}
	}

	private void LSMDragDrop_FileSelected(string obj)
	{
		var assets = LsmUtil.LoadMissingAssets(obj);

		Form.PushPanel(null, new PC_GenericPackageList(assets) { Text = Locale.MissingLSMReport });
	}

	private bool LSMDragDrop_ValidFile(object sender, string arg)
	{
		return LsmUtil.IsValidLsmReportFile(arg);
	}

	private void LSM_UnusedDrop_FileSelected(string obj)
	{
		var assets = LsmUtil.LoadUnusedAssets(obj);

		Form.PushPanel(null, new PC_GenericPackageList(assets) { Text = Locale.UnusedLSMReport });
	}

	private void DD_BOB_FileSelected(string obj)
	{
		var matches = Regex.Matches(File.ReadAllText(obj), "[\\>\"](\\d{8,20})\\.(.+?)[\\<\"]");
		var assets = new List<Profile.Asset>();

		foreach (Match item in matches)
		{
			if (ulong.TryParse(item.Groups[1].Value, out var id) && !assets.Any(x => x.SteamId == id))
			{
				assets.Add(new()
				{
					Name = item.Groups[2].Value,
					SteamId = id
				});
			}
		}

		Form.PushPanel(null, new PC_GenericPackageList(assets) { Text = LocaleHelper.GetGlobalText(P_BOB.Text) });
	}

	private bool DD_BOB_ValidFile(object sender, string arg)
	{
		return Path.GetExtension(arg).ToLower() == ".xml";
	}

	private bool DD_TextImport_ValidFile(object arg1, string arg2)
	{
		return true;
	}

	private void DD_TextImport_FileSelected(string obj)
	{
		var matches = Regex.Matches(File.ReadAllText(obj), "(\\d{8,20})");
		var assets = new List<Profile.Asset>();

		foreach (Match item in matches)
		{
			if (ulong.TryParse(item.Groups[1].Value, out var id) && !assets.Any(x => x.SteamId == id))
			{
				assets.Add(new()
				{
					Name = item.Groups[1].Value,
					SteamId = id
				});
			}
		}

		Form.PushPanel(null, new PC_GenericPackageList(assets) { Text = LocaleHelper.GetGlobalText(P_Text.Text) });
	}

	private void B_ImportClipboard_Click(object sender, EventArgs e)
	{
		if (!Clipboard.ContainsText())
		{
			return;
		}

		var matches = Regex.Matches(Clipboard.GetText(), "(\\d{8,20})");
		var assets = new List<Profile.Asset>();

		foreach (Match item in matches)
		{
			if (ulong.TryParse(item.Groups[1].Value, out var id) && !assets.Any(x => x.SteamId == id))
			{
				assets.Add(new()
				{
					Name = item.Groups[1].Value,
					SteamId = id
				});
			}
		}

		Form.PushPanel(null, new PC_GenericPackageList(assets) { Text = LocaleHelper.GetGlobalText(B_ImportClipboard.Text) });
	}

	private void B_Cleanup_Click(object sender, EventArgs e)
	{
		if (CitiesManager.RunStub())
		{
			B_Cleanup.Loading = true;
			SubscriptionsUtil.Redownload = true;
		}
	}

	private void slickScroll1_Scroll(object sender, ScrollEventArgs e)
	{
		slickSpacer1.Visible = slickScroll1.Percentage != 0;
	}
}
