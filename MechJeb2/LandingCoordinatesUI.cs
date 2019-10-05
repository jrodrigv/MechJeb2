using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using KSP.UI.Screens;
using UnityEngine;

namespace MuMech
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class LandingCoordinatesWindow : MonoBehaviour
    {
        internal static EventVoid OnActionGroupEditorOpened = new EventVoid("OnActionGroupEditorOpened");
        internal static EventVoid OnActionGroupEditorClosed = new EventVoid("OnActionGroupEditorClosed");

        private static GUIStyle unchanged;
        private static GUIStyle changed;
        private static GUIStyle greyed;
        private static GUIStyle overfull;

        private static LandingCoordinatesWindow instance;
        private static Vector3 mousePos = Vector3.zero;

        private bool ActionGroupMode;

        private Rect guiWindowRect = new Rect(0, 0, 0, 0);

        private MechJebCore core;

        [KSPField] public int offsetGUIPos = -1;

        private Vector2 scrollPos;

        [KSPField(isPersistant = false, guiActiveEditor = true, guiActive = false, guiName = "Show Weapon Name Editor"), UI_Toggle(enabledText = "Weapon Name GUI", disabledText = "GUI")] [NonSerialized] public bool showRFGUI;

        private bool styleSetup;

        private string latitude = String.Empty;
        private string longitude = String.Empty;

        public static void HideGUI()
        {
            if (instance != null && instance.core != null)
            {
                instance.core = null;
                instance.UpdateGUIState();
            }
            EditorLogic editor = EditorLogic.fetch;
            if (editor != null)
                editor.Unlock("BD_MN_GUILock");
        }

        public static void ShowGUI(MechJebCore core)
        {
            if (instance != null)
            {
                instance.core = core;
                instance.UpdateGUIState();
            }
        }

        private void UpdateGUIState()
        {
            enabled = core != null;
            EditorLogic editor = EditorLogic.fetch;
            if (!enabled && editor != null)
                editor.Unlock("BD_MN_GUILock");
        }

        private IEnumerator<YieldInstruction> CheckActionGroupEditor()
        {
            while (EditorLogic.fetch == null)
            {
                yield return null;
            }
            EditorLogic editor = EditorLogic.fetch;
            while (EditorLogic.fetch != null)
            {
                if (editor.editorScreen == EditorScreen.Actions)
                {
                    if (!ActionGroupMode)
                    {
                        HideGUI();
                        OnActionGroupEditorOpened.Fire();
                    }
                    EditorActionGroups age = EditorActionGroups.Instance;
                    if (core && !age.GetSelectedParts().Contains(core.part))
                    {
                        HideGUI();
                    }
                    ActionGroupMode = true;
                }
                else
                {
                    if (ActionGroupMode)
                    {
                        HideGUI();
                        OnActionGroupEditorClosed.Fire();
                    }
                    ActionGroupMode = false;
                }
                yield return null;
            }
        }

        private void Awake()
        {
            enabled = false;
            instance = this;
            StartCoroutine(CheckActionGroupEditor());
        }

        private void OnDestroy()
        {
            instance = null;
        }

        public void OnGUI()
        {
            if (!styleSetup)
            {
                styleSetup = true;
                Styles.InitStyles();
            }

            EditorLogic editor = EditorLogic.fetch;
            if (!HighLogic.LoadedSceneIsEditor || !editor)
            {
                return;
            }
            bool cursorInGUI = false; // nicked the locking code from Ferram
            mousePos = Input.mousePosition; //Mouse location; based on Kerbal Engineer Redux code
            mousePos.y = Screen.height - mousePos.y;

            int posMult = 0;
            if (offsetGUIPos != -1)
            {
                posMult = offsetGUIPos;
            }
            if (ActionGroupMode)
            {
                if (guiWindowRect.width == 0)
                {
                    guiWindowRect = new Rect(430 * posMult, 365, 438, 50);
                }
                new Rect(guiWindowRect.xMin + 440, mousePos.y - 5, 300, 20);
            }
            else
            {
                if (guiWindowRect.width == 0)
                {
                    //guiWindowRect = new Rect(Screen.width - 8 - 430 * (posMult + 1), 365, 438, (Screen.height - 365));
                    guiWindowRect = new Rect(Screen.width - 8 - 430 * (posMult + 1), 365, 438, 50);
                }
                new Rect(guiWindowRect.xMin - (230 - 8), mousePos.y - 5, 220, 20);
            }
            cursorInGUI = guiWindowRect.Contains(mousePos);
            if (cursorInGUI)
            {
                editor.Lock(false, false, false, "BD_MN_GUILock");
                //if (EditorTooltip.Instance != null)
                //    EditorTooltip.Instance.HideToolTip();
            }
            else
            {
                editor.Unlock("BD_MN_GUILock");
            }
            guiWindowRect = GUILayout.Window(GetInstanceID(), guiWindowRect, GUIWindow, "Landing Coordinates", Styles.styleEditorPanel);
        }

        public void GUIWindow(int windowID)
        {
            InitializeStyles();

            GUILayout.BeginVertical();
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();

            GUILayout.Label("Latitude: ");
            latitude = GUILayout.TextField(latitude);

            GUILayout.Label("Longitude: ");
            longitude = GUILayout.TextField(longitude);

            if (GUILayout.Button("Save & Close"))
            {
                core.LandingLatitude = float.Parse(latitude);
                core.LandingLongitude = float.Parse(longitude);
                HideGUI();
            }

            GUILayout.EndHorizontal();

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            GUI.DragWindow();
            RepositionWindow(ref guiWindowRect);
        }
        internal static void RepositionWindow(ref Rect windowPosition)
        {
            // This method uses Gui point system.
            if (windowPosition.x < 0) windowPosition.x = 0;
            if (windowPosition.y < 0) windowPosition.y = 0;

            if (windowPosition.xMax > Screen.width)
                windowPosition.x = Screen.width - windowPosition.width;
            if (windowPosition.yMax > Screen.height)
                windowPosition.y = Screen.height - windowPosition.height;
        }
        private static void InitializeStyles()
        {
            if (unchanged == null)
            {
                if (GUI.skin == null)
                {
                    unchanged = new GUIStyle();
                    changed = new GUIStyle();
                    greyed = new GUIStyle();
                    overfull = new GUIStyle();
                }
                else
                {
                    unchanged = new GUIStyle(GUI.skin.textField);
                    changed = new GUIStyle(GUI.skin.textField);
                    greyed = new GUIStyle(GUI.skin.textField);
                    overfull = new GUIStyle(GUI.skin.label);
                }

                unchanged.normal.textColor = Color.white;
                unchanged.active.textColor = Color.white;
                unchanged.focused.textColor = Color.white;
                unchanged.hover.textColor = Color.white;

                changed.normal.textColor = Color.yellow;
                changed.active.textColor = Color.yellow;
                changed.focused.textColor = Color.yellow;
                changed.hover.textColor = Color.yellow;

                greyed.normal.textColor = Color.gray;

                overfull.normal.textColor = Color.red;
            }
        }
    }

    internal class Styles
    {
        // Base styles
        public static GUIStyle styleEditorTooltip;
        public static GUIStyle styleEditorPanel;

        /// <summary>
        ///     This one sets up the styles we use
        /// </summary>
        internal static void InitStyles()
        {
            styleEditorTooltip = new GUIStyle();
            styleEditorTooltip.name = "Tooltip";
            styleEditorTooltip.fontSize = 12;
            styleEditorTooltip.normal.textColor = new Color32(207, 207, 207, 255);
            styleEditorTooltip.stretchHeight = true;
            styleEditorTooltip.wordWrap = true;
            styleEditorTooltip.normal.background = CreateColorPixel(new Color32(7, 54, 66, 200));
            styleEditorTooltip.border = new RectOffset(3, 3, 3, 3);
            styleEditorTooltip.padding = new RectOffset(4, 4, 6, 4);
            styleEditorTooltip.alignment = TextAnchor.MiddleLeft;

            styleEditorPanel = new GUIStyle();
            styleEditorPanel.normal.background = CreateColorPixel(new Color32(7, 54, 66, 200));
            styleEditorPanel.border = new RectOffset(27, 27, 27, 27);
            styleEditorPanel.padding = new RectOffset(10, 10, 10, 10);
            styleEditorPanel.normal.textColor = new Color32(147, 161, 161, 255);
            styleEditorPanel.fontSize = 12;
        }

        /// <summary>
        ///     Creates a 1x1 texture
        /// </summary>
        /// <param name="Background">Color of the texture</param>
        /// <returns></returns>
        internal static Texture2D CreateColorPixel(Color32 Background)
        {
            Texture2D retTex = new Texture2D(1, 1);
            retTex.SetPixel(0, 0, Background);
            retTex.Apply();
            return retTex;
        }
    }
}
