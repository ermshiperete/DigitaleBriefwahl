// Copyright (c) 2016 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System.Collections.Generic;
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
				sublayout.Items.Add(new Label { Text = $"{nominee}:"});
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
			SetTextColorForAllRadioButtons(Colors.Black);

			var allOk = true;
			var votes = 0;
			for (var i = 0; i < Election.Nominees.Count; i++)
			{
				if (!_radioButtons[i][0].Checked && !_radioButtons[i][1].Checked &&
					!_radioButtons[i][2].Checked)
				{
					SetTextColorForRadioButtonGroup(i, Colors.Red);
					allOk = false;
				}
				if (_radioButtons[i][0].Checked || _radioButtons[i][2].Checked)
					votes++;
			}

			if (votes == Election.Votes)
				return allOk;

			SetTextColorForAllRadioButtons(Colors.Red);
			allOk = false;
			return allOk;
		}

		private void SetTextColorForAllRadioButtons(Color color)
		{
			for (var i = 0; i < Election.Nominees.Count; i++)
			{
				SetTextColorForRadioButtonGroup(i, color);
			}
		}

		private void SetTextColorForRadioButtonGroup(int i, Color color)
		{
			_radioButtons[i][0].TextColor = color;
			_radioButtons[i][1].TextColor = color;
			_radioButtons[i][2].TextColor = color;
		}

		public override string GetResult()
		{
			var votes = new List<string>();
			for (var i = 0; i < Election.Nominees.Count; i++)
			{
				if (_radioButtons[i][0].Checked)
					votes.Add(YesNoElectionModel.Yes);
				else if (_radioButtons[i][1].Checked)
					votes.Add(YesNoElectionModel.No);
				else
					votes.Add(YesNoElectionModel.Abstention);
			}

			return Election.GetResult(votes);
		}
	}
}
