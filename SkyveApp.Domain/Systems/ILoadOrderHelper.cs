﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Domain.Systems;
public interface ILoadOrderHelper
{
	IEnumerable<IMod> GetOrderedMods();
}