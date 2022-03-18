// Copyright (c) 2022 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LibGit2Sharp;
using Octokit;
using Credentials = Octokit.Credentials;

namespace Packer
{

	public class GitHubGist
	{
		private readonly GitHubClient _client;
		private readonly PushOptions  _pushOptions;
		private          User         _user;

		public GitHubGist()
		{
			_client = new GitHubClient(new ProductHeaderValue("DigitaleBriefwahl.Packer"));
			_pushOptions = new PushOptions();
		}

		public async Task Initialize(string token)
		{
			var tokenAuth = new Credentials(token);
			_client.Credentials = tokenAuth;
			_user = await _client.User.Current();
			_pushOptions.CredentialsProvider =
				(url, user, cred) => new UsernamePasswordCredentials
					{ Username = _user.Name, Password = token };
		}

		public async Task<string> Upload(string title, string zipFile)
		{
			var userSignature = new LibGit2Sharp.Signature(_user.Name,
				_user.Email ?? $"{_user.Login}@users.noreply.github.com", DateTimeOffset.Now);

			var gist = await GetOrCreateGist(title);
			var repoDir = Path.Combine(Path.GetTempPath(), gist.Id);
			if (Directory.Exists(repoDir))
			{
				using var tmpRepo = new LibGit2Sharp.Repository(repoDir);
				var pullOptions = new PullOptions {
					MergeOptions = new MergeOptions {
						FastForwardStrategy = FastForwardStrategy.FastForwardOnly
					}
				};
				var mergeResult = Commands.Pull(tmpRepo, userSignature, pullOptions);
				if (mergeResult.Status == MergeStatus.Conflicts)
				{
					Directory.Delete(repoDir, true);
					return await Upload(title, zipFile);
				}
			}
			else
			{
				LibGit2Sharp.Repository.Clone(gist.GitPullUrl, repoDir);
			}

			var localZipFile = Path.GetFileName(zipFile);
			var localZipFilePath = Path.Combine(repoDir, localZipFile);
			if (File.Exists(localZipFilePath))
				File.Delete(localZipFilePath);
			File.Copy(zipFile, localZipFilePath);

			using var repo = new LibGit2Sharp.Repository(repoDir);

			var status = repo.RetrieveStatus();
			if (!status.Modified.Select(file => file.FilePath).Any() &&
				!status.Untracked.Select(file => file.FilePath).Any())
			{
				Console.WriteLine("File didn't change, re-using previous one.");
				return await GetGistUrl(title, zipFile);
			}

			Commands.Stage(repo, localZipFile);

			repo.Commit($"{title} ({Version})", userSignature, userSignature);

			repo.Network.Push(repo.Head, _pushOptions);
			return await GetGistUrl(title, zipFile);
		}

		private async Task<Gist> GetOrCreateGist(string title)
		{
			var gist = await GetGist(title);
			if (gist != null)
				return gist;

			var newGist = new NewGist { Description = title, Public = false };
			newGist.Files.Add(title, title);
			var createdGist = await _client.Gist.Create(newGist);
			return createdGist;
		}

		private async Task<Gist> GetGist(string title)
		{
			var allGists = await _client.Gist.GetAll();
			return allGists.FirstOrDefault(gist => gist.Description == title);
		}

		private static string Version => FileVersionInfo
			.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

		private async Task<string> GetGistUrl(string title, string zipFile)
		{
			var gist = await GetGist(title);
			var filename = Path.GetFileName(zipFile);
			return gist.Files.TryGetValue(filename, out var gistFile) ? gistFile.RawUrl : null;
		}
	}
}