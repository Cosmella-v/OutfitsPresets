using AmongUs.Data;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor;
using Reactor.Networking;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using System.Text.Json;
using Epic.OnlineServices.Mods;
using static Rewired.Controller;
using Il2CppInterop.Runtime.Injection;
using UnityEngine.UI;
using OutfitsPresets.FastDestroyableSingleton;
using System.Security.Cryptography;
using static Rewired.Utils.Classes.Data.SerializedObject;
using static Rewired.Data.CustomController_Editor;
using System.IO;

namespace OutfitsPresets;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
public partial class OutfitsPresetsPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    //public ConfigEntry<string> ConfigName { get; private set; }


    public override void Load()
    {
        // ConfigName = Config.Bind("Fake", "Name", ":>");
        FileUtils.GetPath();
        Harmony.PatchAll();
    }
   



    // [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    //  public static class ExamplePatch
    //{
    //    public static void Postfix(PlayerControl __instance)
    //    {
    //        __instance.cosmetics.nameText.text = PluginSingleton<OutfitsPresetsPlugin>.Instance.ConfigName.Value;
    //    }
    // }

    static public void SwapPos(Transform Object1, Transform Object2)
    {
        var pos1 = Object1.position;
        var pos2 = Object2.position;
        Object1.position = pos2;
        Object2.position = pos1;
    }
    [HarmonyPatch(typeof(PlayerCustomizationMenu))]
    public static class TabSync
    {
        internal static TextBoxTMP FileTextBox { get; set; }
        private static bool EnabledDelete = false;
        public class Outfit
        {
            public string Hat { get; set; }
            public string Skin { get; set; }
            public string Visor { get; set; }
            public string Pet { get; set; }

            public int Color { get; set; }
    }

    public static void ClickEquipHat(string hatname)
        {
            DataManager.Player.Customization.Hat = hatname;
            var hat = FastDestroyableSingleton<HatManager>.Instance.GetHatById(hatname);
            FakeNameTab.currentHat = hat;

            if (PlayerControl.LocalPlayer)
            {
                PlayerControl.LocalPlayer.RpcSetHat(hatname);
            }

        }
        public static void ClickEquipSkins(string RpcSetSkin)
        {
            DataManager.Player.Customization.skin = RpcSetSkin;
            
            if (PlayerControl.LocalPlayer)
            {
                PlayerControl.LocalPlayer.RpcSetSkin(RpcSetSkin);
            }
        }
        public static void ClickEquipVisor(string RpcSetVisor)
        {
            DataManager.Player.Customization.visor = RpcSetVisor;

            if (PlayerControl.LocalPlayer)
            {
                PlayerControl.LocalPlayer.RpcSetVisor(RpcSetVisor);
            }
        }
        public static void ClickEquipPet(string RpcSetPet)
        {
            DataManager.Player.Customization.Pet = RpcSetPet;

            if (PlayerControl.LocalPlayer)
            {
                PlayerControl.LocalPlayer.RpcSetPet(RpcSetPet);
            }
        }

        private static void SelectHat(string hatstr, int color)
        {
            var hat = FastDestroyableSingleton<HatManager>.Instance.GetHatById(hatstr);
            FakeNameTab.PlayerPreview.SetHat(hat, color);
            if (hat.BlocksVisors)
            {
                FakeNameTab.PlayerPreview.SetVisor(string.Empty, 0);
                return;
            }
            FakeNameTab.PlayerPreview.SetVisor(FakeNameTab.HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.VisorId : DataManager.Player.Customization.Visor, color);
        }

        private static void SelectSkin(string str, int color)
        {
            var Object = FastDestroyableSingleton<HatManager>.Instance.GetSkinById(str);
            FakeNameTab.PlayerPreview.SetSkin(Object,color);
        }
        private static void SelectVisor(string str, int color)
        {
            var Object = FastDestroyableSingleton<HatManager>.Instance.GetVisorById(str);
            FakeNameTab.PlayerPreview.SetVisor(Object, color);
        }
        private static void SelectPet(string str, int color)
        {
            var Object = FastDestroyableSingleton<HatManager>.Instance.GetPetById(str);
            FakeNameTab.PlayerPreview.SetPetIdle(Object, color);
        }


        public static HatsTab FakeNameTab;
        public static void push(PlayerCustomizationMenu __instance)
        {
            __instance.OpenTab(FakeNameTab);
            EnabledDelete = false;
            LoadTab(__instance);
        }
        public static void LoadTab(PlayerCustomizationMenu __instance)
        {
            PlayerCustomizationMenu _this = PlayerCustomizationMenu.Instance;
            var inner = FakeNameTab.scroller.Inner;

            foreach (var child in inner)
            {
                var childTransform = child.TryCast<Transform>();
                if (childTransform)
                {
                    Object.Destroy(childTransform.gameObject);
                }

            }
            FakeNameTab.ColorChips.Clear();
            int pos = 0;
            var Saver = Object.Instantiate(FakeNameTab.ColorTabPrefab, inner.transform);
            FakeNameTab.ColorChips.Add(Saver);
            Transform Text = FakeNameTab.transform.Find("Text");
            TextTranslatorTMP ModeTextx = Text.GetComponent<TextTranslatorTMP>();

            if (ModeTextx)
            {
                ModeTextx.defaultStr = "Outfits";
                ModeTextx.TargetText = StringNames.None;
                Text.GetComponent<TextMeshPro>().SetText("Outfits");
                ModeTextx.ResetText();
            }
            var defaultstring = Object.Instantiate(Text.transform, Saver.transform);
            ModeTextx = defaultstring.GetComponent<TextTranslatorTMP>();

            if (ModeTextx)
            {
                ModeTextx.defaultStr = "+";
                ModeTextx.TargetText = StringNames.None;
                defaultstring.GetComponent<TextMeshPro>().SetText("+");
                defaultstring.GetComponent<TextMeshPro>().color = Palette.AcceptedGreen;
                ModeTextx.ResetText();
            }
            defaultstring.gameObject.transform.localScale = new(3.5f, 3.5f, 1);
            defaultstring.gameObject.transform.localPosition = new(8.1481f ,0.267f, -1);

            float num22 = FakeNameTab.XRange.Lerp((float)(pos % FakeNameTab.NumPerRow) / ((float)FakeNameTab.NumPerRow - 1f));
            float num33 = FakeNameTab.YStart - (float)(pos / FakeNameTab.NumPerRow) * FakeNameTab.YOffset;
            Saver.transform.localPosition = new Vector3(num22, num33, -1f);
            pos += 1;

            Saver.Button.OnClick.AddListener((System.Action)delegate ()
            {
                var popup = Object.Instantiate(DiscordManager.Instance.discordPopup, FakeNameTab.transform);

                var background = popup.transform.Find("Background").GetComponent<SpriteRenderer>();
                var size = background.size;
                size.x *= 2.5f;
                background.size = size;
                popup.gameObject.transform.localPosition = new(3.5f, -1.8f, popup.gameObject.transform.localPosition.z);
                popup.TextAreaTMP.fontSizeMin = 2;
                popup.Show("What would you like to save this outfit as?");
                var btn = popup.transform.Find("ExitGame");
                btn.transform.localPosition = new(-1, btn.localPosition.y, btn.localPosition.z);
                Transform Text_TMP = btn.Find("Text_TMP");
                TextTranslatorTMP ModeText = Text_TMP.GetComponent<TextTranslatorTMP>();

                if (ModeText)
                {
                    ModeText.defaultStr = "Cancel";
                    ModeText.TargetText = StringNames.None;
                    Text_TMP.GetComponent<TextMeshPro>().SetText("Cancel");
                    ModeText.ResetText();
                }

                var Deny = Object.Instantiate(btn, popup.transform);
                Deny.transform.localPosition = new(1, btn.localPosition.y, btn.localPosition.z);
                Text_TMP = Deny.Find("Text_TMP");
                ModeText = Text_TMP.GetComponent<TextTranslatorTMP>();

                if (ModeText)
                {
                    ModeText.defaultStr = "Confirm";
                    ModeText.TargetText = StringNames.None;
                    Text_TMP.GetComponent<TextMeshPro>().SetText("Confirm");
                    ModeText.ResetText();
                }


                // 0.1
                /*var TextBox = Object.Instantiate(btn, popup.transform);
                TextBox.transform.position = new Vector3(0, 0.1f, btn.position.z); // Center it
                InputField inputField = TextBox.transform.gameObject.AddComponent<InputField>();
                TextBox.GetComponent<PassiveButton>().OnClick = new();
               */
                var editName = FastDestroyableSingleton<AccountManager>.Instance.accountTab.editNameScreen;
                var nameText = Object.Instantiate(editName.nameText.gameObject, popup.gameObject.transform);
                nameText.GetComponent<NameTextBehaviour>().Destroy();
                var Background = nameText.transform.Find("Background");
                var textBox = nameText.GetComponent<TextBoxTMP>();
                textBox.outputText.alignment = TextAlignmentOptions.CenterGeoAligned;
                textBox.outputText.transform.position = nameText.transform.position;
                nameText.SetActive(false);
                nameText.SetActive(true);
                textBox.SetText("Example");
                textBox.text = "Example";
                textBox.outputText.SetText("Example");
                textBox.AllowSymbols = true;
                textBox.AllowPaste = true;
                textBox.characterLimit = 30;
                var normalSprite = Background.GetComponent<SpriteRenderer>();
                normalSprite.size = new(9, 0.7f);
                Background.gameObject.layer = LayerMask.NameToLayer("UI");
                nameText.GetComponent<BoxCollider2D>().size = new(9, 0.8f);
                nameText.transform.localScale = new(0.5f, 0.5f, 0.5f);
                nameText.transform.localPosition = new(0, 0.15f, -2);
                btn.GetComponent<PassiveButton>().OnClick.AddListener((System.Action)delegate ()
                {
                    popup.gameObject.Destroy();
                    popup.Destroy();
                });
                Deny.GetComponent<PassiveButton>().OnClick.AddListener((System.Action)delegate ()
                {
                     string content = "{" +
                    "\n" + "\"Hat\" :" + "\"" + DataManager.Player.Customization.Hat + "\"" +
                     ",\n" + "\"Visor\" :" + "\"" + DataManager.Player.Customization.Visor + "\"" +
                      ",\n" + "\"Skin\" :" + "\"" + DataManager.Player.Customization.skin + "\"" +
                       ",\n" + "\"Color\" :" +  DataManager.Player.Customization.colorID +
                       ",\n" + "\"Pet\" :" + "\"" + DataManager.Player.Customization.Pet + "\"" +
                    "}";
                    if (!textBox.text.EndsWith(".json"))
                    {
                        FileUtils.SaveFile(textBox.text+".json", content);
                    }
                    else
                    {
                        FileUtils.SaveFile(textBox.text, content);
                    }
                    LoadTab(__instance);
                    popup.gameObject.Destroy();
                    popup.Destroy();
                });
            });

            Saver = Object.Instantiate(FakeNameTab.ColorTabPrefab, inner.transform);
            defaultstring = Object.Instantiate(Text.transform, Saver.transform);
            ModeTextx = defaultstring.GetComponent<TextTranslatorTMP>();

            if (ModeTextx)
            {
                ModeTextx.defaultStr = "-";
                ModeTextx.TargetText = StringNames.None;
                defaultstring.GetComponent<TextMeshPro>().SetText("-");
                defaultstring.GetComponent<TextMeshPro>().color = Palette.ImpostorRed;
                ModeTextx.ResetText();
            }
            defaultstring.gameObject.transform.localScale = new(3.5f, 3.5f, 1);
            defaultstring.gameObject.transform.localPosition = new(8.1481f, 0.267f, -1);

            num22 = FakeNameTab.XRange.Lerp((float)(pos % FakeNameTab.NumPerRow) / ((float)FakeNameTab.NumPerRow - 1f));
            num33 = FakeNameTab.YStart - (float)(pos / FakeNameTab.NumPerRow) * FakeNameTab.YOffset;
            Saver.transform.localPosition = new Vector3(num22, num33, -1f);
            pos += 1;

            Saver.Button.OnClick.AddListener((System.Action)delegate ()
            {
                EnabledDelete = !EnabledDelete;
                LoadTab(__instance);
            });





            foreach (var child in FileUtils.GetPath())
            {
                var Contain = JsonSerializer.Deserialize<Outfit>(FileUtils.ReadJsonFile(child));
                if (Contain.Skin == null)
                {
                    Contain.Skin = "";
                }
                if (Contain.Hat == null)
                {
                    Contain.Hat = "";
                }
                if (Contain.Visor == null)
                {
                    Contain.Visor = "";
                }
                if (Contain.Pet == null)
                {
                    Contain.Pet = "";
                }
                var FileName = System.IO.Path.GetFileNameWithoutExtension(child);
                /*Debug.LogWarning("The file contains: " +
                    "Hat:" + Contain.Hat +
                    "\nVisor:" + Contain.Visor +
                    "\nSkin:" + Contain.Skin +
                    "\nPet:" + Contain.Pet +
                    "\nColor:" + Contain.Color +
                    "\nName:" + FileName
                    );*/

                float num2 = FakeNameTab.XRange.Lerp((float)(pos % FakeNameTab.NumPerRow) / ((float)FakeNameTab.NumPerRow - 1f));
                float num3 = FakeNameTab.YStart - (float)(pos / FakeNameTab.NumPerRow) * FakeNameTab.YOffset;
                var ColorTabPrefab = Object.Instantiate(FakeNameTab.ColorTabPrefab, inner.transform);
                ColorTabPrefab.transform.localPosition = new Vector3(num2, num3, -1f);
                FakeNameTab.ColorChips.Add(ColorTabPrefab);
                if (!EnabledDelete)
                {
                    ColorTabPrefab.Button.OnMouseOver.AddListener((System.Action)delegate ()
                    {
                        PlayerCustomizationMenu.Instance.SetItemName(FileName);
                        TabSync.SelectHat(Contain.Hat, Contain.Color);
                        TabSync.SelectSkin(Contain.Skin, Contain.Color);
                        TabSync.SelectVisor(Contain.Visor, Contain.Color);
                        TabSync.SelectPet(Contain.Pet, Contain.Color);
                    });
                    ColorTabPrefab.Button.OnMouseOut.AddListener((System.Action)delegate ()
                    {
                        PlayerCustomizationMenu.Instance.SetItemName("None");
                        TabSync.SelectHat(DataManager.Player.Customization.Hat, DataManager.Player.Customization.Color);
                        TabSync.SelectSkin(DataManager.Player.Customization.skin, DataManager.Player.Customization.Color);
                        TabSync.SelectVisor(DataManager.Player.Customization.Visor, DataManager.Player.Customization.Color);
                    });
                    ColorTabPrefab.Button.OnClick.AddListener((System.Action)delegate ()
                    {
                        PlayerCustomizationMenu.Instance.SetItemName(FileName);
                        TabSync.ClickEquipHat(Contain.Hat);
                        TabSync.ClickEquipSkins(Contain.Skin);
                        TabSync.ClickEquipVisor(Contain.Visor);
                        TabSync.ClickEquipPet(Contain.Pet);
                    });
                } else
                {
                    defaultstring = Object.Instantiate(Text.transform, ColorTabPrefab.transform);
                    ModeTextx = defaultstring.GetComponent<TextTranslatorTMP>();

                    if (ModeTextx)
                    {
                        ModeTextx.defaultStr = "-";
                        ModeTextx.TargetText = StringNames.None;
                        defaultstring.GetComponent<TextMeshPro>().SetText("-");
                        defaultstring.GetComponent<TextMeshPro>().color = Palette.ImpostorRed;
                        ModeTextx.ResetText();
                    }
                    defaultstring.gameObject.transform.localScale = new(3.5f, 3.5f, 1);
                    defaultstring.gameObject.transform.localPosition = new(8.1481f, 0.267f, -50);

                    ColorTabPrefab.Button.OnClick.AddListener((System.Action)delegate (){
                        File.Delete(child);
                        LoadTab(__instance);
                    });
                }
                var SillyHat = Object.Instantiate(FakeNameTab.PlayerPreview, ColorTabPrefab.transform);
                SillyHat.SetBodyColor(Contain.Color);
                SillyHat.transform.localScale = new(0.4f, 0.4f, 0.4f);
                SillyHat.transform.localPosition = new(0, 0, -40);
                SillyHat.SetHat(Contain.Hat, SillyHat.ColorId);
                SillyHat.SetSkin(Contain.Skin, SillyHat.ColorId);
                SillyHat.SetVisor(Contain.Visor, SillyHat.ColorId);
                if (SillyHat.cosmetics.transform.Find("PetSlot") && SillyHat.cosmetics.transform.Find("PetSlot").GetChild(0))
                {
                    SillyHat.cosmetics.transform.Find("PetSlot").GetChild(0).gameObject.SetActive(false);
                }
                SillyHat.SetPetIdle(FastDestroyableSingleton<HatManager>.Instance.GetPetById(Contain.Pet), SillyHat.ColorId);
                ColorTabPrefab.SelectionHighlight.gameObject.SetActive(false);
                pos += 1;
            }
            FakeNameTab.SetScrollerBounds();

        }
        [HarmonyPatch(nameof(PlayerCustomizationMenu.Update))]
        [HarmonyPostfix]
        public static void Update_Postfix(PlayerCustomizationMenu __instance)
        {
            if (!FakeNameTab)
            {
                TabButton TabList = new TabButton();
                FakeNameTab = Object.Instantiate(__instance.Tabs[1].Tab, __instance.Tabs[1].Tab.transform.parent).TryCast<HatsTab>();
                FakeNameTab.name = "Outfits Presets";
                FakeNameTab.gameObject.active = false;
                TabList.Tab = FakeNameTab;
                TabList.Tab.name = "Outfits-Presets";


                System.Action OpenTab = () =>
                        TabSync.push(__instance);
                TabList.tabText = FakeNameTab.transform.Find("Text").GetComponent<TextMeshPro>();
               
                var btn = Object.Instantiate(__instance.Tabs[1].Button.transform.parent.parent, __instance.Tabs[1].Button.transform.parent.parent.parent);
                btn.name = "OutfitsButtons";
                btn.position = new(btn.position.x + 3, btn.position.y, btn.position.z);
                var CubesTab = btn.transform.parent.Find("CubesTab");
                btn.position = new(CubesTab.position.x + 1, btn.position.y, btn.position.z);
                SwapPos(CubesTab, btn);
                var bg = btn.GetChild(0).Find("Tab Background");
                var button = bg.GetComponent<PassiveButton>();
                button.OnClick = new();
                button.OnClick.AddListener(OpenTab);
                TabList.Button = bg.GetComponent<SpriteRenderer>();
                var tabsList = __instance.Tabs.ToList();
                tabsList.Add(TabList);
                __instance.Tabs = tabsList.ToArray();
            }
          
        }

    }
}






