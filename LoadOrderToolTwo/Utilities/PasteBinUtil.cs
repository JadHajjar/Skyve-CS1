using System;
using System.IO;
using System.Net;
using System.Text;

namespace LoadOrderToolTwo.Utilities;
internal class PasteBinUtil
{
	private const string ApiBaseUrl = "https://pastebin.com/api";
	private readonly string _apiDevKey;
	private readonly string _apiUserKey;

	public PasteBinUtil(string apiDevKey, string apiUserKey = null)
	{
		_apiDevKey = apiDevKey;
		_apiUserKey = apiUserKey;
	}

	public string CreatePaste(string pasteCode, string pasteName = null, string pasteFormat = null, int pasteExpireDate = 0, bool pastePrivate = false)
	{
		var requestUrl = $"{ApiBaseUrl}/api_post.php";
		var requestParameters = new StringBuilder();
		requestParameters.Append($"api_dev_key={_apiDevKey}");
		if (!string.IsNullOrEmpty(_apiUserKey))
		{
			requestParameters.Append($"&api_user_key={_apiUserKey}");
		}
		requestParameters.Append($"&api_option=paste");
		requestParameters.Append($"&api_paste_code={Uri.EscapeDataString(pasteCode)}");
		if (!string.IsNullOrEmpty(pasteName))
		{
			requestParameters.Append($"&api_paste_name={Uri.EscapeDataString(pasteName)}");
		}
		if (!string.IsNullOrEmpty(pasteFormat))
		{
			requestParameters.Append($"&api_paste_format={Uri.EscapeDataString(pasteFormat)}");
		}
		if (pasteExpireDate > 0)
		{
			requestParameters.Append($"&api_paste_expire_date={pasteExpireDate}");
		}
		if (pastePrivate)
		{
			requestParameters.Append($"&api_paste_private=1");
		}
		var postData = Encoding.UTF8.GetBytes(requestParameters.ToString());
		var request = (HttpWebRequest)WebRequest.Create(requestUrl);
		request.Method = "POST";
		request.ContentType = "application/x-www-form-urlencoded";
		request.ContentLength = postData.Length;
		using (var stream = request.GetRequestStream())
		{
			stream.Write(postData, 0, postData.Length);
		}
		var response = (HttpWebResponse)request.GetResponse();
		var responseStream = response.GetResponseStream();
		if (responseStream == null)
		{
			throw new Exception("Response stream is null.");
		}
		using (var reader = new StreamReader(responseStream))
		{
			var responseText = reader.ReadToEnd();
			if (responseText.StartsWith("Bad API request"))
			{
				throw new Exception($"Error creating paste: {responseText}");
			}
			return responseText.Trim();
		}
	}

	public string ReadPaste(string pasteKey)
	{
		var requestUrl = $"{ApiBaseUrl}/api_raw.php";
		var requestParameters = new StringBuilder();
		requestParameters.Append($"api_dev_key={_apiDevKey}");
		requestParameters.Append($"&api_option=show_paste");
		requestParameters.Append($"&api_paste_key={pasteKey}");
		var postData = Encoding.UTF8.GetBytes(requestParameters.ToString());
		var request = (HttpWebRequest)WebRequest.Create(requestUrl);
		request.Method = "POST";
		request.ContentType = "application/x-www-form-urlencoded";
		request.ContentLength = postData.Length;
		using (var stream = request.GetRequestStream())
		{
			stream.Write(postData, 0, postData.Length);
		}
		var response = (HttpWebResponse)request.GetResponse();
		var responseStream = response.GetResponseStream();
		if (responseStream == null)
		{
			throw new Exception("Response stream is null.");
		}

		using (var reader = new StreamReader(responseStream))
		{
			var responseText = reader.ReadToEnd();
			return responseText.Trim();
		}
	}
}