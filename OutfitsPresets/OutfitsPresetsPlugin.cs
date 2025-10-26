using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using BepInEx.Logging;
using Il2CppInterop.Runtime.Injection;
namespace OutfitsPresets;

[BepInAutoPlugin]
[BepInDependency(TabsBuilderApi.TabBuilderPlugin.Id)]
[BepInProcess("Among Us.exe")]
public partial class OutfitsPresetsPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    public static ManualLogSource mls;
    public override void Load()
    {
        mls = BepInEx.Logging.Logger.CreateLogSource(Id);
        mls.LogInfo($"{Id} is now awake!");
        FileUtils.GetPath();
        Harmony.PatchAll();
        ClassInjector.RegisterTypeInIl2Cpp<OutfitsPresetsTab>();
    }
}
