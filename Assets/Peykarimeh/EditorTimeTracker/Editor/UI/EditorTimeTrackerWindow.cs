using UnityEngine;
using UnityEditor;
using System;

namespace Peykarimeh.EditorTimeTracker.UI
{
    public class EditorTimeTrackerWindow : EditorWindow
    {
        // Layout constants. The window must stay usable on small / low-resolution
        // screens, so every fixed-size element is clamped against the available
        // width and the whole body lives inside a vertical scroll view.
        private const float CIRCLE_MAX_SIZE = 230f;
        private const float CIRCLE_MIN_SIZE = 120f;
        private const float SCROLLBAR_ALLOWANCE = 16f;
        private const float CARD_INSET = 16f;

        private Texture2D statusIconTexture;
        private bool currentRunningState = false;
        private Vector2 scrollPosition;

        // Width available for content inside a card, recalculated every OnGUI.
        private float innerWidth = CIRCLE_MAX_SIZE;

        [MenuItem("Tools/Project Time Tracker")]
        public static void ShowWindow()
        {
            EditorTimeTrackerWindow window = GetWindow<EditorTimeTrackerWindow>("Tracker");
            window.minSize = new Vector2(300, 220);
        }

        private void OnEnable()
        {
            TimeTrackerEngine.OnDataUpdated += Repaint;
            UpdateStatusIcon();
        }

        private void OnDisable()
        {
            TimeTrackerEngine.OnDataUpdated -= Repaint;
            if (statusIconTexture != null) DestroyImmediate(statusIconTexture);
        }

        private void OnGUI()
        {
            TimeTrackerStyles.Init();
            HandleInput();
            UpdateStatusIcon();

            TimeTrackerStyles.DrawBackgroundGradient(position);

            // Tighten the outer padding on short windows so the small screens
            // that need scrolling the most don't waste height on margins.
            float padding = position.height < 320f ? 8f : 16f;
            innerWidth = Mathf.Max(CIRCLE_MIN_SIZE,
                position.width - (padding * 2f) - SCROLLBAR_ALLOWANCE - CARD_INSET);

            GUILayout.BeginArea(new Rect(padding, padding, position.width - (padding * 2f), position.height - (padding * 2f)));

            // GUIStyle.none for the horizontal bar disables horizontal scrolling
            // outright, so content is clamped to the view width instead of
            // silently sliding off-screen.
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false,
                GUIStyle.none, GUI.skin.verticalScrollbar, GUIStyle.none);

            if (TimeTrackerEngine.IsWaitingForAfkResponse) DrawAfkCorrectionUI();
            else if (TimeTrackerEngine.IsOnBreak) DrawBreakModeUI();
            else DrawStandardUI();

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void HandleInput()
        {
            Event e = Event.current;
            if (e != null && (e.type == EventType.MouseMove || e.type == EventType.MouseDown ||
                              e.type == EventType.MouseDrag || e.type == EventType.MouseUp ||
                              e.type == EventType.KeyDown || e.type == EventType.KeyUp ||
                              e.type == EventType.ScrollWheel))
            {
                TimeTrackerEngine.RegisterActivity();
            }
        }

        private void DrawStandardUI()
        {
            // Top Bar: Today's Date (Top Right)
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUIStyle dateStyle = new GUIStyle(EditorStyles.label) {
                fontSize = 11,
                fontStyle = FontStyle.Bold
            };
            dateStyle.normal.textColor = TimeTrackerStyles.TEXT_PRIMARY;
            GUILayout.Label(TimeTrackerEngine.FormatDate(DateTime.Now), dateStyle);
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(4);

            if (!TimeTrackerEngine.Settings.minimalMode)
            {
                // Session mode has no weekly target to chart, so the top card
                // carries the wider context — the day and the session clock.
                if (IsSessionMode) TimeTrackerStyles.DrawCard(() => DrawSessionSummary(), 8);
                else TimeTrackerStyles.DrawCard(() => DrawWeeklyProgressBar(), 8);
                GUILayout.Space(12);
            }

            TimeTrackerStyles.DrawCard(() => {
                DrawDailySessionCircle();
                
                if (!TimeTrackerEngine.Settings.minimalMode)
                {
                    GUILayout.Space(12);
                    
                    GUIStyle totalStyle = new GUIStyle(EditorStyles.label) {
                        fontSize = 12,
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold
                    };
                    totalStyle.normal.textColor = TimeTrackerStyles.TEXT_SECONDARY;
                    GUILayout.Label($"Total Project Time: {TimeTrackerEngine.FormatTime(TimeTrackerEngine.Data.totalProjectTime)}", totalStyle);
                    GUILayout.Space(4);
                }
            }, 16);

            GUILayout.Space(16);
            DrawControls();
            
            GUILayout.FlexibleSpace();
            DrawSettings();
        }

        private void DrawAfkCorrectionUI()
        {
            GUILayout.FlexibleSpace();
            TimeTrackerStyles.DrawCard(() => {
                GUILayout.Space(24);
                
                GUIStyle header = new GUIStyle(EditorStyles.boldLabel) { 
                    alignment = TextAnchor.MiddleCenter, fontSize = 20, fontStyle = FontStyle.Bold
                };
                header.normal.textColor = TimeTrackerStyles.ACCENT_YELLOW;
                GUILayout.Label(EditorGUIUtility.IconContent("console.warnicon"), header);
                GUILayout.Label("👋 Welcome Back!", header);
                
                GUILayout.Space(20);
                GUILayout.Label("You were away for:", TimeTrackerStyles.SubtextStyle);
                GUILayout.Space(8);
                
                string timeAway = TimeTrackerEngine.FormatTime(TimeTrackerEngine.PotentialIdleTimeToAdd);
                GUILayout.Label(timeAway, TimeTrackerStyles.BigTimerStyle);
                
                GUILayout.Space(20);
                GUILayout.Label("Was this work related?", TimeTrackerStyles.SubtextStyle);
                GUILayout.Space(24);

                float answerWidth = Mathf.Clamp((innerWidth - 12f) * 0.5f, 84f, 140f);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (TimeTrackerStyles.DrawStyledButton("✓ Yes, Add Time", TimeTrackerStyles.ACCENT_GREEN, 48, answerWidth))
                    TimeTrackerEngine.ResumeFromAfk(true);
                GUILayout.Space(12);
                if (TimeTrackerStyles.DrawStyledButton("✕ No, Discard", TimeTrackerStyles.ACCENT_RED, 48, answerWidth))
                    TimeTrackerEngine.ResumeFromAfk(false);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(24);
            }, 20);
            GUILayout.FlexibleSpace();
        }

        private void DrawBreakModeUI()
        {
            GUILayout.FlexibleSpace();
            TimeTrackerStyles.DrawCard(() => {
                GUILayout.Space(24);
                
                GUIStyle header = new GUIStyle(EditorStyles.boldLabel) { 
                    alignment = TextAnchor.MiddleCenter, fontSize = 20, fontStyle = FontStyle.Bold
                };
                header.normal.textColor = TimeTrackerStyles.ACCENT_BLUE;
                GUILayout.Label("☕ Taking a Break", header);
                
                GUILayout.Space(20);
                GUILayout.Label("Break duration:", TimeTrackerStyles.SubtextStyle);
                GUILayout.Space(8);
                
                double breakElapsed = EditorApplication.timeSinceStartup - TimeTrackerEngine.BreakStartTime;
                GUILayout.Label(TimeTrackerEngine.FormatTime(breakElapsed), TimeTrackerStyles.BigTimerStyle);
                
                GUILayout.Space(20);
                GUILayout.Label("Take your time and recharge! 💪", TimeTrackerStyles.SubtextStyle);
                GUILayout.Space(24);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (TimeTrackerStyles.DrawStyledButton("✓ End Break & Resume", TimeTrackerStyles.ACCENT_GREEN, 48, Mathf.Clamp(innerWidth, 120f, 180f)))
                    TimeTrackerEngine.EndBreak();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(24);
            }, 20);
            GUILayout.FlexibleSpace();
        }

        /// <summary>
        /// Session-mode replacement for the weekly bar: today's tracked total plus
        /// when this editor session began and how long the project has been open.
        /// </summary>
        private void DrawSessionSummary()
        {
            GUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            GUIStyle headerStyle = new GUIStyle(EditorStyles.label) { fontSize = 11, fontStyle = FontStyle.Bold };
            headerStyle.normal.textColor = TimeTrackerStyles.TEXT_SECONDARY;
            GUILayout.Label("TODAY", headerStyle);

            GUILayout.FlexibleSpace();
            GUIStyle totalStyle = new GUIStyle(EditorStyles.label) { fontSize = 14, fontStyle = FontStyle.Bold };
            totalStyle.normal.textColor = TimeTrackerStyles.ACCENT_BLUE;
            GUILayout.Label(TimeTrackerEngine.FormatTimeShort(TimeTrackerEngine.GetTodayTotalSeconds()), totalStyle);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(6);
            TimeTrackerStyles.DrawDivider();

            DrawSummaryRow("Session started", TimeTrackerEngine.SessionStartTime.ToString("HH:mm"));
            DrawSummaryRow("Project open for", TimeTrackerEngine.FormatTimeShort(TimeTrackerEngine.GetSessionElapsedSeconds()));
            DrawSummaryRow("Tracked this session", TimeTrackerEngine.FormatTimeShort(TimeTrackerEngine.CurrentSessionTime));

            GUILayout.Space(4);
        }

        private void DrawSummaryRow(string label, string value)
        {
            GUIStyle labelStyle = new GUIStyle(EditorStyles.label) { fontSize = 11 };
            labelStyle.normal.textColor = TimeTrackerStyles.TEXT_SECONDARY;
            GUIStyle valueStyle = new GUIStyle(EditorStyles.label) { fontSize = 11, fontStyle = FontStyle.Bold };
            valueStyle.normal.textColor = TimeTrackerStyles.TEXT_PRIMARY;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, labelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label(value, valueStyle);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2);
        }

        private void DrawWeeklyProgressBar()
        {
            TimeTrackerEngine.GetWeeklyStats(out double weekTotal, out _, out int daysLeft);
            double weeklyTargetSeconds = TimeTrackerEngine.GetWeeklyTargetSeconds();
            
            float progress = weeklyTargetSeconds > 0.1f ? Mathf.Clamp01((float)(weekTotal / weeklyTargetSeconds)) : (weekTotal > 0 ? 1f : 0f);
            int percentage = Mathf.FloorToInt(progress * 100);

            GUILayout.Space(4);
            
            EditorGUILayout.BeginHorizontal();
            GUIStyle headerStyle = new GUIStyle(EditorStyles.label) { fontSize = 11, fontStyle = FontStyle.Bold };
            headerStyle.normal.textColor = TimeTrackerStyles.TEXT_SECONDARY;
            GUILayout.Label("WEEKLY PROGRESS", headerStyle);
            
            GUILayout.FlexibleSpace();
            GUIStyle percentStyle = new GUIStyle(EditorStyles.label) { fontSize = 14, fontStyle = FontStyle.Bold };
            percentStyle.normal.textColor = progress >= 1f ? TimeTrackerStyles.ACCENT_GREEN : TimeTrackerStyles.ACCENT_BLUE;
            GUILayout.Label($"{percentage}%", percentStyle);
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(6);
            
            Rect barRect = EditorGUILayout.GetControlRect(false, 24);
            barRect.x += 4; barRect.width -= 8;
            
            EditorGUI.DrawRect(barRect, new Color(0.1f, 0.1f, 0.1f));
            if (progress > 0)
            {
                Rect filled = new Rect(barRect.x + 2, barRect.y + 2, (barRect.width - 4) * progress, barRect.height - 4);
                EditorGUI.DrawRect(filled, Color.Lerp(TimeTrackerStyles.ACCENT_BLUE, TimeTrackerStyles.ACCENT_GREEN, progress));
            }
            TimeTrackerStyles.DrawRectBorder(barRect, TimeTrackerStyles.DIVIDER, 1);
            
            GUILayout.Space(6);
            
            EditorGUILayout.BeginHorizontal();
            GUIStyle timeStyle = new GUIStyle(EditorStyles.label) { fontSize = 11 };
            timeStyle.normal.textColor = TimeTrackerStyles.TEXT_PRIMARY;
            GUILayout.Label($"{TimeTrackerEngine.FormatTimeShort(weekTotal)} / {TimeTrackerEngine.FormatTimeShort(weeklyTargetSeconds)}", timeStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{daysLeft} day(s) left", timeStyle);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);
            DateTime weekStart = TimeTrackerEngine.GetStartOfCurrentCycle(DateTime.Now.Date);
            DateTime weekEnd = weekStart.AddDays(6);
            GUIStyle rangeStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft };
            rangeStyle.normal.textColor = TimeTrackerStyles.TEXT_PRIMARY;
            GUILayout.Label($"{TimeTrackerEngine.FormatDate(weekStart)} - {TimeTrackerEngine.FormatDate(weekEnd)}", rangeStyle);
            GUILayout.Space(4);
        }

        private static bool IsSessionMode =>
            TimeTrackerEngine.Settings.goalType == TimeTrackerEngine.TrackerSettings.GoalType.PerSession;

        private void DrawDailySessionCircle()
        {
            // The dial always tracks whichever goal is selected: a session in
            // session mode, otherwise the day (fixed, or the week's daily share).
            double achievedSeconds, targetSeconds;
            string goalCaption;

            if (IsSessionMode)
            {
                achievedSeconds = TimeTrackerEngine.CurrentSessionTime;
                targetSeconds = TimeTrackerEngine.GetSessionTargetSeconds();
                goalCaption = "SESSION GOAL";
            }
            else
            {
                achievedSeconds = TimeTrackerEngine.GetTodayTotalSeconds();
                targetSeconds = Mathf.Max(0f, TimeTrackerEngine.CalculateTargetDailyHours() * 3600f);
                goalCaption = "DAILY PROGRESS";
            }

            float progress = targetSeconds > 0.1 ? Mathf.Clamp01((float)(achievedSeconds / targetSeconds)) : (achievedSeconds > 0 ? 1f : 0f);
            int percent = Mathf.RoundToInt(progress * 100f);

            GUIStyle statusStyle = new GUIStyle(EditorStyles.label) {
                alignment = TextAnchor.MiddleCenter, fontSize = 11, fontStyle = FontStyle.Bold
            };
            statusStyle.normal.textColor = TimeTrackerEngine.IsRunning ? TimeTrackerStyles.ACCENT_GREEN : TimeTrackerStyles.ACCENT_YELLOW;
            GUILayout.Label(TimeTrackerEngine.IsRunning ? "● FOCUS SESSION" : "⏸ SESSION PAUSED", statusStyle);

            GUILayout.Space(8);

            // Shrink the dial to fit narrow windows instead of letting it clip.
            float circleSize = Mathf.Clamp(innerWidth, CIRCLE_MIN_SIZE, CIRCLE_MAX_SIZE);
            float scale = circleSize / CIRCLE_MAX_SIZE;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            Rect circleRect = GUILayoutUtility.GetRect(circleSize, circleSize, GUILayout.Width(circleSize), GUILayout.Height(circleSize));
            TimeTrackerStyles.DrawCircularProgress(circleRect, progress, new Color(0.11f, 0.11f, 0.11f),
                Color.Lerp(TimeTrackerStyles.ACCENT_BLUE, TimeTrackerStyles.ACCENT_GREEN, progress), 22f * scale);

            // Labels inside circle
            GUIStyle captionStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = ScaledFont(10, scale, 8), fontStyle = FontStyle.Bold };
            captionStyle.normal.textColor = TimeTrackerStyles.TEXT_MUTED;
            GUIStyle detailStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = ScaledFont(11, scale, 9) };
            detailStyle.normal.textColor = TimeTrackerStyles.TEXT_SECONDARY;
            GUIStyle dailyProgressStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = ScaledFont(14, scale, 10), fontStyle = FontStyle.Bold };
            dailyProgressStyle.normal.textColor = TimeTrackerStyles.TEXT_PRIMARY;

            GUIStyle timerStyle = TimeTrackerStyles.BigTimerStyle;
            if (scale < 0.99f)
            {
                timerStyle = new GUIStyle(TimeTrackerStyles.BigTimerStyle) { fontSize = ScaledFont(28, scale, 15) };
            }

            string sessionTime = TimeTrackerEngine.FormatTime(TimeTrackerEngine.CurrentSessionTime);

            if (TimeTrackerEngine.Settings.minimalMode)
            {
                GUI.Label(CircleLabelRect(circleRect, -3f, scale, timerStyle.fontSize), sessionTime, timerStyle);
            }
            else
            {
                GUI.Label(CircleLabelRect(circleRect, -54f, scale, captionStyle.fontSize), "CURRENT SESSION", captionStyle);
                GUI.Label(CircleLabelRect(circleRect, -15f, scale, timerStyle.fontSize), sessionTime, timerStyle);
                GUI.Label(CircleLabelRect(circleRect, 22f, scale, captionStyle.fontSize), goalCaption, captionStyle);
                GUI.Label(CircleLabelRect(circleRect, 44f, scale, dailyProgressStyle.fontSize), $"{TimeTrackerEngine.FormatTimeShort(achievedSeconds)} / {TimeTrackerEngine.FormatTimeShort(targetSeconds)}", dailyProgressStyle);
                GUI.Label(CircleLabelRect(circleRect, 66f, scale, detailStyle.fontSize), $"{percent}% complete", detailStyle);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>Font size scaled with the dial, never below a legible floor.</summary>
        private static int ScaledFont(int baseSize, float scale, int minSize)
        {
            return Mathf.Max(minSize, Mathf.RoundToInt(baseSize * scale));
        }

        /// <summary>
        /// A full-width label row inside the dial, positioned by its distance from
        /// the dial's centre so the stack keeps its proportions as the dial scales.
        /// </summary>
        private static Rect CircleLabelRect(Rect circleRect, float centerOffset, float scale, int fontSize)
        {
            float height = fontSize + 6f;
            float y = circleRect.center.y + (centerOffset * scale) - (height * 0.5f);
            return new Rect(circleRect.x, y, circleRect.width, height);
        }

        private void DrawControls()
        {
            // 120px is the two mini buttons plus their spacing; the main button
            // takes whatever is left, down to a still-clickable minimum.
            float mainButtonWidth = Mathf.Clamp(innerWidth - 120f, 88f, 180f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (TimeTrackerStyles.DrawMiniAdjustButton("-1m", TimeTrackerStyles.ACCENT_RED, "Remove 1 minute"))
                TimeTrackerEngine.RemoveTime(60);

            GUILayout.Space(10);

            if (TimeTrackerEngine.IsRunning)
            {
                if (TimeTrackerStyles.DrawStyledButton("⏸ Pause", TimeTrackerStyles.ACCENT_YELLOW, 46, mainButtonWidth))
                    TimeTrackerEngine.PauseTimer();
            }
            else
            {
                if (TimeTrackerStyles.DrawStyledButton("▶ Resume", TimeTrackerStyles.ACCENT_GREEN, 46, mainButtonWidth))
                    TimeTrackerEngine.StartTimer();
            }

            GUILayout.Space(10);

            if (TimeTrackerStyles.DrawMiniAdjustButton("+3m", TimeTrackerStyles.ACCENT_GREEN, "Add 3 minutes"))
                TimeTrackerEngine.AddValidTime(180);
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSettings()
        {
            TimeTrackerStyles.DrawDivider();
            
            var settings = TimeTrackerEngine.Settings;
            settings.showSettings = EditorGUILayout.Foldout(settings.showSettings, "⚙ SETTINGS & TOOLS", true, TimeTrackerStyles.SectionHeaderStyle);
            
            if (settings.showSettings)
            {
                GUILayout.Space(4);
                TimeTrackerStyles.DrawCard(() => {
                    EditorGUI.BeginChangeCheck();
                    
                    int modeIndex = settings.minimalMode ? 1 : 0;
                    int newModeIndex = EditorGUILayout.Popup("View Mode", modeIndex, new string[] { "Default", "Minimalist" });
                    if (newModeIndex != modeIndex) settings.minimalMode = (newModeIndex == 1);
                    
                    int newTheme = EditorGUILayout.Popup("UI Theme", settings.themeIndex, new string[] { "Default (Dark)", "Monochrome", "Cyberpunk", "Midnight" });
                    if (newTheme != settings.themeIndex)
                    {
                        settings.themeIndex = newTheme;
                        Repaint();
                    }
                    
                    GUILayout.Space(8);

                    settings.goalType = (TimeTrackerEngine.TrackerSettings.GoalType)EditorGUILayout.EnumPopup(new GUIContent("Goal Mode"), settings.goalType);
                    GUILayout.Space(4);
                    
                    if (settings.goalType == TimeTrackerEngine.TrackerSettings.GoalType.FixedDaily)
                    {
                        settings.dailyGoalHours = EditorGUILayout.IntSlider("Daily Goal (Hours)", settings.dailyGoalHours, 1, 24);
                    }
                    else if (settings.goalType == TimeTrackerEngine.TrackerSettings.GoalType.PerSession)
                    {
                        settings.sessionGoalMinutes = EditorGUILayout.IntSlider(
                            new GUIContent("Session Goal (Minutes)", "Target for a single editor session, from opening the project until you close it."),
                            settings.sessionGoalMinutes, 15, 720);

                        GUIStyle hintStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight };
                        hintStyle.normal.textColor = TimeTrackerStyles.TEXT_MUTED;
                        GUILayout.Label($"= {TimeTrackerEngine.FormatTimeShort(settings.sessionGoalMinutes * 60.0)} per session", hintStyle);
                    }
                    else
                    {
                        settings.weeklyGoalHours = EditorGUILayout.IntSlider("Weekly Goal (Hours)", settings.weeklyGoalHours, 1, 168);
                        
                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 80;
                        settings.startOfWeek = (DayOfWeek)EditorGUILayout.EnumPopup("Week Start", settings.startOfWeek);
                        GUILayout.Space(10);
                        settings.endOfWeek = (DayOfWeek)EditorGUILayout.EnumPopup("End", settings.endOfWeek);
                        EditorGUIUtility.labelWidth = 0;
                        EditorGUILayout.EndHorizontal();
                        
                    }
                    
                    GUILayout.Space(4);
                    settings.dateFormatIndex = EditorGUILayout.Popup("Date Format", settings.dateFormatIndex, 
                        new string[] { "24 Nov 2025", "Nov 24, 2025", "2025-11-24", "24-11-2025" });
                    
                    GUILayout.Space(8);
                    
                    // Focus Reminder
                    settings.showFocusReminderSettings = EditorGUILayout.Foldout(settings.showFocusReminderSettings, "⏰ Focus Break Reminders", true, EditorStyles.foldoutHeader);
                    if (settings.showFocusReminderSettings)
                    {
                        EditorGUI.indentLevel++;
                        settings.enableFocusReminder = EditorGUILayout.Toggle("Enable Reminders", settings.enableFocusReminder);
                        EditorGUI.BeginDisabledGroup(!settings.enableFocusReminder);
                        settings.focusReminderIntervalMinutes = EditorGUILayout.IntSlider("Interval (Minutes)", settings.focusReminderIntervalMinutes, 5, 120);
                        settings.focusReminderUseDialog = EditorGUILayout.Toggle("Show Dialog Box", settings.focusReminderUseDialog);
                        settings.focusReminderPlaySound = EditorGUILayout.Toggle("Play Sound Alert", settings.focusReminderPlaySound);
                        EditorGUI.EndDisabledGroup();
                        EditorGUI.indentLevel--;
                    }
                    
                    GUILayout.Space(8);
                    
                    // Idle Detection
                    settings.showIdleDetectionSettings = EditorGUILayout.Foldout(settings.showIdleDetectionSettings, "💤 Idle Detection (AFK)", true, EditorStyles.foldoutHeader);
                    if (settings.showIdleDetectionSettings)
                    {
                        EditorGUI.indentLevel++;
                        settings.autoPauseLimitSeconds = EditorGUILayout.IntSlider("Timeout (Seconds)", settings.autoPauseLimitSeconds, 30, 3600);
                        settings.afkAutoFocus = EditorGUILayout.Toggle("Auto-Focus Window", settings.afkAutoFocus);
                        settings.afkPlaySound = EditorGUILayout.Toggle("Play Sound Alert", settings.afkPlaySound);
                        EditorGUI.indentLevel--;
                    }

                    if (EditorGUI.EndChangeCheck()) TimeTrackerEngine.SaveData();

                    GUILayout.Space(12);
                    
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("📊 Export CSV", GUILayout.Height(28))) TimeTrackerEngine.ExportCSV();
                    EditorGUILayout.EndHorizontal();
                }, 8);
            }
        }

        private void UpdateStatusIcon()
        {
            if (currentRunningState == TimeTrackerEngine.IsRunning && statusIconTexture != null) return;
            currentRunningState = TimeTrackerEngine.IsRunning;
            
            Color col = currentRunningState ? new Color(0.2f, 0.9f, 0.2f) : new Color(0.9f, 0.8f, 0.1f);
            if (TimeTrackerEngine.IsWaitingForAfkResponse || TimeTrackerEngine.IsOnBreak) col = new Color(0.9f, 0.3f, 0.3f);

            if (statusIconTexture != null) DestroyImmediate(statusIconTexture);
            statusIconTexture = MakeCircleTex(16, 16, col);
            this.titleContent = new GUIContent("Tracker", statusIconTexture);
        }

        private Texture2D MakeCircleTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            Vector2 center = new Vector2(width/2f, height/2f);
            float radius = (Mathf.Min(width, height) / 2f) - 2;
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    pix[y * width + x] = (Vector2.Distance(new Vector2(x,y), center) <= radius) ? col : Color.clear;
                }
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}