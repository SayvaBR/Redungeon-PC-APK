using System;
using System.Collections.Generic;

namespace Knighter.Messages;

public sealed class MessageManager : Component
{
	private sealed class MessageDesc
	{
		public Message Message;

		public object Sender;

		public string Handle;

		public int Delay;
	}

	private readonly Dictionary<MessageType, List<Component>> subscribers;

	private readonly Dictionary<string, Component> handles;

	private readonly List<MessageDesc> messagesDesc;

	public MessageManager()
	{
		subscribers = new Dictionary<MessageType, List<Component>>();
		handles = new Dictionary<string, Component>();
		messagesDesc = new List<MessageDesc>();
	}

	public override void Load()
	{
	}

	public override void Unload()
	{
	}

	public override void Update()
	{
		SendAll();
	}

	public void Subscribe(MessageType type, Component subscriber)
	{
		if (!subscribers.ContainsKey(type))
		{
			subscribers[type] = new List<Component>();
		}
		subscribers[type].Add(subscriber);
	}

	public void Unsubscribe(MessageType type, Component subscriber)
	{
		if (subscribers.ContainsKey(type))
		{
			subscribers[type].Remove(subscriber);
		}
	}

	public void UnsubscribeFromAll(Component subscriber)
	{
		foreach (MessageType value in Enum.GetValues(typeof(MessageType)))
		{
			if (subscribers.ContainsKey(value))
			{
				subscribers[value].Remove(subscriber);
			}
		}
	}

	public void Send(Message message, object sender, int delay = 0)
	{
		messagesDesc.Add(new MessageDesc
		{
			Message = message,
			Sender = sender,
			Handle = string.Empty,
			Delay = delay
		});
	}

	private void SendAll()
	{
		List<MessageDesc> list = new List<MessageDesc>(messagesDesc);
		messagesDesc.Clear();
		List<MessageDesc> list2 = new List<MessageDesc>();
		foreach (MessageDesc item in list)
		{
			if (item.Delay > 0)
			{
				item.Delay--;
				list2.Add(item);
			}
			else if (item.Handle == string.Empty)
			{
				if (!subscribers.ContainsKey(item.Message.Type))
				{
					continue;
				}
				foreach (Component item2 in subscribers[item.Message.Type])
				{
					item2.OnMessage(item.Message, item.Sender);
				}
			}
			else if (handles.ContainsKey(item.Handle))
			{
				handles[item.Handle].OnMessage(item.Message, item.Sender);
			}
		}
		messagesDesc.AddRange(list2);
	}

	public void SubscribeByHandle(string handle, Component subscriber)
	{
		handles.Add(handle, subscriber);
	}

	public void UnsubscribeByHandle(string handle)
	{
		handles.Remove(handle);
	}

	public void SendByHandle(string handle, Message message, object sender, int delay = 0)
	{
		messagesDesc.Add(new MessageDesc
		{
			Message = message,
			Sender = sender,
			Handle = handle,
			Delay = delay
		});
	}
}
