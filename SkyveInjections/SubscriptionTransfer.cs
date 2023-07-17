using System.Collections.Generic;

namespace SkyveShared;
public class SubscriptionTransfer
{
	public List<ulong> SubscribeTo { get; set; }
	public List<ulong> UnsubscribingFrom { get; set; }
}
