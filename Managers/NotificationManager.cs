/*
 * United Goyim College Fund  Managers/NotificationManager.cs
 * A stupid shit hole i fuckjing hate this game with over 1000+ mods
 *
 * Copyright (C) 2026  robin williams
 * https://github.com/iiDk-the-inactual/UnitedGoyimCollegeFund
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using GorillaLocomotion;
using Seralyth.Classes.Menu;
using Seralyth.Extensions;
using Seralyth.Menu;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Seralyth.Menu.Main;

namespace Seralyth.Managers
{
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance { get; private set; }

        public GameObject canvas;
        private GameObject mainCamera;
        //private Material textMaterial;

        public static string PreviousNotifi;
        /// <summary>
        /// Elements in this dictionary are displayed on the user's
        /// screen as an overlay in the top left or right corner of the view.
        /// </summary>
        public static readonly Dictionary<string, string> information = new Dictionary<string, string>();

        public static TextMeshProUGUI notificationText;
        public static TextMeshProUGUI arraylistText;
        public static TextMeshProUGUI informationText;

        private bool hasInitialized;
        public static bool noRichText;
        public static bool soundOnError;
        public static bool noPrefix;
        public static bool narrateNotifications;

        public static int NotifiCounter;
        private class NotificationEntry
        {
            public string RawText;
            public string BaseDisplay;
            public int Counter;
            public float ExpireTime;
        }

        private static readonly List<NotificationEntry> activeNotifications = new List<NotificationEntry>();

        private void Start()
        {
            Instance = this;
            LogManager.Log("Notifications loaded");
        }

        private void Init()
        {
            mainCamera = Camera.main.gameObject;

            GameObject canvasParent = new GameObject("Israel_NotificationParent");
            canvasParent.transform.position = mainCamera.transform.position;

            canvas = new GameObject("Canvas");
            canvas.AddComponent<Canvas>();
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();

            Canvas canvasComponent = canvas.GetComponent<Canvas>();
            canvasComponent.enabled = true;
            canvasComponent.renderMode = RenderMode.WorldSpace;
            canvasComponent.worldCamera = mainCamera.GetComponent<Camera>();

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(5f, 5f);
            canvasRect.position = mainCamera.transform.position;

            canvas.transform.parent = canvasParent.transform;
            canvasRect.localPosition = new Vector3(0f, 0f, 1.6f);
            canvasRect.localScale = Vector3.one;

            Vector3 rotation = canvasRect.rotation.eulerAngles;
            rotation.y = -270f;
            canvasRect.rotation = Quaternion.Euler(rotation);

            //textMaterial = new Material(Shader.Find("GUI/Text Shader"));

            notificationText = CreateText(canvas.transform, new Vector3(-1f, -1f, -0.5f),
                new Vector2(450f, 210f), 30, TextAlignmentOptions.BottomLeft);

            arraylistText = CreateText(canvas.transform, new Vector3(-1f, -1f, -0.5f),
                new Vector2(450f, 1000f), 20, TextAlignmentOptions.TopLeft);

            informationText = CreateText(canvas.transform, new Vector3(-1f, -1f, 0.5f),
                new Vector2(450f, 1000f), 30, TextAlignmentOptions.TopRight);

            StartCoroutine(SetShaderAfterInit());
        }

        private TextMeshProUGUI CreateText(Transform parent, Vector3 localPos, Vector2 size, int fontSize, TextAlignmentOptions anchor)
        {
            GameObject textObj = new GameObject { transform = { parent = parent } };
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();

            text.SafeSetText("");
            text.fontSize = fontSize;
            text.SafeSetFont(AgencyFB);
            text.rectTransform.sizeDelta = size;
            text.alignment = anchor;
            text.overflowMode = anchor == TextAlignmentOptions.BottomLeft ? TextOverflowModes.Overflow : TextOverflowModes.Truncate;
            text.rectTransform.localScale = new Vector3(0.00333333333f, 0.00333333333f, 0.33333333f);
            text.rectTransform.localPosition = localPos;
            //text.material = textMaterial;
            text.characterSpacing = -9f;

            return text;
        }

        private float updateArraylistTimer;
        private void FixedUpdate()
        {
            try
            {
                if (!hasInitialized && Camera.main != null)
                {
                    Init();
                    hasInitialized = true;
                }

                canvas.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 2f;

                canvas.transform.position = mainCamera.transform.TransformPoint(0f, 0f, 1.6f);
                canvas.transform.rotation = mainCamera.transform.rotation * Quaternion.Euler(0, 90, 0);
                canvas.transform.localScale = Vector3.one * (scaleWithPlayer ? GTPlayer.Instance.scale : 1f);

                UpdateNotificationExpiry();

                try
                {
                    arraylistText.SafeSetFont(activeFont);
                    arraylistText.SafeSetFontStyle(activeFontStyle);
                    arraylistText.SafeSetFontSize(arraylistScale);
                    arraylistText.Chams();

                    notificationText.SafeSetFont(activeFont);
                    notificationText.SafeSetFontStyle(activeFontStyle);
                    notificationText.SafeSetFontSize(notificationScale);
                    notificationText.rectTransform.localPosition = new Vector3(-1f, disableNotifications ? -100f : -1f, -0.5f);
                    notificationText.Chams();

                    informationText.SafeSetFont(activeFont);
                    informationText.SafeSetFontStyle(activeFontStyle);
                    informationText.SafeSetFontSize(overlayScale);
                    informationText.Chams();

                    FollowMenuSettings(arraylistText);
                    FollowMenuSettings(notificationText);
                    FollowMenuSettings(informationText);
                }
                catch { }

                arraylistText.rectTransform.localPosition = new Vector3(-1f, -1f, flipArraylist ? 0.5f : -0.5f);
                arraylistText.alignment = flipArraylist ? TextAlignmentOptions.TopRight : TextAlignmentOptions.TopLeft;

                informationText.rectTransform.localPosition = new Vector3(-1f, -1f, flipArraylist ? -0.5f : 0.5f);
                informationText.alignment = flipArraylist ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.TopRight;

                if (information.Count > 0)
                {
                    Color targetColor = Buttons.GetIndex("Swap GUI Colors").enabled ? buttonColors[1].GetCurrentColor() : backgroundColor.GetCurrentColor();

                    List<string> statsLines = information
                        .Select(item => $"<color=#{ColorToHex(targetColor)}>{item.Key}</color> <color=#{ColorToHex(textColors[1].GetColor(0))}>{item.Value}</color>")
                        .Select(line => (text: line, width: informationText.GetPreferredValues(NoRichtextTags(line)).x))
                        .OrderByDescending(t => t.width)
                        .Select(t => t.text)
                        .ToList();

                    informationText.SafeSetText(string.Join("\n", statsLines));
                    informationText.color = Color.white;
                }
                else if (!informationText.text.IsNullOrEmpty())
                    informationText.SafeSetText("");

                if (showEnabledModsVR)
                {
                    if (Time.time > updateArraylistTimer)
                    {
                        updateArraylistTimer = Time.time + (advancedArraylist ? 0.1f : 0.5f);
                        List<string> enabledMods = new List<string>();
                        int categoryIndex = 0;

                        foreach (ButtonInfo[] buttonList in Buttons.buttons)
                        {
                            foreach (ButtonInfo button in buttonList)
                            {
                                try
                                {
                                    if (Buttons.buttons[Buttons.GetCategory("Temporary Category")].Contains(button) || button.hideFromArraylist)
                                        continue;

                                    if (!button.enabled || (hideSettings && (!hideSettings ||
                                                                             Buttons.categoryNames[categoryIndex]
                                                                                 .Contains("Settings")))) continue;
                                    string buttonText = button.overlapText ?? button.buttonText;

                                    if (inputTextColor != "green")
                                        buttonText = buttonText.Replace(" <color=grey>[</color><color=green>", " <color=grey>[</color><color=" + inputTextColor + ">");

                                    buttonText = FixTMProTags(buttonText);
                                    buttonText = FollowMenuSettings(buttonText);
                                    enabledMods.Add(buttonText);
                                }
                                catch { }
                            }
                            categoryIndex++;
                        }

                        string[] sortedMods = enabledMods
                            .Select(s => (text: s, width: arraylistText.GetPreferredValues(NoRichtextTags(s)).x))
                            .OrderByDescending(t => t.width)
                            .Select(t => t.text)
                            .ToArray();


                        var sb = new System.Text.StringBuilder();
                        for (int i = 0; i < sortedMods.Length; i++)
                        {
                            if (advancedArraylist)
                                /* Flipped */
                                sb.Append(flipArraylist ? $"..." : $"...").Append('\n');
                            else
                                /* Normal  */
                                sb.Append(sortedMods[i]).Append('\n');
                        }
                        arraylistText.SafeSetText(sb.ToString());

                        arraylistText.color = Buttons.GetIndex("Swap GUI Colors").enabled ? textColors[1].GetColor(0) : backgroundColor.GetCurrentColor();
                    }
                }
                else if (!arraylistText.text.IsNullOrEmpty())
                    arraylistText.SafeSetText("");

                if (lowercaseMode)
                {
                    if (!arraylistText.text.IsNullOrEmpty())
                        arraylistText.SafeSetText(arraylistText.text.ToLower());

                    if (!notificationText.text.IsNullOrEmpty())
                        notificationText.SafeSetText(notificationText.text.ToLower());

                    if (!informationText.text.IsNullOrEmpty())
                        informationText.SafeSetText(informationText.text.ToLower());
                }

                if (uppercaseMode)
                {
                    if (!arraylistText.text.IsNullOrEmpty())
                        arraylistText.SafeSetText(arraylistText.text.ToUpper());

                    if (!notificationText.text.IsNullOrEmpty())
                        notificationText.SafeSetText(notificationText.text.ToUpper());

                    if (!informationText.text.IsNullOrEmpty())
                        informationText.SafeSetText(informationText.text.ToUpper());
                }

                canvas.layer = Buttons.GetIndex("Hide Notifications on Camera").enabled ? 19 : 0;
            }
            catch (Exception e) { LogManager.Log(e); }
        }

        /// <summary>
        /// Displays a notification message to the user, with optional customization for display duration and
        /// formatting.
        /// </summary>
        /// <remarks>If the notification text matches the previous notification and notification stacking
        /// is enabled, the notification count is incremented instead of displaying a new message. The method applies
        /// various formatting and translation options based on current settings, and may play a notification sound or
        /// narrate the message if those features are enabled. Rich text support and text casing are also configurable.
        /// This method is thread-unsafe and should be called from the main UI thread.</remarks>
        /// <param name="notificationText">The text of the notification to display. May include rich text formatting tags. If translation is enabled,
        /// the text will be translated before display.</param>
        /// <param name="clearTime">The time, in milliseconds, before the notification is cleared. Specify -1 to use the default notification
        /// decay time.</param>
        public static void SendNotification(string notificationText, int clearTime = -1)
        {
            if (clearTime < 0)
                clearTime = notificationDecayTime;

            if (disableNotifications && !Buttons.GetIndex("Conduct Notifications").enabled) return;
            try
            {
                if (translate)
                {
                    if (TranslationManager.translateCache.ContainsKey(notificationText))
                        notificationText = TranslationManager.TranslateText(notificationText);
                    else
                    {
                        TranslationManager.TranslateText(notificationText, delegate { SendNotification(notificationText, clearTime); });
                        return;
                    }
                }

                if (/*notificationSoundIndex != 0 && */(!soundOnError || notificationText.Contains("<color=red>ERROR</color>")) && Time.time > timeMenuStarted + 5f)
                    SoundManager.Play(SoundManager.DefaultSounds["Notification"], action: clip => AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, buttonClickVolume / 10f));

                if (inputTextColor != "green")
                    notificationText = notificationText.Replace("<color=green>", "<color=" + inputTextColor + ">");

                notificationText = FixTMProTags(notificationText);

                if (hideBrackets)
                    notificationText = notificationText.Replace("[", "").Replace("]", "");

                notificationText = notificationText.TrimEnd('\n', '\r');

                float expireTime = Time.time + (clearTime / 1000f);
                NotificationEntry lastEntry = activeNotifications.Count > 0 ? activeNotifications[^1] : null;

                if (lastEntry != null && PreviousNotifi == notificationText && stackNotifications)
                {
                    lastEntry.Counter++;
                    lastEntry.ExpireTime = expireTime;
                    NotifiCounter = lastEntry.Counter;
                }
                else
                {
                    NotifiCounter = 0;
                    PreviousNotifi = notificationText;

                    activeNotifications.Add(new NotificationEntry
                    {
                        RawText = notificationText,
                        BaseDisplay = notificationText,
                        Counter = 0,
                        ExpireTime = expireTime
                    });
                }

                RebuildNotificationText();

                if (lowercaseMode)
                    NotificationManager.notificationText.SafeSetText(NotificationManager.notificationText.text.ToLower());

                if (uppercaseMode)
                    NotificationManager.notificationText.SafeSetText(NotificationManager.notificationText.text.ToUpper());

                if (narrateNotifications)
                    NarrateText(NoRichtextTags(noPrefix ? RemovePrefix(notificationText) : notificationText));
            }
            catch (Exception e)
            {
                LogManager.LogError($"Notification failed.\nNotification: {notificationText}\nException: {e.Message}");
            }
        }

        public static void PlayNotificationSound() =>
            SoundManager.Play(SoundManager.DefaultSounds["Notification"], action: clip => AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, buttonClickVolume / 10f));

        /// <summary>
        /// Clears all active notifications and stops any ongoing notification clearing operations.
        /// </summary>
        /// <remarks>Call this method to immediately remove all notification text and halt any scheduled
        /// notification clearing. This method is typically used to reset the notification system or when notifications
        /// are no longer relevant.</remarks>
        public static void ClearAllNotifications()
        {
            activeNotifications.Clear();
            notificationText.SafeSetText("");
        }

        /// <summary>
        /// Removes a specified number of past notification entries from the notification text.
        /// </summary>
        /// <param name="amount">The number of past notification lines to remove. Must be zero or greater. If the value is greater than or
        /// equal to the total number of notification lines, all notifications are cleared.</param>
        public static void ClearPastNotifications(int amount)
        {
            if (activeNotifications.Count == 0)
                return;

            int removeCount = Math.Min(amount, activeNotifications.Count);
            activeNotifications.RemoveRange(0, removeCount);
            RebuildNotificationText();
        }

        /// <summary>
        /// Removes any notification entries whose individual expiry time has passed, then
        /// refreshes the on-screen text if anything changed. Called once per FixedUpdate.
        /// </summary>
        private static void UpdateNotificationExpiry()
        {
            if (activeNotifications.Count == 0)
                return;

            bool changed = false;
            for (int i = activeNotifications.Count - 1; i >= 0; i--)
            {
                if (Time.time >= activeNotifications[i].ExpireTime)
                {
                    activeNotifications.RemoveAt(i);
                    changed = true;
                }
            }

            if (changed)
                RebuildNotificationText();
        }

        /// <summary>
        /// Rebuilds the visible notification text from the current list of active entries.
        /// </summary>
        private static void RebuildNotificationText()
        {
            List<string> lines = new List<string>(activeNotifications.Count);
            foreach (NotificationEntry entry in activeNotifications)
            {
                lines.Add(entry.Counter > 0
                    ? $"{entry.BaseDisplay} <color=grey>(x{entry.Counter + 1})</color>"
                    : entry.BaseDisplay);
            }

            string combined = string.Join(Environment.NewLine, lines);

            if (noRichText)
                combined = NoRichtextTags(combined);

            notificationText.SafeSetText(combined);
            notificationText.richText = !noRichText;
        }

        private IEnumerator SetShaderAfterInit()
        {
            yield return null; yield return null; yield return null; yield return null; yield return null;

            notificationText.Chams();
            arraylistText.Chams();
            informationText.Chams();
        }

        private static readonly System.Text.RegularExpressions.Regex PrefixRegex =
            new System.Text.RegularExpressions.Regex(@"^<color=grey>\[</color><color=[^>]+>.*?</color><color=grey>\]</color> ", System.Text.RegularExpressions.RegexOptions.Compiled);

        private static string RemovePrefix(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return PrefixRegex.Replace(text, "");
        }
    }
}