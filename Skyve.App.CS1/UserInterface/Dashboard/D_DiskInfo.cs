using Skyve.App.UserInterface.Dashboard;
using Skyve.App.UserInterface.Panels;
using Skyve.Systems.CS1.Utilities;

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Skyve.App.CS1.UserInterface.Dashboard;

internal class D_DiskInfo : IDashboardItem
{
	private readonly ILogger _logger;
	private readonly ISettings _settings;
	private readonly ILocationManager _locationManager;
	private readonly INotifier _notifier;

	private ContentInfo? info;

	private class ContentInfo
	{
		internal bool Error;
		internal long TotalCitiesSize;
		internal long TotalSavesSize;
		internal long TotalSubbedSize;
		internal long TotalLocalModsSize;
		internal long TotalOtherSize;
	}

	public D_DiskInfo()
	{
		ServiceCenter.Get(out _logger, out _settings, out _notifier, out _locationManager);
	}

	protected override void OnCreateControl()
	{
		base.OnCreateControl();

		_notifier.ContentLoaded += LoadData;

		LoadData();
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		_notifier.ContentLoaded -= LoadData;
	}

	protected override void OnDataLoadError(Exception ex)
	{
		info = new ContentInfo { Error = true };
		Loading = false;

		OnResizeRequested();

		_logger.Exception(ex, "Failed to get Disk Info Summary");
	}

	protected override Task<bool> ProcessDataLoad(CancellationToken token)
	{
		var contentInfo = new ContentInfo();

		if (token.IsCancellationRequested)
		{
			return Task.FromResult(false);
		}

		var savesFolder = CrossIO.Combine(_settings.FolderSettings.AppDataPath, "Saves");
		var localModsFolder = CrossIO.Combine(_settings.FolderSettings.AppDataPath, "Addons");
		var subbedFolder = _locationManager.WorkshopContentPath;

		foreach (var item in new DirectoryInfo(_settings.FolderSettings.AppDataPath).EnumerateFiles("*", SearchOption.AllDirectories))
		{
			contentInfo.TotalCitiesSize += item.Length;

			if (item.FullName.PathContains(savesFolder))
			{
				contentInfo.TotalSavesSize += item.Length;
			}
			else if (item.FullName.PathContains(localModsFolder))
			{
				contentInfo.TotalLocalModsSize += item.Length;
			}
		}

		foreach (var item in new DirectoryInfo(subbedFolder).EnumerateFiles("*", SearchOption.AllDirectories))
		{
			contentInfo.TotalCitiesSize += item.Length;
			contentInfo.TotalSubbedSize += item.Length;
		}

		contentInfo.TotalOtherSize = contentInfo.TotalCitiesSize - contentInfo.TotalSavesSize - contentInfo.TotalLocalModsSize - contentInfo.TotalSubbedSize;

		if (token.IsCancellationRequested)
		{
			return Task.FromResult(false);
		}

		info = contentInfo;

		OnResizeRequested();

		return Task.FromResult(true);
	}

	protected override DrawingDelegate GetDrawingMethod(int width)
	{
		return Draw;
	}

	protected override void DrawHeader(PaintEventArgs e, bool applyDrawing, ref int preferredHeight)
	{
		DrawSection(e, applyDrawing, ref preferredHeight, LocaleCS1.DiskStatus, "Drive");
	}

	private void Draw(PaintEventArgs e, bool applyDrawing, ref int preferredHeight)
	{
		if (Loading)
		{
			DrawLoadingSection(e, applyDrawing, ref preferredHeight, LocaleCS1.DiskStatus);
		}
		else
		{
			DrawSection(e, applyDrawing, ref preferredHeight, LocaleCS1.DiskStatus, "Drive");
		}

		if (info is null)
		{
			return;
		}

		var fadedColor = FormDesign.Design.ForeColor.MergeColor(FormDesign.Design.BackColor, 75);

		DrawValue(e, e.ClipRectangle.Pad(Margin), LocaleCS1.TotalSubbedSize, info.TotalSubbedSize.SizeString(), applyDrawing, ref preferredHeight, "Steam", fadedColor, false);
		DrawValue(e, e.ClipRectangle.Pad(Margin), LocaleCS1.TotalLocalModsSize, info.TotalLocalModsSize.SizeString(), applyDrawing, ref preferredHeight, "Mods", fadedColor, false);
		DrawValue(e, e.ClipRectangle.Pad(Margin), LocaleCS1.TotalSavesSize, info.TotalSavesSize.SizeString(), applyDrawing, ref preferredHeight, "City", fadedColor, false);
		DrawValue(e, e.ClipRectangle.Pad(Margin), LocaleCS1.TotalOtherSize, info.TotalOtherSize.SizeString(), applyDrawing, ref preferredHeight, "Folder", fadedColor, false);

		preferredHeight += BorderRadius;

		DrawValue(e, e.ClipRectangle.Pad(Margin), LocaleCS1.TotalCitiesSize, info.TotalCitiesSize.SizeString(), applyDrawing, ref preferredHeight, "CS");

		preferredHeight += BorderRadius;
	}

	private void OpenOptionsPanel()
	{
		App.Program.MainForm.PushPanel<PC_Options>((App.Program.MainForm as MainForm)!.PI_Options);
	}
}
