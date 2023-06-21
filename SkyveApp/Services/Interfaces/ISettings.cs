using SkyveApp.Domain.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Services.Interfaces;
public interface ISettings
{
	SessionSettings SessionSettings { get; }
}
