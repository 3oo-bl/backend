namespace ProfitableViewApp.Interfaces;

public interface ISearcher
{
    Task<string> Search(string query, int targetValue);
}