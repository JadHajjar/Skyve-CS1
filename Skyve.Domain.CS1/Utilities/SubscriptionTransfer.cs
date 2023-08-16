using System.Collections.Generic;

namespace Skyve.Domain.CS1.Utilities;
public class SubscriptionTransfer
{
	public List<ulong>? SubscribeTo { get; set; }
	public List<ulong>? UnsubscribingFrom { get; set; }
}
