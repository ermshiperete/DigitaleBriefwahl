// Copyright (c) 2016 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using System.Collections.Generic;
using System.Text;
using DigitaleBriefwahl.Model;
using Eto.Drawing;
using Eto.Forms;

namespace DigitaleBriefwahl.Views
{
    internal class YesNoElectionView: ElectionViewBase
	{
		private List<RadioButton[]> _radioButtons;

		public YesNoElectionView(ElectionModel election)
			: base(election)
		{
		}

		public override TabPage Layout()
		{
			var page = base.Layout();
			_radioButtons = new List<RadioButton[]>(Election.Nominees.Count);

			var layout = page.Content as StackLayout;
			foreach (var nominee in Election.Nominees)
			{
				var sublayout = new StackLayout { Orientation = Orientation.Horizontal };
				sublayout.Items.Add(new Label { Text = string.Format("{0}:", nominee) });
				var radioLayout = new StackLayout { Orientation = Orientation.Vertical };
				sublayout.Items.Add(radioLayout);
				var radio1 = new RadioButton { Text = "Ja" };
				radioLayout.Items.Add(radio1);
				var radio2 = new RadioButton(radio1) { Text = "Nein" };
				radioLayout.Items.Add(radio2);
				var radio3 = new RadioButton(radio1) { Text = "Enthaltung" };
				radioLayout.Items.Add(radio3);
				layout.Items.Add(sublayout);
				_radioButtons.Add(new[] { radio1, radio2, radio3 });
			}
			return page;
		}

		public override bool VerifyOk()
		{
			for (int i = 0; i < Election.Nominees.Count; i++)
			{
				_radioButtons[i][0].TextColor = Colors.Black;
				_radioButtons[i][1].TextColor = Colors.Black;
				_radioButtons[i][2].TextColor = Colors.Black;
			}

			var allOk = true;
			for (int i = 0; i < Election.Nominees.Count; i++)
			{
				if (!_radioButtons[i][0].Checked && !_radioButtons[i][1].Checked &&
					!_radioButtons[i][2].Checked)
				{
					_radioButtons[i][0].TextColor = Colors.Red;
					_radioButtons[i][1].TextColor = Colors.Red;
					_radioButtons[i][2].TextColor = Colors.Red;
					allOk = false;
				}
			}
			return allOk;
		}

		public override string GetResult()
		{
			var bldr = new StringBuilder();
			bldr.AppendLine(Election.Name);

			for (int i = 0; i < Election.Nominees.Count; i++)
			{
				bldr.AppendFormat("{0}. [{1}] {2}\n", i + 1, _radioButtons[i][0].Checked ? "J" :
					_radioButtons[i][1].Checked ? "N" : "E", Election.Nominees[i]);
			}
			return bldr.ToString();
		}
	}
}
