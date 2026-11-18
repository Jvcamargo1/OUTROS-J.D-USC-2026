#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FlappyCanarinho.EditorTools
{
    /// <summary>
    /// Monta as cenas Menu e Game, o prefab dos canos e toda a UI. Menu: Flappy Canarinho > Construir cenas.
    /// </summary>
    public static class SceneBuilder
    {
        const string Kit = "Assets/Jovial Games/Basic 2D Side Scroller Art Kit";
        const string Root = "Assets/FlappyCanarinho";
        const string TemplateScene = "Assets/Scenes/SampleScene.unity";
        const string MenuPath = Root + "/Scenes/Menu.unity";
        const string GamePath = Root + "/Scenes/Game.unity";
        const string PipePrefabPath = Root + "/Prefabs/PipePair.prefab";
        const string FontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        const float TileW = 19.2f;

        static readonly Color Yellow = new Color(1f, 0.82f, 0.15f);
        static readonly Color DarkBrown = new Color(0.25f, 0.13f, 0.02f);

        static TMP_FontAsset font;
        static Material outlineMat;

        [MenuItem("Flappy Canarinho/Construir cenas")]
        public static void BuildAll()
        {
            EnsureFolder(Root);
            EnsureFolder(Root + "/Scenes");
            EnsureFolder(Root + "/Prefabs");
            EnsureFolder(Root + "/Materials");

            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            outlineMat = CreateOutlineMaterial();

            var pipePrefab = BuildPipePrefab();

            BuildGameScene(pipePrefab);
            BuildMenuScene();

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MenuPath, true),
                new EditorBuildSettingsScene(GamePath, true),
            };

            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(MenuPath, OpenSceneMode.Single);
            Debug.Log("[Flappy Canarinho] Cenas Menu e Game construídas e adicionadas ao Build Settings.");
        }

        // ---------------------------------------------------------------- cenas

        static void BuildGameScene(GameObject pipePrefab)
        {
            var scene = OpenCopyOfTemplate(GamePath);
            ConfigureCamera();

            var world = new GameObject("World").transform;
            BuildBackground(world, menuMode: false);

            // Chão sólido (mata o pássaro ao encostar)
            var groundCol = new GameObject("GroundCollider");
            groundCol.transform.SetParent(world, false);
            groundCol.transform.position = new Vector3(0f, -3.9f, 0f);
            var box = groundCol.AddComponent<BoxCollider2D>();
            box.size = new Vector2(60f, 1f);

            // Pássaro
            var bird = new GameObject("Bird");
            bird.transform.position = new Vector3(-3f, 1.2f, 0f);
            var rb = bird.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var circle = bird.AddComponent<CircleCollider2D>();
            circle.radius = 0.32f;

            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Kit + "/Prefabs/Player.prefab");
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            visual.name = "Visual";
            visual.transform.SetParent(bird.transform, false);
            visual.transform.localScale = Vector3.one * 0.4f;
            visual.GetComponent<SpriteRenderer>().sortingOrder = 5;

            var birdCtrl = bird.AddComponent<BirdController>();
            birdCtrl.visual = visual.transform;

            // Spawner de canos (mapa infinito)
            var spawnerGo = new GameObject("PipeSpawner");
            var spawner = spawnerGo.AddComponent<PipeSpawner>();
            spawner.pipePairPrefab = pipePrefab;
            spawner.pipeSprites = new[] { LoadSprite("Obstacle"), LoadSprite("Obstacle_2") };

            // UI
            var canvas = MakeCanvas();

            var scoreText = MakeText(canvas.transform, "Score", "0", 150, new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(600f, 200f), Color.white, true);

            var ready = MakeEmpty(canvas.transform, "ReadyPanel");
            Stretch(ready);
            MakeText(ready.transform, "ReadyText", "Pressione ESPAÇO", 90, new Vector2(0.5f, 0.5f), new Vector2(0f, -150f), new Vector2(1400f, 160f), Color.white, true);

            var over = MakeEmpty(canvas.transform, "GameOverPanel");
            Stretch(over);
            var dim = over.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.55f);
            MakeText(over.transform, "GameOverTitle", "GAME OVER", 180, new Vector2(0.5f, 0.5f), new Vector2(0f, 250f), new Vector2(1500f, 240f), new Color(1f, 0.38f, 0.28f), true);
            var finalScore = MakeText(over.transform, "FinalScore", "Pontos: 0", 100, new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(1000f, 140f), Color.white, true);
            var bestScore = MakeText(over.transform, "BestScore", "Melhor: 0", 70, new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(1000f, 110f), Yellow, true);
            var restartBtn = MakeButton(over.transform, "RestartButton", "Reiniciar", new Vector2(0.5f, 0.5f), new Vector2(-270f, -200f), new Vector2(460f, 130f));
            var menuBtn = MakeButton(over.transform, "MenuButton", "Menu", new Vector2(0.5f, 0.5f), new Vector2(270f, -200f), new Vector2(460f, 130f));

            var gmGo = new GameObject("GameManager");
            var gm = gmGo.AddComponent<GameManager>();
            gm.pipeSpeed = 4f;
            gm.scoreText = scoreText;
            gm.readyPanel = ready;
            gm.gameOverPanel = over;
            gm.finalScoreText = finalScore;
            gm.bestScoreText = bestScore;

            UnityEventTools.AddPersistentListener(restartBtn.onClick, gm.Restart);
            UnityEventTools.AddPersistentListener(menuBtn.onClick, gm.BackToMenu);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        static void BuildMenuScene()
        {
            var scene = OpenCopyOfTemplate(MenuPath);
            ConfigureCamera();

            var world = new GameObject("World").transform;
            BuildBackground(world, menuMode: true);

            // Canarinho decorativo (só a animação de voo)
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Kit + "/Prefabs/Player.prefab");
            var bird = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            bird.name = "CanarinhoMenu";
            bird.transform.position = new Vector3(0f, 4.15f, 0f);
            bird.transform.localScale = Vector3.one * 0.5f;
            bird.GetComponent<SpriteRenderer>().sortingOrder = 5;

            var canvas = MakeCanvas();

            MakeText(canvas.transform, "Title", "Flappy Canarinho", 170, new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(1700f, 240f), Yellow, true);
            var playBtn = MakeButton(canvas.transform, "PlayButton", "Iniciar", new Vector2(0.5f, 0.5f), new Vector2(0f, -50f), new Vector2(480f, 130f));
            var creditsBtn = MakeButton(canvas.transform, "CreditsButton", "Créditos", new Vector2(0.5f, 0.5f), new Vector2(0f, -200f), new Vector2(480f, 130f));

            // Painel de créditos
            var panel = MakeEmpty(canvas.transform, "CreditsPanel");
            Stretch(panel);
            var dim = panel.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.8f);

            MakeText(panel.transform, "CreditsTitle", "Créditos", 130, new Vector2(0.5f, 0.5f), new Vector2(0f, 330f), new Vector2(1200f, 180f), Yellow, true);
            var body = MakeText(panel.transform, "CreditsBody",
                "Desenvolvimento\nJoão Vitor Camargo\n\nArte\nBasic 2D Side Scroller Art Kit - Jovial Games\n\nFeito com Unity",
                62, new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(1500f, 600f), Color.white, false);
            body.textWrappingMode = TextWrappingModes.Normal;
            var backBtn = MakeButton(panel.transform, "BackButton", "Voltar", new Vector2(0.5f, 0.5f), new Vector2(0f, -400f), new Vector2(420f, 120f));

            var menuGo = new GameObject("MenuController");
            var menu = menuGo.AddComponent<MenuController>();
            menu.creditsPanel = panel;

            UnityEventTools.AddPersistentListener(playBtn.onClick, menu.StartGame);
            UnityEventTools.AddPersistentListener(creditsBtn.onClick, menu.OpenCredits);
            UnityEventTools.AddPersistentListener(backBtn.onClick, menu.CloseCredits);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        static UnityEngine.SceneManagement.Scene OpenCopyOfTemplate(string targetPath)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(targetPath) != null) AssetDatabase.DeleteAsset(targetPath);
            AssetDatabase.CopyAsset(TemplateScene, targetPath);
            return EditorSceneManager.OpenScene(targetPath, OpenSceneMode.Single);
        }

        static void ConfigureCamera()
        {
            var cam = Camera.main;
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.transform.position = new Vector3(0f, 0.4f, -10f);
            cam.backgroundColor = new Color(0.69f, 0.9f, 0.95f);
        }

        // ---------------------------------------------------------------- fundo infinito

        static void BuildBackground(Transform world, bool menuMode)
        {
            // Céu fixo
            AddSprite(world, "Sky_Background", new Vector2(0f, 0f), -10);

            // Camadas com parallax (posições e ordens copiadas da Demo_Scene do kit)
            SimpleLayer(world, "Clouds_1", "Clouds_1", 0f, 0.25f, -9, menuMode);
            SimpleLayer(world, "Clouds_2", "Clouds_2", -2.31f, 0.5f, -8, menuMode);
            SimpleLayer(world, "Mountains_1", "Moutains_1", -1.83f, 0.8f, -7, menuMode);
            SimpleLayer(world, "Mountains_2", "Mountain_2", -3.46f, 1.2f, -6, menuMode);

            MakeLayer(world, "Bushes", 2.2f, false, menuMode, tile =>
            {
                AddSprite(tile, "Bush_1", new Vector2(-7.28f, -3.05f), -4);
                AddSprite(tile, "Bush_2", new Vector2(0f, -3.17f), -4);
                AddSprite(tile, "Bush_3", new Vector2(6.43f, -3.17f), -4);
            });

            // Chão: acompanha a velocidade dos canos
            MakeLayer(world, "Ground", 4f, true, menuMode, tile =>
                AddSprite(tile, "Ground", new Vector2(0f, -4.37f), 1));
        }

        static void SimpleLayer(Transform parent, string name, string spriteName, float y, float speed, int order, bool menuMode)
        {
            MakeLayer(parent, name, speed, false, menuMode, tile => AddSprite(tile, spriteName, new Vector2(0f, y), order));
        }

        static void MakeLayer(Transform parent, string name, float speed, bool worldSpeed, bool alwaysScroll, Action<Transform> fillTile)
        {
            var layer = new GameObject(name);
            layer.transform.SetParent(parent, false);

            var sb = layer.AddComponent<ScrollingBackground>();
            sb.speed = speed;
            sb.useWorldSpeed = worldSpeed;
            sb.alwaysScroll = alwaysScroll;
            sb.tileWidth = TileW;

            for (int i = 0; i < 3; i++)
            {
                var tile = new GameObject("Tile" + i).transform;
                tile.SetParent(layer.transform, false);
                tile.localPosition = new Vector3(i * TileW, 0f, 0f);
                fillTile(tile);
            }
        }

        static void AddSprite(Transform parent, string spriteName, Vector2 pos, int order)
        {
            var go = new GameObject(spriteName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(spriteName);
            sr.sortingOrder = order;
        }

        static Sprite LoadSprite(string spriteName)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(Kit + "/Sprites/Background/" + spriteName + ".png");
        }

        // ---------------------------------------------------------------- prefab dos canos

        static GameObject BuildPipePrefab()
        {
            var root = new GameObject("PipePair");
            root.AddComponent<Pipe>();
            var rb = root.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            var sprite = LoadSprite("Obstacle");
            MakePipePart(root.transform, "Top", sprite, true);
            MakePipePart(root.transform, "Bottom", sprite, false);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PipePrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        static void MakePipePart(Transform parent, string name, Sprite sprite, bool flipY)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.flipY = flipY;
            sr.sortingOrder = 0;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.3f, 5.5f);
        }

        // ---------------------------------------------------------------- UI

        static Canvas MakeCanvas()
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            return canvas;
        }

        static GameObject MakeEmpty(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static void Stretch(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static void SetRect(GameObject go, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        static TMP_Text MakeText(Transform parent, string name, string text, float size, Vector2 anchor, Vector2 pos, Vector2 box, Color color, bool outline)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            SetRect(go, anchor, pos, box);

            var t = go.GetComponent<TextMeshProUGUI>();
            t.font = font;
            if (outline) t.fontSharedMaterial = outlineMat;
            t.text = text;
            t.fontSize = size;
            t.fontStyle = FontStyles.Bold;
            t.alignment = TextAlignmentOptions.Center;
            t.color = color;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.raycastTarget = false;
            return t;
        }

        static Button MakeButton(Transform parent, string name, string label, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            SetRect(go, anchor, pos, size);

            var img = go.GetComponent<Image>();
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;
            img.color = Color.white;

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.normalColor = new Color(1f, 0.78f, 0.1f);
            colors.highlightedColor = new Color(1f, 0.9f, 0.4f);
            colors.pressedColor = new Color(0.82f, 0.58f, 0.05f);
            colors.selectedColor = colors.normalColor;
            btn.colors = colors;

            var text = MakeText(go.transform, "Label", label, 70, new Vector2(0.5f, 0.5f), Vector2.zero, size, DarkBrown, false);
            Stretch(text.gameObject);
            return btn;
        }

        static Material CreateOutlineMaterial()
        {
            const string path = Root + "/Materials/Texto Contorno.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) AssetDatabase.DeleteAsset(path);

            var mat = new Material(font.material) { name = "Texto Contorno" };
            mat.EnableKeyword(ShaderUtilities.Keyword_Outline);
            mat.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.28f);
            mat.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0.2f, 0.1f, 0.02f, 1f));
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
