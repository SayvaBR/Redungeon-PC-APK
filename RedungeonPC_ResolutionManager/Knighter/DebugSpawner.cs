using System;
using System.Collections.Generic;
using System.Diagnostics;
using Knighter.Entities;
using Knighter.Gameplay;
using Knighter.Helpers;
using Knighter.Messages;
using Knighter.States;
using Microsoft.Xna.Framework;

namespace Knighter;

/// <summary>
/// Ferramenta de debug pra spawnar entidades perto do player em pleno
/// jogo, sem precisar editar fase/level nem sair do PlayState. Nunca
/// compila fora de DEBUG (todo método público tem [Conditional("DEBUG")],
/// igual ao padrão já usado em Achievements.DebugTestToast).
///
/// Atalhos (DevTools):
///  [ / ]   -> troca qual entidade está selecionada pra spawnar
///  Enter   -> spawna a selecionada, a poucos blocos do player
///
/// Pra adicionar uma entidade nova na lista, basta um novo item em
/// `recipes` — mesmo padrão dos existentes. `MakeProgramDesc` cobre o
/// caso comum (entidades com o mini-DSL de movimento "program", tipo
/// slime/spider/rotoblade/zapper); entidades mais simples (Ghost) só
/// chamam o próprio construtor direto.
/// </summary>
public class DebugSpawner : Component
{
	private struct Recipe
	{
		public string Name;

		public Func<int, int, Entity> Build;
	}

	private readonly List<Recipe> recipes = new List<Recipe>
	{
		new Recipe
		{
			Name = "Bat",
			Build = (x, y) => new BatEntity(x, y, null)
		},
		new Recipe
		{
			Name = "Ghost",
			Build = (x, y) => new GhostEntity(x, y)
		},
		new Recipe
		{
			Name = "Slime",
			Build = (x, y) => new SlimeEntity(x, y, MakeProgramDesc(ElementType.Slime, "n20e20s20w20", ("delay", 0), ("default-delay", 30), ("jump-time", 20), ("no-limit", 0)))
		},
		new Recipe
		{
			Name = "Spider",
			Build = (x, y) => new SpiderEntity(x, y, MakeProgramDesc(ElementType.Spider, "n20e20s20w20", ("delay", 0), ("default-delay", 30), ("jump-time", 20)))
		},
		new Recipe
		{
			Name = "Rotoblade",
			Build = (x, y) => new RotobladeEntity(x, y, MakeProgramDesc(ElementType.Rotoblade, "n20e20s20w20", ("delay", 0), ("start-dir", 0), ("default-delay", 30), ("turn-time", 20)))
		},
		new Recipe
		{
			Name = "Zapper",
			Build = (x, y) => new ZapperEntity(x, y, "n20e20s20w20", 0, 30, flipped: false)
		}
	};

	private int selected;

	private static TileDesc MakeProgramDesc(ElementType type, string program, params (string key, int value)[] fields)
	{
		JsonObject jsonObject = new JsonObject();
		jsonObject.Fields["program"] = new JsonText(program);
		foreach (var (key, value) in fields)
		{
			jsonObject.Fields[key] = new JsonNumber(value);
		}
		return new TileDesc
		{
			ElementType = type,
			ElementJson = jsonObject,
			Flipped = false
		};
	}

	public override void Update()
	{
		if (Settings.DrawDebugWatches)
		{
			base.core.DebugWatch("spawner (Enter para spawnar, [ ] pra trocar)", recipes[selected].Name);
		}
		base.Update();
	}

	[Conditional("DEBUG")]
	public void CycleSelection(int direction)
	{
		selected = (selected + direction + recipes.Count) % recipes.Count;
	}

	[Conditional("DEBUG")]
	public void SpawnSelected()
	{
		var playState = base.core.CurrentPlayState;
		if (playState == null || playState.Player == null || !playState.IsTopState)
		{
			return;
		}
		Vector2 center = playState.Player.CenterCoordinates;
		int x = (int)center.X + 2;
		int y = (int)center.Y;
		Entity entity = recipes[selected].Build(x, y);
		SendMessage(new SpawnEntityMessage(entity, playState.Player.CurrentPlatform));
	}
}
