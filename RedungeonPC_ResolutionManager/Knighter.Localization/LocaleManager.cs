using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Knighter.Helpers;

namespace Knighter.Localization;

public class LocaleManager : Component
{
	public readonly Dictionary<Language, Locale> Locales;

	public Language CurrentLocale => base.core.ProfileData.Locale;

	public LocaleManager()
	{
		Locales = new Dictionary<Language, Locale>();
	}

	public void SetCurrentLocale(Language newLocale)
	{
		base.core.ProfileData.Locale = newLocale;
	}

	public override void Load()
	{
		for (int i = 0; i < Enum.GetNames(typeof(Language)).Length; i++)
		{
			Locales.Add((Language)i, LoadLocaleFromFile((Language)i));
		}
		base.Load();
	}

	private string PrepareString(string str, Language language)
	{
		string text = str;
		text = text.Replace("\\n", "\n");
		string text2 = "[TODO]";
		if (text.StartsWith(text2, StringComparison.InvariantCulture))
		{
			text = text.Substring(text2.Length);
		}
		if (language == Language.fr_FR)
		{
			Match match = new Regex("\\p{L}\\ [!?:;]").Match(text);
			while (match.Success)
			{
				text = text.Replace(match.Value, match.Value.Replace(' ', '\u00a0'));
				match = match.NextMatch();
			}
		}
		return text;
	}

	private Locale LoadLocaleFromFile(Language language)
	{
		JsonReader jsonReader = JsonReader.FromFile($"Content/Locales/Redungeon_{language.ToString()}.json");
		Locale locale = new Locale(jsonReader["language_name"].ToString());
		Dictionary<string, JsonBase> fields = (jsonReader["strings"] as JsonObject).Fields;
		foreach (string key in fields.Keys)
		{
			Dictionary<string, JsonBase> fields2 = (fields[key] as JsonObject).Fields;
			foreach (string key2 in fields2.Keys)
			{
				locale.Add($"{key}_{key2}", PrepareString(fields2[key2].ToString(), language));
			}
		}
		return locale;
	}

	public string GetForCurrentLocale(string id)
	{
		string text = "";
		if (Locales[CurrentLocale].Exists(id))
		{
			return Locales[CurrentLocale].Get(id);
		}
		return Locales[Language.en_US].Get(id);
	}

	public string GetOrdinal(int n)
	{
		string text = n.ToString();
		if (CurrentLocale == Language.en_US)
		{
			string text2 = "th";
			if (n < 0)
			{
				n *= -1;
			}
			int num = n % 10;
			int num2 = n / 10 % 10;
			if ((num == 1 || num == 2 || num == 3) && num2 != 1)
			{
				text2 = num switch
				{
					2 => "nd", 
					1 => "st", 
					_ => "rd", 
				};
			}
			text += text2;
		}
		return text;
	}
}
