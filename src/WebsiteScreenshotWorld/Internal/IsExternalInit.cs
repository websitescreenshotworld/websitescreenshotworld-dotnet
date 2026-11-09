#if NETSTANDARD2_0
namespace System.Runtime.CompilerServices;

// Lets init-only properties and records compile for netstandard2.0.
internal static class IsExternalInit
{
}
#endif
