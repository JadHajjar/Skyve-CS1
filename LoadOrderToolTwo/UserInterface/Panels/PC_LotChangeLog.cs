using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LoadOrderToolTwo.UserInterface.Panels;
internal class PC_LotChangeLog : PC_Changelog
{
	public PC_LotChangeLog() : base(
		Assembly.GetExecutingAssembly(), 
		nameof(LoadOrderToolTwo) + ".Properties.Changelog.json", 
		new Version(Assembly.GetExecutingAssembly().GetName().Version.ToString(3)))
	{
	}
}
