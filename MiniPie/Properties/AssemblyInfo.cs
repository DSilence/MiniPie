using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("MiniPie")]
[assembly: AssemblyDescription("Miniplayer for Spotify on Windows")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("MiniPie")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("AB96AA9B-B77D-4774-AE0F-B2324203B1F0")]
[assembly: AssemblyMetadata("SquirrelAwareVersion", "1")]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page, 
    // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
    //(used if a resource is not found in the page, 
    // app, or any theme specific resource dictionaries)
)]
[assembly: InternalsVisibleTo("MiniPie.Tests")]


//Supressions
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Potential Code Quality Issues", "RECS0165:Asynchronous methods should return a Task instead of void", Justification = "<Pending>", Scope = "member", Target = "~M:MiniPie.AppBootstrapper.ProcessTokenUpdate(System.String)")]