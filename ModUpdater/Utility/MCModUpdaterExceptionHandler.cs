﻿//    File:        MCModUpdaterExceptionHandler.cs
//    Copyright:   Copyright (C) 2012 Christian Wilson. All rights reserved.
//    Website:     https://github.com/seaboy1234/Minecraft-Mod-Updater
//    Description: This is intended to help Minecraft server owners who use mods make the experience of adding new mods and updating old ones easier for everyone.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;

namespace ModUpdater.Utility
{
    public class MCModUpdaterExceptionHandler
    {
        private static Dictionary<IExceptionHandler, Type[]> _handlers = new Dictionary<IExceptionHandler, Type[]>();
        private static int maxPriority = 0;

        static MCModUpdaterExceptionHandler()
        {
            RegisterExceptionHandler(new BasicExceptionHandler());
        }
        /// <summary>
        /// Registers an exception handler to handle exceptions.
        /// </summary>
        /// <param name="handler">The object to be notified when an unhandled exception occurs in the ModUpdater Assembly.</param>
        /// <param name="handledExceptions">An array of exception types that are to be handled by this handler.</param>
        public static bool RegisterExceptionHandler(IExceptionHandler handler, Type[] handledExceptions = null)
        {
            if (handledExceptions != null)
            {
                foreach (Type exception in handledExceptions)
                {
                    Type ex = exception;
                    while (ex.BaseType != null)
                    {
                        ex = exception.BaseType;
                    }
                    if (ex != typeof(Exception))
                    {
                        throw new InvalidOperationException("All types must contain Exception as a base type.");
                    }
                }
            }
            int p = handler.GetPriority();
            if (maxPriority < p)
            {
                maxPriority = p;
            }
            _handlers.Add(handler, handledExceptions);
            return true;
            
        }
        /// <summary>
        /// Handles an exception.
        /// </summary>
        /// <param name="sender">The object that sent the exception.</param>
        /// <param name="e">The exception.</param>
        public static void HandleException(object sender, Exception e)
        {
            bool handled = false;
            ExceptionObject o = new ExceptionObject(sender, handled, e);
            List<IExceptionHandler> handledBy = new List<IExceptionHandler>();
            for (int i = 0; i < maxPriority; i++)
            {
                foreach (var exch in _handlers.ToArray())
                {
                    if (exch.Value == null || exch.Value.Contains(e.GetType()))
                    {
                        if (exch.Key.GetPriority() == i)
                        {
                            bool h = exch.Key.Handle(o);
                            if(!handled)handled = h;
                            if(h)
                            {
                                handledBy.Add(exch.Key);
                                o.Handled = h;
                            }
                        }
                    }
                }
            }
            if (!handled)
            {
                MinecraftModUpdater.Logger.Log(Logger.Level.Error, "Exception handler failed to handle.");
                MinecraftModUpdater.Logger.Log(e);
                throw e; //This line should NEVER excute.
            }
            string handlers = "";
            foreach(IExceptionHandler handler in handledBy)
            {
                handlers += handler.GetName();
                handlers += ", ";
            }
            handlers = handlers.Remove(handlers.Length - 2);
            MinecraftModUpdater.Logger.Log(Logger.Level.Warning, e.GetType().Name + " handled by " + handlers + ".");
        }
    }
    public interface IExceptionHandler
    {
        /// <summary>
        /// The name of the exception handler.
        /// </summary>
        string GetName();
        /// <summary>
        /// The priority that this exception handler uses.
        /// </summary>
        int GetPriority();
        /// <summary>
        /// Handles an exception.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        bool Handle(ExceptionObject e);
    }
    /// <summary>
    /// An object that holds data from an exception.
    /// </summary>
    public class ExceptionObject
    {
        public object Sender { get; private set; }
        public bool Handled { get; internal set; }
        public Exception Exception { get; private set; }

        public ExceptionObject(object sender, bool handled, Exception e)
        {
            Sender = sender;
            Handled = handled;
            Exception = e;
        }
    }
    
    class BasicExceptionHandler : IExceptionHandler
    {

        string IExceptionHandler.GetName()
        {
            return "Basic Exception Handler";
        }

        int IExceptionHandler.GetPriority()
        {
            return 100; //This is the last chance to save the program.
        }

        bool IExceptionHandler.Handle(ExceptionObject e)
        {
            if (!e.Handled)
            {
                MessageBox.Show(e.Exception.ToString(), "Unhandled Exception");
            }
            return false; // We did NOT handle the exception.  Thus, we won't pretend we did.
        }
    }
}
