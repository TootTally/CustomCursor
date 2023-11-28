﻿using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace TootTallyCustomCursor
{
    public static class CustomCursor
    {
        private const string NOTETARGET_PATH = "GameplayCanvas/GameSpace/TargetNote";
        private const string NOTEDOT_PATH = "GameplayCanvas/GameSpace/TargetNote/note-dot";
        private const string NOTEDOTGLOW_PATH = "GameplayCanvas/GameSpace/TargetNote/note-dot-glow";
        private const string NOTEDOTGLOW1_PATH = "GameplayCanvas/GameSpace/TargetNote/note-dot-glow/note-dot-glow (1)";

        private static Texture2D _noteTargetTexture, _noteDotTexture, _noteDotGlowTexture, _noteDotGlow1Texture;
        private static string _lastCursorName;

        public static void LoadCursorTexture(GameController __instance, string cursorName)
        {
            //If textures are already set, skip
            if (AreAllTexturesLoaded() && !ConfigCursorNameChanged()) return;

            var cursorPath = Path.Combine(Paths.BepInExRootPath, Plugin.CURSORS_FOLDER_PATH, cursorName);

            //Dont know which will request will finish first...
            Plugin.Instance.StartCoroutine(LoadCursorTexture(cursorPath + "/TargetNote.png", texture =>
            {
                _noteTargetTexture = texture;
                Plugin.LogInfo("Target Texture Loaded.");
                if (AreAllTexturesLoaded() && __instance != null)
                    OnAllTextureLoadedAfterConfigChange(__instance);
            }));
            Plugin.Instance.StartCoroutine(LoadCursorTexture(cursorPath + "/note-dot.png", texture =>
            {
                _noteDotTexture = texture;
                Plugin.LogInfo("Dot Texture Loaded.");
                if (AreAllTexturesLoaded() && __instance != null)
                    OnAllTextureLoadedAfterConfigChange(__instance);
            }));
            Plugin.Instance.StartCoroutine(LoadCursorTexture(cursorPath + "/note-dot-glow.png", texture =>
            {
                _noteDotGlowTexture = texture;
                Plugin.LogInfo("Dot Glow Texture Loaded.");
                if (AreAllTexturesLoaded() && __instance != null)
                    OnAllTextureLoadedAfterConfigChange(__instance);
            }));
            Plugin.Instance.StartCoroutine(LoadCursorTexture(cursorPath + "/note-dot-glow (1).png", texture =>
            {
                _noteDotGlow1Texture = texture;
                Plugin.LogInfo("Dot Glow1 Texture Loaded.");
                if (AreAllTexturesLoaded() && __instance != null)
                    OnAllTextureLoadedAfterConfigChange(__instance);
            }));
        }

        public static void UnloadTextures()
        {
            GameObject.DestroyImmediate(_noteTargetTexture);
            GameObject.DestroyImmediate(_noteDotTexture);
            GameObject.DestroyImmediate(_noteDotGlowTexture);
            GameObject.DestroyImmediate(_noteDotGlow1Texture);
            Plugin.LogInfo("Custom Cursor Textures Destroyed.");
        }

        public static void OnAllTextureLoadedAfterConfigChange(GameController __instance)
        {
            ApplyCustomTextureToCursor(__instance);
            _lastCursorName = Plugin.Instance.CursorName.Value;

        }

        public static bool AreAllTexturesLoaded() => _noteTargetTexture != null && _noteDotTexture != null && _noteDotGlowTexture != null && _noteDotGlow1Texture != null;

        public static bool ConfigCursorNameChanged() => Plugin.Instance.CursorName.Value != _lastCursorName;

        public static IEnumerator<UnityWebRequestAsyncOperation> LoadCursorTexture(string filePath, Action<Texture2D> callback)
        {
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(filePath);
            yield return webRequest.SendWebRequest();

            if (!webRequest.isNetworkError && !webRequest.isHttpError)
                callback(DownloadHandlerTexture.GetContent(webRequest));
            else
                Plugin.LogInfo("Cursor does not exist or have the wrong format.");
        }

        public static void ApplyCustomTextureToCursor(GameController __instance)
        {
            if (!AreAllTexturesLoaded()) return;

            Plugin.LogInfo("Applying Custom Textures to cursor.");
            GameObject noteTarget = GameObject.Find(NOTETARGET_PATH).gameObject;
            GameObject noteDot = GameObject.Find(NOTEDOT_PATH).gameObject;
            GameObject noteDotGlow = GameObject.Find(NOTEDOTGLOW_PATH).gameObject;
            GameObject noteDotGlow1 = GameObject.Find(NOTEDOTGLOW1_PATH).gameObject;

            noteTarget.GetComponent<Image>().sprite = Sprite.Create(_noteTargetTexture, new Rect(0, 0, _noteTargetTexture.width, _noteTargetTexture.height), Vector2.one);
            noteTarget.GetComponent<RectTransform>().sizeDelta = new Vector2(_noteTargetTexture.width, _noteTargetTexture.height) / 2;
            noteDot.GetComponent<Image>().sprite = Sprite.Create(_noteDotTexture, new Rect(0, 0, _noteDotTexture.width, _noteDotTexture.height), Vector2.zero);
            noteDot.GetComponent<RectTransform>().sizeDelta = new Vector2(_noteDotTexture.width, _noteDotTexture.height) / 2;
            noteDotGlow.GetComponent<Image>().sprite = Sprite.Create(_noteDotGlowTexture, new Rect(0, 0, _noteDotGlowTexture.width, _noteDotGlowTexture.height), Vector2.zero);
            noteDotGlow.GetComponent<RectTransform>().sizeDelta = new Vector2(_noteDotGlowTexture.width, _noteDotGlowTexture.height) / 2;
            noteDotGlow1.GetComponent<Image>().sprite = Sprite.Create(_noteDotGlow1Texture, new Rect(0, 0, _noteDotGlow1Texture.width, _noteDotGlow1Texture.height), Vector2.zero);
            noteDotGlow1.GetComponent<RectTransform>().sizeDelta = new Vector2(_noteDotGlow1Texture.width, _noteDotGlow1Texture.height) / 2;

            __instance.dotsize = _noteTargetTexture.width / 2;
        }

        public static void AddTrail(GameController __instance, string cursorName)
        {
            var trailPath = Path.Combine(Paths.BepInExRootPath, Plugin.CURSORS_FOLDER_PATH, cursorName);

            Plugin.Instance.StartCoroutine(LoadCursorTexture(trailPath + "/trail.png", texture =>
            {
                __instance.pointer.AddComponent<CursorTrail>().Init(
                    texture.height * Plugin.Instance.TrailSize.Value,
                    Plugin.Instance.TrailLength.Value,
                    Plugin.Instance.TrailSpeed.Value,
                    120f,
                    Plugin.Instance.TrailStartColor.Value,
                    Plugin.Instance.TrailEndColor.Value,
                    __instance.notelinesholder.transform.GetChild(0).GetComponent<LineRenderer>().material,
                    texture);
            }));
        }

        public static void ResolvePresets(GameController __instance)
        {
            if ((!AreAllTexturesLoaded() || __instance == null) && Plugin.Instance.CursorName.Value != Plugin.DEFAULT_CURSORNAME)
            {
                Plugin.LogInfo($"[{Plugin.Instance.CursorName.Value}] preset loading...");
                LoadCursorTexture(__instance, Plugin.Instance.CursorName.Value);
            }
            else if (Plugin.Instance.CursorName.Value != Plugin.DEFAULT_CURSORNAME)
                ApplyCustomTextureToCursor(__instance);
            else
                Plugin.LogInfo("[Default] preset selected. Not loading any Custom Cursor.");
        }
    }
}
