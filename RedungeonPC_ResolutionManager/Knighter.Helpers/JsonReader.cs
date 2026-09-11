using System;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;

namespace Knighter.Helpers;

public sealed class JsonReader
{
	private readonly string text;

	private readonly JsonObject rootObject;

	private int currentPosition;

	private int currentLine;

	private int currentColumn;

	private bool readingText;

	public JsonBase this[string name] => rootObject.Fields[name];

	private JsonReader(string text)
	{
		this.text = text;
		rootObject = Block();
	}

	public static JsonReader FromFile(string filePath)
	{
		string text = string.Empty;
		using (StreamReader streamReader = new StreamReader(TitleContainer.OpenStream(filePath)))
		{
			text = streamReader.ReadToEnd();
		}
		return new JsonReader(text);
	}

	public static JsonReader FromText(string text)
	{
		return new JsonReader(text);
	}

	private JsonObject Block()
	{
		JsonObject jsonObject = new JsonObject();
		Read('{');
		while (LookAhead('"'))
		{
			JsonField jsonField = Field();
			jsonObject.Fields.Add(jsonField.Name, jsonField.Value);
			if (!LookAhead(','))
			{
				break;
			}
			Read(',');
		}
		Read('}');
		return jsonObject;
	}

	private JsonArray Array()
	{
		JsonArray jsonArray = new JsonArray();
		Read('[');
		while (LookAhead('{'))
		{
			jsonArray.Objects.Add(Block());
			if (!LookAhead(','))
			{
				break;
			}
			Read(',');
		}
		Read(']');
		return jsonArray;
	}

	private JsonField Field()
	{
		JsonField jsonField = new JsonField();
		jsonField.Name = Text().Value;
		Read(':');
		if (LookAhead('"'))
		{
			jsonField.Value = Text();
		}
		else if (LookAhead('{'))
		{
			jsonField.Value = Block();
		}
		else if (LookAhead('['))
		{
			jsonField.Value = Array();
		}
		else if (char.IsDigit(LookAhead()) || LookAhead('-'))
		{
			jsonField.Value = Number();
		}
		else if (LookAhead('n'))
		{
			jsonField.Value = Null();
		}
		else if (LookAhead('t') || LookAhead('f'))
		{
			jsonField.Value = Boolean();
		}
		return jsonField;
	}

	private JsonBoolean Boolean()
	{
		if (LookAhead('t'))
		{
			Read("true");
			return new JsonBoolean(value: true);
		}
		Read("false");
		return new JsonBoolean(value: false);
	}

	private JsonNull Null()
	{
		Read("null");
		return new JsonNull();
	}

	private JsonNumber Number()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (LookAhead('-'))
		{
			Read('-');
			stringBuilder.Append('-');
		}
		while (char.IsDigit(LookAhead()))
		{
			stringBuilder.Append(Read());
		}
		int result = 0;
		if (!int.TryParse(stringBuilder.ToString(), out result))
		{
			return new JsonNumber(0);
		}
		return new JsonNumber(result);
	}

	private JsonText Text()
	{
		StringBuilder stringBuilder = new StringBuilder();
		Read('"');
		while (!LookAhead('"'))
		{
			readingText = true;
			stringBuilder.Append(Read());
			readingText = false;
		}
		Read('"');
		return new JsonText(stringBuilder.ToString());
	}

	private void Read(string chars)
	{
		foreach (char ch in chars)
		{
			Read(ch);
		}
	}

	private char Read()
	{
		return LookAhead(move: true);
	}

	private void Read(char ch)
	{
		char c = LookAhead(move: true);
		if (ch != c)
		{
			throw new Exception($"Unexpected symbol: line {currentLine}, column {currentColumn - 1}, expected {ch}, got {c}");
		}
	}

	private bool LookAhead(char ch)
	{
		return LookAhead() == ch;
	}

	private char LookAhead(bool move = false)
	{
		int num = currentPosition;
		char c;
		while (true)
		{
			if (Eof())
			{
				throw new Exception("Unexpected end of file");
			}
			c = text[num++];
			currentColumn++;
			if (!char.IsControl(c) && (readingText || (!char.IsSeparator(c) && !char.IsWhiteSpace(c))))
			{
				break;
			}
			if (c == '\n')
			{
				currentLine++;
				currentColumn = 0;
			}
		}
		if (move)
		{
			currentPosition = num;
		}
		return c;
	}

	private bool Eof()
	{
		return currentPosition == text.Length;
	}
}
