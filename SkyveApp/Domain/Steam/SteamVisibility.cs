using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Domain.Steam;
public enum SteamVisibility
{
	Local = -1,
	Public = 0,
	FriendsOnly = 1,
	Private = 2,
	Unlisted = 3,
}
