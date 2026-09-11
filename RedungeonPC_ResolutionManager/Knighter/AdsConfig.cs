using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Xml;

namespace Knighter;

public class AdsConfig : Component
{
	private static readonly string XmlUrl = "http://www.nitrome.com/dev/redungeon/admob_config.xml";

	private string xmlString;

	private DateTime timeOfLastAd;

	public bool Downloaded { get; private set; }

	public bool Parsed { get; private set; }

	public bool Enabled { get; private set; }

	public string Mode { get; private set; }

	public List<Tuple<int, int>> SessionStarts { get; private set; }

	public List<Tuple<int, int>> SessionFrequencies { get; private set; }

	public AdsConfig()
	{
		SessionStarts = new List<Tuple<int, int>>();
		SessionFrequencies = new List<Tuple<int, int>>();
		timeOfLastAd = DateTime.Now;
	}

	public void Initialize()
	{
#if ANDROID
		Enabled = false;
#else
		try
		{
			Task.Run(delegate
			{
				RunInBg();
			});
		}
		catch (Exception ex)
		{
			Exception("AdsConfig.Initialize: " + ex.Message, isFatal: false);
		}
#endif
	}

	private void RunInBg()
	{
		Downloaded = DownloadFromInternet();
		if (Downloaded)
		{
			Parsed = ParseXmlString(xmlString);
			var _ = Parsed;
		}
	}

	private bool DownloadFromInternet()
	{
		try
		{
			WebClient webClient = new WebClient();
			xmlString = webClient.DownloadString(XmlUrl);
			if (xmlString != string.Empty)
			{
				return true;
			}
		}
		catch (Exception)
		{
		}
		return false;
	}

	private bool ParseXmlString(string str)
	{
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(str);
			XmlElement documentElement = xmlDocument.DocumentElement;
			XmlNode xmlNode = null;
			foreach (XmlNode childNode in documentElement.ChildNodes)
			{
				string text = childNode.Attributes["platform"]?.InnerText;
				if (text != null && ((Settings.Os == Settings.OS.iOS && text == "ios") || (Settings.Os == Settings.OS.Android && text == "android")))
				{
					xmlNode = childNode;
					break;
				}
			}
			if (xmlNode == null)
			{
				return false;
			}
			foreach (XmlNode item in xmlNode)
			{
				switch (item.Name)
				{
				case "enabled":
					Enabled = bool.Parse(item.InnerText);
					break;
				case "mode":
					Mode = item.InnerText;
					break;
				case "forceCrossPromotion":
					base.core.CrossPromotion.Disabled = !bool.Parse(item.InnerText);
					break;
				case "start":
				{
					string text2 = item.Attributes["session"]?.InnerText;
					if (text2 != null)
					{
						SessionStarts.Add(new Tuple<int, int>(int.Parse(text2), int.Parse(item.InnerText)));
					}
					break;
				}
				case "frequency":
				{
					string text2 = item.Attributes["session"]?.InnerText;
					if (text2 != null)
					{
						SessionFrequencies.Add(new Tuple<int, int>(int.Parse(text2), int.Parse(item.InnerText)));
					}
					break;
				}
				}
			}
			return true;
		}
		catch (Exception)
		{
		}
		return false;
	}

	public bool IsTimeToShowAds()
	{
		if (!Parsed || SessionFrequencies.Count == 0)
		{
			return (base.core.AttemptsFromStart + 1) % 2 == 0;
		}
		int val = _stat(Stat.Sessions);
		int item = SessionFrequencies[Math.Min(val, SessionFrequencies.Count - 1)].Item2;
		if (Mode == "games")
		{
			int item2 = SessionStarts[Math.Min(val, SessionStarts.Count - 1)].Item2;
			return (base.core.AttemptsFromStart + item2 + 1) % item == 0;
		}
		if (Mode == "time")
		{
			if ((DateTime.Now - timeOfLastAd).Seconds >= item)
			{
				timeOfLastAd = DateTime.Now;
				return true;
			}
			return false;
		}
		return false;
	}
}
