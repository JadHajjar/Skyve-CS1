using Extensions;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SkyveApp.Utilities.Managers;

public static class ImageManager
{
	private static readonly Dictionary<string, object> _lockObjects = new();
	private static readonly System.Timers.Timer _cacheClearTimer;
	private static readonly Dictionary<string, (Bitmap image, DateTime lastAccessed)> _cache = new Dictionary<string, (Bitmap, DateTime)>();
	private static readonly TimeSpan _expirationTime = TimeSpan.FromMinutes(1);
	private static readonly HttpClient _httpClient = new();
	private static readonly SteamImageProcessor _imageProcessor = new();

	static ImageManager()
	{
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

	public static FileInfo File(string url, string? fileName = null)
	{
		var filePath = LocationManager.Combine(ISave.DocsFolder, "Thumbs", fileName ?? (Path.GetFileNameWithoutExtension(RemoveQueryParamsFromUrl(url).TrimEnd('/', '\\')) + Path.GetExtension(url).IfEmpty(".png")));

		return new FileInfo(filePath);
	}

	public static string RemoveQueryParamsFromUrl(string url)
	{
		var index = url.IndexOf('?');
		return index >= 0 ? url.Substring(0, index) : url;
	}

	public static async Task<Bitmap?> GetImage(string? url)
	{
		var image = await GetImage(url, false);

		if (image is not null)
		{
			return new(image);
		}

		return null;
	}

	public static async Task<Bitmap?> GetImage(string? url, bool localOnly, string? fileName = null)
	{
		if (url is null || !await Ensure(url, localOnly, fileName))
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

	public static async Task<bool> Ensure(string? url, bool localOnly = false, string? fileName = null, bool square = true)
	{
		if (url is null or "")
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
				_imageProcessor.Add(new(url, fileName, square));

				return false;
			}
		}

		var tries = 1;
		start:

		if (!ConnectionHandler.IsConnected)
		{
			return false;
		}

		try
		{
			using var ms = await _httpClient.GetStreamAsync(url);
			using var img = Image.FromStream(ms);

			var squareSize = img.Width <= 64 ? img.Width : 512;
			var size = string.IsNullOrEmpty(fileName) ? img.Size.GetProportionalDownscaledSize(squareSize) : img.Size;
			using var image = string.IsNullOrEmpty(fileName) ? square ? new Bitmap(squareSize, squareSize) : new Bitmap(size.Width, size.Height) : img;

			if (string.IsNullOrEmpty(fileName))
			{
				using var imageGraphics = Graphics.FromImage(image);

				imageGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				imageGraphics.DrawImage(img, square
					? new Rectangle((squareSize - size.Width) / 2, (squareSize - size.Height) / 2, size.Width, size.Height)
					: new Rectangle(Point.Empty, size));
			}

			Directory.GetParent(filePath.FullName).Create();

			lock (LockObj(url))
			{
				if (filePath.FullName.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || filePath.FullName.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase))
				{
					image.Save(filePath.FullName, System.Drawing.Imaging.ImageFormat.Jpeg);
				}
				else
				{
					image.Save(filePath.FullName);
				}
			}

			Program.MainForm?.TryInvoke(() => Program.MainForm.Invalidate(true));

			return true;
		}
		catch (Exception ex)
		{
			if (ex is WebException we && we.Response is HttpWebResponse hwr && hwr.StatusCode == HttpStatusCode.BadGateway)
			{
				await Task.Delay(1000);

				goto start;
			}
			else if (tries < 2)
			{
				tries++;
				goto start;
			}

			return false;
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
		try
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
		catch { }
	}
}