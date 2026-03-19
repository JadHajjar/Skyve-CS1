using Extensions;

using Skyve.Domain.Systems;
using Skyve.Systems;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyve.Domain.CS1.Notifications;
public class MultipleSkyvesNotification : INotificationInfo
{
	private readonly List<ILocalPackageData> _skyveInstances;
	private readonly IInterfaceService _interfaceService;

	public DateTime Time { get; }
    public string Title { get; }
    public string? Description { get; }
    public string Icon { get; }
    public Color? Color { get; }
    public bool HasAction { get; }
	public bool CanBeRead { get; }

	public MultipleSkyvesNotification(List<ILocalPackageData> skyveInstances, IInterfaceService interfaceService)
    {
        Time = DateTime.Now;
        Title = Locale.MultipleSkyvesDetected;
        Description = Locale.MultipleLOM;
        Icon = "Hazard";
        Color = FormDesign.Design.RedColor;
        HasAction = true;
		_skyveInstances = skyveInstances;
		_interfaceService = interfaceService;
	}

    public void OnClick()
    {
		_interfaceService.ViewSpecificPackages(_skyveInstances.ToList(x => (IPackageIdentity)x), Title);
	}

	public void OnRightClick()
    { }

	public void OnRead()
	{
	}
}
