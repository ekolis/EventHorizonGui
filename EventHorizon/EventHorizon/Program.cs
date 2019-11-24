using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace EventHorizon
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
#if DEBUG
			// Display error console messages in the output window
			Console.SetError(Console.Out);
#endif
			if (args.Length > 0)
				Application.Run(new MainForm(args[0]));
			else
				Application.Run(new MainForm());
		}

		static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
		{
			var ex = e.Exception;
			var logfilename = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "errorlog.txt");
			MessageBox.Show("An unhandled error has occurred in Event Horizon.\nSorry for the inconvenience!\nPlease check\n" + logfilename + "\nif you need to see debugging details.");
			try
			{
				var sw = new StreamWriter(logfilename);
				var inner = ex;
				while (inner != null)
				{
					if (ex == inner)
						sw.WriteLine("Exception caught: " + ex.GetType() + ": " + ex.Message);
					else
					{
						sw.WriteLine();
						sw.WriteLine("Caused by: " + ex.GetType() + ": " + ex.Message);
					}
					sw.WriteLine(ex.StackTrace);
					inner = ex.InnerException;
				}
				sw.Close();
			}
			catch (Exception ex2)
			{
				MessageBox.Show("Could not write to error log!\n" + ex2.GetType() + ": " + ex2.Message);
			}
			Application.Exit();
		}
	}
}
