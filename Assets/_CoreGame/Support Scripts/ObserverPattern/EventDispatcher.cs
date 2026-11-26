using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Observer
{

	public class EventDispatcher : MonoBehaviour
	{
		#region Singleton
		static EventDispatcher s_instance;
		static bool s_isQuitting;

		// Create the dispatcher before any scene loads so it is never spawned during
		// scene/Awake/OnDisable teardown, which caused Unity's "objects not cleaned up" warning.
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void CreateInstance()
		{
			s_isQuitting = false;
			if (s_instance != null) return;

			var singletonObject = new GameObject("Singleton - EventDispatcher");
			s_instance = singletonObject.AddComponent<EventDispatcher>();
			DontDestroyOnLoad(singletonObject);
		}

		public static EventDispatcher Instance
		{
			get
			{
				if (s_instance == null)
				{
					// Never create a GameObject while shutting down or between play sessions.
					if (s_isQuitting || !Application.isPlaying) return null;
#if UNITY_EDITOR
					if (!UnityEditor.EditorApplication.isPlaying) return null;
#endif
					CreateInstance();
				}
				return s_instance;
			}
			private set { }
		}

		public static bool HasInstance()
		{
			return s_instance != null;
		}

		void Awake ()
		{
			// check if there's another instance already exist in scene
			if (s_instance != null && s_instance.GetInstanceID() != this.GetInstanceID())
			{
				// Destroy this instances because already exist the singleton of EventsDispatcer
				Destroy(gameObject);
			}
			else
			{
				// set instance
				s_instance = this as EventDispatcher;
				if (Application.isPlaying)
				{
					DontDestroyOnLoad(gameObject);
				}
			}
		}

		void OnApplicationQuit()
		{
			s_isQuitting = true;
		}

		void OnDestroy ()
		{
			if (s_instance == this)
			{
				ClearAllListener();
				// Keep the reference during teardown: scene objects' OnDisable may still
				// unregister listeners. A destroyed instance compares == null in Unity,
				// so CreateInstance() will rebuild it on the next play session.
			}
		}
		#endregion


		#region Fields
		/// Store all "listener"
		Dictionary<EventID, Action<object>> _listeners = new Dictionary<EventID, Action<object>>();
		#endregion


		#region Add Listeners, Post events, Remove listener

		/// <summary>
		/// Register to listen for eventID
		/// </summary>
		/// <param name="eventID">EventID that object want to listen</param>
		/// <param name="callback">Callback will be invoked when this eventID be raised</para	m>
		public void RegisterListener (EventID eventID, Action<object> callback)
		{
			// checking params

			// check if listener exist in distionary
			if (_listeners.ContainsKey(eventID))
			{
				// add callback to our collection
				_listeners[eventID] += callback;
			}
			else
			{
				// add new key-value pair
				_listeners.Add(eventID, null);
				_listeners[eventID] += callback;
			}
		}

		/// <summary>
		/// Posts the event. This will notify all listener that register for this event
		/// </summary>
		/// <param name="eventID">EventID.</param>
		/// <param name="sender">Sender, in some case, the Listener will need to know who send this message.</param>
		/// <param name="param">Parameter. Can be anything (struct, class ...), Listener will make a cast to get the data</param>
		public void PostEvent (EventID eventID, object param = null)
		{
			if (!_listeners.ContainsKey(eventID))
			{
				return;
			}

			// posting event
			var callbacks = _listeners[eventID];
			// if there's no listener remain, then do nothing
			if (callbacks != null)
			{
				callbacks(param);
			}
			else
			{
				_listeners.Remove(eventID);
			}
		}

		/// <summary>
		/// Removes the listener. Use to Unregister listener
		/// </summary>
		/// <param name="eventID">EventID.</param>
		/// <param name="callback">Callback.</param>
		public void RemoveListener (EventID eventID, Action<object> callback)
		{
			// checking params

			if (_listeners.ContainsKey(eventID))
			{
				_listeners[eventID] -= callback;
			}
			else
			{
			}
		}

		/// <summary>
		/// Clears all the listener.
		/// </summary>
		public void ClearAllListener ()
		{
			_listeners.Clear();
		}
		#endregion
	}


	#region Extension class
	/// <summary>
	/// Delare some "shortcut" for using EventDispatcher easier
	/// </summary>
	public static class EventDispatcherExtension
	{
		/// Use for registering with EventsManager
		public static void RegisterListener (this MonoBehaviour listener, EventID eventID, Action<object> callback)
		{
			if (!EventDispatcher.HasInstance()) return;
			EventDispatcher.Instance.RegisterListener(eventID, callback);
		}

		public static void RemoveListener (this MonoBehaviour listener, EventID eventID, Action<object> callback)
		{
			if (!EventDispatcher.HasInstance()) return;
			EventDispatcher.Instance.RemoveListener(eventID, callback);
		}

		/// Post event with param
		public static void PostEvent (this MonoBehaviour listener, EventID eventID, object param)
		{
			if (!EventDispatcher.HasInstance()) return;
			EventDispatcher.Instance.PostEvent(eventID, param);
		}

		/// Post event with no param (param = null)
		public static void PostEvent (this MonoBehaviour sender, EventID eventID)
		{
			if (!EventDispatcher.HasInstance()) return;
			EventDispatcher.Instance.PostEvent(eventID, null);
		}
	}
	#endregion
}