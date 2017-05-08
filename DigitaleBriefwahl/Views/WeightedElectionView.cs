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
    internal class WeightedElectionView: ElectionViewBase
	{
		private List<ComboBox> _ComboBoxes;
		private Color _defaultTextColor;

		public WeightedElectionView(ElectionModel election)
			: base(election)
		{
		}

		public override TabPage Layout()
		{
			var page = base.Layout();

			var layout = page.Content as StackLayout;
			_ComboBoxes = new List<ComboBox>(Election.Votes);
			for (int i = 0; i < Election.Votes; i++)
			{
				if (Election.TextBefore[i] != null)
				{
					var label = new Label { Text = Election.TextBefore[i] };
					layout.Items.Add(new StackLayoutItem(label));
				}

				var voteLine = new StackLayout
				{
					Orientation = Orientation.Horizontal,
					VerticalContentAlignment = VerticalAlignment.Center,
					Spacing = 10,
					Items =
					{
						new Label { Text = string.Format("{0}.", i + 1) }
					}
				};
				var combo = new ComboBox();
				_ComboBoxes.Add(combo);
				foreach (var nominee in Election.Nominees)
				{
					if (SkipNominee(nominee, i))
						continue;
					combo.Items.Add(new ListItem { Text = nominee });
				}
				voteLine.Items.Add(combo);
				layout.Items.Add(new StackLayoutItem(voteLine));
			}
			if (Election.Votes > 0)
				_defaultTextColor = _ComboBoxes[0].TextColor;
			return page;
		}

		private bool SkipNominee(string name, int iVote)
		{
			int vote = iVote + 1;
			if (Election.NomineeLimits.ContainsKey(name))
			{
				var limit = Election.NomineeLimits[name];
				return vote < limit.Item1 || vote > limit.Item2;
			}
			return false;
		}

		public override bool VerifyOk()
		{
			var allOk = true;
			for (int i = 0; i < Election.Votes; i++)
				_ComboBoxes[i].TextColor = _defaultTextColor;

			for (int i = 0; i < Election.Votes; i++)
			{
				if (string.IsNullOrEmpty(_ComboBoxes[i].SelectedKey))
				{
					_ComboBoxes[i].TextColor = Colors.Red;
					if (allOk)
						_ComboBoxes[i].Focus();
					allOk = false;
					continue;
				}

				for (int j = i + 1; j < Election.Votes; j++)
				{
					if (_ComboBoxes[i].SelectedKey == _ComboBoxes[j].SelectedKey)
					{
						_ComboBoxes[i].TextColor = Colors.Red;
						_ComboBoxes[j].TextColor = Colors.Red;
						if (allOk)
							_ComboBoxes[i].Focus();
						allOk = false;
					}
				}

				if (SkipNominee(_ComboBoxes[i].SelectedKey, i))
				{
					_ComboBoxes[i].TextColor = Colors.Red;
					if (allOk)
						_ComboBoxes[i].Focus();
					allOk = false;
				}
			}
			return allOk;
		}

		public override string GetResult()
		{
			var bldr = new StringBuilder();
			bldr.AppendLine(Election.Name);

			for (int i = 0; i < Election.Votes; i++)
			{
				bldr.AppendFormat("{0}. {1}\n", i + 1, _ComboBoxes[i].SelectedKey);
			}
			return bldr.ToString();
		}
	}
}
