using System;
using System.Collections.Generic;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter;

public class TouchMenu<T> : Component where T : struct, IConvertible
{
	private readonly Knighter.UI.MenuCursor selectionCursor = new();
	public class ButtonDesc
	{
		public float Opacity = 1f;
		public bool IsToggle;

		public bool ToggleValue;

		public int ToggleT;

		public RectangleF Rectangle;

		public RectSector Sector;

		public Sprite Sprite;

		public Sprite PressedSprite;

		public Sprite DisabledSprite;

		public bool Stretch;

		public Sprite StretchSprite;

		public Sprite StretchPressedSprite;

		public Sprite StretchDisabledSprite;

		private bool preparedDisabledSprite;

		public ButtonColor Color;

		public Color ButtonTint;

		public Color? FontColor;

		public bool Blink;

		public SpriteFlip Flip;

		public int TouchId = -1;

		public bool Hidden;

		public float YShift = -3f;

		public float XShift;

		public float FontSize = 1f;

		public float ShrinkFactor = 1f;

		public string Label = "";

		public Sprite LabelSprite;

		public bool Icon;

		public bool IconIsPicture;

		public string LabelAnimSequence = "";

		public Animation LabelAnim;

		public float LabelAnimSpeed = 0.095f;

		public bool DrawShadow;

		public bool IsTouched;

		/// <summary>Definido somente por um navegador de menu externo.</summary>
		public bool IsFocused;
		public int PressedUntil;

		public SoundName SoundDown;

		public SoundName SoundUp;

		public bool SeeThrough;

		public float Scale = 1f;

		public bool Visible
		{
			get
			{
				return !Hidden;
			}
			set
			{
				Hidden = !value;
			}
		}

		public bool Disabled { get; set; }

		public bool Pressed => TouchId >= 0;

		public bool IsDown
		{
			get
			{
				if (Pressed)
				{
					return IsTouched;
				}
				return false;
			}
		}

		public void Init()
		{
			if (Stretch)
			{
				StretchSprite = Sprite.Reduce(Sprite.Width - 1, 0, 0, 0);
				Sprite = Sprite.Reduce(0, 0, 1, 0);
				if (PressedSprite != null)
				{
					StretchPressedSprite = PressedSprite.Reduce(PressedSprite.Width - 1, 0, 0, 0);
					PressedSprite = PressedSprite.Reduce(0, 0, 1, 0);
				}
				if (DisabledSprite != null && !preparedDisabledSprite)
				{
					StretchDisabledSprite = DisabledSprite.Reduce(DisabledSprite.Width - 1, 0, 0, 0);
					DisabledSprite = DisabledSprite.Reduce(0, 0, 1, 0);
					preparedDisabledSprite = true;
				}
			}
			if (LabelAnimSequence != "")
			{
				string[] array = LabelAnimSequence.Split('|');
				LabelAnim = new Animation(LabelAnimSpeed);
				LabelAnim.Add("live", array[0], array[1]);
				LabelAnim.Play("live");
				IconIsPicture = true;
			}
		}

		public bool Contains(Vector2 point)
		{
			bool result = false;
			bool flag = Rectangle.Contains(point);
			if (Sector == RectSector.Whole)
			{
				result = flag;
			}
			else if (flag)
			{
				float num = point.X - Rectangle.Left;
				float num2 = point.Y - Rectangle.Top;
				float num3 = Rectangle.Right - point.X;
				bool flag2 = num > num2;
				bool flag3 = num3 > num2;
				switch (Sector)
				{
				case RectSector.North:
					result = flag3 && flag2;
					break;
				case RectSector.East:
					result = !flag3 && flag2;
					break;
				case RectSector.West:
					result = flag3 && !flag2;
					break;
				case RectSector.South:
					result = !flag3 && !flag2;
					break;
				}
			}
			return result;
		}

		public void Update()
		{
			if (LabelAnim != null)
			{
				LabelAnim.Update();
			}
			if (IsToggle && ToggleT > 0)
			{
				ToggleT--;
			}
		}

		public Color TextColor()
		{
			int value = 0;
			if (Disabled)
			{
				value = 5463138;
			}
			else
			{
				switch (Color)
				{
				case ButtonColor.Orange:
				case ButtonColor.OrangeOnStone:
					value = 16430139;
					break;
				case ButtonColor.Green:
					value = 5481258;
					break;
				case ButtonColor.None:
					value = 16777215;
					break;
				case ButtonColor.Maroon:
					value = 9175085;
					break;
				case ButtonColor.Purple:
					value = 5199247;
					break;
				case ButtonColor.Blue:
					value = 9810914;
					break;
				case ButtonColor.Stone:
					value = 5462882;
					break;
				}
			}
			return default(Color).FromRgb(value) * (SeeThrough ? 0.3f : 1f);
		}

		public Color MiddleColor()
		{
			int value = 0;
			if (Disabled)
			{
				value = 3619654;
			}
			else
			{
				switch (Color)
				{
				case ButtonColor.Orange:
				case ButtonColor.OrangeOnStone:
					value = 13790224;
					break;
				case ButtonColor.Green:
					value = 2839043;
					break;
				case ButtonColor.None:
					value = 16777215;
					break;
				case ButtonColor.Maroon:
					value = 6816817;
					break;
				case ButtonColor.Purple:
					value = 4076639;
					break;
				case ButtonColor.Blue:
					value = 5665696;
					break;
				case ButtonColor.Stone:
					value = 5462882;
					break;
				}
			}
			return default(Color).FromRgb(value) * (SeeThrough ? 0.3f : 1f);
		}

		public Color ShadowColor()
		{
			int value = 0;
			if (Disabled)
			{
				value = 2435639;
			}
			else
			{
				switch (Color)
				{
				case ButtonColor.Orange:
					value = 11688223;
					break;
				case ButtonColor.Green:
					value = 605192;
					break;
				case ButtonColor.None:
					value = 0;
					break;
				case ButtonColor.Maroon:
					value = 9462096;
					break;
				case ButtonColor.OrangeOnStone:
					value = 1514280;
					break;
				case ButtonColor.Purple:
					value = 4076639;
					break;
				case ButtonColor.Blue:
					value = 658190;
					break;
				case ButtonColor.Stone:
					value = 1974829;
					break;
				}
			}
			return default(Color).FromRgb(value) * (SeeThrough ? 0.2f : 1f);
		}
	}

	public readonly Action<T> OnButtonPress;

	public readonly Action<T> OnButtonRelease;

	public Action<T, bool> OnToggle;

	private readonly Dictionary<T, ButtonDesc> buttons;

	private readonly string layer;

	private readonly int depth;

	public ButtonDesc this[T button] => buttons[button];

	public IEnumerable<KeyValuePair<T, ButtonDesc>> ButtonEntries => buttons;

	public TouchMenu(Action<T> onPress, Action<T> onRelease = null, string layer = "fg", int depth = 0, Action<T, bool> onToggle = null)
	{
		buttons = new Dictionary<T, ButtonDesc>();
		OnButtonPress = onPress;
		OnButtonRelease = onRelease;
		OnToggle = onToggle;
		this.layer = layer;
		this.depth = depth;
	}

	public bool HasButton(T button)
	{
		return buttons.ContainsKey(button);
	}

	public void SetupButton(T button, RectangleF rectangle, Sprite sprite, Sprite pressedSprite, Sprite disabledSprite = null, bool stretch = false, SpriteFlip flip = SpriteFlip.None, ButtonColor color = ButtonColor.Orange, string label = "", Sprite labelSprite = null, bool icon = true, bool iconIsPicture = false, bool blink = false, Color? buttonTint = null, Color? fontColor = null, float yShift = -3f, float xShift = 0f, float fontSize = 1f, string labelAnim = "", float labelAnimSpeed = 0.095f, bool drawShadow = false, SoundName soundDown = SoundName.button_down, SoundName soundUp = SoundName.button_up, bool seeThrough = false, float scale = 1f, RectSector sector = RectSector.Whole)
	{
		buttons[button] = new ButtonDesc
		{
			Rectangle = rectangle,
			Sprite = sprite,
			PressedSprite = pressedSprite,
			DisabledSprite = (disabledSprite ?? sprite),
			Stretch = stretch,
			Flip = flip,
			Label = label,
			LabelSprite = labelSprite,
			Icon = icon,
			IconIsPicture = iconIsPicture,
			Color = color,
			ButtonTint = (buttonTint ?? Color.White),
			Blink = blink,
			FontColor = fontColor,
			YShift = yShift,
			XShift = xShift,
			FontSize = fontSize,
			LabelAnimSequence = labelAnim,
			LabelAnimSpeed = labelAnimSpeed,
			DrawShadow = drawShadow,
			SoundDown = soundDown,
			SoundUp = soundUp,
			SeeThrough = seeThrough,
			Scale = scale,
			Sector = sector
		};
		buttons[button].Init();
	}

	public void SetupToggle(T button, Vector2 position, bool initialValue, int width = 28)
	{
		buttons[button] = new ButtonDesc
		{
			Rectangle = new RectangleF(position.X, position.Y, width, 20f),
			XShift = 0f,
			YShift = 3f,
			ToggleValue = initialValue,
			IsToggle = true
		};
		buttons[button].Init();
	}

	public bool HandleInput()
	{
		bool result = false;
		foreach (TouchLocation item in base.core.TouchState)
		{
			foreach (KeyValuePair<T, ButtonDesc> button in buttons)
			{
				if (button.Value.Hidden || button.Value.Disabled)
				{
					continue;
				}
				if (item.State == TouchLocationState.Released && !button.Value.IsToggle && button.Value.TouchId == item.Id)
				{
					button.Value.TouchId = -1;
					if (OnButtonRelease != null && button.Value.Contains(item.Position))
					{
						OnButtonRelease(button.Key);
						SendMessage(new PlaySoundMessage(button.Value.SoundUp));
						result = true;
					}
				}
				if (button.Value.Contains(item.Position) && item.State == TouchLocationState.Pressed)
				{
					if (button.Value.IsToggle)
					{
						button.Value.ToggleValue = !button.Value.ToggleValue;
						button.Value.ToggleT = 9;
						if (OnToggle != null)
						{
							OnToggle(button.Key, button.Value.ToggleValue);
						}
						SendMessage(new PlaySoundMessage((!button.Value.ToggleValue) ? SoundName.swoosh_1 : SoundName.swoosh_2));
					}
					else
					{
						button.Value.TouchId = item.Id;
						button.Value.IsTouched = true;
						if (OnButtonPress != null)
						{
							OnButtonPress(button.Key);
						}
						result = true;
						SendMessage(new PlaySoundMessage(button.Value.SoundDown));
					}
				}
				if (button.Value.TouchId == item.Id && item.State == TouchLocationState.Moved)
				{
					button.Value.IsTouched = button.Value.Contains(item.Position);
				}
			}
		}
		return result;
	}

	public override void Update()
	{
		RectangleF selected = null;
		foreach (ButtonDesc value in buttons.Values)
		{
			value.Update();
			if (value.IsFocused && !value.Hidden && !value.Disabled) selected = value.Rectangle;
		}
		selectionCursor.Step(selected);
		base.Update();
	}

	public override void Draw()
	{
		selectionCursor.Render(layer, depth + 20);
		foreach (ButtonDesc value2 in buttons.Values)
		{
			if (value2.Hidden)
			{
				continue;
			}
			if (value2.IsToggle)
			{
				int num = (value2.ToggleValue ? 4 : 0);
				int num2 = 0;
				if (value2.ToggleT > 0)
				{
					num2 = ((!value2.ToggleValue) ? 1 : (-1)) * value2.ToggleT / 3;
				}
				num += num2;
				base.core.Renderer[layer, depth + 5, false].DrawSpriteS(_("ui_toggle_" + num), value2.Rectangle.TopLeft.Shift(value2.XShift, value2.YShift), Color.White * (value2.Disabled ? 0.5f : 1f));
				continue;
			}
			bool flag = (value2.Pressed && value2.IsTouched) || ticks < value2.PressedUntil;
			Vector2 vector = Vector2.Zero;
			if ((value2.Blink || value2.IsFocused) && !value2.Disabled && !value2.IsDown)
			{
				if (value2.Sprite != null)
				{
					Sprite sprite = _(SpriteName.button_gloss);
					int width = sprite.Width;
					float num3 = value2.Rectangle.Width - 6f;
					float num4 = Component._m((float)(base.ticks * 3) % ((float)width + num3 + 180f) - (float)width, num3) + (float)width;
					vector = Vector2.Zero;
					num4 -= (float)width;
					sprite = sprite.Reduce(-(int)Component._m(num4, 0f), 0, (int)Component._M(0f, num4 + (float)width - num3), 0);
					if (sprite.Width > 0)
					{
						base.core.Renderer[layer, depth + 5, false].DrawSpriteS(sprite, value2.Rectangle.TopLeft.Shift(3f + Component._M(0f, num4), value2.IsDown ? 3f : (0f + vector.Y)), default(Color).FromRgb(value2.IsDown ? 13659170 : 16765291) * 0.45f);
					}
				}
				else
				{
					vector = new Vector2(0f, -3f * Component._M(0f, Component._sin((float)base.ticks * 0.1f)));
				}
			}
			Sprite sprite2 = (value2.Disabled ? value2.DisabledSprite : (flag ? value2.PressedSprite : value2.Sprite));
			if (sprite2 != null)
			{
				if (!value2.Stretch)
				{
					base.core.Renderer[layer, depth, false].DrawSpriteS(sprite2, value2.Rectangle.Center - (new Vector2(value2.Sprite.Width / 2, value2.Sprite.Height / 2) + vector) * value2.Scale, Color.White * value2.Opacity * (value2.SeeThrough ? 0.2f : 1f), flip: value2.Flip, scale: Vector2.One * value2.Scale);
				}
				else
				{
					float y = value2.Rectangle.Top + (value2.Rectangle.Height - (float)value2.Sprite.Height) / 2f;
					Sprite sprite3 = (value2.Disabled ? value2.StretchDisabledSprite : (flag ? value2.StretchPressedSprite : value2.StretchSprite));
					base.core.Renderer[layer, depth, false].DrawSpriteS(sprite2, new Vector2(value2.Rectangle.Left, y) + vector, value2.ButtonTint);
					base.core.Renderer[layer, depth, false].DrawSpriteS(sprite3, new Vector2(value2.Rectangle.Left + (float)sprite2.Width, y) + vector, value2.ButtonTint, new Vector2(value2.Rectangle.Width - (float)(sprite2.Width * 2) + 1f, 1f));
					base.core.Renderer[layer, depth, false].DrawSpriteS(sprite2, new Vector2(value2.Rectangle.Right - (float)sprite2.Width, y) + vector, value2.ButtonTint, null, 0f, SpriteFlip.Horizontal);
				}
			}
			if (value2.Label != "")
			{
				Vector2 position = value2.Rectangle.Center + new Vector2(value2.XShift, value2.YShift + (flag ? 3f : 0f)) + vector;
				RectangleF rectangleF = base.core.Renderer[layer, depth, false].DrawTextS(value2.Label, position, TextProfile.OrangeBoldText.Alter(value2.FontColor ?? ((!flag) ? value2.TextColor() : value2.MiddleColor()), value2.ShadowColor(), boxAlignment: Alignment2D.Middle, textAlignment: Alignment2D.Middle, width: (int)value2.Rectangle.Width - 6, height: (int)value2.Rectangle.Height - 6, decoration: (!flag && !value2.FontColor.HasValue) ? TextDecoration.Extrude1 : TextDecoration.None, font: null, scale: value2.FontSize * value2.ShrinkFactor));
				if ((rectangleF.Width > value2.Rectangle.Width - 8f || rectangleF.Height > value2.Rectangle.Height - 10f) && value2.ShrinkFactor >= 0.4f)
				{
					value2.ShrinkFactor -= 0.1f;
				}
			}
			if (value2.LabelSprite != null || value2.LabelAnim != null)
			{
				Sprite sprite4 = ((value2.LabelAnim != null) ? value2.LabelAnim.GetCurrentFrame() : value2.LabelSprite);
				Vector2 vector2 = value2.Rectangle.Center + new Vector2((0f - (float)sprite4.Width) * value2.Scale / 2f + value2.XShift, (0f - (float)sprite4.Height) * value2.Scale / 2f + (float)(flag ? 3 : 0) + value2.YShift + 1f) + vector;
				if (!value2.IconIsPicture && value2.LabelAnim == null && !flag)
				{
					base.core.Renderer[layer, depth, false].DrawSpriteS(value2.LabelSprite, vector2.Shift(0f, 1f), value2.ShadowColor(), Vector2.One * value2.Scale);
				}
				if (value2.DrawShadow)
				{
					base.core.Renderer[layer, depth, false].DrawSpriteS(sprite4, vector2.Shift(0f, sprite4.Height - 5), Color.Black * 0.1f, new Vector2(1f, 0.8f) * value2.Scale, 0f, SpriteFlip.Vertical);
				}
				Color value = ((!value2.Icon && value2.IconIsPicture) ? ((value2.LabelAnim == null) ? (Color.White * (flag ? 0.3f : 1f)) : Color.White) : ((!flag) ? value2.TextColor() : value2.MiddleColor()));
				base.core.Renderer[layer, depth, false].DrawSpriteS(sprite4, vector2, value, Vector2.One * value2.Scale);
			}
		}
		base.Draw();
	}

	public void ReleaseButtons()
	{
		foreach (KeyValuePair<T, ButtonDesc> button in buttons)
		{
			button.Value.TouchId = -1;
		}
	}

	public void SetFocusedButton(T? focusedButton)
	{
		foreach (KeyValuePair<T, ButtonDesc> button in buttons)
		{
			button.Value.IsFocused = focusedButton.HasValue
				&& EqualityComparer<T>.Default.Equals(button.Key, focusedButton.Value);
		}
		selectionCursor.Step(focusedButton.HasValue && buttons.TryGetValue(focusedButton.Value, out var selected) ? selected.Rectangle : null);
	}

	/// <summary>
	/// Ativa um item pelo mesmo caminho usado por toque, mas sem fabricar um
	/// TouchLocation. Chamado exclusivamente por TouchMenuNavigator.
	/// </summary>
	public bool ActivateButton(T button)
	{
		if (!buttons.TryGetValue(button, out ButtonDesc desc) || desc.Hidden || desc.Disabled)
		{
			return false;
		}

		if (desc.IsToggle)
		{
			desc.ToggleValue = !desc.ToggleValue;
			desc.ToggleT = 9;
			OnToggle?.Invoke(button, desc.ToggleValue);
			SendMessage(new PlaySoundMessage(desc.ToggleValue ? SoundName.swoosh_2 : SoundName.swoosh_1));
			return true;
		}

		desc.PressedUntil = core.Ticks + 7;
		OnButtonPress?.Invoke(button);
		OnButtonRelease?.Invoke(button);
		SendMessage(new PlaySoundMessage(desc.SoundUp));
		return true;
	}
}
