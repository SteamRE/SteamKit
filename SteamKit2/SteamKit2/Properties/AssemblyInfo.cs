/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "SteamKit2 ")]
[assembly: AssemblyDescription( ".NET library that aims to interoperate with the Steam network." )]
[assembly: AssemblyConfiguration( "" )]
[assembly: AssemblyCompany( "SteamRE Team" )]
[assembly: AssemblyProduct( "SteamKit2" )]
[assembly: AssemblyCopyright( "Copyright © SteamRE Team 2018" )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible( false )]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "cd42d0bc-72e4-451e-bcd0-5d09c4bca2a9" )]

// These are automatically modified by AppVeyor for CI builds and automated deployment.
[assembly: AssemblyVersion( "2.0.0.0" )]
[assembly: AssemblyFileVersion( "2.0.0.0" )]
[assembly: AssemblyInformationalVersion( "2.0.0 - Development" )]

#if DEBUG
[assembly: InternalsVisibleTo( "Tests" )]
#endif
