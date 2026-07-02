using HarmonyLib;
using MelonLoader;
using SMBZG.CharacterSelect;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(StageLoader.Core), "StageLoader", "1.6.0", "Headshotnoby/headshot2017", null)]
[assembly: MelonGame("Jonathan Miller aka Zethros", "SMBZ-G")]

namespace StageLoader
{
    public class Core : MelonMod
    {
        public static List<StageDataExt> customStages;
        public static Dropdown stageDropdown;

        public static GameObject StageLoaderObj = null;

        public static BattleCache.StageEnum[] OriginalStageArray;

        public override void OnInitializeMelon()
        {
            customStages = new List<StageDataExt>();
            stageDropdown = null;
        }

        public override void OnLateInitializeMelon()
        {
            OriginalStageArray = BattleCache.StageArray;
            LoadCustomStages();
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F12))
                StageLoaderEditor.isEnabled ^= true;
            StageLoaderEditor.OnUpdate();
        }

        public override void OnGUI()
        {
            StageLoaderEditor.OnGUI();
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            StageLoaderEditor.isInGame = buildIndex == 4;
            if (StageLoaderEditor.isInGame) StageLoaderEditor.Reset();

            if (buildIndex == 7)
                LoadCustomStageUI();
        }

        public void LoadCustomStages()
        {
            if (StageLoaderObj) return;

            StageLoaderObj = new GameObject("StageLoader");
            GameObject.DontDestroyOnLoad(StageLoaderObj);
            StageLoaderComponent dl = StageLoaderObj.AddComponent<StageLoaderComponent>();
        }

        public void LoadCustomStageUI()
        {
            GameObject root = CharacterSelectScript.ins.Section_StageSelect;

            Transform Text_SelectStage = root.transform.Find("Text_SelectStage");
            GameObject StageGrid = root.transform.Find("StageGrid").gameObject;

            // move Canvas/StageSelectPage/Text_SelectStage transform.y += 50
            Text_SelectStage.transform.position += new Vector3(0, 50);

            // instantiate Scroll View gameobject from CharacterSelectPage
            // if any, remove all children from Scroll View(Clone)/ Viewport / Content
            // (need to use DestroyImmediate instead of Destroy, otherwise an exception occurs)
            GameObject ScrollView = GameObject.Instantiate(CharacterSelectScript.ins.Section_CharacterSelect.transform.Find("Scroll View").gameObject);
            GameObject Viewport = ScrollView.transform.Find("Viewport").gameObject;
            GameObject Content = Viewport.transform.Find("Content").gameObject;
            while (Content.transform.childCount > 0)
                GameObject.DestroyImmediate(Content.transform.GetChild(0).gameObject);

            // set parent of Scroll View(Clone) to StageSelectPage
            ScrollView.transform.SetParent(root.transform);
            ScrollView.name = "Scroll View";

            // set parents of StageGrid's children to Scroll View(Clone)/Viewport/Content
            while (StageGrid.transform.childCount > 0)
                StageGrid.transform.GetChild(0).SetParent(Content.transform);

            GameObject.Destroy(StageGrid.gameObject);

            // set local position to 0,0,0 of: Scroll View(Clone), Viewport, Content
            ScrollView.transform.localScale = Vector3.one;
            ScrollView.transform.localPosition = Vector3.zero;
            Viewport.transform.localPosition = Vector3.zero;
            Content.transform.localPosition = Vector3.zero;

            // set Scroll View(Clone) RectTransform.sizeDelta to 0,-300
            ScrollView.GetComponent<RectTransform>().sizeDelta = new Vector2(0, -300);
            ScrollView.GetComponent<ScrollRect>().inertia = false;
            ScrollView.GetComponent<ScrollRect>().movementType = ScrollRect.MovementType.Clamped;
            ScrollView.GetComponent<ScrollRect>().scrollSensitivity = 50;

            // set Scroll View(Clone)/ Viewport / Content GridLayoutGroup.cellSize to 230,170(or 208.3333 149.4) and GridLayoutGroup.constraintCount to 6(FixedColumnCount)
            GridLayoutGroup grid = Content.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(208.3333f, 149.4f);
            grid.spacing = new Vector2(15, 20);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 6;
            grid.padding.top = 3;
            grid.padding.bottom = 3;


            // add all the stages!
            for (int i=0; i<customStages.Count; i++)
            {
                StageDataExt stage = customStages[i];
                GameObject stageClone = GameObject.Instantiate(Content.transform.GetChild(1).gameObject, Content.transform);
                StageSelector stageSelectorComp = stageClone.GetComponent<StageSelector>();
                Text stageText = stageClone.transform.Find("Image").GetComponentInChildren<Text>();
                Image stageImage = stageClone.transform.Find("Mask").Find("Image").GetComponent<Image>();

                stageClone.name = stage.name;
                stageSelectorComp.ChooseRandomStage = false;
                stageSelectorComp.FixedBattleBackgroundSelection = true;
                stageSelectorComp.BattleBackgroundIndex = 0;
                stageSelectorComp.Stage = (BattleCache.StageEnum)(i + 100);
                stageText.text = stage.name;
                stageText.resizeTextForBestFit = true;
                stageText.resizeTextMaxSize = stageText.fontSize;

                if (stage.thumbnail)
                {
                    stageImage.sprite = stage.thumbnail;
                    stageImage.transform.localPosition = Vector3.zero;
                }
                else
                    stageImage.enabled = false;

                CharacterSelectScript.ins.StageList.Add(stageSelectorComp);
            }

            foreach (Transform stage in Content.transform)
            {
                stage.localScale = Vector3.one;
            }
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

        [HarmonyPatch(typeof(CharacterSelectPlayerInputHandler), "HoverStage", new Type[] { typeof(StageSelector) })]
        private static class HoverStagePatch
        {
            private static void Postfix(CharacterSelectPlayerInputHandler __instance, StageSelector stage)
            {
                // only P1 controls scrolling
                int ind = (int)__instance.GetType().BaseType.GetField("InputPlayerIndex", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
                if (ind != 0) return;

                // StageSelector -> Content -> Viewport -> Scroll View
                ScrollRect scrollRect = stage.transform.parent.parent.parent.GetComponent<ScrollRect>();
                Transform target = stage.transform;

                Canvas.ForceUpdateCanvases();
                Vector2 anchoredPosition = scrollRect.content.anchoredPosition;
                Vector2 anchoredPosition2 = (Vector2)scrollRect.transform.InverseTransformPoint(scrollRect.content.position) - (Vector2)scrollRect.transform.InverseTransformPoint(target.position);
                anchoredPosition2 += -(new Vector2(scrollRect.content.rect.width, scrollRect.content.rect.height) * 0.2f);
                if (!scrollRect.horizontal)
                {
                    anchoredPosition2.x = anchoredPosition.x;
                }

                if (!scrollRect.vertical)
                {
                    anchoredPosition2.y = anchoredPosition.y;
                }

                scrollRect.content.anchoredPosition = anchoredPosition2;
            }
        }

        [HarmonyPatch(typeof(BattleBackgroundManager), "SetSkyBackground", new Type[] { typeof(SkyBackgroundData) })]
        private static class SetSkyBackgroundPatch
        {
            private static bool Prefix(BattleBackgroundManager __instance, SkyBackgroundData skyBackground)
            {
                foreach (StageDataExt stage in customStages)
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
                SMBZG.BattleSettings BattleSettings = (SMBZG.BattleSettings)typeof(GC).GetField("BattleSetting", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(GC.ins);
                BattleCache.StageEnum? SelectedStage = BattleSettings.Stage;

                StageLoaderEditor.isCustomStage = !SelectedStage.HasValue || (int)SelectedStage >= 100;
                return true;
            }
        }
    }
}