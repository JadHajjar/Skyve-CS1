using Extensions;

using LoadOrderToolTwo.UserInterface.StatusBubbles;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.IO;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_MainPage : PanelContent
{
	private bool buttonStateRunning;
	public PC_MainPage()
	{
		InitializeComponent();

		B_StartStop.Enabled = CentralManager.IsContentLoaded && CitiesManager.CitiesAvailable();

		if (!CentralManager.IsContentLoaded)
		{
			CentralManager.ContentLoaded += SetButtonEnabledOnLoad;
		}

		CitiesManager.MonitorTick += CitiesManager_MonitorTick;

		RefreshButtonState(CitiesManager.IsRunning(), true);

		SlickTip.SetTo(B_StartStop, string.Format(Locale.LaunchTooltip, "[F5]"));

		label1.Text = Locale.MultipleLOM;

		ProfileManager.ProfileUpdated += ProfileManager_ProfileUpdated;

		if (ProfileManager.ProfilesLoaded)
		{
			ProfileManager_ProfileUpdated();
		}
	}

	private void ProfileManager_ProfileUpdated()
	{
		this.TryInvoke(() =>
		{
			TLP_Profiles.Controls.Clear(true, x => x is FavoriteProfileBubble);
			TLP_Profiles.RowStyles.Clear();
			TLP_Profiles.RowStyles.Add(new());

			foreach (var item in ProfileManager.Profiles.Where(x => x.IsFavorite))
			{
				TLP_Profiles.RowStyles.Add(new());
				TLP_Profiles.Controls.Add(new FavoriteProfileBubble(item), 0, TLP_Profiles.RowStyles.Count - 1);
			}
		});
	}

	private void SetButtonEnabledOnLoad()
	{
		this.TryInvoke(() =>
		{
			B_StartStop.Enabled = CitiesManager.CitiesAvailable();

			label1.Visible = ModLogicManager.AreMultipleLOMsPresent();
		});
	}

	protected override void LocaleChanged()
	{
		Text = Locale.Dashboard;
	}

	private void CitiesManager_MonitorTick(bool isAvailable, bool isRunning)
	{
		this.TryInvoke(() => B_StartStop.Enabled = isAvailable);

		RefreshButtonState(isRunning);
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		B_StartStop.Font = UI.Font(9.75F, FontStyle.Bold);
		label1.Font = UI.Font(10.5F, FontStyle.Bold);
		label1.Margin = UI.Scale(new Padding(10), UI.FontScale);
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		label1.ForeColor = design.RedColor;
	}

	private void ProfileBubble_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			Form.PushPanel<PC_Profiles>((Form as MainForm)?.PI_Profiles);
		}
	}

	private void ModsBubble_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			Form.PushPanel<PC_Mods>((Form as MainForm)?.PI_Mods);
		}
	}

	private void AssetsBubble_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			Form.PushPanel<PC_Assets>((Form as MainForm)?.PI_Assets);
		}
	}

	private void CompatibilityReportBubble_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			if (CentralManager.IsContentLoaded && !CompatibilityManager.CatalogAvailable)
			{
				PlatformUtil.OpenUrl("https://steamcommunity.com/sharedfiles/filedetails/?id=2881031511");
			}
			else
			{
				var current = CentralManager.SessionSettings.UserSettings.PackageSorting;
				CentralManager.SessionSettings.UserSettings.PackageSorting = Domain.Enums.PackageSorting.CompatibilityReport;
				Form.PushPanel<PC_Packages>((Form as MainForm)?.PI_Packages);
				CentralManager.SessionSettings.UserSettings.PackageSorting = current;
				//Form.PushPanel<PC_CompatibilityReport>((Form as MainForm)?.PI_Compatibility);
			}
		}
	}

	private void B_StartStop_Click(object sender, System.EventArgs e)
	{
		Program.MainForm.LaunchStopCities();
	}

	private void RefreshButtonState(bool running, bool firstTime = false)
	{
		if (!running)
		{
			if (buttonStateRunning || firstTime)
			{
				this.TryInvoke(() =>
				{
					B_StartStop.Text = Locale.StartCities;
					B_StartStop.Image = Properties.Resources.I_AppIcon_24;
					buttonStateRunning = false;
				});
			}

			return;
		}

		if (!buttonStateRunning || firstTime)
		{
			this.TryInvoke(() =>
			{
				B_StartStop.Text = Locale.StopCities;
				B_StartStop.Image = Properties.Resources.I_Stop_24;
				buttonStateRunning = true;
			});
		}
	}
}
