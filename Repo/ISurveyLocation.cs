using SurveyApp.Models;
using System.Collections.Generic;

namespace SurveyApp.Repo
{
    public interface ISurveyLocation
    {
        List<SurveyLocationModel> GetAllLocations();
        SurveyLocationModel? GetLocationById(int locId);
        bool AddLocation(SurveyLocationModel location);
        bool UpdateLocation(SurveyLocationModel location);
        bool DeleteLocation(int locId);
    }
}