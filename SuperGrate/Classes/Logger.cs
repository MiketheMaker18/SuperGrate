﻿using SuperGrate.ComInterop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperGrate
{
    class Logger
    {
        /// <summary>
        /// Should verbose mode be enabled on the main UI?
        /// </summary>
        public static bool VerboseEnabled = true;
        public static List<string> Log = new List<string>();
        /// <summary>
        /// Writes a custom message to memory.
        /// </summary>
        /// <param name="Text">Text to log.</param>
        /// <param name="Color">Color of text.</param>
        /// <param name="Raw">Should the text to log be prefixed with a time?</param>
        private static void WriteLog(string Text, Color Color, bool Raw = false)
        {
            if (!Raw)
            {
                Text = DateTime.Now.ToLongTimeString() + "> " + Text;
                Text += "\n";
            }
            try
            {
                Main.Form.Invoke(new Action(() =>
                {
                    Main.LoggerBox.SelectionColor = Color;
                    Main.LoggerBox.AppendText(Text);
                    try
                    {
                        Main.LoggerBox.ScrollToCaret();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Ignoring an error when user clicks off of an input into the Log Box.");
                    }
                }));
            }
            catch (Exception) {
                Console.WriteLine(Text);
            }
        }
        /// <summary>
        /// Prefixes string with time and type string then writes it into memory.
        /// </summary>
        /// <param name="Text">Text to log.</param>
        /// <param name="Type">Type of log.</param>
        private static void WriteMemoryLog(string Text, string Type)
        {
            Log.Add("[" + Type + "]<" + DateTime.Now.ToLongTimeString() + "> " + Text);
        }
        /// <summary>
        /// Logs an info statement.
        /// </summary>
        /// <param name="Text">Text to log.</param>
        /// <param name="Raw">Should the text to log be prefixed with a time?</param>
        public static void Information(string Text, bool Raw = false)
        {
            WriteLog(Text, Color.White, Raw);
            WriteMemoryLog(Text, "INFO");
        }
        /// <summary>
        /// Logs a success statement.
        /// </summary>
        /// <param name="Text">Text to log.</param>
        /// <param name="Raw">Should the text to log be prefixed with a time?</param>
        public static void Success(string Text, bool Raw = false)
        {
            WriteLog(Text, Color.Green, Raw);
            WriteMemoryLog(Text, "SUCCESS");
        }
        /// <summary>
        /// Logs a warning.
        /// </summary>
        /// <param name="Text">Text to log.</param>
        /// <param name="Raw">Should the text to log be prefixed with a time?</param>
        public static void Warning(string Text, bool Raw = false)
        {
            WriteLog(Text, Color.Yellow, Raw);
            WriteMemoryLog(Text, "WARNING");
        }
        /// <summary>
        /// Logs an error.
        /// </summary>
        /// <param name="Text">Text to log.</param>
        /// <param name="Raw">Should the text to log be prefixed with a time?</param>
        public static void Error(string Text, bool Raw = false)
        {
            WriteLog(Text, Color.Red, Raw);
            WriteMemoryLog(Text, "ERROR");
        }
        /// <summary>
        /// Logs a verbose string. This text is usually hidden by default and only appears in a log file or if verbose mode is enabled.
        /// </summary>
        /// <param name="Text">Text to log.</param>
        /// <param name="Raw">Should the text to log be prefixed with a time?</param>
        public static void Verbose(string Text, bool Raw = false)
        {
            if (VerboseEnabled)
            {
                WriteLog(Text, Color.Gray, Raw);
            }
            WriteMemoryLog(Text, "VERBOSE");
        }
        /// <summary>
        /// Logs an exception.
        /// </summary>
        /// <param name="Exception">The exception to log.</param>
        /// <param name="Text">The text to convey with the exception.</param>
        public static void Exception(Exception Exception, string Text)
        {
            Error(Exception.Message);
            Verbose("ERROR\r" + Exception.StackTrace);
            if(Exception.InnerException != null)
            {
                Error(Exception.InnerException.Message);
                Verbose("ERROR\r" + Exception.InnerException.StackTrace);
            }
            Error(Text);
        }
        /// <summary>
        /// Updates the UI with the new progress info.
        /// </summary>
        /// <param name="Value">Value of the progress.</param>
        /// <param name="Max">Max value the progress can be.</param>
        public static void UpdateProgress(int Value, int Max = 100)
        {
            if (Main.Form.IsDisposed) return;
            int Percent = (int)Math.Round(((double)Value / (double)Max) * 100, 0);
            Main.Form.Invoke(new Action(() => {
                if (Percent < 100 && Percent > 0)
                {
                    Main.TaskbarList.SetProgressValue(Main.Form.Handle, (ulong)Percent, 100);
                    Main.Progress.Style = ProgressBarStyle.Continuous;
                    Main.Progress.Value = Percent;
                }
                else if(Percent < 0)
                {
                    Main.TaskbarList.SetProgressState(Main.Form.Handle, TBPFlags.TBPF_NOPROGRESS);
                    Main.Progress.Style = ProgressBarStyle.Continuous;
                    Main.Progress.Value = 0;
                }
                else
                {
                    Main.TaskbarList.SetProgressState(Main.Form.Handle, TBPFlags.TBPF_INDETERMINATE);
                    Main.Progress.Style = ProgressBarStyle.Marquee;
                }
            }));
        }
        /// <summary>
        /// Writes the log in memory to a file.
        /// </summary>
        /// <param name="fs">The file stream to write the data to.</param>
        /// <returns>An async task.</returns>
        public static Task WriteLogToFile(Stream fs)
        {
            return Task.Run(() => {
                StreamWriter sw = new StreamWriter(fs);
                foreach(string line in Log)
                {
                    sw.WriteLine(line);
                }
                sw.Flush();
                sw.Close();
            });
        }
    }
}
