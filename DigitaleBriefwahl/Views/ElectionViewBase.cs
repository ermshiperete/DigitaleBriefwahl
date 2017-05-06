// Copyright (c) 2016 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using Eto.Drawing;
using Eto.Forms;
using DigitaleBriefwahl.Model;

namespace DigitaleBriefwahl.Views
{
	internal abstract class ElectionViewBase
	{
		private TabPage _page;

		protected ElectionModel Election { get; private set; }

		protected ElectionViewBase(ElectionModel election)
		{
			Election = election;
		}

		public virtual TabPage Layout()
		{
			_page = new TabPage
			{
				Text = Election.Name
			};
			var layout = new StackLayout()
			{
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				Spacing = 10,
				Padding = new Padding(10)
			};

			layout.Items.Add(new StackLayoutItem(new Label { Text = Election.Description }));
			_page.Content = layout;
			return _page;
		}

		public abstract bool VerifyOk();

		public abstract string GetResult();
	}
}
