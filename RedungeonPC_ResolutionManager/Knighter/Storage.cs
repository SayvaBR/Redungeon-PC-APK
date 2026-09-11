using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace Knighter;

/// <summary>
/// Desktop replacement for Android's Storage.cs (which used SharedPreferences).
/// Persists key/value pairs to a plain text file next to the executable's
/// save folder. Same public surface as the original (IStorage), so every
/// other file that calls Core.Instance.Storage.* keeps working unchanged.
/// </summary>
public class Storage : Component, IStorage
{
	private readonly string filePath;
	private readonly Dictionary<string, string> fields = new Dictionary<string, string>();

	public Storage(string overrideFilePath = null)
	{
		if (!string.IsNullOrWhiteSpace(overrideFilePath))
		{
			filePath = Path.GetFullPath(overrideFilePath);
			string overrideFolder = Path.GetDirectoryName(filePath);
			if (!string.IsNullOrEmpty(overrideFolder))
			{
				Directory.CreateDirectory(overrideFolder);
			}
			LoadFromDisk();
			return;
		}

		string folder = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			"RedungeonPC");
		Directory.CreateDirectory(folder);
		filePath = Path.Combine(folder, "save.dat");
		LoadFromDisk();
	}

	private void LoadFromDisk()
	{
		if (!File.Exists(filePath))
		{
			return;
		}
		foreach (string line in File.ReadAllLines(filePath))
		{
			int separatorIndex = line.IndexOf('=');
			if (separatorIndex <= 0)
			{
				continue;
			}
			string key = line.Substring(0, separatorIndex);
			string value = line.Substring(separatorIndex + 1);
			fields[key] = value.Replace("\\n", "\n");
		}
	}

	public void Save()
	{
		SaveFields();
	}

	public void BackupProgress()
	{
		Save();
		File.Copy(filePath, filePath + ".before-reset-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + ".bak", false);
	}

	private void SaveFields()
	{
		var lines = new List<string>();
		foreach (var pair in fields)
		{
			lines.Add(pair.Key + "=" + pair.Value.Replace("\n", "\\n"));
		}
		File.WriteAllLines(filePath, lines);
	}

	public void SetField(string key, string value)
	{
		fields[key] = value ?? string.Empty;
	}

	public string GetField(string key)
	{
		return fields.TryGetValue(key, out string value) ? value : string.Empty;
	}

	public bool FieldExist(string key)
	{
		return fields.ContainsKey(key);
	}

	public bool GetBool(string key)
	{
		return bool.TryParse(GetField(key), out var result) && result;
	}

	public int GetInt(string key)
	{
		return int.TryParse(GetField(key), out var result) ? result : 0;
	}

	public float GetFloat(string key)
	{
		return float.TryParse(GetField(key), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : 0f;
	}

	public void SetBool(string key, bool value)
	{
		SetField(key, value.ToString());
	}

	public void SetInt(string key, int value)
	{
		SetField(key, value.ToString());
	}

	public void SetFloat(string key, float value)
	{
		SetField(key, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
	}

	public void SetString(string key, string value)
	{
		SetField(key, value);
	}

	public bool TryGetBool(string key, ref bool result)
	{
		if (FieldExist(key))
		{
			result = GetBool(key);
			return true;
		}
		return false;
	}

	public bool TryGetInt(string key, ref int result)
	{
		if (FieldExist(key))
		{
			result = GetInt(key);
			return true;
		}
		return false;
	}

	public bool TryGetFloat(string key, ref float result)
	{
		if (FieldExist(key))
		{
			result = GetFloat(key);
			return true;
		}
		return false;
	}

	public bool TryGetString(string key, ref string result)
	{
		if (FieldExist(key))
		{
			result = GetField(key);
			return true;
		}
		return false;
	}
}
