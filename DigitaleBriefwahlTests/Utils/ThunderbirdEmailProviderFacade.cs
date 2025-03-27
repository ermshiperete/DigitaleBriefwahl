using DigitaleBriefwahl.Mail;

namespace DigitaleBriefwahlTests.Utils
{
	public class ThunderbirdEmailProviderFacade : ThunderbirdEmailProvider
	{
		private readonly string _path;
		private bool _hasFlatpak;

		public ThunderbirdEmailProviderFacade()
		{

		}

		public ThunderbirdEmailProviderFacade(string path)
		{
			_path = path;
		}

		public void SetFlatpak(bool hasFlatpak)
		{
			_hasFlatpak = hasFlatpak;
		}

		protected override bool IsThunderbirdFlatpak => _hasFlatpak;

		public string GetEmailCommand() => string.IsNullOrEmpty(_path) ? EmailCommand : _path;
		public string GetExtraEmailArgs() => ExtraEmailArgs;
		public bool GetIsApplicable() => IsApplicable;
	}

}