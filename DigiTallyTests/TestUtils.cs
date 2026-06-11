// Copyright (c) 2026 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System.Runtime.CompilerServices;
using DigitaleBriefwahl.Model;
using NUnit.Framework;

namespace DigitaleBriefwahl.Tally.Tests
{
	public static class TestUtils
	{
		internal static void CheckWeightedResult(CandidateResult result, int expectedPoints, [CallerLineNumber] int lineNumber = 0)
		{
			var weightedResult = result as WeightedCandidateResult;
			Assert.That(weightedResult?.Points, Is.EqualTo(expectedPoints),
				$"Expected {expectedPoints} points but got {weightedResult?.Points} in line {lineNumber}");
		}

		internal static void CheckYesNoResult(CandidateResult result, int expectedTotal, int expectedYes, int expectedNo, int expectedAbstain, [CallerLineNumber] int lineNumber = 0)
		{
			var ynResult = result as YesNoCandidateResult;
			Assert.That(ynResult.Yes, Is.EqualTo(expectedYes),
				$"Expected {expectedYes} Yes but got {ynResult.Yes} in line {lineNumber}");
			Assert.That(ynResult.No, Is.EqualTo(expectedNo),
				$"Expected {expectedNo} No but got {ynResult.No} in line {lineNumber}");
			Assert.That(ynResult.Abstention, Is.EqualTo(expectedAbstain),
				$"Expected {expectedAbstain} Abstain but got {ynResult.Abstention} in line {lineNumber}");
			Assert.That(ynResult.Yes + ynResult.No + ynResult.Abstention, Is.EqualTo(expectedTotal),
				$"Expected {expectedTotal} votes, but got {ynResult.Yes + ynResult.No + ynResult.Abstention} in line {lineNumber}");
		}
	}
}