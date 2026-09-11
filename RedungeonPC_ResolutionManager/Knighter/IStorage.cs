namespace Knighter;

public interface IStorage
{
	void Save();

	void SetField(string key, string value);

	string GetField(string key);

	bool FieldExist(string key);
}
