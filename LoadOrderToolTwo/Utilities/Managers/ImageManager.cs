using Extensions;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace LoadOrderToolTwo.Utilities.Managers;

public static class ImageManager
{
	private static readonly Dictionary<string, object> _lockObjects = new();
	private static readonly HashSet<string> _badURLs;

	static ImageManager()
	{
		ISave.Load(out string[] badURLs, "BadSteamURLs.json");

		_badURLs = new(badURLs ?? new string[0]);
	}

	private static object LockObj(string path)
	{
		lock (_lockObjects)
		{
			if (!_lockObjects.ContainsKey(path))
			{
				_lockObjects.Add(path, new object());
			}

			return _lockObjects[path];
		}
	}

	public static Bitmap GetIcon(string name)
	{
		return (Bitmap)Properties.Resources.ResourceManager.GetObject(UI.FontScale >= 1.25 ? name : $"{name}_16", Properties.Resources.Culture);
	}

	public static FileInfo File(string url)
	{
		var filePath = Path.Combine(ISave.DocsFolder, "Thumbs", Path.GetFileNameWithoutExtension(url.TrimEnd('/', '\\')) + Path.GetExtension(url).IfEmpty(".png"));

		return new FileInfo(filePath);
	}

	public static Bitmap? GetImage(string? url) => GetImage(url, false);

	public static Bitmap? GetImage(string? url, bool localOnly)
	{
		if (url is null || !Ensure(url, localOnly))
		{
			return null;
		}

		var filePath = File(url);

		lock (LockObj(url))
		{
			if (filePath.Exists)
			{
				try
				{
					return (Bitmap)Image.FromFile(filePath.FullName);
				}
				catch { }
			}
		}

		return null;
	}

	public static bool Ensure(string? url, bool localOnly = false)
	{
		if (url is null or "" || _badURLs.Contains(url!))
		{
			return false;
		}

		var filePath = File(url);

		lock (LockObj(url))
		{
			if (filePath.Exists)
			{
				return true;
			}

			if (localOnly)
			{
				return false;
			}

			var tries = 1;
		start:

			if (!ConnectionHandler.IsConnected)
			{
				return false;
			}

			try
			{
				using var webClient = new WebClient();
				var imageData = webClient.DownloadData(url);

				using var ms = new MemoryStream(imageData);
				using var img = Image.FromStream(ms);

				var squareSize = img.Width <= 64 ? img.Width : 256;
				var size = img.Size.GetProportionalDownscaledSize(squareSize);
				using var image = new Bitmap(squareSize, squareSize);

				using (var imageGraphics = Graphics.FromImage(image))
				{
					imageGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
					imageGraphics.DrawImage(img, new Rectangle((squareSize - size.Width) / 2, (squareSize - size.Height) / 2, size.Width, size.Height));
				}

				Directory.GetParent(filePath.FullName).Create();

				if (filePath.FullName.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || filePath.FullName.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase))
				{
					image.Save(filePath.FullName, System.Drawing.Imaging.ImageFormat.Jpeg);
				}
				else
				{
					image.Save(filePath.FullName);
				}

				return true;
			}
			catch (Exception ex)
			{
				if (ex is WebException we && we.Response is HttpWebResponse hwr && hwr.StatusCode == HttpStatusCode.BadGateway)
				{
					Thread.Sleep(1000);
					goto start;
				}
				else if (tries < 2)
				{
					tries++;
					goto start;
				}
				else
				{
					try
					{
						if (ConnectionHandler.IsConnected)
						{
							_badURLs.Add(url);

							ISave.Save(_badURLs.ToList(), "BadSteamURLs.json");
						}
					}
					catch (Exception ex2) { Log.Exception(ex2, "Too many images are failing to load"); }

					return false;
				}
			}
		}
	}
}