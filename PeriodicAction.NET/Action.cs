/*
*MIT License
*
*Copyright (c) 2023 S Christison
*
*Permission is hereby granted, free of charge, to any person obtaining a copy
*of this software and associated documentation files (the "Software"), to deal
*in the Software without restriction, including without limitation the rights
*to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
*copies of the Software, and to permit persons to whom the Software is
*furnished to do so, subject to the following conditions:
*
*The above copyright notice and this permission notice shall be included in all
*copies or substantial portions of the Software.
*
*THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
*IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
*FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
*AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
*LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
*OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
*SOFTWARE.
*/

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Threading
{
    /// <summary>
    /// Periodically trigger an <see cref="Action"/> at the <see href="Interval"/>
    /// <para>If the action is still running at the <see href="Interval"/> it will become a <see href="Timeout"/> period</para>
    /// </summary>
    public class PeriodicAction
    {
        private readonly Action _Action;
        private readonly long _Interval;
        private readonly SemaphoreWeak _SlimWeak;

        private const string INTERVAL = "Increase Interval of: ";
        private const string EXCEPTION = "Exception: ";
        private const int ONE = 1;

        /// <summary>
        /// Occurs when a Periodic <see cref="Action"/> doesn't run because the <see href="Interval"/> overlapped 
        /// <para>This will only occur if <see cref="ErrorStrategy"/> is set to <see href="Event"/></para>
        /// <para><see cref="PeriodicActionOverlapped"/> will throw if the <see href="Interval"/> needs to be increased</para>
        /// </summary>
        public EventHandler<Exception>? ExceptionHandler;

        /// <summary>
        /// Periodically trigger an <see cref="Action"/> at the <see href="Interval"/>
        /// <para>If the action is still running at the <see href="Interval"/> it will become a <see href="Timeout"/> period</para>
        /// </summary>
        /// <param name="action">The Action</param>
        /// <param name="intervalMs">The <see href="Interval"/> to trigger the Action</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public PeriodicAction(Action action, long intervalMs)
        {
            if (intervalMs < 0)
            {
                throw new ArgumentOutOfRangeException("Time machine not implemented");
            }

            _Interval = intervalMs;
            _SlimWeak = new SemaphoreWeak();
            _Action = action;
        }

        /// <summary>
        /// Run Periodic <see cref="Action"/> at the <see href="Interval"/> until it is canceled by a CancellationTokenSource
        /// <para>Uses a Semaphore, this means that the <see href="Interval"/> will become a <see href="Timeout"/> period if your work overlaps</para>
        /// </summary>
        /// <param name="ct">Cancellation Token</param>
        /// <param name="exceptions">What strategy to use when an <see cref="Exception"/> is encountered</param>
        /// <param name="taskCreationOptions">Options to use with the <see cref="Task.Factory"/> that will be created during <see cref="Run"/></param>
        public void Run(CancellationToken ct = default, ErrorStrategy exceptions = ErrorStrategy.Ignore, TaskCreationOptions taskCreationOptions = TaskCreationOptions.DenyChildAttach)
        {
            switch (exceptions)
            {
                case ErrorStrategy.Event:
                    RunExceptEvent(ct, taskCreationOptions);
                    break;
                case ErrorStrategy.Throw:
                    RunThrow(ct, taskCreationOptions);
                    break;
                default:
                    RunIgnore(ct, taskCreationOptions);
                    break;
            }
        }

        internal void RunThrow(CancellationToken ct, TaskCreationOptions taskCreationOptions)
        {
            Task.Factory.StartNew(() =>
            {
                Stopwatch measureInterval = Stopwatch.StartNew();

                while (!ct.IsCancellationRequested)
                {
                    if (measureInterval.ElapsedMilliseconds >= _Interval)
                    {
                        if (_SlimWeak.CanEnter())
                        {
                            try
                            {
                                measureInterval.Restart();
                                _Action();
                            }
                            finally
                            {
                                _SlimWeak.Release();
                            }
                        }
                    }

                    Delay.Wait(ONE);
                }
            }, TaskCreationOptions.DenyChildAttach).ConfigureAwait(false);
        }

        internal void RunExceptEvent(CancellationToken ct, TaskCreationOptions taskCreationOptions)
        {
            Task.Factory.StartNew(() =>
            {
                Stopwatch measureInterval = Stopwatch.StartNew();

                while (!ct.IsCancellationRequested)
                {
                    if (measureInterval.ElapsedMilliseconds >= _Interval)
                    {
                        if (_SlimWeak.CanEnter())
                        {
                            try
                            {
                                measureInterval.Restart();
                                _Action();
                            }
                            catch (Exception ex)
                            {
                                ExceptionHandler?.Invoke(null, ex);
                            }
                            finally
                            {
                                _SlimWeak.Release();
                            }
                        }
                        else
                        {
                            ExceptionHandler?.Invoke(null, new PeriodicActionOverlapped(INTERVAL + _Action.Method.Name));
                        }
                    }

                    Delay.Wait(ONE);
                }
            }, TaskCreationOptions.DenyChildAttach).ConfigureAwait(false);
        }

        internal void RunIgnore(CancellationToken ct, TaskCreationOptions taskCreationOptions)
        {
            Task.Factory.StartNew(() =>
            {
                Stopwatch measureInterval = Stopwatch.StartNew();

                while (!ct.IsCancellationRequested)
                {
                    if (measureInterval.ElapsedMilliseconds >= _Interval)
                    {
                        if (_SlimWeak.CanEnter())
                        {
                            try
                            {
                                measureInterval.Restart();
                                _Action();
                            }
                            catch
                            {
                                // Ignore
                            }
                            finally
                            {
                                _SlimWeak.Release();
                            }
                        }
                    }

                    Delay.Wait(ONE);
                }
            }, taskCreationOptions).ConfigureAwait(false);
        }

        /// <summary>
        /// Occurs when a Periodic <see cref="Action"/> doesn't run because the <see href="Interval"/> overlapped and needs to be increased
        /// </summary>
        [Serializable]
        public class PeriodicActionOverlapped : Exception
        {
            /// <summary>
            /// Occurs when a Periodic <see cref="Action"/> doesn't run because the <see href="Interval"/> overlapped and needs to be increased
            /// </summary>
            /// <param name="message">User friendly error message</param>
            public PeriodicActionOverlapped(string message)
                : base(message)
            {
                Console.WriteLine(message);
            }
        }
    }

    /// <summary>
    /// The strategy to use when an error is encounter by a Periodic <see cref="Action"/>
    /// </summary>
    public enum ErrorStrategy
    {
        /// <summary>
        /// Ignore <see cref="Exception"/>, Your work will silently fail
        /// <para>Your <see cref="Action"/> will run again at the next <see href="Interval"/> like it didn't happen.</para>
        /// </summary>
        Ignore,

        /// <summary>
        /// Throw <see cref="Exception"/> to Caller
        /// <para>This could crash your application if you don't catch it.</para>
        /// </summary>
        Throw,

        /// <summary>
        /// Use <see href="ExceptionHandler"/>
        /// <para>This is the same as Ignoring Exceptions but the <see cref="Exception"/> will be forwarded to the <see href="ExceptionHandler"/> so you can log it.</para>
        /// </summary>
        Event,
    }

    /// <summary>
    /// Platform Dependent Wait
    /// Accurately wait down to 1ms if your platform will allow it
    /// Replacement for Thread.Sleep()
    /// </summary>
    public static class Delay
    {
        internal const string windowsMultimediaAPIString = "winmm.dll";

        [DllImport(windowsMultimediaAPIString)]
        internal static extern int timeBeginPeriod(int period);

        [DllImport(windowsMultimediaAPIString)]
        internal static extern int timeEndPeriod(int period);

        [DllImport(windowsMultimediaAPIString)]
        internal static extern int timeGetDevCaps(ref TimerCapabilities caps, int sizeOfTimerCaps);

        internal static TimerCapabilities Capabilities;

        static Delay()
        {
            timeGetDevCaps(ref Capabilities, Marshal.SizeOf(Capabilities));
        }

        /// <summary>
        /// Platform Dependent Wait
        /// Accurately wait down to 1ms if your platform will allow it
        /// Replacement for Thread.Sleep()
        /// </summary>
        /// <param name="delayMs"></param>
        public static void Wait(int delayMs)
        {
            timeBeginPeriod(Capabilities.PeriodMinimum);
            Thread.Sleep(delayMs);
            timeEndPeriod(Capabilities.PeriodMinimum);
        }

        /// <summary>
        /// The Min/Max supported period in milliseconds
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TimerCapabilities
        {
            /// <summary>Minimum supported period in milliseconds.</summary>
            public int PeriodMinimum;

            /// <summary>Maximum supported period in milliseconds.</summary>
            public int PeriodMaximum;
        }
    }

    /// <summary>
    /// Semaphore Weak
    /// A Single Entrant Semaphore,
    /// Technically not safe if you use it wrong
    /// </summary>
    public class SemaphoreWeak
    {
        protected private volatile bool canEnter = true;

        protected private SpinWait SpinWait { get; set; }

        /// <summary>
        /// True if the Semaphore Weak can be Entered
        /// </summary>
        /// <returns></returns>
        public bool CanEnter()
        {
            SpinWait.SpinOnce();

            if (!canEnter)
            {
                return false;
            }
            else
            {
                canEnter = false;
                return true;
            }
        }

        /// <summary>
        /// If you Enter you must Release
        /// </summary>
        public void Release()
        {
            canEnter = true;
        }
    }
}
