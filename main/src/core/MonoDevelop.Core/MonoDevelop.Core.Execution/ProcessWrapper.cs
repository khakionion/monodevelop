// created on 12/18/2004 at 16:28
using System;
using System.Threading;
using System.Diagnostics;

namespace MonoDevelop.Core.Execution
{
	public delegate void ProcessEventHandler(object sender, string message);
	
	public class ProcessWrapper : Process, IProcessAsyncOperation
	{
		private Thread captureOutputThread;
		private Thread captureErrorThread;
		ManualResetEvent endEventOut = new ManualResetEvent (false);
		ManualResetEvent endEventErr = new ManualResetEvent (false);
		bool done;
		
		public ProcessWrapper ()
		{
			Exited += new EventHandler (OnExited);
		}
		
		public new void Start ()
		{
			base.Start ();
			
			captureOutputThread = new Thread (new ThreadStart(CaptureOutput));
			captureOutputThread.IsBackground = true;
			captureOutputThread.Start ();
			
			captureErrorThread = new Thread (new ThreadStart(CaptureError));
			captureErrorThread.IsBackground = true;
			captureErrorThread.Start ();
		}
		
		public void WaitForOutput (int milliseconds)
		{
			WaitForExit (milliseconds);
			lock (this) {
				done = true;
			}
			WaitHandle.WaitAll (new WaitHandle[] {endEventOut, endEventErr});
		}
		
		public void WaitForOutput ()
		{
			WaitForOutput (-1);
		}
		
		private void CaptureOutput ()
		{
			try {
				string s;
				if (OutputStreamChanged != null) {
					while ((s = StandardOutput.ReadLine()) != null) {
						if (OutputStreamChanged != null)
							OutputStreamChanged (this, s);
					}
				}
			} finally {
				endEventOut.Set ();
			}
		}
		
		private void CaptureError ()
		{
			try {
				string s;
				if (ErrorStreamChanged != null) {
					while ((s = StandardError.ReadLine()) != null) {
						if (ErrorStreamChanged != null)
							ErrorStreamChanged (this, s);
					}					
				}
			} finally {
				endEventErr.Set ();
			}
		}
		
		int IProcessAsyncOperation.ExitCode {
			get { return ExitCode; }
		}
		
		int IProcessAsyncOperation.ProcessId {
			get { return Id; }
		}
		
		void IAsyncOperation.Cancel ()
		{
			try {
				if (!done) {
					try {
						Kill ();
					} catch {
						// Ignore
					}
					captureOutputThread.Abort ();
					captureErrorThread.Abort ();
				}
			} catch (Exception ex) {
				LoggingService.LogError (ex.ToString ());
			}
		}

		void IAsyncOperation.WaitForCompleted ()
		{
			WaitForOutput ();
		}
		
		void OnExited (object sender, EventArgs args)
		{
			try {
				WaitForOutput ();
			} finally {
				lock (this) {
					done = true;
					if (completedEvent != null)
						completedEvent (this);
				}
			}
		}
		
		event OperationHandler IAsyncOperation.Completed {
			add {
				bool raiseNow = false;
				lock (this) {
					if (done)
						raiseNow = true;
					else
						completedEvent += value;
				}
				if (raiseNow)
					value (this);
			}
			remove {
				lock (this) {
					completedEvent -= value;
				}
			}
		}
	
		bool IAsyncOperation.Success {
			get { return done ? ExitCode == 0 : false; }
		}
		
		bool IAsyncOperation.IsCompleted {
			get { return done; }
		}
		
		event OperationHandler completedEvent;
		
		public event ProcessEventHandler OutputStreamChanged;
		public event ProcessEventHandler ErrorStreamChanged;
	}
}
