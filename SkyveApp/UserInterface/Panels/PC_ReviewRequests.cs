using SkyveApp.Domain.Compatibility;

using SlickControls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Panels;
public partial class PC_ReviewRequests : PanelContent
{
	private readonly ReviewRequest[]? _reviewRequests;

	public PC_ReviewRequests(ReviewRequest[]? reviewRequests)
	{
		InitializeComponent();

		_reviewRequests = reviewRequests;
	}


}
