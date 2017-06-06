// Copyright (c) 2016 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using DigitaleBriefwahl.Model;

namespace DigitaleBriefwahl.Views
{
	internal static class ElectionViewFactory
	{
		public static ElectionViewBase Create(ElectionModel election)
		{
			switch (election.Type)
			{
				case ElectionType.YesNo:
					return new YesNoElectionView(election);
				case ElectionType.Weighted:
					return new WeightedElectionView(election);
			}

			return null;
		}
	}
}

