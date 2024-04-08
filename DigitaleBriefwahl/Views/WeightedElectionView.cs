// Copyright (c) 2016-2024 Eberhard Beilharz
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
		private List<ComboBox> _comboBoxes;
		private Color _defaultTextColor;

		public WeightedElectionView(ElectionModel election)
			: base(election)
		{
		}

		public override TabPage Layout()
		{
			var page = base.Layout();

			var layout = page.Content as StackLayout;
			_comboBoxes = new List<ComboBox>(Election.Votes);
			for (var i = 0; i < Election.Votes; i++)
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
						new Label { Text = $"{i + 1}." }
					}
				};
				var combo = new ComboBox();
				_comboBoxes.Add(combo);
				foreach (var nominee in Election.Nominees)
				{
					if (Election.SkipNominee(nominee, i))
						continue;
					combo.Items.Add(new ListItem { Text = nominee });
				}
				combo.Items.Add(new ListItem { Text = Configuration.Abstention });
				voteLine.Items.Add(combo);
				layout.Items.Add(new StackLayoutItem(voteLine));
			}
			if (Election.Votes > 0)
				_defaultTextColor = _comboBoxes[0].TextColor;
			return page;
		}

		public override bool VerifyOk()
		{
			for (var i = 0; i < Election.Votes; i++)
				_comboBoxes[i].TextColor = _defaultTextColor;

			var invalids = Election.GetInvalidVotes(GetResultList());
			var isFirst = true;
			foreach (var invalid in invalids)
			{
				_comboBoxes[invalid].TextColor = Colors.Red;
				if (isFirst)
				{
					_comboBoxes[invalid].Focus();
					isFirst = false;
				}
			}
			return invalids.Count == 0;
		}

		private List<string> GetResultList()
		{
			var electedNominees = new List<string>();
			for (var i = 0; i < Election.Votes; i++)
			{
				electedNominees.Add(_comboBoxes[i].SelectedKey);
			}

			return electedNominees;
		}

		public override string GetResult(bool writeEmptyBallot)
		{
			return Election.GetResult(GetResultList(), writeEmptyBallot);
		}
	}
}
