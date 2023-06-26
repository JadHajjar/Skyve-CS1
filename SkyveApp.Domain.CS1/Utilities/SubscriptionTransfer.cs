using System.Collections.Generic;

namespace SkyveApp.Domain.Utilities;
public class SubscriptionTransfer
{
	public List<ulong>? SubscribeTo { get; set; }
	public List<ulong>? UnsubscribingFrom { get; set; }
}
