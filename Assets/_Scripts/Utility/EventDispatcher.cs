using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

public enum EventType
{
    None,
    Vfx,
    Sound,
    Animation,
    RaycastHit,
    Collision,
}

// Used this implementation to handle internal app components communications.

namespace com.trashpandaboy.events
{
    public class EventDispatcher : MonoBehaviour
    {
        /// <summary>
        /// Contains all the available Events
        /// </summary>
        private static Dictionary<string, UnityAction<object>> eventDictionary;

        /// <summary>
        /// Initialize the dictionary
        /// </summary>
        public virtual void Awake()
        {
            if (eventDictionary == null)
            {
                eventDictionary = new Dictionary<string, UnityAction<object>>();
            }
        }

        /// <summary>
        /// Add a given listening action to a specified event name
        /// </summary>
        /// <param name="eventName">The name of event who will trigger the action</param>
        /// <param name="listeningAction">The action to trigger</param>
        public static void StartListening(string eventName, UnityAction<object> listeningAction)
        {
            if (eventDictionary == null)
            {
                eventDictionary = new Dictionary<string, UnityAction<object>>();
            }
            UnityAction<object> eventObject;
            if (eventDictionary.TryGetValue(eventName, out eventObject))
            {
                //Add more listening action to the existing event
                eventObject += listeningAction;

                //Update the Dictionary
                eventDictionary[eventName] = eventObject;
            }
            else
            {
                //Add event to the Dictionary with the given listening action
                eventObject += listeningAction;
                eventDictionary.Add(eventName, eventObject);
            }
        }

        /// <summary>
        /// Add a given listening action to a specified event name
        /// </summary>
        /// <param name="enumValue">The name of the value given by Enum value</param>
        /// <param name="listeningAction">The action to trigger</param>
        public static void StartListening(Enum enumValue, UnityAction<object> listeningAction)
        {
            StartListening(enumValue.ToString(), listeningAction);
        }

        /// <summary>
        /// Remove the given listener from the event
        /// </summary>
        /// <param name="eventName">The name of event</param>
        /// <param name="listeningAction">The listening action</param>
        public static void StopListening(string eventName, UnityAction<object> listeningAction)
        {
            UnityAction<object> eventObject;
            if (eventDictionary.TryGetValue(eventName, out eventObject))
            {
                //Remove event from the existing one
                eventObject -= listeningAction;

                //Update the Dictionary
                eventDictionary[eventName] = eventObject;
            }
        }

        /// <summary>
        /// Remove the given listener from the event
        /// </summary>
        /// <param name="enumValue">The name of the event given by Enum value</param>
        /// <param name="listeningAction">The listening action</param>
        public static void StopListening(Enum enumValue, UnityAction<object> listeningAction)
        {
            StopListening(enumValue.ToString(), listeningAction);
        }

        /// <summary>
        /// If found in the eventDictionary the event with event name specified will be Invoked
        /// 
        /// </summary>
        /// <param name="eventName">Event name contained in the dictionary.</param>
        /// <param name="eventParameters">Parameters to pass to the event</param>
        public static void TriggerEvent(string eventName, object eventParameters = null)
        {
            if (eventDictionary == null) return;

            UnityAction<object> eventObject;
            if (eventDictionary.TryGetValue(eventName, out eventObject))
            {
                eventObject?.Invoke(eventParameters);
            }
        }

        /// <summary>
        /// Trigger an event using an Enum converting it's value to string
        /// </summary>
        /// <param name="enumValue">Enum value desired</param>
        /// <param name="eventParameters">Parameters to pass to the event</param>
        public static void TriggerEvent(Enum enumValue, object eventParameters = null)
        {
            TriggerEvent(enumValue.ToString(), eventParameters);
        }
    }
}