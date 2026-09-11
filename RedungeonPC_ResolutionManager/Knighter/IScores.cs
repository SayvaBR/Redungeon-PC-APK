using System.Collections.Generic;

namespace Knighter;

public interface IScores
{
	void Authenticate();

	void ReportBestScore(bool gold);

	void ReportAchievmentsProgress(List<Achievement> achievmentsToReport);

	void ReportAchievment(Achievement achievment);
}
