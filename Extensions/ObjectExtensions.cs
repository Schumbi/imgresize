﻿namespace ImageResizer.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using LanguageExt;

    public static class ObjectExtensions
    {
        /// <summary>
        /// Do stuff if pred matched.
        /// </summary>
        /// <typeparam name="T">Type of input.</typeparam>
        /// <typeparam name="R">Type of return.</typeparam>
        /// <param name="obj">Object to use.</param>
        /// <param name="pred">Evaluator.</param>
        /// <param name="func">Function to execute if <paramref name="pred"/> matches.</param>
        /// <returns></returns>
        public static Option<R> DoIf<T, R>(this T obj, Func<T, bool> pred, Func<T, Option<R>> func)
            where T : notnull
            where R : notnull
        {
            if (!pred.Invoke(obj)) return Option<R>.None;
            return func.Invoke(obj);
        }

        /// <summary>
        /// Invoke action, if input is true.
        /// </summary>
        /// <param name="test">Input value.</param>
        /// <param name="action">Action to invoke.</param>
        /// <returns>Unit.</returns>
        public static bool IfTrue(this bool input, Action action)
        {
            if(input)
            {
                action.Invoke();
            }
            return input;
        }

        /// <summary>
        /// Invoke action, if input is false.
        /// </summary>
        /// <param name="test">Input value.</param>
        /// <param name="action">Action to invoke.</param>
        /// <returns>Unit.</returns>
        public static bool IfFalse(this bool input, Action action)
        {
            if (!input)
            {
                action.Invoke();
            }
            return input;
        }

        /// <summary>
        /// Switch type to unit.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="_">Discarded.</param>
        /// <returns>Unit.</returns>
        public static Unit AsUnit<T>(this T _) => Unit.Default;
    }
}
