﻿using System.Collections.Generic;
using System.Linq;

namespace Model
{
	[ObjectEvent]
	public class PlayerComponentSystem : ObjectSystem<PlayerComponent>, IAwake
	{
		public void Awake()
		{
			this.Get().Awake();
		}
	}
	
	public class PlayerComponent : Component
	{
		public static PlayerComponent Instance { get; private set; }

		public Player MyPlayer;
		
		private readonly Dictionary<long, Player> idPlayers = new Dictionary<long, Player>();

		public void Awake()
		{
			Instance = this;
		}
		
		public void Add(Player player)
		{
			this.idPlayers.Add(player.Id, player);
		}

		public Player Get(long id)
		{
			Player player;
			this.idPlayers.TryGetValue(id, out player);
			return player;
		}

		public void Remove(long id)
		{
			this.idPlayers.Remove(id);
		}

		public int Count
		{
			get
			{
				return this.idPlayers.Count;
			}
		}

		public Player[] GetAll()
		{
			return this.idPlayers.Values.ToArray();
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}
			
			base.Dispose();

			foreach (Player player in this.idPlayers.Values)
			{
				player.Dispose();
			}

			Instance = null;
		}
	}
}