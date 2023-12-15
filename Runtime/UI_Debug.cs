using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryEngine
{
    /// <summary>
    /// Internal debug system that connects the system to the graphics engine console
    /// which takes into account the static parameters of the manager system.
    /// </summary>
    public class UI_Debug
    {
        /// <summary>
        /// Log default info message
        /// </summary>
        /// <param name="Message"></param>
        public static void Log(object Message)
        {
            if (UI_Manager.Settings.Debug)
                Debug.Log(Message);
        }

        public static void Log(object Message, Object Context)
        {
            if (UI_Manager.Settings.Debug)
                Debug.Log(Message, Context);
        }

        /// <summary>
        /// Log error message with an error alert if the system is deactivated 
        /// </summary>
        /// <param name="Message"></param>
        public static void LogError(object Message, bool Fatal = false)
        {
            string FatalText = (Fatal) ? "[Fatal Error] " : string.Empty;

            if (UI_Manager.Settings.Debug)
                Debug.LogError(FatalText + Message);
            else if(Fatal)
                Debug.LogError(FatalText + Message);
        }

        public static void LogError(object Message, Object Context, bool Fatal = false)
        {
            string FatalText = (Fatal) ? "[Fatal Error] " : string.Empty;

            if (UI_Manager.Settings.Debug)
                Debug.LogError(FatalText + Message, Context);
            else if(Fatal)
                Debug.LogError(FatalText + Message, Context);
        }

        /// <summary>
        /// Logs an warning message
        /// </summary>
        /// <param name="Message"></param>
        public static void LogWarning(object Message)
        {
            if (UI_Manager.Settings.Debug)
                Debug.LogWarning(Message);
        }

        public static void LogWarning(object Message, Object Context)
        {
            if (UI_Manager.Settings.Debug)
                Debug.LogWarning(Message, Context);
        }

        /// <summary>
        /// Log to the console a format string (rich text)
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="args"></param>
        public static void LogFormat(string Message, params object[] args)
        {
            if (UI_Manager.Settings.Debug)
                Debug.LogFormat(Message, args);
        }

        public static void LogFormat(string Message, LogType Type, params object[] args)
        {
            if (UI_Manager.Settings.Debug)
                Debug.LogFormat(Type, LogOption.None, null, Message, args);
        }
    }
}
