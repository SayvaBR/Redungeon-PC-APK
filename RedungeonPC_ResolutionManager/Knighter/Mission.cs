using System;
using System.Collections.Generic;
using System.Linq;

namespace Knighter;

public class Mission : Component
{
	private readonly Dictionary<string, string> fields;

	public MissionAction Action => (MissionAction)Enum.Parse(typeof(MissionAction), GetString("action"));

	public bool SingleRun
	{
		get
		{
			return GetBool("single-run");
		}
		set
		{
			SetBool("single-run", value);
		}
	}

	public Mission()
	{
		fields = new Dictionary<string, string>();
	}

	public Mission(MissionAction action)
	{
		fields = new Dictionary<string, string>();
		SingleRun = false;
		SetString("action", action.ToString());
	}

	public Mission SetString(string name, string value)
	{
		fields[name] = value;
		return this;
	}

	public string GetString(string name)
	{
		if (!fields.ContainsKey(name))
		{
			return string.Empty;
		}
		return fields[name];
	}

	public Mission SetInt(string name, int value)
	{
		fields[name] = value.ToString();
		return this;
	}

	public int GetInt(string name)
	{
		if (!int.TryParse(GetString(name), out var result))
		{
			return 0;
		}
		return result;
	}

	public Mission SetBool(string name, bool value)
	{
		fields[name] = value.ToString();
		return this;
	}

	public bool GetBool(string name)
	{
		if (!bool.TryParse(GetString(name), out var result))
		{
			return false;
		}
		return result;
	}

	public override string ToString()
	{
		return string.Join(";", fields.Select((KeyValuePair<string, string> x) => x.Key + "=" + x.Value).ToArray());
	}

	public static Mission FromString(string str)
	{
		Mission mission = new Mission();
		string[] array = str.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split('=');
			mission.SetString(array2[0], array2[1]);
		}
		return mission;
	}

	public virtual void Reset()
	{
	}

	public virtual bool Completed()
	{
		return false;
	}
}
