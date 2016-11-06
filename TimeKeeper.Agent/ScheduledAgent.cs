using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using TimeKeeper.Core;
using System;

namespace TimeKeeper.Agent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }
        
        public static StatStack Statistics;

        /// <summary>
        /// Agent that runs a scheduled Task
        /// </summary>
        /// <param Name="Task">
        /// The invoked Task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive Task is invoked
        /// </remarks>
        protected override async void OnInvoke(ScheduledTask task)
        {
            //  Blocks messages output.
            try
            {
                var data = await TimeExpensesDataBase.Load();
                Statistics = await StatStack.Load(data);

                Statistics.RecalculateStatisitics();

                if (Debugger.IsAttached)
                    ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(data.BackgroundAgentInterval));
            }
            catch
            { 
                //  Blocks all exceptions.
            }

            NotifyComplete();
        }
    }
}