// Copyright (c) 2021 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DigitaleBriefwahl.Utils
{
	public static class Downloader
	{
		public static async Task DownloadFile(Uri uri, string targetFile)
		{
			using var httpClient = new HttpClient();
			using var httpResponse = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

			var contentLength = httpResponse.Content.Headers.ContentLength;
			using var download = await httpResponse.Content.ReadAsStreamAsync();
			using var stream = new FileStream(targetFile, FileMode.CreateNew);

			// Convert absolute progress (bytes downloaded) into relative progress (0% - 100%)
			var prevProgress = 0;
			var relativeProgress = new Progress<int>(value =>
			{
				var diff = Math.Max((int)(value  * 100 / contentLength.Value) - prevProgress, 0);
				Console.Write(new string('.', diff));
				prevProgress += diff;
			});
			// Use extension method to report progress while downloading
			await CopyToAsync(download, stream, 81920, relativeProgress);
			Console.WriteLine();
		}

		private static async Task CopyToAsync(Stream source, Stream destination, int bufferSize,
			IProgress<int> progress)
		{
			var buffer = new byte[bufferSize];
			var totalBytesRead = 0;
			int bytesRead;
			while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) != 0)
			{
				await destination.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
				totalBytesRead += bytesRead;
				progress?.Report(totalBytesRead);
			}
		}

	}
}