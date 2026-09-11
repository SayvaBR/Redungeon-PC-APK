using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Xna.Framework.Graphics;

namespace Knighter;

public class CrossPromotion : Component
{
	public class Slot
	{
		public int Id;

		public string Image;

		public string Icon;

		public string Game;

		public string PackageName;

		public string AppId;

		public string UrlScheme;

		public string Url;
	}

	private static readonly string XmlUrl = "http://www.nitrome.com/interchange/crosspromotion/xml";

	public bool Shown;

	public bool Showing;

	public bool Disabled;

	private readonly List<Slot> slots;

	private string xmlString;

	public bool Downloaded { get; private set; }

	public bool Parsed { get; private set; }

	public bool CanShow { get; private set; }

	public int Frequency { get; private set; }

	public int Start { get; private set; }

	public int Reset { get; private set; }

	public Slot ActiveSlot { get; private set; }

	public CrossPromotion()
	{
		slots = new List<Slot>();
	}

	public void Initialize()
	{
#if ANDROID
		Disabled = true;
#else
		if (base.core.ProfileData.AdsRemoved)
		{
			return;
		}
		try
		{
			Task.Run(delegate
			{
				RunInBg();
			});
		}
		catch (Exception ex)
		{
			Exception("CrossPromotion.Initialize: " + ex.Message, isFatal: false);
		}
#endif
	}

	private void RunInBg()
	{
		Downloaded = DownloadFromInternet();
		if (!Downloaded)
		{
			return;
		}
		Parsed = ParseXmlString(xmlString);
		if (Parsed && Start < _stat(Stat.Sessions) && _stat(Stat.Sessions) % Frequency == 0)
		{
			ResetCurrentSlotIndexIfSessionExpired();
			int num = base.core.ProfileData.CurrentSlotIndex;
			if (slots[num].PackageName == "com.nitrome.redungeon")
			{
				num = (num + 1) % slots.Count;
			}
			if (DownloadSlotData(slots[num % slots.Count]))
			{
				base.core.ProfileData.CurrentSlotIndex = (num + 1) % slots.Count;
				base.core.ProfileData.SaveIntoStorage();
				CanShow = true;
			}
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
			using XmlReader xmlReader = XmlReader.Create(new StringReader(str));
			xmlReader.ReadToFollowing("crossPromotion");
			Frequency = int.Parse(xmlReader.GetAttribute("frequency"));
			Start = int.Parse(xmlReader.GetAttribute("start"));
			Reset = int.Parse(xmlReader.GetAttribute("reset"));
			if (xmlReader.ReadToDescendant("slot"))
			{
				do
				{
					slots.Add(new Slot
					{
						Id = int.Parse(xmlReader.GetAttribute("id")),
						Image = xmlReader.GetAttribute("image"),
						Icon = xmlReader.GetAttribute("icon"),
						Game = xmlReader.GetAttribute("game"),
						PackageName = xmlReader.GetAttribute("packageName"),
						AppId = xmlReader.GetAttribute("appId"),
						UrlScheme = xmlReader.GetAttribute("urlScheme"),
						Url = xmlReader.GetAttribute("url")
					});
				}
				while (xmlReader.ReadToNextSibling("slot"));
			}
			return true;
		}
		catch (Exception)
		{
		}
		return false;
	}

	private bool DownloadSlotData(Slot slot)
	{
		try
		{
			MemoryStream stream = new MemoryStream(new WebClient().DownloadData(slot.Image));
			Texture2D texture = Texture2D.FromStream(base.core.GraphicsDevice, stream);
			base.core.SpriteManager.AddOrReplaceTexture("slot-image", texture);
			ActiveSlot = slot;
			return true;
		}
		catch (Exception)
		{
		}
		return false;
	}

	private void ResetCurrentSlotIndexIfSessionExpired()
	{
		string previousSessionTime = base.core.ProfileData.PreviousSessionTime;
		if (!(previousSessionTime == string.Empty) && (DateTime.Now - DateTime.Parse(previousSessionTime)).Hours >= Reset)
		{
			base.core.ProfileData.CurrentSlotIndex = 0;
			base.core.ProfileData.SaveIntoStorage();
		}
	}
}
