using LoadOrderToolTwo.Domain.Enums;
using LoadOrderToolTwo.Domain.Steam;

using System;
using System.Collections.Generic;
using System.Drawing;

namespace LoadOrderToolTwo.Domain.Interfaces;

public interface IPackage : IGenericPackage
{
	bool BuiltIn { get; }
	bool IsPseudoMod { get; set; }
}