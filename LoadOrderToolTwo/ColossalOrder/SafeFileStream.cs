using Extensions;

using LoadOrderToolTwo.Utilities.Managers;

using System.IO;

namespace LoadOrderToolTwo.ColossalOrder;

public class SafeFileStream : Stream
{
	private readonly string _mainFilePath;
	private readonly string _tempFilePath;
	private readonly string _backupFilePath;

	private readonly FileStream _tempStream;
	private readonly FileStream _mainStream;
	private bool saved;

	public SafeFileStream(string path, FileMode mode)
	{
		_mainFilePath = path;
		_tempFilePath = Path.GetTempFileName();
		_backupFilePath = Path.GetTempFileName();

		// Open the main file stream
		_mainStream = new FileStream(_mainFilePath, mode);

		// If the file exists, create a backup copy
		if (File.Exists(_mainFilePath))
		{
			File.Copy(_mainFilePath, _backupFilePath, true);
		}

		// Open the temporary file stream
		_tempStream = new FileStream(_tempFilePath, FileMode.Create);
	}

	public override bool CanRead => _mainStream.CanRead;

	public override bool CanSeek => _mainStream.CanSeek;

	public override bool CanWrite => _mainStream.CanWrite;

	public override long Length => _mainStream.Length;

	public override long Position
	{
		get => _mainStream.Position;
		set => _mainStream.Position = value;
	}

	public override void Flush()
	{
		_tempStream.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return _mainStream.Read(buffer, offset, count);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return _mainStream.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		_mainStream.SetLength(value);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		_tempStream.Write(buffer, offset, count);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && !saved)
		{
			try
			{
				// Flush and close the temporary stream
				_tempStream.Flush();
				_tempStream.Close();

				// Create a backup copy of the main file
				if (File.Exists(_mainFilePath))
				{
					File.Copy(_mainFilePath, _backupFilePath, true);
				}

				_mainStream?.Dispose();

				// Replace the main file with the temporary file
				File.Copy(_tempFilePath, _mainFilePath, true);
			}
			finally
			{
				saved = true;

				File.Delete(_backupFilePath);
				File.Delete(_tempFilePath);
			}
		}

		base.Dispose(disposing);
	}
}

