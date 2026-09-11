using System.Collections.Generic;

namespace Knighter.Localization;

public class Locale
{
	public static Dictionary<Language, string> ShortName = new Dictionary<Language, string>
	{
		{
			Language.en_US,
			"en"
		},
		{
			Language.ru_RU,
			"ru"
		},
		{
			Language.uk_UA,
			"ua"
		},
		{
			Language.es_ES,
			"es"
		},
		{
			Language.pl_PL,
			"pl"
		},
		{
			Language.de_DE,
			"de"
		},
		{
			Language.pt_PT,
			"pt"
		},
		{
			Language.fr_FR,
			"fr"
		},
		{
			Language.ja_JP,
			"ja"
		}
	};

	public readonly string LanguageName;

	private readonly Dictionary<string, string> strings;

	public static bool UsesExternalFont(Language language)
	{
		return language == Language.ja_JP;
	}

	public Locale(string languageName)
	{
		LanguageName = languageName;
		strings = new Dictionary<string, string>();
	}

	public void Add(string id, string value)
	{
		strings.Add(id, value);
	}

	public string Get(string id)
	{
		if (!Exists(id))
		{
			return id;
		}
		return strings[id];
	}

	public bool Exists(string id)
	{
		return strings.ContainsKey(id);
	}
}
