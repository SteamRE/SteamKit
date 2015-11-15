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
[assembly: AssemblyTitle( "SteamKit2" )]
[assembly: AssemblyDescription( "Steam Networking Library" )]
[assembly: AssemblyConfiguration( "" )]
[assembly: AssemblyCompany( "SteamRE Team" )]
[assembly: AssemblyProduct( "SteamKit2" )]
[assembly: AssemblyCopyright( "Copyright © SteamRE Team 2015" )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible( false )]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "cd42d0bc-72e4-451e-bcd0-5d09c4bca2a9" )]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion( "1.6.5.*" )]

#if DEBUG
[assembly: InternalsVisibleTo( "Tests" )]
#endif