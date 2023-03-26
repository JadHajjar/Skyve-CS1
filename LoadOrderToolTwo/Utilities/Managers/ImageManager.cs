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
	private static readonly System.Timers.Timer _cacheClearTimer;
	private static readonly Dictionary<string, (Bitmap image, DateTime lastAccessed)> _cache = new Dictionary<string, (Bitmap, DateTime)>();
	private static readonly TimeSpan _expirationTime = TimeSpan.FromMinutes(1);

	static ImageManager()
	{
		ISave.Load(out string[] badURLs, "BadSteamURLs.json");

		_badURLs = new(badURLs ?? new string[0]);

		_cacheClearTimer = new System.Timers.Timer(_expirationTime.TotalMilliseconds);
		_cacheClearTimer.Elapsed += CacheClearTimer_Elapsed;
		_cacheClearTimer.Start();
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

	public static FileInfo File(string url, string? fileName = null)
	{
		var filePath = Path.Combine(ISave.DocsFolder, "Thumbs", fileName ?? (Path.GetFileNameWithoutExtension(url.TrimEnd('/', '\\')) + Path.GetExtension(url).IfEmpty(".png")));

		return new FileInfo(filePath);
	}

	public static Bitmap? GetImage(string? url)
	{
		var image = GetImage(url, false);

		if (image is not null)
		{
			return new(image);
		}

		return null;
	}

	public static Bitmap? GetImage(string? url, bool localOnly, string? fileName = null)
	{
		if (url is null || !Ensure(url, localOnly, fileName))
		{
			return null;
		}

		var cache = GetCache(url);

		if (cache != null)
		{
			return cache;
		}

		var filePath = File(url, fileName);

		if (filePath.Exists)
		{
			lock (LockObj(url))
			{
				try
				{
					return AddCache(url, (Bitmap)Image.FromFile(filePath.FullName));
				}
				catch { }
			}
		}

		return null;
	}

	public static bool Ensure(string? url, bool localOnly = false, string? fileName = null, bool square = true)
	{
		if (url is null or "" || _badURLs.Contains(url!))
		{
			return false;
		}

		var filePath = File(url, fileName);

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
				using var image = square ? new Bitmap(squareSize, squareSize) : new Bitmap(size.Width, size.Height);

				using (var imageGraphics = Graphics.FromImage(image))
				{
					imageGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
					imageGraphics.DrawImage(img, square
						? new Rectangle((squareSize - size.Width) / 2, (squareSize - size.Height) / 2, size.Width, size.Height)
						: new Rectangle(Point.Empty, size));
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

	private static Bitmap AddCache(string key, Bitmap image)
	{
		if (key is null or "")
		{
			return image;
		}

		if (_cache.ContainsKey(key))
		{
			_cache[key] = (image, DateTime.Now);
		}
		else
		{
			_cache.Add(key, (image, DateTime.Now));
		}

		return image;
	}

	public static Bitmap? GetCache(string key)
	{
		if (_cache.TryGetValue(key, out var value))
		{
			if (DateTime.Now - value.lastAccessed > _expirationTime)
			{
				value.image.Dispose();
				_cache.Remove(key);
				return null;
			}

			value.lastAccessed = DateTime.Now;
			return value.image;
		}

		return null;
	}

	private static void CacheClearTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
	{
		var keys = _cache.Keys.ToList();

		foreach (var key in keys)
		{
			if (_cache.TryGetValue(key, out var value))
			{
				if (DateTime.Now - value.lastAccessed > _expirationTime)
				{
					value.image.Dispose();
					_cache.Remove(key);
				}
			}
		}
	}
}