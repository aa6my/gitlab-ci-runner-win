﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gitlab_ci_runner.api;
using gitlab_ci_runner.helper;

namespace gitlab_ci_runner.runner
{
    class Runner
    {
        /// <summary>
        /// Build process
        /// </summary>
        private static Build build = null;

        /// <summary>
        /// Start the configured runner
        /// </summary>
        public static void run()
        {
            Console.WriteLine("* Gitlab CI Runner started");
            Console.WriteLine("* Waiting for builds");
            waitForBuild();
        }

        /// <summary>
        /// Build completed?
        /// </summary>
        public static bool completed
        {
            get
            {
                return running && build.completed;
            }
        }

        /// <summary>
        /// Build running?
        /// </summary>
        public static bool running
        {
            get
            {
                return build != null;
            }
        }

        /// <summary>
        /// Wait for an incoming build or update current Build
        /// </summary>
        private static void waitForBuild()
        {
            while (true)
            {
                if (completed || running)
                {
                    // Build is running or completed
                    // Update build
                    updateBuild();
                }
                else
                {
                    // Get new build
                    getBuild();
                }
                Thread.Sleep(5000);
            }
        }

        /// <summary>
        /// Update the current running build progress
        /// </summary>
        private static void updateBuild()
        {
            if (build.completed)
            {
                // Build finished
                if (pushBuild())
                {
                    Console.WriteLine("[" + DateTime.Now.ToString() + "] Completed build " + build.buildInfo.id);
                    build = null;
                }
            }
            else
            {
                // Build is currently running
                pushBuild();
            }
        }

        /// <summary>
        /// PUSH Build Status to Gitlab CI
        /// </summary>
        /// <returns>true on success, false on fail</returns>
        private static bool pushBuild()
        {
            return Network.pushBuild(build.buildInfo.id, build.state, build.output);
        }

        /// <summary>
        /// Get a new build job
        /// </summary>
        private static void getBuild()
        {
            BuildInfo binfo = Network.getBuild();
            if (binfo != null)
            {
                // Create Build Job
                build = new Build(binfo);
                Console.WriteLine("[" + DateTime.Now.ToString() + "] Build " + binfo.id + " started...");
                Thread t = new Thread(build.run);
                t.Start();
            }
        }
    }
}
