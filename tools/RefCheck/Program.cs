using Mono.Cecil;

// Verifies that every type/member the mod DLL uses exists in the given game assemblies.
// Run it after a game update to know - without launching the game - whether the mod still binds.
//
//   dotnet run --project tools/RefCheck -- <LuffyAbilities.dll> <BladeAndSorcery_Data/Managed> [more search dirs...]
if (args.Length < 2)
{
    Console.Error.WriteLine("usage: RefCheck <mod.dll> <search dir> [search dir...]");
    return 2;
}

var resolver = new DefaultAssemblyResolver();
foreach (string dir in args.Skip(1)) resolver.AddSearchDirectory(dir);
var module = ModuleDefinition.ReadModule(args[0], new ReaderParameters { AssemblyResolver = resolver });

int resolved = 0, missing = 0;
void Check(string kind, MemberReference reference, Func<object> resolve)
{
    try
    {
        if (resolve() != null) { resolved++; return; }
    }
    catch (Exception e) when (e is AssemblyResolutionException or InvalidOperationException) { }
    Console.WriteLine($"MISSING {kind}: {reference.FullName}");
    missing++;
}

foreach (TypeReference type in module.GetTypeReferences()) Check("type", type, type.Resolve);
foreach (MemberReference member in module.GetMemberReferences()) Check("member", member, member.Resolve);

Console.WriteLine($"resolved={resolved} missing={missing}");
return missing == 0 ? 0 : 1;
