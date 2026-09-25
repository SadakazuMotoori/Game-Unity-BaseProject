using SGGames.Game.Sys;
using TMPro;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.UI;

namespace SGGames.Game.Develop.Editor
{
    public static class DebugSystemCheckAssets
    {
        const string kPrefabPath = "Assets/SGGame/Scenes/DebugScenes/DebugSystemCheckPopup.prefab";
        const string kGroupName = "Debug System Checks";
        const string kFontPath = "Assets/Plugins/TextMesh Pro/Resources/Fonts & Materials/NotoSansJP-Bold SDF.asset";

        [InitializeOnLoadMethod]
        static void Initialize()
        {
            EditorApplication.delayCall += Ensure;
        }

        [MenuItem("SGGame/Debug/Prepare System Checks")]
        public static void Ensure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(kPrefabPath) == null)
            {
                CreatePopupPrefab();
            }

            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            string guid = AssetDatabase.AssetPathToGUID(kPrefabPath);
            if (settings.FindAssetEntry(guid) != null)
            {
                return;
            }

            var group = settings.FindGroup(kGroupName);
            if (group == null)
            {
                group = settings.CreateGroup(kGroupName, false, false, true, null,
                    typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            }

            var entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = DebugSystemCheckPopup.AssetAddress;
            AssetDatabase.SaveAssets();
        }

        static void CreatePopupPrefab()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(kFontPath);
            if (font == null)
            {
                throw new System.InvalidOperationException("System Check Popup用の日本語フォントが見つかりません。");
            }

            var root = new GameObject("DebugSystemCheckPopup", typeof(RectTransform));
            root.SetActive(false);
            root.layer = 5;
            try
            {
                var canvas = root.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 10;
                var scaler = root.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280, 720);
                scaler.matchWidthOrHeight = 0.5f;
                root.AddComponent<GraphicRaycaster>();
                var popup = root.AddComponent<DebugSystemCheckPopup>();

                var panel = CreateRect("Panel", root.transform, new Vector2(600, 260), Vector2.zero);
                panel.anchorMin = new Vector2(0.70f, 0.5f);
                panel.anchorMax = new Vector2(0.70f, 0.5f);
                panel.gameObject.AddComponent<Image>().color = new Color(0.08f, 0.10f, 0.14f, 1);
                var group = panel.gameObject.AddComponent<UISelectableGroup>();
                CreateText("Title", panel, font, "Popup 開閉確認", new Vector2(500, 50), new Vector2(0, 80), 30);
                CreateText("Description", panel, font, "Space / ゲームパッドの決定 / クリックで閉じる", new Vector2(550, 60), new Vector2(0, 15), 21);

                var button = CreateRect("Close", panel, new Vector2(260, 55), new Vector2(0, -75));
                var buttonImage = button.gameObject.AddComponent<Image>();
                buttonImage.color = Color.white;
                var selectable = button.gameObject.AddComponent<UISelectable>();
                selectable.targetGraphic = buttonImage;
                selectable.IDInt = 1;
                selectable.IDString = "close";
                var colors = selectable.colors;
                colors.normalColor = new Color(0.65f, 0.70f, 0.80f, 1);
                colors.selectedColor = new Color(1, 0.85f, 0.25f, 1);
                selectable.colors = colors;
                var navigation = selectable.navigation;
                navigation.mode = Navigation.Mode.None;
                selectable.navigation = navigation;
                var label = CreateText("Label", button, font, "閉じる", new Vector2(250, 50), Vector2.zero, 25);
                label.color = Color.black;

                var selectableSettings = new SerializedObject(selectable);
                selectableSettings.FindProperty("_uiText").objectReferenceValue = label;
                selectableSettings.FindProperty("_onDecideEvent.m_PersistentCalls.m_Calls").arraySize = 0;
                selectableSettings.ApplyModifiedPropertiesWithoutUndo();

                var groupSettings = new SerializedObject(group);
                groupSettings.FindProperty("_firstSelected").objectReferenceValue = selectable;
                groupSettings.FindProperty("_cursorObjectData._cursorTrans").objectReferenceValue = null;
                groupSettings.FindProperty("_cursorObjectData._duration").floatValue = 0;
                groupSettings.ApplyModifiedPropertiesWithoutUndo();

                var popupSettings = new SerializedObject(popup);
                popupSettings.FindProperty("_topUITransform").objectReferenceValue = panel;
                popupSettings.FindProperty("_selectableGroup").objectReferenceValue = group;
                popupSettings.FindProperty("_inputActionMap").stringValue = "UI";
                popupSettings.ApplyModifiedPropertiesWithoutUndo();

                if (PrefabUtility.SaveAsPrefabAsset(root, kPrefabPath) == null)
                {
                    throw new System.InvalidOperationException("System Check PopupのPrefabを保存できませんでした。");
                }
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        static RectTransform CreateRect(string objectName, Transform parent, Vector2 size, Vector2 position)
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            go.layer = 5;
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return rect;
        }

        static TextMeshProUGUI CreateText(string objectName, Transform parent, TMP_FontAsset font, string value, Vector2 size, Vector2 position, float fontSize)
        {
            var rect = CreateRect(objectName, parent, size, position);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            return text;
        }
    }
}
