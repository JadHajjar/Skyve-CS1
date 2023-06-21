using SkyveApp.Domain.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Services.Interfaces;
public interface ISubscriptionsManager
{
	List<ulong> PendingSubscribingTo { get; }
	List<ulong> PendingUnsubscribingFrom { get; }
	bool Redownload { get; set; }
	List<ulong> SubscribingTo { get; }
	bool SubscriptionsPending { get; }
	List<ulong> UnsubscribingFrom { get; }

	void CancelPendingItems();
	bool IsSubscribing(IPackage package);
	void Start();
	bool Subscribe(IEnumerable<ulong> ids);
	bool UnSubscribe(IEnumerable<ulong> ids);
}
