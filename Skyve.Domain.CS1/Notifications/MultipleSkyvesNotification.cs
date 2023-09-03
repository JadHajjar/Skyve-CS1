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
	private readonly List<ILocalPackageWithContents> _skyveInstances;

	public DateTime Time { get; }
    public string Title { get; }
    public string? Description { get; }
    public string Icon { get; }
    public Color? Color { get; }
    public bool HasAction { get; }

    public MultipleSkyvesNotification(List<ILocalPackageWithContents> skyveInstances)
    {
        Time = DateTime.Now;
        Title = Locale.MultipleSkyvesDetected;
        Description = Locale.MultipleLOM;
        Icon = "I_Hazard";
        Color = FormDesign.Design.RedColor;
        HasAction = true;
		_skyveInstances = skyveInstances;
	}

    public void OnClick()
    {
		ServiceCenter.Get<IInterfaceService>().ViewSpecificPackages(_skyveInstances, Title);
	}

	public void OnRightClick()
    { }
}
