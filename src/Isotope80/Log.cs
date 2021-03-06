﻿using LanguageExt;
using System;
using static LanguageExt.Prelude;

namespace Isotope80
{
    /// <summary>
    /// Log entry type
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// Contextual header
        /// </summary>
        Context,
        
        /// <summary>
        /// Information message
        /// </summary>
        Info,
        
        /// <summary>
        /// Warning message
        /// </summary>
        Warn,
        
        /// <summary>
        /// Error message
        /// </summary>
        Error
    }

    /// <summary>
    /// Nested log
    /// </summary>
    public class Log
    {
        /// <summary>
        /// Empty log
        /// </summary>
        public static readonly Log Empty = new Log(0, default, "", default, DateTime.MinValue, "", "", 0);
        
        /// <summary>
        /// Number of tabs to indent
        /// </summary>
        public readonly int Indent;

        /// <summary>
        /// Log type
        /// </summary>
        public readonly LogType Type;
        
        /// <summary>
        /// Node message
        /// </summary>
        public readonly string Message;
        
        /// <summary>
        /// Child messages
        /// </summary>
        public readonly Seq<Log> Children;

        /// <summary>
        /// The time the log was captured.
        /// </summary>
        public readonly DateTime Time;
        
        /// <summary>
        /// The name of the method the log was called from.
        /// </summary>
        public readonly string CallerMemberName;
        
        /// <summary>
        /// The full path of the file the log was called from.
        /// </summary>
        public readonly string CallerFilePath;
        
        /// <summary>
        /// The line number of the line the log was called from.
        /// </summary>
        public readonly int CallerLineNumber;

        /// <summary>
        /// Ctor
        /// </summary>
        internal Log(int indent, LogType type, string message, Seq<Log> children, DateTime time, string callerMemberName, string callerFilePath, int callerLineNumber)
        {
            Indent           = indent >= 0 ? indent : throw new ArgumentOutOfRangeException(nameof(indent));
            Type             = type;
            Message          = message ?? throw new ArgumentNullException(nameof(message));
            Children         = children;
            Time             = time;
            CallerMemberName = callerMemberName;
            CallerFilePath   = callerFilePath;
            CallerLineNumber = callerLineNumber;
        }

        /// <summary>
        /// Add a log entry
        /// </summary>
        public (Log Log, Log Added) Add(Log log)
        {
            var nlog = log.Type == LogType.Context ? log.Rebase(Indent + 1) : log.Rebase(Indent);
            return (new Log(Indent, Type, Message, Children.Add(nlog), Time, CallerMemberName, CallerFilePath, CallerLineNumber), nlog);
        }

        /// <summary>
        /// Set the new base indent for the log
        /// </summary>
        /// <param name="indent">Base indent</param>
        /// <returns>Rebased log</returns>
        public Log Rebase(int indent) =>
            new Log(indent, Type, Message, Children.Map(c => c.Rebase(indent + 1)), Time, CallerMemberName, CallerFilePath, CallerLineNumber);

        /// <summary>
        /// Add a message to the log
        /// </summary>
        /// <param name="ctx">Context</param>
        public static Log Context(string ctx, DateTime time, string callerMemberName, string callerFilePath, int callerLineNumber) =>
            new Log(0, LogType.Info, ctx, default, time, callerMemberName, callerFilePath, callerLineNumber);

        /// <summary>
        /// Add a message to the log
        /// </summary>
        /// <param name="message">Message to log</param>
        public static Log Info(string message, DateTime time, string callerMemberName, string callerFilePath, int callerLineNumber) =>
            new Log(0, LogType.Info, $"INFO: {message}", default, time, callerMemberName, callerFilePath, callerLineNumber); 

        /// <summary>
        /// Add a message to the log
        /// </summary>
        /// <param name="message">Message to log</param>
        public static Log Warning(string message, DateTime time, string callerMemberName, string callerFilePath, int callerLineNumber) =>
            new Log(0, LogType.Warn, $"WARN: {message}", default, time, callerMemberName, callerFilePath, callerLineNumber);

        /// <summary>
        /// Add a message to the log
        /// </summary>
        /// <param name="message">Message to log</param>
        public static Log Error(string message, DateTime time, string callerMemberName, string callerFilePath, int callerLineNumber) =>
            new Log(0, LogType.Error, $"ERRO: {message}", default, time, callerMemberName, callerFilePath, callerLineNumber);

        /// <summary>
        /// ToString
        /// </summary>
        public override string ToString() =>
            String.Join(Environment.NewLine, ToSeq());

        /// <summary>
        /// ToSeq
        /// </summary>
        public Seq<string> ToSeq() =>
            Seq1(Text.Tabs(Indent, Message))
                .Filter(s => !String.IsNullOrWhiteSpace(s))
                .Append(Children.Map(c => c.ToSeq()));

        /// <summary>
        /// Add a log entry
        /// </summary>
        public static Log operator +(Log lhs, Log rhs) =>
            lhs.Add(rhs).Log;
    }

    /// <summary>
    /// Log output
    /// </summary>
    public readonly struct LogOutput
    {
        /// <summary>
        /// Log message
        /// </summary>
        public readonly string Message;
        
        /// <summary>
        /// Severity type
        /// </summary>
        public readonly LogType Type;
        
        /// <summary>
        /// Indentation
        /// </summary>
        public readonly int Indent;

        /// <summary>
        /// The time the log was captured.
        /// </summary>
        public readonly DateTime Time;
        
        /// <summary>
        /// The name of the method the log was called from.
        /// </summary>
        public readonly string CallerMemberName;
        
        /// <summary>
        /// The full path of the file the log was called from.
        /// </summary>
        public readonly string CallerFilePath;
        
        /// <summary>
        /// The line number of the line the log was called from.
        /// </summary>
        public readonly int CallerLineNumber;

        /// <summary>
        /// Ctor
        /// </summary>
        public LogOutput(string message, LogType type, int indent, DateTime time, string callerMemberName, string callerFilePath, int callerLineNumber)
        {
            Message          = message ?? throw new ArgumentNullException(nameof(message));
            Type             = type;
            Indent           = indent >= 0 ? indent : throw new ArgumentOutOfRangeException(nameof(indent));
            Time             = time;
            CallerMemberName = callerMemberName;
            CallerFilePath   = callerFilePath;
            CallerLineNumber = callerLineNumber;
        }

        /// <summary>
        /// Tabbed format display 
        /// </summary>
        public override string ToString() =>
            Text.Tabs(Indent, Message);

        /// <summary>
        /// Tabbed format display including the file path, line number and time.
        /// </summary>
        /// <param name="expectedMaxMessageLength">Number of characters reserved for the message so the output looks lined up.</param>
        /// <remarks>
        /// Message format: [TIME]: [INDENT][MESSAGE][GAP][FILE]:line [LINE] 
        /// </remarks>
        public string ToVerboseString(int expectedMaxMessageLength = 60) =>
            Time.ToString("HH:mm:ss.fff: ") +
            Text.Tabs(Indent, $"{Message}        {"".PadRight(Math.Max(0, expectedMaxMessageLength - Message.Length - Text.Tabs(Indent).Length), ' ')}{CallerFilePath}:line {CallerLineNumber}");
    }
}
