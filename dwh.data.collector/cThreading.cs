using System;
using System.Threading;
using System.Diagnostics;
using NLog;
using System.Reflection;

namespace dwh.data.collector
{
    public class cThreading
    {
        static Logger Nlogger = LogManager.GetCurrentClassLogger();
        public int _threadsallowed { get; set; }
        public int _cores { get; set; } = 1; //Environment.ProcessorCount;


        internal Process myProcess;


        /// <summary>
        /// Display active threads
        /// </summary>
        /// <returns></returns>
        public int activeThreads()
        {
            try
            {
                int availableWorkerThreads;
                int availableIOThreads;
                int maxWorkerThreads;
                int maxIOThreads;

                ThreadPool.GetAvailableThreads(out availableWorkerThreads, out availableIOThreads);
                ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxIOThreads);
                return (maxWorkerThreads - availableWorkerThreads);
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                //if (Debugger.IsAttached == true) { Console.WriteLine(err); }
                Nlogger.Error(err);
                return 0;
            }
        }

        /// <summary>
        /// insert task in queue
        /// </summary>
        /// <param name="_wi"></param>
        /// <param name="_params"></param>
        public void add2queue(ref WaitCallback _wi, object _params)
        {
            try
            {
                while (ThreadPool.QueueUserWorkItem(_wi, _params) != true)
                { }
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                //if (Debugger.IsAttached == true) { Console.WriteLine(err); }
                Nlogger.Error(err);
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="_maxThreadsPerCore"></param>
        /// <param name="_maxThreadsTotal"></param>
        /// <param name="_prio"></param>
        public cThreading(int _maxThreadsPerCore, int _maxThreadsTotal, ProcessPriorityClass _prio,bool useAllCores = false)
        {
            try
            {
                if (useAllCores == true) { _cores = Environment.ProcessorCount; }
                this._threadsallowed = _cores * _maxThreadsPerCore;
                if (this._threadsallowed > _maxThreadsTotal)
                {
                    this._threadsallowed = _maxThreadsTotal;
                }
                myProcess = Process.GetCurrentProcess();
                myProcess.PriorityClass = _prio;
                ThreadPool.SetMaxThreads(this._threadsallowed, this._threadsallowed);
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                //if (Debugger.IsAttached == true) { Console.WriteLine(err); }
                Nlogger.Error(err);
            }
        }




        public bool prevInstance()
        {
            bool bReturn = false;
            try
            {
                if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName.ToString()).Length > 1)
                {
                    bReturn = true;
                }
                return bReturn;
            }
            catch (Exception ex)
            {
                string err = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                //if (Debugger.IsAttached == true) { Console.WriteLine(err); }
                Nlogger.Error(err);
                return true;
            }
        }

    }
}
