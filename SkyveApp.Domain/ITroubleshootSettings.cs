using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Domain;
public interface ITroubleshootSettings
{
	bool ItemIsCausingIssues { get; }
	bool ItemIsMissing { get; }
	bool NewItemCausingIssues { get; }
	bool Mods { get; }
}
