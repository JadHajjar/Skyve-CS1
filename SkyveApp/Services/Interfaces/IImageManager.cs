using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Services.Interfaces;
public interface IImageManager
{
	Task<bool> Ensure(string? url, bool localOnly = false, string? fileName = null, bool square = true);
	FileInfo File(string url, string? fileName = null);
	Bitmap? GetCache(string key);
	Task<Bitmap?> GetImage(string? url);
	Task<Bitmap?> GetImage(string? url, bool localOnly, string? fileName = null);
}
