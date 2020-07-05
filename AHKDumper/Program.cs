using System;
using System.IO;
using System.Text;
using AsmResolver;
using AsmResolver.PE;
using AsmResolver.PE.Win32Resources;

namespace AHKDumper
{
    internal static class Program
    {
        private static void Main( string[] args )
        {
            Console.WriteLine( "AHK-Dumper by drakonia - https://github.com/dr4k0nia/AHK-Dumper \r\n" );

            if ( args.Length == 0 )
            {
                Console.WriteLine( "Usage: AHKDumper.exe <file>" );
                Console.ReadKey();
                return;
            }

            // Open the PE image.
            string filePath = args[0].Replace( "\"", "" );
            var peImage = PEImage.FromFile( filePath );

            //Check resources for AHK data
            CheckResourceDirectory( peImage.Resources );

            Console.ReadKey();
        }

        private static void CheckResourceDirectory( IResourceDirectory directory )
        {
            //Loop trough all entries
            foreach ( var entry in directory.Entries )
            {
                string displayName = directory.Name ?? "ID: " + directory.Id;

                if ( displayName.Contains( "AHK" ) || displayName.Contains( "AUTOHOTKEY" ) )
                {
                    Console.WriteLine( "Found AHK resource: {0}", displayName );
                    if ( entry.IsData )
                        DumpData( (IResourceData) entry );
                }

                if ( entry.IsDirectory )
                    CheckResourceDirectory( (IResourceDirectory) entry );
            }
        }

        private static void DumpData( IResourceData data )
        {
            var reader = data.Contents.CreateReader( data.Contents.FileOffset, data.Contents.GetPhysicalSize() );
            byte[] dump = reader.ReadToEnd();
            PrintCompilerVersion( dump );
            File.WriteAllBytes( "dump.ahk", dump );
            Console.WriteLine( "Dumped AHK script with a size of {0} bytes",
                data.Contents.GetPhysicalSize().ToString() );
        }

        private static void PrintCompilerVersion( byte[] data )
        {
            //Check for AHK compiler version signature
            string text = Encoding.Default.GetString( data );
            if ( text.Contains( "<COMPILER:" ) )
            {
                Console.WriteLine( "Found AHK compiler signature: {0}",
                    text.Substring( text.IndexOf( "<" ), text.IndexOf( ">" ) ) );
            }
        }
    }
}