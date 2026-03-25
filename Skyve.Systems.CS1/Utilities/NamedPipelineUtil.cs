using Extensions;

using Skyve.Domain.Systems;

using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Skyve.Systems.CS1.Utilities;

public class NamedPipelineUtil(ICentralManager centralManager)
{
	public const string PIPE_NAME = "SkyveCs1ProtocolPipe";

	public bool SendToRunningInstance(string[] args)
	{
		try
		{
			using var pipeClient = new NamedPipeClientStream(".", PIPE_NAME, PipeDirection.Out, PipeOptions.Asynchronous);

			pipeClient.Connect(1500);

			using var writer = new StreamWriter(pipeClient);

			writer.WriteLine(args.ListStrings(x => $"\"{x}\"", " "));


			return true;
		}
		catch
		{
			return false;
		}
	}

	public void StartNamedPipeServer()
	{
		Task.Run(PipeServerLoop);
	}

	private async void PipeServerLoop()
	{
		try
		{
			var pipeSecurity = new PipeSecurity();

			pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null), PipeAccessRights.FullControl, AccessControlType.Allow));

			while (true)
			{
				using var pipeServer = new NamedPipeServerStream(PIPE_NAME, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 0, 0, pipeSecurity);

				pipeServer.WaitForConnection();

				using var reader = new StreamReader(pipeServer);

				var message = reader.ReadLine();

				CommandUtil.Parse(SplitArgs(message));

				centralManager.RunCommands();

				Application.OpenForms[0].TryInvoke(Application.OpenForms[0].ShowUp);

				pipeServer.Disconnect();
			}
		}
		catch { }
	}

	private string[] SplitArgs(string input)
	{
		var args = new List<string>();
		var pattern = @"[\""].+?[\""]|[^ ]+";

		foreach (Match match in Regex.Matches(input, pattern))
		{
			args.Add(match.Value.Trim('"'));
		}

		return [.. args];
	}
}
