// Copyright (c) 2018-2021 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DigitaleBriefwahl.ExceptionHandling;
using SIL.PlatformUtilities;

namespace DigitaleBriefwahl.Launcher
{
	/// <summary>
	/// The launcher app has to be installed on the user's system. It will take the URL file,
	/// download the voting app distributed as a zip file, extract and launch it. We expect that
	/// the launcher has to be installed once and can then be used for several years while the
	/// voting app can be updated every year.
	/// </summary>
	internal static class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			ExceptionLogging.Initialize("5012aef9a281f091c1fceea40c03003b", "Launcher", args);

			Logger.Log($"{SquirrelInstallerSupport.Executable} {string.Join(" ", args.Select(s => $"\"{s}\""))}");

			using var writer = new StreamWriter(Logger.LogFile, true);
			var options = Options.ParseCommandLineArgs(writer, args);
			if (options == null)
			{
				Console.WriteLine("Kann übergebene Parameter nicht interpretieren.");
				Logger.Log("Couldn't parse passed in arguments");
				return;
			}

			if (!string.IsNullOrEmpty(options.RunApp) && !File.Exists(options.RunApp))
			{
				Console.WriteLine($"Datei '{options.RunApp}' nicht gefunden.");
				Logger.Log($"Can't find file {options.RunApp}");
				return;
			}

			if (!string.IsNullOrEmpty(options.RunDirectory) && !Directory.Exists(options.RunDirectory))
			{
				Console.WriteLine($"Verzeichnis '{options.RunDirectory}' nicht gefunden.");
				Logger.Log($"Can't find directory {options.RunDirectory}");
				return;
			}

			if (!string.IsNullOrEmpty(options.UrlFile) && !options.UrlFile.StartsWith("wahlurl://")
				&& !File.Exists(options.UrlFile))
			{
				Console.WriteLine($"URL-Datei '{options.UrlFile}' nicht gefunden.");
				Logger.Log($"Can't find URL file {options.UrlFile}");
				return;
			}

			if (options.IsInstall || options.FirstRun)
				Console.WriteLine("Anwendung wird installiert...");

			var didUpdate = UpdateAndLaunchApp(options);

			if (Debugger.IsAttached || didUpdate)
				return;

			if (options.IsInstall || options.FirstRun)
				Console.WriteLine("Installation erfolgreich.");

			Console.WriteLine("Zum Beenden 'Enter'-Taste drücken");
			Console.ReadLine();
		}

		private static bool UpdateAndLaunchApp(Options options)
		{
			var retVal = false;
			try
			{
				using var launcher = new Launcher(options.RunDirectory);
				Console.WriteLine(options.SkipUpdateCheck || options.IsInstall
					? "Anwendung wird geladen..."
					: "Überprüfung auf Updates...");

				var didUpdate = UpdateApp(options, launcher).GetAwaiter().GetResult();

				if (string.IsNullOrEmpty(options.RunApp) &&
					string.IsNullOrEmpty(options.RunDirectory) || didUpdate)
				{
					retVal = didUpdate;
				}
				else if (!string.IsNullOrEmpty(options.PackageDir))
					SquirrelInstallerSupport.ExecuteUpdatedApp(options);
				else
				{
					Console.WriteLine("Anwendung wird gestartet...");
					launcher.LaunchVotingApp();
				}
			}
			catch (CannotUnloadAppDomainException e)
			{
				// just ignore
				Logger.Log($"Got {e.GetType()} trying to dispose launcher - IGNORED ({e.Message})");
			}
			return retVal;
		}

		[DllImport("wininet.dll")]
		private static extern bool InternetGetConnectedState(out int description, int reservedValue);

		private static async Task<bool> UpdateApp(Options options, Launcher launcher)
		{
			Task<bool> updateManagerTask = null;
			var didUpdate = false;

			Task<string> unzipVotingApp = null;
			Task<string> downloadVotingApp = null;

			if (Platform.IsWindows)
			{
				if (options.SkipUpdateCheck)
					Console.WriteLine("Ignoriere eventuell vorhandene Updates.");
				else
				{
					if (InternetGetConnectedState(out var flags, 0))
					{
						try
						{
							updateManagerTask = SquirrelInstallerSupport.HandleSquirrelInstallEvent(options);
						}
						catch (HttpRequestException e)
						{
							// some network problem - ignore
							Console.WriteLine("Netzwerkproblem - Überprüfung auf Updates wird übersprungen.");
							Logger.Log(
								$"Network problem - skipping updates: {e.Message} ({e.InnerException?.Message})");
						}
						catch (WebException we)
						{
							// some network problem - ignore
							Console.WriteLine("Netzwerkproblem - Überprüfung auf Updates wird übersprungen.");
							Logger.Log($"Network problem - skipping updates: {we.Message}");
						}
					}
					else
					{
						Console.WriteLine("Netzwerk nicht verbunden - Überprüfung auf Updates wird übersprungen.");
						Logger.Log($"Network not connected - skipping updates (0x{flags:X2})");
					}
				}
			}
			else
				Console.WriteLine("Keine Updates vorhanden.");

			if (!string.IsNullOrEmpty(options.UrlFile) && File.Exists(options.UrlFile))
				downloadVotingApp = DownloadVotingAppFromUrlFile(options.UrlFile);

			if (!string.IsNullOrEmpty(options.RunApp) && File.Exists(options.RunApp))
				unzipVotingApp = launcher.UnzipVotingAppAsync(options.RunApp);

			if (updateManagerTask != null)
			{
				try
				{
					didUpdate = await updateManagerTask;
				}
				catch (HttpRequestException e)
				{
					// just ignore
					Logger.Log($"Got {e.GetType()} in trying to update launcher - IGNORED ({e.Message})");
					Console.WriteLine("Netzwerkprobleme - kann Updates nicht herunterladen.");
					didUpdate = false;
				}
			}

			if (downloadVotingApp != null)
			{
				try
				{
					options.RunApp = await downloadVotingApp;
					if (!string.IsNullOrEmpty(options.RunApp) && File.Exists(options.RunApp))
						unzipVotingApp = launcher.UnzipVotingAppAsync(options.RunApp);
				}
				catch (HttpRequestException e)
				{
					// just ignore
					Logger.Log($"Got {e.GetType()} in trying to download voting app");
					ReportNetworkProblemsForDownload(GetUrlFromFile(options.UrlFile), GetBallotFilename(options.UrlFile));
				}
			}

			if (unzipVotingApp != null)
				options.RunDirectory = await unzipVotingApp;

			if (didUpdate && !string.IsNullOrEmpty(options.PackageDir))
				SquirrelInstallerSupport.ExecuteUpdatedApp(options);

			return didUpdate;
		}

		private static async Task<string> DownloadVotingAppFromUrlFile(string urlFile)
		{
			var url = GetUrlFromFile(urlFile);
			var uri = new Uri(url);
			var ballotFile = GetBallotFilename(urlFile);

			if (Platform.IsWindows && !InternetGetConnectedState(out var flags, 0))
			{
				Logger.Log($"Network not connected - skipping download from URL (0x{flags:X2})");
				ReportNetworkProblemsForDownload(url, ballotFile);
				return null;
			}

			Console.WriteLine("Die Wahlunterlagen werden heruntergeladen...");
			Logger.Log($"Downloading from {uri}...");
			var targetFile = await DownloadVotingApp(uri, ballotFile);
			if (string.IsNullOrEmpty(targetFile))
			{
				Logger.Log("Didn't get a file after download - network problems?");
				ReportNetworkProblemsForDownload(uri.AbsoluteUri, ballotFile);
			}
			else
			{
				Logger.Log($"Download of '{targetFile}' finished.");
				Console.WriteLine("Download abgeschlossen.");
			}

			return targetFile;
		}

		private static string GetBallotFilename(string urlFile)
		{
			return Path.GetFileNameWithoutExtension(urlFile) + ".wahl";
		}

		private static string GetUrlFromFile(string urlFile)
		{
			return File.ReadAllText(urlFile).Replace("wahlurl://", "https://");
		}

		private static void ReportNetworkProblemsForDownload(string url, string ballotFile)
		{
			Console.WriteLine("Netzwerkprobleme - kann Stimmzettel nicht automatisch herunterladen.");
			Console.WriteLine($"Bitte Stimmzettel von {url} herunterladen und als '{ballotFile}' abspeichern.");
			Console.WriteLine("Diese Datei kann dann aufgerufen werden.");
		}

		private static async Task<string> DownloadVotingApp(Uri uri, string ballotFile)
		{
			var targetFile = Path.Combine(Path.GetTempPath(), ballotFile);
			// Assign values to these objects here so that they can
			// be referenced in the finally block
			Stream remoteStream = null;
			Stream localStream = null;
			HttpWebResponse response = null;

			// Use a try/catch/finally block as both the WebRequest and Stream
			// classes throw exceptions upon error
			try
			{
				var tmpTargetFile = targetFile + ".~tmp";

				// Create a request for the specified remote file name
				var request = WebRequest.Create(uri) as HttpWebRequest;

				// REVIEW: would it be better to use ETag in the HTTP header instead of relying
				// on the timestamp for caching and continuing incomplete downloads?

				var appendFile = false;
				long tmpFileLength = 0;
				if (File.Exists(tmpTargetFile))
				{
					// Interrupted download
					Logger.Log($"Found incomplete download file, continuing download of {targetFile}...");

					var fi = new FileInfo(tmpTargetFile);
					request.Headers.Add("If-Unmodified-Since", fi.LastWriteTimeUtc.ToString("r"));
					tmpFileLength = fi.Length;
					request.AddRange(tmpFileLength);
					appendFile = true;
				}
				else if (File.Exists(targetFile))
				{
					Logger.Log($"Checking {targetFile}, downloading if newer...");

					var fi = new FileInfo(targetFile);
					request.IfModifiedSince = fi.LastWriteTimeUtc;
				}
				else
					Logger.Log($"Downloading {targetFile}...");

				// Send the request to the server and retrieve the
				// WebResponse object
				response = (HttpWebResponse) await Task.Factory.FromAsync(
					request.BeginGetResponse, request.EndGetResponse, null);

				if (File.Exists(tmpTargetFile) && (response.StatusCode == HttpStatusCode.PreconditionFailed ||
					response.LastModified > new FileInfo(tmpTargetFile).LastWriteTimeUtc))
				{
					// file got changed on the server since we downloaded the incomplete file
					Logger.Log($"File {targetFile} changed on server since start of incomplete download; initiating complete download");
					File.Delete(tmpTargetFile);
					response.Close();
					return await DownloadVotingApp(uri, ballotFile);
				}

				// Once the WebResponse object has been retrieved,
				// get the stream object associated with the response's data
				remoteStream = response.GetResponseStream();
				Console.Write("...");

				// Create the local file
				Directory.CreateDirectory(Path.GetDirectoryName(targetFile));
				localStream = File.OpenWrite(tmpTargetFile);
				if (appendFile && tmpFileLength < response.ContentLength)
					localStream.Position = localStream.Length;

				// Allocate a 10k buffer
				const int bufferSize = 10240;
				var buffer = new byte[bufferSize];
				int bytesRead;
				var increment = (float)100 / (response.ContentLength / bufferSize);
				var totalProgress = 0f;

				// Simple do/while loop to read from stream until
				// no bytes are returned
				do
				{
					// Read data (up to 10k) from the stream
					bytesRead = await remoteStream.ReadAsync(buffer, 0, buffer.Length);

					if (bytesRead <= 0)
						break;

					// Write the data to the local file
					await localStream.WriteAsync(buffer, 0, bytesRead);
					Console.Write(new string('.', (int)(totalProgress + increment) - (int)totalProgress));
					totalProgress += increment;
				} while (bytesRead > 0);

				Console.WriteLine();

				var localStreamLength = localStream.Length;
				localStream.Close();

				if (localStreamLength < response.ContentLength + tmpFileLength)
				{
					Logger.Log($"WARNING: couldn't download complete file {targetFile}, continuing next time");
					Logger.Log($"{targetFile}: Expected file length: {response.ContentLength + tmpFileLength}, but received {localStreamLength} bytes");
					return null;
				}

				File.Delete(targetFile);
				File.Move(tmpTargetFile, targetFile);
				return targetFile;
			}
			catch (WebException wex)
			{
				Console.WriteLine();
				if (wex.Status == WebExceptionStatus.ProtocolError)
				{
					var resp = wex.Response as HttpWebResponse;
					if (resp.StatusCode == HttpStatusCode.NotModified)
					{
						Logger.Log($"File {targetFile} not modified.");
						return targetFile;
					}
				}
				else if (wex.Status == WebExceptionStatus.ConnectFailure || wex.Status == WebExceptionStatus.NameResolutionFailure)
				{
					// We probably don't have a network connection (despite the check in the caller).
					Logger.Log(File.Exists(targetFile)
						? $"Could not retrieve latest {targetFile}. No network connection. Keeping existing file."
						: $"Could not retrieve latest {targetFile}. No network connection.");
					Console.WriteLine("Kann Netzwerkverbindung mit dem Server nicht herstellen. Download nicht erfolgreich.");
					return null;
				}
				if (wex.Response != null)
				{
					using var streamReader = new StreamReader(wex.Response.GetResponseStream());
					var html = await streamReader.ReadToEndAsync();
					Logger.Log($"Could not download from {uri}: {wex.Message} Server responds '{html}'. Status {wex.Status}.");
					Console.WriteLine("Server meldet einen Fehler. Download nicht erfolgreich.");
				}
				else
				{
					Logger.Log($"Could not download from {uri}. Exception: {wex.Message} No server response. Status {wex.Status}.");
					Console.WriteLine("Keine Antwort vom Server. Download nicht erfolgreich.");
				}
				return null;
			}
			finally
			{
				// Close the response and streams objects here
				// to make sure they're closed even if an exception
				// is thrown at some point
				response?.Close();
				remoteStream?.Close();
				localStream?.Close();
			}
		}
	}
}