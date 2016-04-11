using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using UnityEngine;

namespace Static_Interface.Internal.Windows
{
	/// <summary>
	/// Creates a console window that actually works in Unity
	/// You should add a script that redirects output using Console.Write to write to it.
	/// </summary>
	public class ConsoleWindow
	{
		TextWriter oldOutput;

		public void Initialize()
		{
			//
			// Attach to any existing consoles we have
			// failing that, create a new one.
			//
			if ( !AttachConsole( uint.MaxValue) )
			{
				AllocConsole();
			}

			oldOutput = System.Console.Out;

            try
            {
                Encoding encoding = Encoding.UTF8;
                System.Console.OutputEncoding = encoding;
                SafeFileHandle handle = new SafeFileHandle(GetStdHandle(-11), true);
                FileStream stream = new FileStream(handle, FileAccess.Write);
                StreamWriter newOut = new StreamWriter(stream, encoding)
                {
                    AutoFlush = true
                };
                System.Console.SetOut(newOut);
            }
            catch (Exception exception)
            {
                Debug.Log("Couldn't redirect output: " + exception.Message);
            }
        }

		public void Shutdown()
		{
            System.Console.SetOut( oldOutput );
			FreeConsole();
		}

		public void SetTitle( string strName )
		{
			SetConsoleTitle( strName );
		}

		private const int STD_OUTPUT_HANDLE = -11;

		[DllImport( "kernel32.dll", SetLastError = true )]
		static extern bool AttachConsole( uint dwProcessId );

		[DllImport( "kernel32.dll", SetLastError = true )]
		static extern bool AllocConsole();

		[DllImport( "kernel32.dll", SetLastError = true )]
		static extern bool FreeConsole();

		[DllImport( "kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall )]
		private static extern IntPtr GetStdHandle( int nStdHandle );

		[DllImport( "kernel32.dll" )]
		static extern bool SetConsoleTitle( string lpConsoleTitle );
	}
}