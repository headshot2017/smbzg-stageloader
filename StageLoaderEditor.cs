using MelonLoader;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;

namespace StageLoader
{
    public static class StageLoaderEditor
    {
        public static bool isEnabled = false;
        public static bool isInGame = false;
        public static bool isCustomStage = false;
        public static bool changesMade = false;
        public static bool editingSky = false;
        public static float savedTimer = 0;

        public static string backX;
        public static string backY;
        public static string backParX;
        public static string backParY;
        public static string backPPU;
        public static bool backFilter;

        public static string bgX;
        public static string bgY;
        public static string bgParX;
        public static string bgParY;
        public static string bgAnimSpd;
        public static string bgPPU;
        public static bool bgFilter;

        public static string groundX;
        public static string groundY;
        public static string groundPPU;
        public static bool groundFilter;

        public static string skyR;
        public static string skyG;
        public static string skyB;

        public static bool craterOn;
        public static bool craterFilter;
        public static bool airMR;

        public static string airSkyR;
        public static string airSkyG;
        public static string airSkyB;
        public static bool cloudFilter;
        public static int airMRPrefab;

        public static BattleBackgroundData lastBg;
        public static SkyBackgroundData currentSky;

        public static void Reset()
        {
            BattleBackgroundManager BackgroundManager =
                (BattleBackgroundManager)typeof(BattleController).GetField("BackgroundManager", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(BattleController.instance);
            StageData ActiveStage =
                (StageData)typeof(BattleBackgroundManager).GetField("ActiveStage", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(BackgroundManager);
            BattleBackgroundData ActiveBattleBackgroundData =
                (BattleBackgroundData)typeof(BattleBackgroundManager).GetField("ActiveBattleBackgroundData", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(BackgroundManager);
            lastBg = ActiveBattleBackgroundData;

            if (!currentSky)
            {
                currentSky = (ActiveStage.SkyDataList.Count > 0) ? ActiveStage.SkyDataList[0] : null;
                ResetSky();
            }

            changesMade = false;

            backX = $"{ActiveBattleBackgroundData.BackgroundBack_Position.x}";
            backY = $"{ActiveBattleBackgroundData.BackgroundBack_Position.y}";
            backParX = $"{ActiveBattleBackgroundData.BackgroundBack_ParralaxSpeedX}";
            backParY = $"{ActiveBattleBackgroundData.BackgroundBack_ParralaxSpeedY}";
            backPPU = $"{(ActiveBattleBackgroundData.BackgroundBack_Sprite ? ActiveBattleBackgroundData.BackgroundBack_Sprite.pixelsPerUnit : 0)}";
            backFilter = ActiveBattleBackgroundData.BackgroundBack_Sprite && ActiveBattleBackgroundData.BackgroundBack_Sprite.texture.filterMode != FilterMode.Point;

            bgX = $"{ActiveBattleBackgroundData.BackgroundPosition.x}";
            bgY = $"{ActiveBattleBackgroundData.BackgroundPosition.y}";
            bgParX = $"{ActiveBattleBackgroundData.ParralaxSpeedX}";
            bgParY = $"{ActiveBattleBackgroundData.ParralaxSpeedY}";
            bgAnimSpd = $"{ActiveBattleBackgroundData.Background_AnimationSpeed}";
            bgPPU = $"{(ActiveBattleBackgroundData.BackgroundSprite ? ActiveBattleBackgroundData.BackgroundSprite.pixelsPerUnit : 0)}";
            bgFilter = ActiveBattleBackgroundData.BackgroundSprite && ActiveBattleBackgroundData.BackgroundSprite.texture.filterMode != FilterMode.Point;

            groundX = $"{ActiveBattleBackgroundData.GroundPosition.x}";
            groundY = $"{ActiveBattleBackgroundData.GroundPosition.y}";
            groundPPU = $"{(ActiveBattleBackgroundData.GroundSprite ? ActiveBattleBackgroundData.GroundSprite.pixelsPerUnit : 0)}";
            groundFilter = ActiveBattleBackgroundData.GroundSprite && ActiveBattleBackgroundData.GroundSprite.texture.filterMode != FilterMode.Point;

            skyR = $"{(int)(ActiveBattleBackgroundData.SkyColor.r * 255)}";
            skyG = $"{(int)(ActiveBattleBackgroundData.SkyColor.g * 255)}";
            skyB = $"{(int)(ActiveBattleBackgroundData.SkyColor.b * 255)}";

            craterOn = ActiveBattleBackgroundData.Prefab_Crater != null;
            craterFilter = craterOn && ActiveBattleBackgroundData.Prefab_Crater.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.texture.filterMode != FilterMode.Point;
            airMR = !ActiveStage.AirRushExceptionIndexList.Contains(ActiveStage.BattleBackgroundDataList.IndexOf(ActiveBattleBackgroundData));
        }

        public static void ResetSky()
        {
            airSkyR = $"{(int)(currentSky.SkyColor.r * 255)}";
            airSkyG = $"{(int)(currentSky.SkyColor.g * 255)}";
            airSkyB = $"{(int)(currentSky.SkyColor.b * 255)}";

            cloudFilter = currentSky.SkyParticleSystem_Prefab.gameObject.GetComponent<ParticleSystem>().textureSheetAnimation.GetSprite(0).texture.filterMode != FilterMode.Point;
            airMRPrefab = currentSky.SkyParticleSystem_Prefab.gameObject.GetComponent<CustomSkyData>().PrefabType;
        }

        public static void SkyGUI(int w, int h)
        {
            BattleBackgroundManager BackgroundManager =
                (BattleBackgroundManager)typeof(BattleController).GetField("BackgroundManager", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(BattleController.instance);
            StageData ActiveStage =
                (StageData)typeof(BattleBackgroundManager).GetField("ActiveStage", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(BackgroundManager);
            BattleBackgroundData ActiveBattleBackgroundData =
                (BattleBackgroundData)typeof(BattleBackgroundManager).GetField("ActiveBattleBackgroundData", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(BackgroundManager);

            int theX = 8;
            int theY = 64;
            int sep = 24;

            if (!currentSky)
            {
                GUI.Label(new Rect(theX, theY, w - 16, 64), "This stage has no Air Movement Rush sky backgrounds.");
                return;
            }

            string[] cloudPrefabName = ["Super Mario World", "Yoshi's Island"];
            string[] cloudPrefabDesc = [
                "Cloud sprites are small and spawn close to each other.",
                "Cloud sprites are big and spawn further away from each other."
            ];

            float outX, outY, outZ;

            GUI.Label(new Rect(theX, theY, 128, 32), "Sky color RGB");
            GUI.Label(new Rect(theX, theY + sep, 128, 32), "Red");
            airSkyR = GUI.TextField(new Rect(theX + 96 + 16, theY + sep, 128 + 8, 20), airSkyR);
            GUI.Label(new Rect(theX, theY + sep * 2, 128, 32), "Green");
            airSkyG = GUI.TextField(new Rect(theX + 96 + 16, theY + sep * 2, 128 + 8, 20), airSkyG);
            GUI.Label(new Rect(theX, theY + sep * 3, 128, 32), "Blue");
            airSkyB = GUI.TextField(new Rect(theX + 96 + 16, theY + sep * 3, 128 + 8, 20), airSkyB);
            if (float.TryParse(airSkyR, out outX) && float.TryParse(airSkyG, out outY) && float.TryParse(airSkyB, out outZ))
                currentSky.SkyColor = new Color(outX / 255f, outY / 255f, outZ / 255f);
            cloudFilter = GUI.Toggle(new Rect(theX, theY + sep * 4, 128 + 8, 32), cloudFilter, "Clouds texture filter");
            GUI.Label(new Rect(theX, theY + sep * 5, 128, 32), "Cloud particles prefab");
            if (GUI.Button(new Rect(theX + 96 + 48, theY + sep * 5, 32, 24), "<<"))
            {
                if (--airMRPrefab < 0) airMRPrefab = 1;
            }
            GUI.Button(new Rect(theX + 96 + 48 + 32 + 8, theY + sep * 5, 128, 24), cloudPrefabName[airMRPrefab]);
            if (GUI.Button(new Rect(theX + 96 + 48 + 32 + 128 + 16, theY + sep * 5, 32, 24), ">>"))
            {
                if (++airMRPrefab > 1) airMRPrefab = 0;
            }
            GUI.Label(new Rect(theX + 96 + 48, theY + sep * 5 + 24+4, 192+16, 64), cloudPrefabDesc[airMRPrefab]);

            GUI.Label(new Rect(8, theY + sep * 8, w - 16, 64), "Changes to cloud texture filter and cloud particles prefab are only applied when restarting the game, or selecting 'Refresh stage list' in the Stage select menu.");

            if (GUI.Button(new Rect(8, h - 8 - 24, 32, 24), "<<"))
            {
                int i = ActiveStage.SkyDataList.IndexOf(currentSky);
                if (i - 1 < 0) i = ActiveStage.SkyDataList.Count;
                currentSky = ActiveStage.SkyDataList[i - 1];
                ResetSky();
            }
            GUI.Button(new Rect(16 + 32, h - 8 - 24, 128, 24), currentSky.name);
            if (GUI.Button(new Rect(24 + 32 + 128, h - 8 - 24, 32, 24), ">>"))
            {
                int i = ActiveStage.SkyDataList.IndexOf(currentSky);
                if (i + 1 >= ActiveStage.SkyDataList.Count) i = -1;
                currentSky = ActiveStage.SkyDataList[i + 1];
                ResetSky();
            }


            if (GUI.Button(new Rect(w - 192 - 8, h - 8 - 24, 192, 24), savedTimer <= 0 ? "Save sky to JSON" : "Saved!"))
            {
                SkyDataJson json = new SkyDataJson();
                json.SkyColor = [int.Parse(skyR), int.Parse(skyG), int.Parse(skyB)];
                json.CloudSprite_Filter = cloudFilter;
                json.AirMovementRush_Prefab = airMRPrefab;

                File.WriteAllText($"{Application.streamingAssetsPath}/Stages/{ActiveStage.name}/sky/{currentSky.name}/sky.json", JsonUtility.ToJson(json));
                Melon<Core>.Logger.Msg($"{Application.streamingAssetsPath}/Stages/{ActiveStage.name}/sky/{currentSky.name}/sky.json");
                savedTimer = 2;
            }
        }

        public static void BackgroundGUI(int w, int h)
        {
            BattleBackgroundManager BackgroundManager =
                (BattleBackgroundManager)typeof(BattleController).GetField("BackgroundManager", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(BattleController.instance);
            StageData ActiveStage =
                (StageData)typeof(BattleBackgroundManager).GetField("ActiveStage", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(BackgroundManager);
            BattleBackgroundData ActiveBattleBackgroundData =
                (BattleBackgroundData)typeof(BattleBackgroundManager).GetField("ActiveBattleBackgroundData", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(BackgroundManager);

            if (lastBg != ActiveBattleBackgroundData)
                Reset();

            float outX, outY, outZ;

            int theX = 8;
            int theY = 64;
            int sep = 24;
            GUI.Label(new Rect(theX, theY, 128, 32), "Background Back");
            GUI.Label(new Rect(theX, theY + sep, 128, 32), "Position X / Y");
            backX = GUI.TextField(new Rect(theX + 96 + 16, theY + sep, 64, 20), backX);
            backY = GUI.TextField(new Rect(theX + 96 + 16 + 64 + 8, theY + sep, 64, 20), backY);
            if (float.TryParse(backX, out outX) && float.TryParse(backY, out outY))
                ActiveBattleBackgroundData.BackgroundBack_Position = new Vector2(outX, outY);
            GUI.Label(new Rect(theX, theY + sep * 2, 128, 32), "Parallax X / Y");
            backParX = GUI.TextField(new Rect(theX + 96 + 16, theY + sep * 2, 64, 20), backParX);
            backParY = GUI.TextField(new Rect(theX + 96 + 16 + 64 + 8, theY + sep * 2, 64, 20), backParY);
            float.TryParse(backParX, out ActiveBattleBackgroundData.BackgroundBack_ParralaxSpeedX);
            float.TryParse(backParY, out ActiveBattleBackgroundData.BackgroundBack_ParralaxSpeedY);
            GUI.Label(new Rect(theX, theY + sep * 3, 128, 32), "Pixels per unit");
            string newPPU = GUI.TextField(new Rect(theX + 96 + 16, theY + sep * 3, 128 + 8, 20), backPPU);
            if (newPPU != backPPU) changesMade = true;
            backPPU = newPPU;
            bool newBool = GUI.Toggle(new Rect(theX, theY + sep * 4, 128 + 8, 32), bgFilter, "Texture filter");
            if (bgFilter != newBool) changesMade = true;
            bgFilter = newBool;

            theX = 288;
            theY = 64;
            GUI.Label(new Rect(theX, theY, 128, 32), "Background");
            GUI.Label(new Rect(theX, theY + sep, 128, 32), "Position X / Y");
            bgX = GUI.TextField(new Rect(theX + 96 + 16, theY + sep, 64, 20), bgX);
            bgY = GUI.TextField(new Rect(theX + 96 + 16 + 64 + 8, theY + sep, 64, 20), bgY);
            if (float.TryParse(bgX, out outX) && float.TryParse(bgY, out outY))
                ActiveBattleBackgroundData.BackgroundPosition = new Vector2(outX, outY);
            GUI.Label(new Rect(theX, theY + sep * 2, 128, 32), "Parallax X / Y");
            bgParX = GUI.TextField(new Rect(theX + 96 + 16, theY + sep * 2, 64, 20), bgParX);
            bgParY = GUI.TextField(new Rect(theX + 96 + 16 + 64 + 8, theY + sep * 2, 64, 20), bgParY);
            float.TryParse(bgParX, out ActiveBattleBackgroundData.ParralaxSpeedX);
            float.TryParse(bgParY, out ActiveBattleBackgroundData.ParralaxSpeedY);
            GUI.Label(new Rect(theX, theY + sep * 3, 128, 32), "Animation speed");
            bgAnimSpd = GUI.TextField(new Rect(theX + 96 + 16, theY + sep * 3, 128 + 8, 20), bgAnimSpd);
            float.TryParse(bgAnimSpd, out ActiveBattleBackgroundData.Background_AnimationSpeed);
            GUI.Label(new Rect(theX, theY + sep * 4, 128, 32), "Pixels per unit");
            newPPU = GUI.TextField(new Rect(theX + 96 + 16, theY + sep * 4, 128 + 8, 20), bgPPU);
            if (newPPU != bgPPU) changesMade = true;
            bgPPU = newPPU;
            newBool = GUI.Toggle(new Rect(theX, theY + sep * 5, 128 + 8, 32), backFilter, "Texture filter");
            if (backFilter != newBool) changesMade = true;
            backFilter = newBool;

            theX = 8;
            theY = 64 + 128 + 32;
            GUI.Label(new Rect(theX, theY, 128, 32), "Ground");
            GUI.Label(new Rect(theX, theY + sep, 128, 32), "Position X / Y");
            groundX = GUI.TextField(new Rect(theX + 96 + 16, theY + sep, 64, 20), groundX);
            groundY = GUI.TextField(new Rect(theX + 96 + 16 + 64 + 8, theY + sep, 64, 20), groundY);
            if (float.TryParse(groundX, out outX) && float.TryParse(groundY, out outY))
                ActiveBattleBackgroundData.GroundPosition = new Vector2(outX, outY);
            GUI.Label(new Rect(theX, theY + sep * 2, 128, 32), "Pixels per unit");
            newPPU = GUI.TextField(new Rect(theX + 96 + 16, theY + sep * 2, 128 + 8, 20), groundPPU);
            if (newPPU != groundPPU) changesMade = true;
            groundPPU = newPPU;
            newBool = GUI.Toggle(new Rect(theX, theY + sep * 3, 128 + 8, 32), groundFilter, "Texture filter");
            if (groundFilter != newBool) changesMade = true;
            groundFilter = newBool;

            theX = 288;
            theY = 64 + 128 + 32;
            GUI.Label(new Rect(theX, theY, 128, 32), "Sky color RGB");
            GUI.Label(new Rect(theX, theY + sep, 128, 32), "Red");
            skyR = GUI.TextField(new Rect(theX + 96 + 16, theY + sep, 128 + 8, 20), skyR);
            GUI.Label(new Rect(theX, theY + sep * 2, 128, 32), "Green");
            skyG = GUI.TextField(new Rect(theX + 96 + 16, theY + sep * 2, 128 + 8, 20), skyG);
            GUI.Label(new Rect(theX, theY + sep * 3, 128, 32), "Blue");
            skyB = GUI.TextField(new Rect(theX + 96 + 16, theY + sep * 3, 128 + 8, 20), skyB);
            if (float.TryParse(skyR, out outX) && float.TryParse(skyG, out outY) && float.TryParse(skyB, out outZ))
                ActiveBattleBackgroundData.SkyColor = Camera.main.backgroundColor = new Color(outX / 255f, outY / 255f, outZ / 255f);

            theX = 8;
            theY = 64 + 128 + 144;
            craterOn = GUI.Toggle(new Rect(theX, theY, 128, 32), craterOn, "Enable crater");
            newBool = GUI.Toggle(new Rect(theX + 144 + 8, theY, 256-48, 32), craterFilter, "Crater texture filtering");
            if (craterFilter != newBool) changesMade = true;
            craterFilter = newBool;
            airMR = GUI.Toggle(new Rect(theX + 352 + 16, theY, 288, 32), airMR, "Allow Aerial Movement Rush");


            theY = 64 + 128 + 144 + 24;
            if (changesMade)
                GUI.Label(new Rect(8, theY, w - 16, 64), "Click 'Apply & reload' to see changes to Pixels per unit and texture filtering.");


            if (GUI.Button(new Rect(8, h - 8 - 24, 32, 24), "<<"))
            {
                int i = ActiveStage.BattleBackgroundDataList.IndexOf(ActiveBattleBackgroundData);
                if (i - 1 < 0) i = ActiveStage.BattleBackgroundDataList.Count;
                typeof(BattleBackgroundManager).GetMethod("TransitionBattleBackground", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(BattleBackgroundData), typeof(float) }, null).Invoke(BackgroundManager, new object[] { ActiveStage.BattleBackgroundDataList[i - 1], 1f });
            }
            GUI.Button(new Rect(16 + 32, h - 8 - 24, 128, 24), ActiveBattleBackgroundData.name);
            if (GUI.Button(new Rect(24 + 32 + 128, h - 8 - 24, 32, 24), ">>"))
            {
                int i = ActiveStage.BattleBackgroundDataList.IndexOf(ActiveBattleBackgroundData);
                if (i + 1 >= ActiveStage.BattleBackgroundDataList.Count) i = -1;
                typeof(BattleBackgroundManager).GetMethod("TransitionBattleBackground", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(BattleBackgroundData), typeof(float) }, null).Invoke(BackgroundManager, new object[] { ActiveStage.BattleBackgroundDataList[i + 1], 1f });
            }


            if (GUI.Button(new Rect(w - 192 - 104 - 16, h - 8 - 24, 104, 24), "Apply & reload"))
            {
                if (ActiveBattleBackgroundData.BackgroundBack_Sprite)
                {
                    Texture2D texture = ActiveBattleBackgroundData.BackgroundBack_Sprite.texture;
                    texture.filterMode = backFilter ? FilterMode.Bilinear : FilterMode.Point;
                    if (float.TryParse(backPPU, out outX))
                        ActiveBattleBackgroundData.BackgroundBack_Sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, 0), outX);
                }
                for (int i = 0; i < ActiveBattleBackgroundData.Background_SpriteList.Count; i++)
                {
                    Texture2D texture = ActiveBattleBackgroundData.Background_SpriteList[i].texture;
                    texture.filterMode = bgFilter ? FilterMode.Bilinear : FilterMode.Point;
                    if (float.TryParse(bgPPU, out outX))
                        ActiveBattleBackgroundData.Background_SpriteList[i] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, 0), outX);
                }
                if (ActiveBattleBackgroundData.BackgroundSprite)
                {
                    Texture2D texture = ActiveBattleBackgroundData.BackgroundSprite.texture;
                    texture.filterMode = bgFilter ? FilterMode.Bilinear : FilterMode.Point;
                    if (float.TryParse(bgPPU, out outX))
                        ActiveBattleBackgroundData.BackgroundSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, 0), outX);
                }
                if (ActiveBattleBackgroundData.GroundSprite)
                {
                    Texture2D texture = ActiveBattleBackgroundData.GroundSprite.texture;
                    texture.filterMode = groundFilter ? FilterMode.Bilinear : FilterMode.Point;
                    if (float.TryParse(groundPPU, out outX))
                        ActiveBattleBackgroundData.GroundSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, 0), outX);
                }
                if (ActiveBattleBackgroundData.GroundSprite_Blurred)
                {
                    Texture2D texture = ActiveBattleBackgroundData.GroundSprite_Blurred.texture;
                    texture.filterMode = groundFilter ? FilterMode.Bilinear : FilterMode.Point;
                    if (float.TryParse(groundPPU, out outX))
                        ActiveBattleBackgroundData.GroundSprite_Blurred = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, 0), outX);
                }
                if (ActiveBattleBackgroundData.Prefab_Crater)
                {
                    ActiveBattleBackgroundData.Prefab_Crater.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.texture.filterMode =
                        craterFilter ? FilterMode.Bilinear : FilterMode.Point;
                }

                typeof(BattleBackgroundManager).GetMethod("TransitionBattleBackground", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(BattleBackgroundData), typeof(float) }, null).Invoke(BackgroundManager, new object[] { ActiveBattleBackgroundData, 1f });
                changesMade = false;
            }
            if (GUI.Button(new Rect(w - 192 - 8, h - 8 - 24, 192, 24), savedTimer <= 0 ? "Save background to JSON" : "Saved!"))
            {
                BackgroundDataJson json = new BackgroundDataJson();
                json.SkyColor = [int.Parse(skyR), int.Parse(skyG), int.Parse(skyB)];
                json.BackgroundBack_Position = [ActiveBattleBackgroundData.BackgroundBack_Position.x, ActiveBattleBackgroundData.BackgroundBack_Position.y];
                json.BackgroundBack_ParralaxSpeedX = ActiveBattleBackgroundData.BackgroundBack_ParralaxSpeedX;
                json.BackgroundBack_ParralaxSpeedY = ActiveBattleBackgroundData.BackgroundBack_ParralaxSpeedY;
                json.BackgroundBack_TextureFilter = backFilter;
                json.BackgroundBack_PixelsPerUnit = float.Parse(backPPU);
                json.Background_AnimationSpeed = ActiveBattleBackgroundData.Background_AnimationSpeed;
                json.Background_Position = [ActiveBattleBackgroundData.BackgroundPosition.x, ActiveBattleBackgroundData.BackgroundPosition.y];
                json.Background_ParralaxSpeedX = ActiveBattleBackgroundData.ParralaxSpeedX;
                json.Background_ParralaxSpeedY = ActiveBattleBackgroundData.ParralaxSpeedY;
                json.Background_TextureFilter = bgFilter;
                json.Background_PixelsPerUnit = float.Parse(bgPPU);
                json.Ground_Position = [ActiveBattleBackgroundData.GroundPosition.x, ActiveBattleBackgroundData.GroundPosition.y];
                json.Ground_TextureFilter = groundFilter;
                json.Ground_PixelsPerUnit = float.Parse(groundPPU);
                json.Crater_Mode = craterOn ? 1 : 0;
                json.CustomCrater_Filter = craterFilter;
                json.Disable_AirMovementRush = !airMR;

                File.WriteAllText($"{Application.streamingAssetsPath}/Stages/{ActiveStage.name}/backgrounds/{ActiveBattleBackgroundData.name}/background.json", JsonUtility.ToJson(json));
                Melon<Core>.Logger.Msg($"{Application.streamingAssetsPath}/Stages/{ActiveStage.name}/backgrounds/{ActiveBattleBackgroundData.name}/background.json");
                savedTimer = 2;
            }
        }

        public static void OnGUI()
        {
            if (!isEnabled || !isInGame || !isCustomStage) return;

            int w = 288 * 2;
            int h = 224 * 2;
            GUI.BeginGroup(new Rect(Screen.width / 2 - w / 2, 32, w, h));
            GUI.Box(new Rect(0, 0, w, h), "StageLoader Editor");
            if (GUI.Button(new Rect(w - 24 - 8, 8, 24, 24), "X"))
            {
                isEnabled = false;
                return;
            }

            if (GUI.Button(new Rect(32, 32, w/2-64, 24), "Backgrounds"))
            {
                editingSky = false;
            }

            if (GUI.Button(new Rect(w/2+32, 32, w/2-64, 24), "Movement Rush skies"))
            {
                editingSky = true;
            }

            if (editingSky)
                SkyGUI(w, h);
            else
                BackgroundGUI(w, h);

            GUI.EndGroup();
        }

        public static void OnUpdate()
        {
            if (savedTimer > 0)
                savedTimer -= Time.deltaTime;
        }
    }
}
