
using System;
using System.IO;
using AmongUs.Data;
using System.Text.Json;
using TMPro;
using UnityEngine;
using Il2CppSystem.Collections.Generic;
using TabsBuilderApi.Utils;
using System.Reflection;

namespace OutfitsPresets
{
    public class Outfit
    {
        public string Hat { get; set; } = "";
        public string Skin { get; set; } = "";
        public string Visor { get; set; } = "";
        public string Pet { get; set; } = "";
        public int Color { get; set; }
    }
    [TabBuilder("Outfits Presets", "OutfitsPresets.icon.png", typeof(OutfitsPresetsTab), TabBuilderType.Before, "CubeGroup")]
    // tests adding more
    //[TabBuilder("Outfits Presets2", "OutfitsPresets.icon.png", typeof(OutfitsPresetsTab), TabBuilderType.Before, "CubeGroup")]
   // [TabBuilder("Outfits Presets3", "OutfitsPresets.icon.png", typeof(OutfitsPresetsTab), TabBuilderType.Before, "CubeGroup")]
   // [TabBuilder("Outfits Presets4", "OutfitsPresets.icon.png", typeof(OutfitsPresetsTab), TabBuilderType.Before, "CubeGroup")]
   // [TabBuilder("Outfits Presets5", "OutfitsPresets.icon.png", typeof(OutfitsPresetsTab), TabBuilderType.Before, "CubeGroup")]
   // [TabBuilder("Outfits Presets6", "OutfitsPresets.icon.png", typeof(OutfitsPresetsTab), TabBuilderType.Before, "CubeGroup")]
    public class OutfitsPresetsTab : InventoryTab
    {
        // Token: 0x06001F80 RID: 8064 RVA: 0x000845EC File Offset: 0x000827EC
        bool EnabledDelete = false;
        private HashSet<int> AvailableColors = new HashSet<int>();

        private void Awake()
        {
            Transform Text = this.transform.Find("Text");
            TextTranslatorTMP ModeTextx = Text.GetComponent<TextTranslatorTMP>();

            if (ModeTextx)
            {
                ModeTextx.defaultStr = "Outfits";
                ModeTextx.TargetText = StringNames.None;
                Text.GetComponent<TextMeshPro>().SetText("Outfits");
                ModeTextx.ResetText();
            }
        }

        public void ClickEquipHat(string hatname)
        {
            DataManager.Player.Customization.Hat = hatname;
            PlayerControl.LocalPlayer?.RpcSetHat(hatname);
        }

        public void ClickEquipSkins(string skin)
        {
            DataManager.Player.Customization.skin = skin;
            PlayerControl.LocalPlayer?.RpcSetSkin(skin);
        }

        public void ClickEquipVisor(string visor)
        {
            DataManager.Player.Customization.visor = visor;
            PlayerControl.LocalPlayer?.RpcSetVisor(visor);
        }

        public void ClickEquipPet(string pet)
        {
            DataManager.Player.Customization.Pet = pet;
            PlayerControl.LocalPlayer?.RpcSetPet(pet);
        }

        private void SelectHat(string hat, int color)
        {
            var hatObj = DestroyableSingleton<HatManager>.Instance.GetHatById(hat);
            this.PlayerPreview.SetHat(hatObj, color);

            if (hatObj?.BlocksVisors == true)
            {
                this.PlayerPreview.SetVisor(string.Empty, 0);
                return;
            }

            var visor = this.HasLocalPlayer() ?
                PlayerControl.LocalPlayer.Data.DefaultOutfit.VisorId :
                DataManager.Player.Customization.Visor;

            this.PlayerPreview.SetVisor(visor, color);
        }

        private void SelectSkin(string skin, int color)
        {
            var skinObj = DestroyableSingleton<HatManager>.Instance.GetSkinById(skin);
            this.PlayerPreview.SetSkin(skinObj, color);
        }

        private void SelectVisor(string visor, int color)
        {
            var visorObj = DestroyableSingleton<HatManager>.Instance.GetVisorById(visor);
            this.PlayerPreview.SetVisor(visorObj, color);
        }

        private void SelectPet(string pet, int color)
        {
            var petObj = DestroyableSingleton<HatManager>.Instance.GetPetById(pet);
            this.PlayerPreview.SetPetIdle(petObj, color);
        }

        public void ClickColorEquip(byte color)
        {
            DataManager.Player.Customization.colorID = color;
            PlayerControl.LocalPlayer?.SetColor(color);
        }


        public override void OnEnable()
        {

            this.PlayerPreview.gameObject.SetActive(true);
        
            if (this.HasLocalPlayer())
            {
                this.PlayerPreview.UpdateFromLocalPlayer(PlayerMaterial.MaskType.None);
            }
            else
            {
                this.PlayerPreview.UpdateFromDataManager(PlayerMaterial.MaskType.None);
            }

            LoadTab();
        }

        public void LoadTab()
        {
            for (int i = 0; i < this.ColorChips.Count; i++)
            {
                UnityEngine.Object.Destroy(this.ColorChips[i].gameObject);
            }
            this.ColorChips.Clear();
            this.PlayerPreview.gameObject.SetActive(true);
            var inner = this.scroller.Inner;
            int pos = 0;

            AddPlusButton(inner, ref pos);

            AddMinusButton(inner, ref pos);

            foreach (var file in FileUtils.GetPath())
            {
                var outfit = JsonSerializer.Deserialize<Outfit>(FileUtils.ReadJsonFile(file)) ?? new Outfit();
                AddOutfitButton(inner, outfit, file, ref pos);
            }

            PlayerCustomizationMenu _this = PlayerCustomizationMenu.Instance;
            _this.nameplateMaskArea.gameObject.SetActive(false);
            SetScrollerBounds();
            Invoke(nameof(playerfix), 0.1f);
        }
        public void playerfix()
        {
            this.PlayerPreview.gameObject.SetActive(true);

            if (this.HasLocalPlayer())
            {
                this.PlayerPreview.UpdateFromLocalPlayer(PlayerMaterial.MaskType.None);
            }
            else
            {
                this.PlayerPreview.UpdateFromDataManager(PlayerMaterial.MaskType.None);
            }
        }

       private void AddPlusButton(Transform inner, ref int pos) {
            ColorChip prefab = UnityEngine.Object.Instantiate<ColorChip>(this.ColorTabPrefab, this.scroller.Inner);
            prefab.transform.localPosition = ChipPosition(pos);
            this.ColorChips.Add(prefab);
            SetButtonText(prefab, "+", Palette.AcceptedGreen);
            pos++;
            System.Action Opensave = () => OpenSavePopup();
            prefab.Button.OnClick.AddListener(Opensave);
        }

        private void AddMinusButton(Transform inner, ref int pos)
        {
            ColorChip prefab = UnityEngine.Object.Instantiate<ColorChip>(this.ColorTabPrefab, this.scroller.Inner);
            prefab.transform.localPosition = ChipPosition(pos);
            this.ColorChips.Add(prefab);
            SetButtonText(prefab, "-", Palette.ImpostorRed);
            pos++;


            System.Action ButtonAction = () =>
            {
                EnabledDelete = !EnabledDelete;
                LoadTab();
            };
            prefab.Button.OnClick.AddListener(ButtonAction);
        }
        // couldn't get the among us thing working?? so i did math :O
        private Vector3 ChipPosition(int num)
        {
            int col = num % 4;
            int row = num / 4;

            float x = -1f + col * 1.0f;  
            float y = -1f - row * 1.0f; 
            float z = -1f;

            return new Vector3(x, y, z);
        }
        private bool ColorEquipable(Outfit outfit)
        {
            for (int i = 0; i < Palette.PlayerColors.Length; i++)
            {
                this.AvailableColors.Add(i);
            }
            if (GameData.Instance)
            {
                List<NetworkedPlayerInfo> allPlayers = GameData.Instance.AllPlayers;
                for (int j = 0; j < allPlayers.Count; j++)
                {
                    NetworkedPlayerInfo networkedPlayerInfo = allPlayers[j];
                    this.AvailableColors.Remove(networkedPlayerInfo.DefaultOutfit.ColorId);
                }
            }

            return (this.AvailableColors.Contains(outfit.Color));
           
           }

        private void AddOutfitButton(Transform inner, Outfit outfit, string file, ref int pos)
        {
            ColorChip prefab = UnityEngine.Object.Instantiate<ColorChip>(this.ColorTabPrefab, this.scroller.Inner);
            prefab.transform.localPosition = ChipPosition(pos);
            prefab.Button.ClickMask = this.scroller.Hitbox;
            this.ColorChips.Add(prefab);
            pos++;

            string fileName = Path.GetFileNameWithoutExtension(file);

            if (!EnabledDelete)
            {
                System.Action View = () =>
                {
                    PlayerCustomizationMenu.Instance.SetItemName(fileName);
                    if (ColorEquipable(outfit))
                    {
                        this.PlayerPreview.SetBodyColor(outfit.Color);
                    };
                    SelectHat(outfit.Hat, outfit.Color);
                    SelectSkin(outfit.Skin, outfit.Color);
                    SelectVisor(outfit.Visor, outfit.Color);
                    SelectPet(outfit.Pet, outfit.Color);
                };
                prefab.Button.OnMouseOver.AddListener(View);

                System.Action None = () =>
                {
                    PlayerCustomizationMenu.Instance.SetItemName("None");
                    this.PlayerPreview.SetBodyColor(DataManager.Player.Customization.Color);
                    SelectHat(DataManager.Player.Customization.Hat, DataManager.Player.Customization.Color);
                    SelectSkin(DataManager.Player.Customization.skin, DataManager.Player.Customization.Color);
                    SelectVisor(DataManager.Player.Customization.Visor, DataManager.Player.Customization.Color);
                    SelectPet(DataManager.Player.Customization.Pet, DataManager.Player.Customization.Color);
                };
                prefab.Button.OnMouseOut.AddListener(None);

                System.Action Equip = () =>
                {
                    PlayerCustomizationMenu.Instance.SetItemName(fileName);
                    if (ColorEquipable(outfit))
                    {
                        ClickColorEquip((byte)outfit.Color);
                    }
                    ClickEquipHat(outfit.Hat);
                    ClickEquipSkins(outfit.Skin);
                    ClickEquipVisor(outfit.Visor);
                    ClickEquipPet(outfit.Pet);
                };
                prefab.Button.OnClick.AddListener(Equip);
            }
            else
            {
                SetButtonText(prefab, "-", Palette.ImpostorRed);
                System.Action Load = () =>
                {
                    Il2CppSystem.IO.File.Delete(file);
                    LoadTab();
                };
                prefab.Button.OnClick.AddListener(Load);
            }

            var preview = UnityEngine.Object.Instantiate(PlayerPreview, prefab.transform);
            preview.ToggleName(false);
            preview.SetBodyColor(outfit.Color);
            preview.transform.localScale = Vector3.one * 0.4f;
            preview.transform.localPosition = new Vector3(0, 0, -40);
            preview.SetHat(outfit.Hat, preview.ColorId);
            preview.SetSkin(outfit.Skin, preview.ColorId);
            preview.SetVisor(outfit.Visor, preview.ColorId);
            preview.SetPetIdle(DestroyableSingleton<HatManager>.Instance.GetPetById(outfit.Pet), preview.ColorId);
            preview.cosmetics.transform.Find("PetSlot")?.GetChild(0)?.gameObject.SetActive(false);
            prefab.SelectionHighlight.gameObject.SetActive(false);
        }

        private void SetButtonText(ColorChip chip, string text, Color color)
        {
            var textTransform = this.transform.Find("Text");
            var textObj = UnityEngine.Object.Instantiate(textTransform, chip.transform);
            var comp = textObj.GetComponent<TextTranslatorTMP>();
            if (comp != null)
            {
                comp.defaultStr = text;
                comp.TargetText = StringNames.None;
                textObj.GetComponent<TextMeshPro>().SetText(text);
                textObj.GetComponent<TextMeshPro>().color = color;
                comp.ResetText();
            }
            textObj.transform.localScale = new Vector3(3.5f, 3.5f, 1);
            textObj.transform.localPosition = new Vector3(8.1481f, 0.267f, -64);
        }

        private void OpenSavePopup()
        {
            var popup = UnityEngine.Object.Instantiate(DiscordManager.Instance.discordPopup, this.transform);

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

            var Deny = UnityEngine.Object.Instantiate(btn, popup.transform);
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


            var editName = DestroyableSingleton<AccountManager>.Instance.accountTab.editNameScreen;
            var nameText = UnityEngine.Object.Instantiate(editName.nameText.gameObject, popup.gameObject.transform);
            NameTextBehaviour.Destroy(nameText.GetComponent<NameTextBehaviour>());
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
                UnityEngine.Object.Destroy(popup.gameObject);
                UnityEngine.Object.Destroy(popup);
            });
            Deny.GetComponent<PassiveButton>().OnClick.AddListener((System.Action)delegate ()
            {
                string content = "{" +
               "\n" + "\"Hat\" :" + "\"" + DataManager.Player.Customization.Hat + "\"" +
                ",\n" + "\"Visor\" :" + "\"" + DataManager.Player.Customization.Visor + "\"" +
                 ",\n" + "\"Skin\" :" + "\"" + DataManager.Player.Customization.skin + "\"" +
                  ",\n" + "\"Color\" :" + DataManager.Player.Customization.colorID +
                  ",\n" + "\"Pet\" :" + "\"" + DataManager.Player.Customization.Pet + "\"" +
               "}";
                if (!textBox.text.EndsWith(".json"))
                {
                    FileUtils.SaveFile(textBox.text + ".json", content);
                }
                else
                {
                    FileUtils.SaveFile(textBox.text, content);
                }
                LoadTab();
                UnityEngine.Object.Destroy(popup.gameObject);
            });
        }

    }}
