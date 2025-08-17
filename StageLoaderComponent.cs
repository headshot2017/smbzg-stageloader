using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using MelonLoader;

namespace StageLoader
{
    public class StageLoaderComponent : MonoBehaviour
    {
        public Texture2D texture;
        string currStage;
        string currStageType;

        void Start()
        {
            StartCoroutine(LoadStages());
        }

        IEnumerator LoadStages()
        {
            Application.logMessageReceived += OnLog;

            Core.messageType = 1;
            Core.customStages.Clear();

            if (!Directory.Exists($"{Application.streamingAssetsPath}/Stages"))
                Directory.CreateDirectory($"{Application.streamingAssetsPath}/Stages");

            string[] stages = Directory.GetDirectories($"{Application.streamingAssetsPath}/Stages");

            foreach (string _stageName in stages)
            {
                string stageName = _stageName.Replace('\\', '/');

                StageData data = ScriptableObject.CreateInstance<StageData>();

                data.name = Path.GetFileName(stageName);
                data.BattleBackgroundDataList = new List<BattleBackgroundData>();
                data.SkyDataList = new List<SkyBackgroundData>();
                data.AirRushExceptionIndexList = new List<int>();

                currStage = data.name;
                Melon<Core>.Logger.Msg($"Loading custom stage: {currStage}");

                Core.guiMsg = $"Loading custom stage:\n{currStage}";
                Core.guiTotalStages = $"{Core.customStages.Count}/{stages.Length} ({Core.customStages.Count / (float)stages.Length * 100f}%) stages loaded";

                // Load global stage music, if available
                UnityWebRequest www;
                AudioClip GlobalLoopMusic = null;
                AudioClip GlobalStartMusic = null;
                if (File.Exists($"{stageName}/loop.ogg"))
                {
                    Core.guiMsg = $"Loading custom stage:\n{currStage}\n\nloop.ogg";
                    www = UnityWebRequestMultimedia.GetAudioClip($"file:///{stageName}/loop.ogg", AudioType.OGGVORBIS);
                    www.SendWebRequest();
                    while (!www.isDone) ;

                    GlobalLoopMusic = DownloadHandlerAudioClip.GetContent(www);

                    if (File.Exists($"{stageName}/start.ogg"))
                    {
                        Core.guiMsg = $"Loading custom stage:\n{currStage}\n\nstart.ogg";
                        www = UnityWebRequestMultimedia.GetAudioClip($"file:///{stageName}/start.ogg", AudioType.OGGVORBIS);
                        www.SendWebRequest();
                        while (!www.isDone) ;

                        GlobalStartMusic = DownloadHandlerAudioClip.GetContent(www);
                    }
                }

                // Load stage backgrounds
                if (Directory.Exists($"{_stageName}/backgrounds"))
                {
                    string[] backgrounds = Directory.GetDirectories($"{_stageName}/backgrounds");
                    foreach (string _bgName in backgrounds)
                    {
                        string bgName = _bgName.Replace('\\', '/');
                        currStageType = $"Background \"{Path.GetFileName(bgName)}\"";

                        BattleBackgroundData bgdata = ScriptableObject.CreateInstance<BattleBackgroundData>();
                        BattleBackgroundData original = BattleCache.ins.Stage_MarioCircuit.BattleBackgroundDataList[0];

                        BackgroundDataJson json = JsonUtility.FromJson<BackgroundDataJson>(File.ReadAllText($"{bgName}/background.json"));

                        Sprite backgroundBack = null;
                        if (File.Exists($"{bgName}/backgroundback.png"))
                        {
                            Core.guiMsg = $"Loading custom stage:\n{currStage}\n\n{currStageType}\nbackgroundback.png";
                            yield return TextureDownload(Uri.EscapeUriString($"file:///{bgName}/backgroundback.png"));
                            if (!json.BackgroundBack_TextureFilter)
                                texture.filterMode = FilterMode.Point;
                            backgroundBack = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, 0), json.BackgroundBack_PixelsPerUnit);
                            backgroundBack.name = "backgroundback.png";
                        }

                        List<Sprite> backgroundAnim = new List<Sprite>();
                        for (int i = 0; File.Exists($"{bgName}/background_{i}.png"); i++)
                        {
                            Core.guiMsg = $"Loading custom stage:\n{currStage}\n\n{currStageType}\nbackground_{i}.png";
                            yield return TextureDownload(Uri.EscapeUriString($"file:///{bgName}/background_{i}.png"));
                            if (!json.Background_TextureFilter)
                                texture.filterMode = FilterMode.Point;
                            Sprite background = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, 0), json.Background_PixelsPerUnit);
                            background.name = $"background_{i}.png";
                            backgroundAnim.Add(background);
                        }

                        Sprite ground = null;
                        if (File.Exists($"{bgName}/ground.png"))
                        {
                            Core.guiMsg = $"Loading custom stage:\n{currStage}\n\n{currStageType}\nground.png";
                            yield return TextureDownload(Uri.EscapeUriString($"file:///{bgName}/ground.png"));
                            if (!json.Ground_TextureFilter)
                                texture.filterMode = FilterMode.Point;
                            ground = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, 0), json.Ground_PixelsPerUnit);
                            ground.name = "ground.png";
                        }

                        Sprite groundBlurred = ground;
                        if (File.Exists($"{bgName}/ground_blurred.png"))
                        {
                            Core.guiMsg = $"Loading custom stage:\n{currStage}\n\n{currStageType}\nground_blurred.png";
                            yield return TextureDownload(Uri.EscapeUriString($"file:///{bgName}/ground_blurred.png"));
                            if (!json.Ground_TextureFilter)
                                texture.filterMode = FilterMode.Point;
                            groundBlurred = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, 0), json.Ground_PixelsPerUnit);
                            groundBlurred.name = "ground_blurred.png";
                        }

                        bgdata.name = Path.GetFileName(bgName);
                        bgdata.SkyColor = new Color(json.SkyColor[0] / 255f, json.SkyColor[1] / 255f, json.SkyColor[2] / 255f);
                        bgdata.BackgroundBack_Sprite = backgroundBack;
                        bgdata.BackgroundBack_Position = new Vector2(json.BackgroundBack_Position[0], json.BackgroundBack_Position[1]);
                        bgdata.BackgroundBack_ParralaxSpeedX = json.BackgroundBack_ParralaxSpeedX;
                        bgdata.BackgroundBack_ParralaxSpeedY = json.BackgroundBack_ParralaxSpeedY;
                        bgdata.BackgroundSprite = (backgroundAnim.Count > 0) ? backgroundAnim[0] : null;
                        bgdata.Background_SpriteList = backgroundAnim;
                        bgdata.Background_AnimationSpeed = json.Background_AnimationSpeed;
                        bgdata.BackgroundPosition = new Vector2(json.Background_Position[0], json.Background_Position[1]);
                        bgdata.ParralaxSpeedX = json.Background_ParralaxSpeedX;
                        bgdata.ParralaxSpeedY = json.Background_ParralaxSpeedY;
                        bgdata.GroundSprite = ground;
                        bgdata.GroundSprite_Blurred = groundBlurred;
                        bgdata.GroundPosition = new Vector2(json.Ground_Position[0], json.Ground_Position[1]);
                        bgdata.MovementRushTransitionScript = original.MovementRushTransitionScript;
                        bgdata.KoopaBros_BackgroundSpriteLayer = original.KoopaBros_BackgroundSpriteLayer;

                        var CustomClosingCinematic = ScriptableObject.CreateInstance<SMBZG.MovementRush.Cinematic.MRC_Custom_Closing>();
                        CustomClosingCinematic.name = original.MovementRushClosingCinematic.name;
                        CustomClosingCinematic.hideFlags = hideFlags;
                        CustomClosingCinematic.Prefab_SimpleCharacter = ((SMBZG.MovementRush.Cinematic.MRC_Basic_Closing)original.MovementRushClosingCinematic).Prefab_SimpleCharacter;
                        bgdata.MovementRushClosingCinematic = CustomClosingCinematic;

                        if (json.Crater_Mode == 0)
                            bgdata.Prefab_Crater = null;
                        else
                        {
                            bgdata.Prefab_Crater = original.Prefab_Crater;
                            if (File.Exists($"{bgName}/crater.png"))
                            {
                                GameObject customCrater = GameObject.Instantiate(original.Prefab_Crater);
                                customCrater.SetActive(false);
                                customCrater.name = "Custom Crater";
                                GameObject.DontDestroyOnLoad(customCrater);

                                yield return TextureDownload(Uri.EscapeUriString($"file:///{bgName}/crater.png"));
                                if (!json.CustomCrater_Filter)
                                    texture.filterMode = FilterMode.Point;
                                Sprite crater = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 30);
                                crater.name = "crater.png";

                                SpriteRenderer spriteRenderer = customCrater.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
                                spriteRenderer.sprite = crater;

                                bgdata.Prefab_Crater = customCrater;
                            }
                        }


                        // Load music.
                        // Try background-specific music first.
                        // If it doesn't exist, try global stage music
                        if (File.Exists($"{bgName}/loop.ogg"))
                        {
                            Core.guiMsg = $"Loading custom stage:\n{currStage}\n\n{currStageType}\nloop.ogg";
                            www = UnityWebRequestMultimedia.GetAudioClip($"file:///{bgName}/loop.ogg", AudioType.OGGVORBIS);
                            www.SendWebRequest();
                            while (!www.isDone) ;

                            bgdata.BackgroundMusic = DownloadHandlerAudioClip.GetContent(www);

                            if (File.Exists($"{bgName}/start.ogg"))
                            {
                                Core.guiMsg = $"Loading custom stage:\n{currStage}\n\n{currStageType}\nstart.ogg";
                                www = UnityWebRequestMultimedia.GetAudioClip($"file:///{bgName}/start.ogg", AudioType.OGGVORBIS);
                                www.SendWebRequest();
                                while (!www.isDone) ;

                                bgdata.StartupBackgroundMusic = DownloadHandlerAudioClip.GetContent(www);
                            }
                        }
                        else if (GlobalLoopMusic != null)
                        {
                            bgdata.BackgroundMusic = GlobalLoopMusic;

                            if (GlobalStartMusic != null)
                            {
                                bgdata.StartupBackgroundMusic = GlobalStartMusic;
                            }
                        }

                        data.BattleBackgroundDataList.Add(bgdata);
                        if (json.Disable_AirMovementRush)
                            data.AirRushExceptionIndexList.Add(data.BattleBackgroundDataList.Count-1);
                    }
                }

                // Load skies for air movement rushes
                if (Directory.Exists($"{_stageName}/sky"))
                {
                    string[] skies = Directory.GetDirectories($"{_stageName}/sky");
                    foreach (string _skyName in skies)
                    {
                        string skyName = _skyName.Replace('\\', '/');
                        currStageType = $"Sky \"{Path.GetFileName(skyName)}\"";

                        SkyBackgroundData skydata = ScriptableObject.CreateInstance<SkyBackgroundData>();

                        SkyDataJson json = JsonUtility.FromJson<SkyDataJson>(File.ReadAllText($"{skyName}/sky.json"));

                        List<Sprite> clouds = new List<Sprite>();
                        for (int i = 0; File.Exists($"{skyName}/cloud_{i}.png"); i++)
                        {
                            Core.guiMsg = $"Loading custom stage:\n{currStage}\n\n{currStageType}\ncloud_{i}.png";
                            yield return TextureDownload(Uri.EscapeUriString($"file:///{skyName}/cloud_{i}.png"));
                            if (!json.CloudSprite_Filter)
                                texture.filterMode = FilterMode.Point;
                            Sprite cloud = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
                            cloud.name = $"cloud_{i}.png";
                            clouds.Add(cloud);
                        }

                        SkyBackgroundData original;
                        if (json.AirMovementRush_Prefab == 1)
                            original = BattleCache.ins.Stage_YoshiIsland.SkyDataList[0];
                        else
                            original = BattleCache.ins.Stage_MarioCircuit.SkyDataList[0];

                        GameObject prefab = GameObject.Instantiate(original.SkyParticleSystem_Prefab.gameObject);
                        prefab.name = $"SkyParticleSystem: \"{Path.GetFileName(skyName)}\"";
                        prefab.SetActive(false);
                        GameObject.DontDestroyOnLoad(prefab);

                        if (clouds.Count > 0)
                        {
                            ParticleSystem particles = prefab.GetComponent<ParticleSystem>();
                            int count = particles.textureSheetAnimation.spriteCount;
                            for (int i = 0; i < count; i++)
                                particles.textureSheetAnimation.RemoveSprite(0);
                            foreach (Sprite cloud in clouds)
                                particles.textureSheetAnimation.AddSprite(cloud);
                        }

                        skydata.name = Path.GetFileName(skyName);
                        skydata.SkyColor = new Color(json.SkyColor[0] / 255f, json.SkyColor[1] / 255f, json.SkyColor[2] / 255f);
                        skydata.BackgroundSprite = null;
                        skydata.SkyParticleSystem_Prefab = prefab.GetComponent<AirRushParticleSystem>();

                        data.SkyDataList.Add(skydata);
                    }
                }

                if (data.BattleBackgroundDataList.Count > 0)
                    Core.customStages.Add(data);
            }

            if (Core.messageType != 2)
                Core.messageType = 0;
            Application.logMessageReceived -= OnLog;

            if (Core.stageDropdown != null)
            {
                Core.stageDropdown.options.Clear();

                List<string> stageNames = new List<string>();
                foreach (StageData stage in Core.customStages)
                    stageNames.Add(stage.name);

                Core.stageDropdown.AddOptions(stageNames);
                Core.stageDropdown.value = 0;
            }

            List<BattleCache.StageEnum> StageList = Core.OriginalStageArray.ToList();
            for (int i = 0; i < Core.customStages.Count; i++)
                StageList.Add((BattleCache.StageEnum)(i + 100));
            BattleCache.StageArray = StageList.ToArray();


            Destroy(gameObject);
        }

        void OnLog(string message, string stacktrace, LogType type)
        {
            if (type == LogType.Exception)
            {
                Core.messageType = 2;
                Core.guiMsg = $"Error loading stage\n{currStage}\n{currStageType}\n\n{message}\n\n{stacktrace}";
            }
        }

        IEnumerator TextureDownload(string url)
        {
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();
                texture = DownloadHandlerTexture.GetContent(www);
            }
        }
    }
}
