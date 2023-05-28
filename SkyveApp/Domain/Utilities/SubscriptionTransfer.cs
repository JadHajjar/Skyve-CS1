using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkyveApp.Domain.Utilities;
public class SubscriptionTransfer
{
	public List<ulong>? SubscribeTo { get; set; }
	public List<ulong>? UnsubscribingFrom { get; set; }
}
