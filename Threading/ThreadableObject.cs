using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace MMT.LVDOS.Threading
{
	/// <summary>
	/// Provides the implementation of an object that is able to thread itself
	/// </summary>
	public abstract class ThreadableObject
	{
		private Thread _thread;

		private ThreadPriority _threadPriority = ThreadPriority.Normal;

		protected ThreadPriority ThreadPriority
		{
			get { return _threadPriority; }
			set 
			{ 
				_threadPriority = value;
				if (_thread != null) _thread.Priority = value;
			}
		}

		private ApartmentState _apartment = ApartmentState.Unknown;

		protected ApartmentState ThreadApartmentState
		{
			get { return _apartment; }
			set 
			{ 
				_apartment = value;
				if (_thread != null) _thread.SetApartmentState(value);
			}
		}

		private bool _backgroundThread = false;

		protected bool IsBackgroundThread
		{
			get { return _backgroundThread; }
			set
			{
				_backgroundThread = value;
				_thread.IsBackground = value;
			}
		}

		/// <summary>
		/// Reaturns the current thread
		/// </summary>
		protected Thread Thread
		{
			get { return _thread; }
		}

		private ThreadStart _threadStartFunction = null;

		private ThreadStart ThreadStartFunction
		{
			get { return _threadStartFunction; }
			set 
			{
				if (_thread != null && (
					_thread.ThreadState == System.Threading.ThreadState.Running ||
					_thread.ThreadState == System.Threading.ThreadState.Background ||
					_thread.ThreadState == System.Threading.ThreadState.AbortRequested ||
					_thread.ThreadState == System.Threading.ThreadState.StopRequested ||
					_thread.ThreadState == System.Threading.ThreadState.SuspendRequested ||
					_thread.ThreadState == System.Threading.ThreadState.WaitSleepJoin))
				{
					throw new InvalidOperationException("Cannot set delegate while thread is running.");
				}

				// Set
				_threadStartFunction = value;
				CreateThread();
			}
		}

		private string _threadName = String.Empty;

		/// <summary>
		/// Gets or sets the thread name
		/// </summary>
		public string ThreadName
		{
			get { return _threadName; }
			set { _threadName = value; }
		}


		private void CreateThread()
		{
			_thread = new Thread(ThreadStartFunction);
		}

		/// <summary>
		/// Erzeugt ein neues ThreadableObject
		/// </summary>
		public ThreadableObject()
		{
			ThreadStartFunction = this.ThreadEntry;
			// CreateThread();
		}

		/// <summary>
		/// Starts the thread
		/// </summary>
		public void StartThread()
		{
			// Spezialwerte setzen
			_thread.Name = _threadName;
			_thread.SetApartmentState(this.ThreadApartmentState);
			_thread.Priority = this.ThreadPriority;
			_threadStartFunction = this.OnThreadRuns;
			_thread.IsBackground = IsBackgroundThread;

			// Starten
			_thread.Start();
			_abortByUser = false;
			OnThreadStarts();
		}

		/// <summary>
		/// Waits until the thread has finished execution
		/// </summary>
		public void JoinThread()
		{
			if (_thread == null) return;
			_thread.Join();
		}

		/// <summary>
		/// Suspends the thread or, if it is already suspended, does nothing
		/// </summary>
		public void SuspendThread()
		{
			if (_thread == null) return;
			_abortByUser = true;
			try
			{
				// Lower the thread's priority to speed up garbage collection
				_thread.Priority = ThreadPriority.Lowest;
				// Raise a ThreadAbortException
				_thread.Abort(this);
				// Wait for the thread to suspend
				_thread.Join(Contract.Timeout);

				// Thread killen
				_thread = null;
			}
			catch (ThreadStateException)
			{ }
		}

		private bool _running = false;

		/// <summary>
		/// Entry point for thread invocation
		/// </summary>
		private void ThreadEntry()
		{
			//Debug.WriteLine("ThreadableObject::ThreadEntry()");
			//Debug.Indent();
			//Debug.WriteLine( this.Thread.Name);
			//Debug.Unindent();

			// Thread glorreich beginnen
			_finishedOkay = false;
			_running = true;
			OnThreadStarts();
			if (this.ThreadStart != null) this.ThreadStart(this);

			// Thread glorreich durchführen
			try
			{
				// Basisfunktion ausführen
				OnThreadRuns();

				// Delegatenliste abarbeiten
				if( ThreadFunction!= null )
				{
					ThreadEntryPoint[] list = (ThreadEntryPoint[])ThreadFunction.GetInvocationList();
					for (int i = 0; i < list.Length; ++i)
					{
						// Ausführen
						list[i]();
					}
				}

				_finishedOkay = true;
			}
			catch (ThreadAbortException)
			{
				// Thread wurde abgebrochen - okay
				_finishedOkay = false;
				_running = false;
				
				//Debug.WriteLine("ThreadableObject::ThreadEntry(): Thread abort exception.");
				//Debug.Indent();
				//Debug.WriteLine(this.Thread.Name);
				//Debug.Unindent();

				OnThreadAbort();
			}
			catch (ThreadInterruptedException)
			{
				// Thread wurde unterbrochen - okay
				_finishedOkay = false;
				_running = false;

				//Debug.WriteLine("ThreadableObject::ThreadEntry(): Thread interrupt exception.");
				//Debug.Indent();
				//Debug.WriteLine(this.Thread.Name);
				//Debug.Unindent();

				OnThreadInterrupt();
			}

			// Thread glorreich beenden

			//Debug.WriteLine("ThreadableObject::ThreadEntry(): Thread terminated normally.");
			//Debug.Indent();
			//Debug.WriteLine(this.Thread.Name);
			//Debug.Unindent();

			OnThreadEnds();
			_running = false;
			if (this.ThreadEnds != null) this.ThreadEnds(this);
		}

		/// <summary>
		/// This function is called when the thread is executed.
		/// It is the core function.
		/// </summary>
		protected abstract void OnThreadRuns();

		/// <summary>
		/// This function is called once the thread starts running.
		/// These methods will only be called if the thread is started by the ThreadableObject routines.
		/// </summary>
		protected virtual void OnThreadStarts() { }

		/// <summary>
		/// This function is called if the thread is terminated.
		/// These methods will only be called if the thread is started by the ThreadableObject routines.
		/// </summary>
		protected virtual void OnThreadEnds() { }

		/// <summary>
		/// This function is called if the thread is aborted
		/// </summary>
		protected virtual void OnThreadAbort() { }

		/// <summary>
		/// This function is called if the thread is interrupted
		/// </summary>
		protected virtual void OnThreadInterrupt() { }

		/// <summary>
		/// This event occurs when the thread starts through invocation by the ThreadableObject code
		/// </summary>
		public event ThreadableObject.ThreadStartDelegate ThreadStart = null;

		/// <summary>
		/// This event occurs when the thread ends due to normal termination by the ThreadableObject code
		/// </summary>
		public event ThreadableObject.ThreadEndDelegate ThreadEnds = null;

		public delegate void ThreadStartDelegate(ThreadableObject sender);
		public delegate void ThreadEndDelegate(ThreadableObject sender);

		private bool _abortByUser = false;

		/// <summary>
		/// Returns true if the thread was aborted by the user
		/// </summary>
		public bool AbortedByUser
		{
			get { return _abortByUser; }
		}

		/// <summary>
		/// Registeres the functions that are called in invocation order while thread execution time
		/// </summary>
		protected event ThreadEntryPoint ThreadFunction = null;

		protected delegate void ThreadEntryPoint();

		/// <summary>
		/// True, if the thread is running
		/// </summary>
		public bool Running
		{
			get
			{
				return _running;
			}
		}

		private bool _finishedOkay = false;

		/// <summary>
		/// True, if the thread finished okay. Also false if the thread did not start yet.
		/// </summary>
		public bool Finished
		{
			get
			{
				return _finishedOkay;
			}
		}

		private object _threadTag = null;

		/// <summary>
		/// Gets or sets a new user-defined tag value to trace the object
		/// </summary>
		public object ThreadTag
		{
			get { return _threadTag; }
			set { _threadTag = value; }
		}

		private object _threadTag2 = null;

		/// <summary>
		/// Gets or sets a new user-defined tag value to trace the object
		/// </summary>
		public object ThreadTag2
		{
			get { return _threadTag2; }
			set { _threadTag2 = value; }
		}
	}
}
