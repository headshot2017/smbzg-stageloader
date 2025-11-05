using MelonLoader;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using SMBZG.CharacterSelect;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(StageLoader.Core), "StageLoader", "1.2", "Headshotnoby/headshot2017", null)]
[assembly: MelonGame("Jonathan Miller aka Zethros", "SMBZ-G")]

namespace StageLoader
{
    public class Core : MelonMod
    {
        public static List<StageData> customStages;
        public static Dropdown stageDropdown;
        public static int messageType;
        public static string guiMsg;
        public static string guiTotalStages;

        public static BattleCache.StageEnum[] OriginalStageArray;

        public override void OnInitializeMelon()
        {
            customStages = new List<StageData>();
            stageDropdown = null;
            messageType = 0;
        }

        public override void OnLateInitializeMelon()
        {
            OriginalStageArray = BattleCache.StageArray;
            LoadCustomStages();
        }

        public override void OnGUI()
        {
            int w = 240 * 2;
            int h = 180 * 2;
            switch (messageType)
            {
                case 1:
                    GUI.BeginGroup(new Rect(Screen.width / 2 - w / 2, Screen.height / 2 - h / 2, w, h));
                    GUI.Box(new Rect(0, 0, w, h), "StageLoader");
                    GUI.Label(new Rect(32, 32, w - 64, h - 64), $"{guiMsg}\n\n\n{guiTotalStages}");
                    GUI.EndGroup();
                    break;

                case 2:
                    w = 480 * 2;
                    h = 360 * 2;
                    GUI.BeginGroup(new Rect(Screen.width / 2 - w / 2, Screen.height / 2 - h / 2, w, h));
                    GUI.Box(new Rect(0, 0, w, h), "StageLoader");
                    GUI.Label(new Rect(32, 32, w - 64, h - 64), guiMsg);
                    if (GUI.Button(new Rect(20, h - 64 - 8, w - 40, 64), "OK"))
                        messageType = 0;
                    GUI.EndGroup();
                    break;
            }
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (buildIndex == 7)
                LoadCustomStageUI();
        }

        public void LoadCustomStages()
        {
            GameObject obj = new GameObject("StageLoader");
            GameObject.DontDestroyOnLoad(obj);
            StageLoaderComponent dl = obj.AddComponent<StageLoaderComponent>();
        }

        public void LoadCustomStageUI()
        {
            Font arial = null;
            Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
            foreach (Font font in fonts)
            {
                if (font.name == "Arial")
                {
                    arial = font;
                    break;
                }
            }

            Sprite uisprite = null;
            Sprite uiarrow = null;
            Sprite[] sprites = Resources.FindObjectsOfTypeAll<Sprite>();
            foreach (Sprite sprite in sprites)
            {
                if (sprite.name == "UISprite")
                    uisprite = sprite;
                if (sprite.name == "DropdownArrow")
                    uiarrow = sprite;
            }

            // Setup the UI

            // Find a dropdown prefab
            // Go through each children until it's found
            GameObject dropdownTemplate = null;
            Stack<Transform> objs = new Stack<Transform>();
            objs.Push(CharacterSelectScript.ins.Section_BattleSettings.transform);
            while (objs.Count > 0)
            {
                Transform obj = objs.Pop();

                if (obj.GetComponent<Dropdown>() != null && obj.Find("Template") != null)
                {
                    dropdownTemplate = obj.Find("Template").gameObject;
                    objs.Clear();
                    break;
                }

                for (int i = 0; i < obj.transform.childCount; i++)
                    objs.Push(obj.GetChild(i));
            }

            if (!dropdownTemplate)
            {
                LoggerInstance.Msg($"================ WARNING ================");
                LoggerInstance.Msg($"UI DROPDOWN TEMPLATE NOT FOUND");
                LoggerInstance.Msg($"Custom stage selector will NOT be visible");
                LoggerInstance.Msg($"=========================================");
                return;
            }

            // Part 1: GameObjects
            GameObject root = CharacterSelectScript.ins.Section_StageSelect;
            GameObject CustomStageRoot = new GameObject("Custom Stage UI");

            GameObject CustomStageLabelObj = new GameObject("Custom Stage Label");
            GameObject CustomStageLabelImgObj = new GameObject("Custom Stage Label Img");
            GameObject CustomStageLabelTextObj = new GameObject("Custom Stage Label Text");

            GameObject CustomStageListObj = new GameObject("Custom Stage List");
            GameObject CustomStageListTextObj = new GameObject("Custom Stage List Text");
            GameObject CustomStageListArrowObj = new GameObject("Custom Stage List Arrow");
            GameObject CustomStageListTemplate = GameObject.Instantiate(dropdownTemplate);

            GameObject CustomStageConfirmObj = new GameObject("Custom Stage Confirm");
            GameObject CustomStageConfirmTextObj = new GameObject("Custom Stage Confirm Text");

            GameObject CustomStageReloadObj = new GameObject("Custom Stage Reload");
            GameObject CustomStageReloadTextObj = new GameObject("Custom Stage Reload Text");


            // Part 2: Children setup
            CustomStageRoot.transform.parent = root.transform;
            CustomStageRoot.transform.localPosition = new Vector3(-50, -400);
            CustomStageRoot.transform.localScale = Vector3.one;

            CustomStageLabelObj.transform.parent = CustomStageRoot.transform;
            CustomStageLabelObj.transform.localPosition = new Vector3(-350, 0);
            CustomStageLabelObj.transform.localScale = Vector3.one;
            CustomStageLabelImgObj.transform.parent = CustomStageLabelObj.transform;
            CustomStageLabelImgObj.transform.localPosition = Vector3.zero;
            CustomStageLabelImgObj.transform.localScale = Vector3.one;
            CustomStageLabelTextObj.transform.parent = CustomStageLabelObj.transform;
            CustomStageLabelTextObj.transform.localPosition = Vector3.zero;
            CustomStageLabelTextObj.transform.localScale = Vector3.one;

            CustomStageListObj.transform.parent = CustomStageRoot.transform;
            CustomStageListObj.transform.localPosition = Vector3.zero;
            CustomStageListObj.transform.localScale = Vector3.one;
            CustomStageListTextObj.transform.parent = CustomStageListObj.transform;
            CustomStageListTextObj.transform.localPosition = new Vector3(8, 0);
            CustomStageListTextObj.transform.localScale = Vector3.one;
            CustomStageListArrowObj.transform.parent = CustomStageListObj.transform;
            CustomStageListArrowObj.transform.localPosition = new Vector3(235, 0);
            CustomStageListArrowObj.transform.localScale = Vector3.one;
            CustomStageListTemplate.transform.SetParent(CustomStageListObj.transform, false);
            CustomStageListTemplate.transform.localPosition = Vector3.zero;
            CustomStageListTemplate.transform.localScale = Vector3.one;

            CustomStageConfirmObj.transform.parent = CustomStageRoot.transform;
            CustomStageConfirmObj.transform.localPosition = new Vector3(310, 0);
            CustomStageConfirmObj.transform.localScale = Vector3.one;
            CustomStageConfirmTextObj.transform.parent = CustomStageConfirmObj.transform;
            CustomStageConfirmTextObj.transform.localPosition = Vector3.zero;
            CustomStageConfirmTextObj.transform.localScale = Vector3.one;

            CustomStageReloadObj.transform.parent = CustomStageRoot.transform;
            CustomStageReloadObj.transform.localPosition = new Vector3(440, 0);
            CustomStageReloadObj.transform.localScale = Vector3.one;
            CustomStageReloadTextObj.transform.parent = CustomStageReloadObj.transform;
            CustomStageReloadTextObj.transform.localPosition = Vector3.zero;
            CustomStageReloadTextObj.transform.localScale = Vector3.one;


            // Part 3: Components
            RectTransform CustomStageLabelTextTr = CustomStageLabelTextObj.AddComponent<RectTransform>();
            RectTransform CustomStageLabelImgTr = CustomStageLabelImgObj.AddComponent<RectTransform>();
            Text CustomStageLabelText = CustomStageLabelTextObj.AddComponent<Text>();
            Image CustomStageLabelImg = CustomStageLabelImgObj.AddComponent<Image>();
            CustomStageLabelTextTr.sizeDelta = new Vector2(150, 32);
            CustomStageLabelImgTr.sizeDelta = new Vector2(150, 32);
            CustomStageLabelText.text = "Custom Stages";
            CustomStageLabelText.color = Color.black;
            CustomStageLabelText.fontSize = 18;
            CustomStageLabelText.fontStyle = FontStyle.Bold;
            CustomStageLabelText.alignment = TextAnchor.MiddleCenter;
            CustomStageLabelText.font = arial;
            CustomStageLabelImg.color = new Color(0.75f, 0.75f, 0.75f);
            CustomStageLabelImg.sprite = uisprite;
            CustomStageLabelImg.type = Image.Type.Sliced;

            RectTransform CustomStageListTr = CustomStageListObj.AddComponent<RectTransform>();
            RectTransform CustomStageListTextTr = CustomStageListTextObj.AddComponent<RectTransform>();
            RectTransform CustomStageListArrowTr = CustomStageListArrowObj.AddComponent<RectTransform>();
            Image CustomStageListImg = CustomStageListObj.AddComponent<Image>();
            Dropdown CustomStageListDropdown = CustomStageListObj.AddComponent<Dropdown>();
            Text CustomStageListText = CustomStageListTextObj.AddComponent<Text>();
            Image CustomStageListArrowImg = CustomStageListArrowObj.AddComponent<Image>();
            CustomStageListTr.sizeDelta = new Vector2(500, 32);
            CustomStageListTextTr.sizeDelta = new Vector2(500, 32);
            CustomStageListArrowTr.sizeDelta = new Vector2(20, 20);
            CustomStageListImg.sprite = uisprite;
            CustomStageListImg.type = Image.Type.Sliced;
            CustomStageListDropdown.transition = Selectable.Transition.ColorTint;
            CustomStageListDropdown.image = CustomStageListImg;
            CustomStageListDropdown.captionText = CustomStageListText;
            CustomStageListDropdown.itemText = CustomStageListTemplate.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(2).gameObject.GetComponent<Text>();
            CustomStageListDropdown.template = CustomStageListTemplate.GetComponent<RectTransform>();
            CustomStageListText.text = "item";
            CustomStageListText.color = Color.black;
            CustomStageListText.fontSize = 18;
            CustomStageListText.alignment = TextAnchor.MiddleLeft;
            CustomStageListText.font = arial;
            CustomStageListArrowImg.sprite = uiarrow;

            RectTransform CustomStageConfirmTr = CustomStageConfirmObj.AddComponent<RectTransform>();
            RectTransform CustomStageConfirmTextTr = CustomStageConfirmTextObj.AddComponent<RectTransform>();
            Image CustomStageConfirmImg = CustomStageConfirmObj.AddComponent<Image>();
            Button CustomStageConfirmBtn = CustomStageConfirmObj.AddComponent<Button>();
            Text CustomStageConfirmText = CustomStageConfirmTextObj.AddComponent<Text>();
            CustomStageConfirmTr.sizeDelta = new Vector2(64, 32);
            CustomStageConfirmTextTr.sizeDelta = new Vector2(64, 32);
            CustomStageConfirmImg.sprite = uisprite;
            CustomStageConfirmImg.type = Image.Type.Sliced;
            CustomStageConfirmBtn.image = CustomStageConfirmImg;
            CustomStageConfirmText.text = "Go";
            CustomStageConfirmText.color = Color.black;
            CustomStageConfirmText.fontSize = 18;
            CustomStageConfirmText.fontStyle = FontStyle.Bold;
            CustomStageConfirmText.alignment = TextAnchor.MiddleCenter;
            CustomStageConfirmText.font = arial;

            RectTransform CustomStageReloadTr = CustomStageReloadObj.AddComponent<RectTransform>();
            RectTransform CustomStageReloadTextTr = CustomStageReloadTextObj.AddComponent<RectTransform>();
            Image CustomStageReloadImg = CustomStageReloadObj.AddComponent<Image>();
            Button CustomStageReloadBtn = CustomStageReloadObj.AddComponent<Button>();
            Text CustomStageReloadText = CustomStageReloadTextObj.AddComponent<Text>();
            CustomStageReloadTr.sizeDelta = new Vector2(192, 32);
            CustomStageReloadTextTr.sizeDelta = new Vector2(192, 32);
            CustomStageReloadImg.sprite = uisprite;
            CustomStageReloadImg.type = Image.Type.Sliced;
            CustomStageReloadBtn.image = CustomStageReloadImg;
            CustomStageReloadText.text = "Refresh stage list";
            CustomStageReloadText.color = Color.black;
            CustomStageReloadText.fontSize = 18;
            CustomStageReloadText.fontStyle = FontStyle.Bold;
            CustomStageReloadText.alignment = TextAnchor.MiddleCenter;
            CustomStageReloadText.font = arial;


            List<string> stageNames = new List<string>();
            foreach (StageData stage in customStages)
                stageNames.Add(stage.name);

            CustomStageListDropdown.AddOptions(stageNames);
            CustomStageConfirmBtn.onClick.AddListener(OnCustomStageGo);
            CustomStageReloadBtn.onClick.AddListener(OnCustomStageRefresh);
            stageDropdown = CustomStageListDropdown;
        }

        void OnCustomStageGo()
        {
            BattleCache.StageEnum? stage = 0;
            typeof(CharacterSelectScript).GetField("SelectedStage", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(CharacterSelectScript.ins, stage);
            CharacterSelectScript.ins.OnSubmit();
        }

        void OnCustomStageRefresh()
        {
            LoadCustomStages();
        }

        [HarmonyPatch(typeof(BattleCache), "Stage_GetData", new Type[] { typeof(BattleCache.StageEnum) })]
        private static class GetStageDataPatch
        {
            private static bool Prefix(ref StageData __result, BattleCache.StageEnum stage)
            {
                int stageInt = (int)stage;
                if (stageInt < 100) return true;
                __result = customStages[stageInt - 100];
                return false;
            }
        }

        [HarmonyPatch(typeof(BattleBackgroundManager), "SetSkyBackground", new Type[] { typeof(SkyBackgroundData) })]
        private static class SetSkyBackgroundPatch
        {
            private static bool Prefix(BattleBackgroundManager __instance, SkyBackgroundData skyBackground)
            {
                foreach (StageData stage in customStages)
                {
                    foreach (SkyBackgroundData sky in stage.SkyDataList)
                    {
                        if (sky == skyBackground)
                        {
                            FieldInfo ActiveSkyBackgroundData = typeof(BattleBackgroundManager).GetField("ActiveSkyBackgroundData", BindingFlags.NonPublic | BindingFlags.Instance);
                            MethodInfo ClearSkyBackground = typeof(BattleBackgroundManager).GetMethod("ClearSkyBackground", BindingFlags.Instance | BindingFlags.NonPublic);

                            if (ActiveSkyBackgroundData.GetValue(__instance) != null)
                            {
                                ClearSkyBackground.Invoke(__instance, null);
                                ActiveSkyBackgroundData.SetValue(__instance, null);
                            }

                            ActiveSkyBackgroundData.SetValue(__instance, skyBackground);
                            if (ActiveSkyBackgroundData.GetValue(__instance) != null)
                            {
                                List<AirRushParticleSystem> SkyCloudParticleSystem_List = (List<AirRushParticleSystem>)typeof(BattleBackgroundManager).GetField("SkyCloudParticleSystem_List", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                                MethodInfo SnapToTarget = typeof(FollowObject).GetMethod("SnapToTarget", BindingFlags.Instance | BindingFlags.NonPublic);

                                Camera.main.backgroundColor = skyBackground.SkyColor;
                                AirRushParticleSystem airRushParticleSystem = UnityEngine.Object.Instantiate(skyBackground.SkyParticleSystem_Prefab);
                                airRushParticleSystem.gameObject.SetActive(true);
                                airRushParticleSystem.Comp_Follow.SetTarget(Camera.main.transform);
                                SnapToTarget.Invoke(airRushParticleSystem.Comp_Follow, null);
                                airRushParticleSystem.Comp_PS.Play();
                                SkyCloudParticleSystem_List.Add(airRushParticleSystem);
                            }

                            return false;
                        }
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(BattleBackgroundManager), "SetupStageData", new Type[] { typeof(StageData), typeof(BattleMusicData), typeof(int?) })]
        private static class SetupStageDataPatch
        {
            private static bool Prefix(BattleBackgroundManager __instance, StageData stage, BattleMusicData song, int? battleBackgroundIndex = null)
            {
                bool isArcade = (bool)typeof(GC).GetProperty("IsInArcadeMode", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(GC.ins);
                SMBZG.BattleSettings BattleSettings = (SMBZG.BattleSettings)typeof(GC).GetField("BattleSetting", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(GC.ins);
                BattleCache.StageEnum? SelectedStage = BattleSettings.Stage;

                if (isArcade || SelectedStage != 0)
                    return true;

                FieldInfo ActiveStage = typeof(BattleBackgroundManager).GetField("ActiveStage", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo ActiveSong = typeof(BattleBackgroundManager).GetField("ActiveSong", BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo SetBattleBackground = typeof(BattleBackgroundManager).GetMethod("SetBattleBackground", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(BattleBackgroundData) }, null);

                Melon<Core>.Logger.Msg($"Playing custom stage: {customStages[stageDropdown.value].name}");

                StageData customStage = customStages[stageDropdown.value];
                ActiveStage.SetValue(__instance, customStage);
                ActiveSong.SetValue(__instance, song);
                //BattleBackgroundData battleBackground = ((!battleBackgroundIndex.HasValue) ? stage.BattleBackgroundDataList.GetRandom() : stage.BattleBackgroundDataList[battleBackgroundIndex.Value]);
                //BattleBackgroundData battleBackground = customStage.BattleBackgroundDataList.GetRandom();
                BattleBackgroundData battleBackground = customStage.BattleBackgroundDataList[0];
                SetBattleBackground.Invoke(__instance, new object[] { battleBackground });

                Melon<Core>.Logger.Msg($"{ActiveStage.GetValue(__instance)}");
                return false;
            }
        }
    }
}