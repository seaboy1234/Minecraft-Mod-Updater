using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace ModUpdater.Utility
{
    public class MCModUpdaterExceptionHandler
    {
        private static Dictionary<IExceptionHandler, Type[]> _handlers = new Dictionary<IExceptionHandler, Type[]>();
        private static int maxPriority = 0;
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
            List<IExceptionHandler> handledBy = new List<IExceptionHandler>();
            for (int i = maxPriority; i > -1; i--)
            {
                foreach (var exch in _handlers.ToArray())
                {
                    if (exch.Value == null || exch.Value.Contains(e.GetType()))
                    {
                        if (exch.Key.GetPriority() == i)
                        {
                            bool h = exch.Key.Handle(sender, e);
                            if(!handled)handled = h;
                            if(h)
                            {
                                handledBy.Add(exch.Key);
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
        bool Handle(object sender, Exception e);
    }
}
