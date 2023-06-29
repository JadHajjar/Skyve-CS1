using Extensions;

using SkyveApp.Services.Interfaces;
using SkyveApp.UserInterface.StatusBubbles;
using SkyveApp.Utilities;

using SlickControls;

using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Panels;
public partial class PC_MainPage : PanelContent
{
	private bool buttonStateRunning;
	public PC_MainPage()
	{
		InitializeComponent();

		var notifier = ServiceCenter.Get<INotifier>();
		var citiesManager = ServiceCenter.Get<ICitiesManager>();
		var profileManager = ServiceCenter.Get<IPlaysetManager>();

		B_StartStop.Enabled = notifier.IsContentLoaded && citiesManager.CitiesAvailable();

		if (!notifier.IsContentLoaded)
		{
			notifier.ContentLoaded += SetButtonEnabledOnLoad;
		}

		citiesManager.MonitorTick += CitiesManager_MonitorTick;

		RefreshButtonState(citiesManager.IsRunning(), true);

		SlickTip.SetTo(B_StartStop, string.Format(Locale.LaunchTooltip, "[F5]"));

		label1.Text = Locale.MultipleLOM;

		profileManager.ProfileUpdated += ProfileManager_ProfileUpdated;

		if (ServiceCenter.Get<INotifier>().ProfilesLoaded)
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

			foreach (var item in ServiceCenter.Get<IPlaysetManager>().Profiles.Where(x => x.IsFavorite))
			{
				TLP_Profiles.RowStyles.Add(new());
				TLP_Profiles.Controls.Add(new FavoriteProfileBubble(item) { Dock = DockStyle.Top }, 0, TLP_Profiles.RowStyles.Count - 1);
			}
		});
	}

	private void SetButtonEnabledOnLoad()
	{
		this.TryInvoke(() =>
		{
			B_StartStop.Enabled = ServiceCenter.Get<ICitiesManager>().CitiesAvailable();

			label1.Visible = ServiceCenter.Get<IModLogicManager>().AreMultipleLOMsPresent();
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
			Form.PushPanel<PC_Profile>();
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
					B_StartStop.ImageName = "I_CS";
					B_StartStop.Text = Locale.StartCities;
					buttonStateRunning = false;
				});
			}

			return;
		}

		if (!buttonStateRunning || firstTime)
		{
			this.TryInvoke(() =>
			{
				B_StartStop.ImageName = "I_Stop";
				B_StartStop.Text = Locale.StopCities;
				buttonStateRunning = true;
			});
		}
	}
}
