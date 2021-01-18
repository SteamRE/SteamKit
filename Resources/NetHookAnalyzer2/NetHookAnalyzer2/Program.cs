using System;
using System.Windows.Forms;

namespace NetHookAnalyzer2
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
            // See https://github.com/dotnet/winforms/issues/4397#issuecomment-749782104
			// Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}
