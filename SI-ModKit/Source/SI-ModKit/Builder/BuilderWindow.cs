using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SIModKit
{
    /// <summary>Milestone 0.26: release-candidate documentation and integration guidance.</summary>
    internal static class BuilderWindow
    {
        private sealed class ControlSpec
        {
            public string Id;
            public string Kind;
            public string Title;
            public string Placeholder;
            public string Value;
            public string ActionName;
            public string ActionMessage;
        }

        // Project documents are the source of truth for preview, persistence,
        // portable exchange, generated source, undo/redo, and named project slots.
        private sealed class WindowDocument
        {
            public string Id;
            public string Title;
            public int Width;
            public int Height;
            public bool Resizable;
            public bool Movable;
            public bool Closable;
            public bool ToolbarMode;
            public int ContentMargin;
            public readonly List<ControlSpec> Controls = new List<ControlSpec>();
        }

        private sealed class ProjectSnapshot
        {
            public string Id;
            public string Name;
            public int CurrentWindow;
            public int NextWindowId;
            public int NextControlId;
            public readonly List<string> WindowIds = new List<string>();
        }

        private const int VisibleRows = 8;
        private static UIWindow editor;
        private static UIWindow preview;
        // Integer view identifiers avoid the game compiler enum-field metadata error.
        private const int ViewHierarchy = 0;
        private const int ViewProperties = 1;
        private const int ViewSettings = 2;
        private const int ViewCode = 3;
        private const int ViewClearConfirm = 4;
        private const int ViewWiki = 5;
        private const int ViewDiagnostics = 6;
        private const int ViewCapacity = 7;
        private const int ViewProject = 8;
        private const int ViewDeleteWindowConfirm = 9;
        private const int ViewOpenProjectConfirm = 10;
        private const int ViewExportProject = 11;
        private const int ViewImportProject = 12;
        private const int ViewImportConfirm = 13;
        private const int ViewProjectSlots = 14;
        private const int ViewProjectTemplates = 15;
        private const int ViewLoadNamedProjectConfirm = 16;
        private const int ViewCreateProjectConfirm = 17;
        private const string PortableProjectSignature = "SIMODKIT-PORTABLE-PROJECT-1";
        private const string ProjectIndexKey = "SIModKit.Project.Index.v1";
        private const string ProjectSlotsKey = "SIModKit.Project.Slots.v1";
        private const string ProjectSerialKey = "SIModKit.Project.NextSerial.v1";
        private const string ProjectKeyPrefix = "SIModKit.Project.";
        private const int MaxWindowChunkCharacters = 262144;
        private static string storageResult = "Not tested";
        private static string interopResult = "Not tested";
        private static string capacityResult = "Not tested";
        private static string persistenceStatus = "Not saved yet";
        private static bool projectDirty = true;
        private static bool currentProjectBackedBySavedSlot = false;
        private static string pendingProjectLoadId = "";
        private static int pendingProjectTemplate = 0;
        private static int wikiPage = 0;
        private static bool previewResizable = true;
        private static bool previewMovable = true;
        private static bool previewClosable = true;
        private static bool previewToolbarMode = false;
        private static int previewContentMargin = 14;
        private static InputField generated;
        private static InputField portableProjectArea;
        private static InputField importProjectArea;
        private static bool generatedWholeProject;
        private static string importBuffer = "";
        private static string importStatus = "Paste a complete SI ModKit portable project package below.";
        private static ProjectSnapshot pendingImportSnapshot;
        private static List<WindowDocument> pendingImportWindows;
        private static Button resizableButton;
        private static Button movableButton;
        private static Button closableButton;
        private static InputField titleEditor;
        private static InputField hintEditor;
        private static InputField valueEditor;
        private static InputField actionEditor;
        private static InputField actionMessageEditor;
        private static InputField windowTitleEditor;
        private static InputField windowWidthEditor;
        private static InputField windowHeightEditor;
        private static InputField windowMarginEditor;
        private static Button toolbarModeButton;
        private static InputField projectNameEditor;
        private static Text projectStatus;
        private static readonly Button[] projectWindowButtons = new Button[6];
        private static int projectWindowPage;
        private static Text validationStatus;
        private static string previewTitle = "My Mod Window";
        private static int previewWidth = 560;
        private static int previewHeight = 460;
        private static Text selectionStatus;
        private static Text hierarchyStatus;
        private static Text layoutContextStatus;
        private static readonly Button[] hierarchyButtons = new Button[VisibleRows];
        private static readonly List<ControlSpec> controls = new List<ControlSpec>();
        private static readonly Dictionary<string, InputField> previewInputs = new Dictionary<string, InputField>();
        private static readonly Dictionary<string, GUICombobox> previewDropdowns = new Dictionary<string, GUICombobox>();
        private static readonly Dictionary<string, Slider> previewSliders = new Dictionary<string, Slider>();
        private static readonly Dictionary<string, Toggle> previewToggles = new Dictionary<string, Toggle>();
        private static readonly Dictionary<string, GUIListView> previewLists = new Dictionary<string, GUIListView>();
        private static readonly Dictionary<string, GUIListView> previewTables = new Dictionary<string, GUIListView>();
        private static readonly List<WindowDocument> windows = new List<WindowDocument>();
        private static string projectId = "project-1";
        private static string projectName = "My Mod Project";
        private static int currentWindow = -1;
        private static int nextWindowId = 1;
        private static int nextControlId = 1;
        private static int selected = -1;
        private static int page;
        private static readonly List<string> undoHistory = new List<string>();
        private static readonly List<string> redoHistory = new List<string>();
        private static bool restoringHistory;
        private const int MaxHistoryEntries = 25;

        public static void Toggle()
        {
            if (editor != null && editor.NativeWindow != null)
            {
                if (editor.NativeWindow.Shown) editor.Hide(); else editor.Show();
                return;
            }
            Build();
        }

        // All editing screens share the same native GUIWindow. ClearContent changes
        // the active view without respawning (and therefore recentering) the editor.
        private static void Build()
        {
            EnsureProject();
            editor = UI.CreateWindow("SI ModKit | UI Builder", 650, 748);
            editor.SetResizable(false);
            ShowView(ViewHierarchy);
            editor.Show();
        }

        private static void ShowView(int next)
        {
            if (editor == null || editor.NativeWindow == null) return;
            editor.ClearContent();
            generated = null;
            portableProjectArea = null;
            importProjectArea = null;
            titleEditor = null;
            hintEditor = null;
            valueEditor = null;
            actionEditor = null;
            actionMessageEditor = null;
            windowTitleEditor = null;
            windowWidthEditor = null;
            windowHeightEditor = null;
            windowMarginEditor = null;
            toolbarModeButton = null;
            projectNameEditor = null;
            projectStatus = null;
            resizableButton = null;
            movableButton = null;
            closableButton = null;
            validationStatus = null;
            selectionStatus = null;
            hierarchyStatus = null;
            layoutContextStatus = null;
            for (int i = 0; i < hierarchyButtons.Length; i++) hierarchyButtons[i] = null;
            for (int i = 0; i < projectWindowButtons.Length; i++) projectWindowButtons[i] = null;

            if (next == ViewHierarchy)
            {
                EnsureProject();
                editor.AddHeading("UI Builder | " + previewTitle + " | control hierarchy");
                validationStatus = editor.AddLabel("Layout: valid");
                layoutContextStatus = editor.AddLabel("Current layout: window root (no closing marker needed)");
                Button[] basicPalette = editor.AddButtonRow(
                    new[] { "Txt", "I_", "[ ]", "Btn", "v", "<->" },
                    new Action[] { () => Add("Label"), () => Add("TextBox"), () => Add("Checkbox"), () => Add("Button"), () => Add("Dropdown"), () => Add("Slider") });
                ConfigurePaletteButton(basicPalette[0], "Label / wrapped text", new Color(0.83f, 0.88f, 0.96f, 1f));
                ConfigurePaletteButton(basicPalette[1], "Text box / input field", new Color(0.83f, 0.88f, 0.96f, 1f));
                ConfigurePaletteButton(basicPalette[2], "Check box", new Color(0.86f, 0.93f, 0.84f, 1f));
                ConfigurePaletteButton(basicPalette[3], "Button / action", new Color(0.86f, 0.93f, 0.84f, 1f));
                ConfigurePaletteButton(basicPalette[4], "Dropdown / combobox", new Color(0.94f, 0.89f, 0.78f, 1f));
                ConfigurePaletteButton(basicPalette[5], "Slider", new Color(0.94f, 0.89f, 0.78f, 1f));

                Button[] extendedPalette = editor.AddButtonRow(
                    new[] { "#", "¶", "List", "Tbl", "?", "%" },
                    new Action[] { () => Add("Numeric"), () => Add("TextArea"), () => Add("List"), () => Add("Table"), () => Add("ConfirmButton"), () => Add("ProgressBar") });
                ConfigurePaletteButton(extendedPalette[0], "Numeric input", new Color(0.83f, 0.88f, 0.96f, 1f));
                ConfigurePaletteButton(extendedPalette[1], "Multiline text area", new Color(0.83f, 0.88f, 0.96f, 1f));
                ConfigurePaletteButton(extendedPalette[2], "Selectable list", new Color(0.88f, 0.84f, 0.96f, 1f));
                ConfigurePaletteButton(extendedPalette[3], "Data table", new Color(0.88f, 0.84f, 0.96f, 1f));
                ConfigurePaletteButton(extendedPalette[4], "Confirmation button", new Color(0.86f, 0.93f, 0.84f, 1f));
                ConfigurePaletteButton(extendedPalette[5], "Progress bar", new Color(0.94f, 0.89f, 0.78f, 1f));

                Button[] layoutPalette = editor.AddButtonRow(new[] { "Row +", "Row -", "Section +", "Section -" },
                    new Action[] { () => Add("RowStart"), () => Add("RowEnd"), () => Add("SectionStart"), () => Add("SectionEnd") });
                for (int i = 0; i < layoutPalette.Length; i++)
                    ConfigurePaletteButton(layoutPalette[i], i < 2 ? (i == 0 ? "Begin responsive row/grid (column count editable in Properties)" : "End current row") : (i == 2 ? "Begin vertical section" : "End current section"),
                        i < 2 ? new Color(0.72f, 0.86f, 0.74f, 1f) : new Color(0.70f, 0.82f, 0.93f, 1f));
                hierarchyStatus = editor.AddLabel("Controls");
                for (int i = 0; i < VisibleRows; ++i)
                {
                    int slot = i;
                    hierarchyButtons[i] = editor.AddButton("(empty)", () => Select(page * VisibleRows + slot));
                }
                editor.AddButtonRow(new[] { "Undo", "Redo", "Previous page", "Next page" },
                    new Action[] { Undo, Redo, () => SetPage(page - 1), () => SetPage(page + 1) });
                editor.AddButtonRow(new[] { "Preview / Refresh", "Window settings", "Generated C#" },
                    new Action[] { RefreshPreview, ShowSettings, ShowCode });
                Button[] bottomButtons = editor.AddButtonRow(
                    new[] { "Project / Windows", "Wiki / How-to", "Clear all..." },
                    new Action[] { ShowProject, ShowWiki, ShowClearConfirmation });
                TintButton(bottomButtons[0], new Color(0.76f, 0.76f, 0.92f, 1f));
                TintButton(bottomButtons[1], new Color(0.66f, 0.80f, 0.91f, 1f));
                TintButton(bottomButtons[2], new Color(0.72f, 0.55f, 0.49f, 1f));
            }
            else if (next == ViewProperties)
            {
                bool hasSelection = selected >= 0 && selected < controls.Count;
                editor.AddHeading(hasSelection
                    ? "Properties | " + (selected + 1) + ". " + controls[selected].Kind
                    : "Properties | no control selected");
                editor.AddButton("< Back to hierarchy", () => ShowView(ViewHierarchy));
                titleEditor = editor.AddTextBox("Label / caption");
                string selectedKind = hasSelection ? controls[selected].Kind : "";
                bool isRange = selectedKind == "Slider" || selectedKind == "ProgressBar" || selectedKind == "Numeric";
                bool isOptions = selectedKind == "Dropdown" || selectedKind == "List";
                bool isTable = selectedKind == "Table";
                bool isTextArea = selectedKind == "TextArea";
                bool isConfirm = selectedKind == "ConfirmButton";
                bool isCheckbox = selectedKind == "Checkbox";
                string hintPrompt = isOptions ? "Items/options, separated by commas" : isTable ? "Column names, separated by commas" : isRange ? "Minimum,Maximum (example: 0,100)" : isConfirm ? "Confirmation message" : "Placeholder";
                hintEditor = editor.AddTextBox(hintPrompt);
                if (isTextArea)
                    valueEditor = editor.AddTextArea("Initial multiline text", "", 88f);
                else if (isTable)
                    valueEditor = editor.AddTextArea("Rows: one row per line; cells separated by |", "", 88f);
                else
                    valueEditor = editor.AddTextBox(isCheckbox ? "Initial checked: true or false" : isOptions ? "Initial selected item" : isRange ? "Initial numeric value" : isConfirm ? "Confirm button label" : "Initial value");
                if (hasSelection && selectedKind == "Dropdown")
                    editor.AddLabel("Dropdown: caption is optional. Enter options separated by commas; initial selection should match one option.");
                if (hasSelection && selectedKind == "List")
                    editor.AddLabel("List: caption is optional. Enter comma-separated items and an optional initial selected item.");
                if (hasSelection && isTable)
                    editor.AddLabel("Table: caption is optional. Columns use commas; rows use one line each with | between cells.");
                if (hasSelection && isRange)
                    editor.AddLabel((selectedKind == "Numeric" ? "Numeric input" : selectedKind == "Slider" ? "Slider" : "Progress") + ": caption is optional. Enter min,max and an initial numeric value.");
                if (hasSelection && isConfirm)
                    editor.AddLabel("Confirmation button: caption is the visible button text; placeholder is the dialog message.");
                bool supportsAction = hasSelection && (selectedKind == "Button" || selectedKind == "ConfirmButton" || selectedKind == "TextBox" || selectedKind == "TextArea" || selectedKind == "Numeric" || selectedKind == "Checkbox" || selectedKind == "Dropdown" || selectedKind == "Slider" || selectedKind == "List" || selectedKind == "Table");
                if (supportsAction)
                {
                    editor.AddLabel((selectedKind == "Button" || selectedKind == "ConfirmButton" ? "Click" : "Change / selection") + " action | use a short name such as SaveSettings");
                    actionEditor = editor.AddTextBox("Action name (blank = no action)");
                    editor.AddLabel("Preview / generated action message (optional)");
                    actionMessageEditor = editor.AddTextBox("Message to log when action runs");
                }
                editor.AddButton("Apply properties", ApplyProperties);
                editor.AddButtonRow(new[] { "Move up", "Move down", "Duplicate" },
                    new Action[] { () => MoveSelected(-1), () => MoveSelected(1), DuplicateSelected });
                editor.AddButton("Remove selected control", RemoveSelected);
                editor.AddLabel("Actions affect only the control identified above.");
            }
            else if (next == ViewSettings)
            {
                editor.AddHeading("Window settings | preview window");
                editor.AddButton("< Back to hierarchy", () => ShowView(ViewHierarchy));
                windowTitleEditor = editor.AddTextBox("Window title");
                windowWidthEditor = editor.AddTextBox("Width in pixels");
                windowHeightEditor = editor.AddTextBox("Height in pixels");
                windowMarginEditor = editor.AddTextBox("Equal left/right content margin (4-64 px)");
                movableButton = editor.AddButton("", ToggleMovable);
                resizableButton = editor.AddButton("", ToggleResizable);
                closableButton = editor.AddButton("", ToggleClosable);
                toolbarModeButton = editor.AddButton("", ToggleToolbarMode);
                UpdateMovableLabel();
                UpdateResizableLabel();
                UpdateClosableLabel();
                UpdateToolbarModeLabel();
                editor.AddButton("Apply window settings", ApplyWindowSettings);
                editor.AddLabel("Presentation is independent from behavior. Toolbar mode keeps compact chrome; Movable, Resizable and Closable are controlled separately.");
            }
            else if (next == ViewClearConfirm)
            {
                editor.AddHeading("Clear all controls | confirmation");
                editor.AddLabel("This will remove all " + controls.Count + " controls and layout markers.");
                editor.AddLabel("The current design cannot be restored after clearing.");
                editor.AddButtonRow(new[] { "Cancel - keep design", "Yes, clear everything" },
                    new Action[] { () => ShowView(ViewHierarchy), ClearConfirmed });
            }
            else if (next == ViewWiki)
            {
                ShowWikiContent();
            }
            else if (next == ViewDiagnostics)
            {
                editor.AddHeading("System tests | Alpha 0.13.5");
                editor.AddButton("< Back to hierarchy", () => ShowView(ViewHierarchy));
                editor.AddLabel("Game-managed storage: saves one diagnostic value using the supported mod settings API.");
                editor.AddButton("Save + verify setting", RunStorageProbe);
                editor.AddLabel(storageResult);
                editor.AddLabel("After restarting Software Inc., check that the diagnostic setting still exists.");
                editor.AddButton("Check saved setting", RunInteropProbe);
                editor.AddLabel(interopResult);
                editor.AddButton("Storage capacity test...", ShowCapacityTests);
                editor.AddLabel("Full results are also written to the Unity log. See docs/ALPHA-13-5-CAPACITY-TEST.md");
            }
            else if (next == ViewCapacity)
            {
                editor.AddHeading("Storage capacity | safe floor test");
                editor.AddButton("< Back to system tests", () => ShowView(ViewDiagnostics));
                editor.AddLabel("Tests one setting at 1 KB, 8 KB, 32 KB, 128 KB, 512 KB, then 1 MB.");
                editor.AddLabel("The same key is overwritten at each step; this is not a maximum-size stress test.");
                editor.AddButton("Run capacity-floor test", RunCapacityProbe);
                editor.AddLabel(capacityResult);
                editor.AddButton("Verify capacity after restart", VerifyCapacityProbe);
                editor.AddButton("Clear capacity test data", ClearCapacityProbe);
                editor.AddLabel("After restart verification, clear the test data to keep SI-ModKitSetting.txt small.");
            }
            else if (next == ViewProject)
            {
                EnsureProject();
                CaptureActiveWindow();
                editor.AddHeading("Project | " + projectName);
                editor.AddButton("< Back to active window", () => ShowView(ViewHierarchy));
                projectNameEditor = editor.AddTextBox("Project name");
                projectNameEditor.text = projectName;
                editor.AddButton("Apply project name", ApplyProjectName);
                projectStatus = editor.AddLabel("Project ID: " + projectId + " | windows: " + windows.Count + " | active: " + (currentWindow + 1));
                int first = projectWindowPage * projectWindowButtons.Length;
                for (int i = 0; i < projectWindowButtons.Length; i++)
                {
                    int index = first + i;
                    int slot = i;
                    string label = index < windows.Count
                        ? (index == currentWindow ? "> " : "") + (index + 1) + ". " + windows[index].Title
                        : "(empty)";
                    projectWindowButtons[i] = editor.AddButton(label, () => SelectProjectWindow(projectWindowPage * projectWindowButtons.Length + slot));
                    if (index >= windows.Count) projectWindowButtons[i].interactable = false;
                    else if (index == currentWindow) TintButton(projectWindowButtons[i], new Color(0.72f, 0.88f, 0.66f, 1f));
                }
                editor.AddButtonRow(new[] { "Previous windows", "Next windows" },
                    new Action[] { () => SetProjectWindowPage(projectWindowPage - 1), () => SetProjectWindowPage(projectWindowPage + 1) });
                editor.AddButtonRow(new[] { "New window", "Duplicate active", "Delete active..." },
                    new Action[] { CreateProjectWindow, DuplicateProjectWindow, RequestDeleteProjectWindow });
                editor.AddButtonRow(new[] { "Save project", "Saved projects...", "New / template..." },
                    new Action[] { SaveProject, () => ShowView(ViewProjectSlots), () => ShowView(ViewProjectTemplates) });
                editor.AddButtonRow(new[] { "Export project", "Import project...", "Whole-project C#" },
                    new Action[] { ShowExportProject, ShowImportProject, ShowWholeProjectCode });
                editor.AddLabel("Current: " + projectName + " | " + projectId + " | " + (projectDirty ? "UNSAVED" : "saved") + " | internal projects: " + GetSavedProjectIds().Count);
                editor.AddLabel("Saved projects manages named internal projects. Export / Import remains the portable backup/share format.");
            }
            else if (next == ViewProjectSlots)
            {
                editor.AddHeading("Saved projects");
                editor.AddButton("< Back to project", () => ShowView(ViewProject));
                editor.AddLabel("Select a saved project to open it. Unsaved current work will require confirmation.");
                List<string> ids = GetSavedProjectIds();
                if (ids.Count == 0) editor.AddLabel("No internal projects saved yet.");
                for (int i = 0; i < 6; i++)
                {
                    if (i < ids.Count)
                    {
                        string id = ids[i];
                        string name = GetSavedProjectName(id);
                        editor.AddButton((id == projectId && currentProjectBackedBySavedSlot ? "> " : "") + name + " | " + id, () => RequestLoadProjectById(id));
                    }
                    else editor.AddLabel("(empty project slot)");
                }
                editor.AddLabel("Status: " + persistenceStatus);
            }
            else if (next == ViewProjectTemplates)
            {
                editor.AddHeading("New project / templates");
                editor.AddButton("< Back to project", () => ShowView(ViewProject));
                editor.AddLabel("Create a new in-memory project. Save it when you want it added to Saved Projects.");
                editor.AddButton("New blank project", () => RequestCreateProjectFromTemplate(0));
                editor.AddButton("Settings template", () => RequestCreateProjectFromTemplate(1));
                editor.AddButton("Toolbar template", () => RequestCreateProjectFromTemplate(2));
                editor.AddLabel("Status: " + persistenceStatus);
            }
            else if (next == ViewLoadNamedProjectConfirm)
            {
                editor.AddHeading("Open saved project | confirmation");
                editor.AddLabel("Current project has unsaved changes.");
                editor.AddLabel("Open: " + GetSavedProjectName(pendingProjectLoadId));
                editor.AddLabel("Opening it will discard the current in-memory changes.");
                editor.AddButtonRow(new[] { "Cancel", "Discard changes + open" },
                    new Action[] { () => ShowView(ViewProjectSlots), ConfirmLoadNamedProject });
            }
            else if (next == ViewCreateProjectConfirm)
            {
                editor.AddHeading("Create new project | confirmation");
                editor.AddLabel("Current project has unsaved changes.");
                editor.AddLabel("Creating another project will discard those in-memory changes.");
                editor.AddButtonRow(new[] { "Cancel", "Discard changes + create" },
                    new Action[] { () => ShowView(ViewProjectTemplates), ConfirmCreateProjectFromTemplate });
            }
            else if (next == ViewDeleteWindowConfirm)
            {
                EnsureProject();
                CaptureActiveWindow();
                editor.AddHeading("Delete window | confirmation");
                if (windows.Count <= 1)
                {
                    editor.AddLabel("A project must contain at least one window. Clear its controls instead if needed.");
                    editor.AddButton("< Back to project", () => ShowView(ViewProject));
                }
                else
                {
                    editor.AddLabel("Delete window: " + windows[currentWindow].Title);
                    editor.AddLabel("This removes the window and all controls in it for the current session.");
                    editor.AddButtonRow(new[] { "Cancel", "Yes, delete window" },
                        new Action[] { () => ShowView(ViewProject), DeleteProjectWindowConfirmed });
                }
            }
            else if (next == ViewOpenProjectConfirm)
            {
                editor.AddHeading("Open saved project | confirmation");
                editor.AddLabel("The current project has unsaved changes.");
                editor.AddLabel("Opening the saved project will discard those in-memory changes.");
                editor.AddButtonRow(new[] { "Cancel - keep current", "Open saved project" },
                    new Action[] { () => ShowView(ViewProject), LoadProject });
            }
            else if (next == ViewExportProject)
            {
                EnsureProject();
                CaptureActiveWindow();
                editor.AddHeading("Export project | portable text");
                editor.AddButton("< Back to project", () => ShowView(ViewProject));
                editor.AddLabel("Copy the complete package below. It contains every window, control, layout marker, action, and project setting.");
                editor.AddLabel("The package is independent of SI ModKit's internal saved-project slot and can be pasted into Import Project later.");
                portableProjectArea = editor.AddStretchTextArea("Portable project package", 154f, 16f);
                portableProjectArea.readOnly = true;
                portableProjectArea.text = BuildPortableProjectPackage();
            }
            else if (next == ViewImportProject)
            {
                editor.AddHeading("Import project | portable text");
                editor.AddButton("< Back to project", () => ShowView(ViewProject));
                editor.AddLabel("Paste one complete SI ModKit portable project package. Nothing is replaced unless the entire package validates first.");
                editor.AddLabel(importStatus);
                importProjectArea = editor.AddStretchTextArea("Portable project package", 184f, 16f);
                importProjectArea.text = importBuffer;
                editor.AddButton("Validate and import", RequestPortableImport);
            }
            else if (next == ViewImportConfirm)
            {
                editor.AddHeading("Import project | confirmation");
                editor.AddLabel("The imported package is valid: " + (pendingImportSnapshot == null ? "(unknown)" : pendingImportSnapshot.Name));
                editor.AddLabel("Importing replaces the current in-memory project. The persisted saved slot is unchanged until you explicitly Save project.");
                if (projectDirty) editor.AddLabel("Current project has unsaved changes.");
                editor.AddButtonRow(new[] { "Cancel - keep current", "Import validated project" },
                    new Action[] { () => ShowView(ViewImportProject), CommitPortableImport });
            }
            else if (next == ViewCode)
            {
                editor.AddHeading(generatedWholeProject ? "Generated C# | entire project" : "Generated C# | active window");
                editor.AddButton("< Back to hierarchy", () => ShowView(ViewHierarchy));
                editor.AddButtonRow(new[] { "Active window", "Entire project" },
                    new Action[] { ShowCurrentWindowCode, ShowWholeProjectCode });
                editor.AddLabel(generatedWholeProject
                    ? "All project windows are included below with logical file boundaries. Copy the complete output or copy one marked window section."
                    : "This output contains only the active window. Use Entire project for all windows in the project.");
                editor.AddLabel("To use this source in your own mod, open Wiki / How-to and select Generated C# and your own mod. Check the SI ModKit Steam Workshop page for current source-kit and integration documentation.");
                generated = editor.AddStretchTextArea("Generated C#", 172f, 16f);
                generated.readOnly = true;
            }
            SyncEditor();
            RefreshCode();
        }

        // One reusable builder window: the Wiki uses a topic list and a large article area.
        private static void ShowWiki() { wikiPage = 0; ShowView(ViewWiki); }
        private static void ShowWikiContent()
        {
            string[] headings = {
                "Start here", "Builder workspace", "Projects and saving", "Windows and templates",
                "Adding controls", "Text and input controls", "Choice and value controls", "Lists, tables and confirmation",
                "Rows, sections and layout", "Editing controls", "Actions and events", "Window settings and toolbars",
                "Preview and troubleshooting", "Export and import", "Generated C# and your own mod", "Full documentation and help"
            };

            string[][] topics = {
                new[] {
                    "SI ModKit lets you design Software Inc. mod interfaces in-game without manually constructing Unity UI objects.",
                    "A normal workflow is: create a project, build one or more windows, add controls, preview the result, save the editable project, then copy generated C# into your own mod project.",
                    "START HERE: add a control from the palette, click that control in the hierarchy, change its Properties, press Apply Properties, then use Preview / Refresh.",
                    "Use Window Settings to control the active window's title, size, margins, movement, resizing, closing, and presentation.",
                    "Use Project / Windows to create additional windows, switch between them, save the project, export a portable backup, or generate the entire project's C#.",
                    "The SI ModKit project is the editable design. Generated C# is output for your mod and is not used to reopen the design."
                },
                new[] {
                    "The palette at the top adds controls and layout markers to the active window.",
                    "The hierarchy shows the order in which controls and layout markers are applied. Click any editable item to open its Properties page.",
                    "Preview / Refresh builds the active window from the current project document.",
                    "Window Settings changes only the active window. Generated C# can show either the active window or the entire project.",
                    "Project / Windows manages windows, named saved projects, templates, portable export/import, and project-level source output.",
                    "Undo and Redo are available for normal project edits. Clear All requires confirmation before removing the active window's controls."
                },
                new[] {
                    "A project is the complete editable SI ModKit design. One project can contain several independent windows.",
                    "Give the project a meaningful name before saving. The Saved Projects list uses that name to identify stored work.",
                    "Save Project stores the current project through Software Inc.'s supported mod-settings system. Saved projects survive a full game restart.",
                    "The DIRTY or UNSAVED indicator means the current project changed after its most recent save.",
                    "Opening another saved project while work is unsaved requires confirmation so the current design is not replaced accidentally.",
                    "For important work, keep both a normal saved project and a portable exported copy."
                },
                new[] {
                    "Use Project / Windows to add, select, duplicate, rename, or delete windows inside the current project.",
                    "Blank creates an empty project.",
                    "Settings creates a normal starter project intended for preferences, options, and configuration controls.",
                    "Toolbar creates a compact utility-window starter intended for quick actions, toggles, filters, or status information.",
                    "Templates are only starting points. After creation they are ordinary editable projects and can be changed freely.",
                    "Each window stores its own title, dimensions, margins, presentation, movement, resize, close behavior, controls, and actions."
                },
                new[] {
                    "Press a control symbol once to append that control to the active window.",
                    "Txt adds static text. I_ adds a single-line text input. [ ] adds a checkbox. Btn adds a button. v adds a dropdown. <-> adds a slider.",
                    "# adds numeric input. The multiline-text control adds a larger wrapped text area. List adds a selectable list. Tbl adds a simple table. ? adds a confirmation button. % adds progress.",
                    "After adding a control, click it in the hierarchy and edit the values shown in Properties.",
                    "Controls render in hierarchy order. Use Move Up, Move Down, Duplicate, and Remove Selected from the selected control's Properties page."
                },
                new[] {
                    "LABEL: use labels for headings, instructions, descriptions, status, and output text. Long labels wrap automatically and receive more vertical space.",
                    "TEXTBOX: configure an optional caption, placeholder, and initial value. Leave the caption empty when you want only the input field with no reserved label space.",
                    "NUMERIC INPUT: configure minimum, maximum, and initial value. Values are clamped to the configured range.",
                    "MULTILINE TEXT: use for notes, descriptions, larger editable text, or copy/paste content. The control wraps and scrolls.",
                    "For user-entered values, generated actions can read the current control value rather than only the initial value."
                },
                new[] {
                    "CHECKBOX: use for a yes/no setting. Configure its caption and initial checked state.",
                    "DROPDOWN: enter options as a comma-separated list such as Low,Normal,High and choose the initial selection.",
                    "SLIDER: configure minimum, maximum, and initial value for a value chosen along a range.",
                    "PROGRESS: display-only range value used for status or completion. Configure minimum, maximum, and current value.",
                    "Checkboxes, dropdowns, and sliders can be connected to named actions when the user changes their value."
                },
                new[] {
                    "LIST: use when the player should select one item from several values. The selected item can be read by a named action.",
                    "TABLE: use for simple rows of related text values such as reports, summaries, or selectable records.",
                    "CONFIRMATION BUTTON: use for destructive or important actions. Software Inc. asks the player to confirm before the named action runs.",
                    "The builder uses sample strings so you can design the interface. In your real mod, replace those samples with values from your own code or game data.",
                    "Lists and tables are intended for display and selection; generated source provides the starting point for filling them with real data."
                },
                new[] {
                    "ROW + begins a horizontal row. Edit the Row marker to choose from 1 to 8 columns. ROW - closes the active row and is required.",
                    "SECTION + begins a titled group. SECTION - closes that section and is required.",
                    "The Current Layout message tells you when you are inside a row or section and which closing marker is expected.",
                    "Close nested layouts in reverse order. For example: Section +, Row +, controls, Row -, Section -.",
                    "Controls in a row share the available width. Resolve layout warnings before relying on Preview or generated source.",
                    "Rows and sections are the supported release layout model. They keep generated code understandable and predictable."
                },
                new[] {
                    "Click a hierarchy item to edit it. The Properties page identifies exactly which control or marker is selected.",
                    "Press Apply Properties to commit values typed into the property fields.",
                    "Move Up and Move Down change hierarchy order. This can also move a control into or out of a surrounding row or section depending on marker position.",
                    "Duplicate copies the selected item and its settings. Remove Selected deletes only that item.",
                    "Undo can recover normal editing mistakes such as accidental moves, removes, duplicates, or property changes."
                },
                new[] {
                    "A named action is a reusable identifier such as SaveSettings, ApplyFilter, OpenDetails, or RefreshReport.",
                    "Buttons invoke their named action when clicked. Confirmation buttons ask first, then invoke the action after approval.",
                    "Text, numeric, checkbox, dropdown, slider, list, and table controls can invoke actions when their value or selection changes.",
                    "Generated actions receive references to live controls so your code can read what the player actually entered or selected.",
                    "When an action fires in Preview, the preview title shows the action name. This provides visible callback testing without needing Unity's console.",
                    "Generated action bodies are starting points. Replace diagnostic behavior with the real logic for your mod."
                },
                new[] {
                    "Open Window Settings to configure the active window only.",
                    "Width and Height set the requested dimensions. Content Margin controls equal left and right padding so content stays visually centered.",
                    "Movable controls whether the player can drag the window. Resizable controls the resize handle. Closable controls the close button.",
                    "Standard presentation keeps normal Software Inc. window chrome. Toolbar presentation removes most normal chrome for a compact floating utility.",
                    "Toolbar presentation does not automatically make a window immovable, non-resizable, or non-closable. Those settings are independent.",
                    "Use a fixed non-resizable window when your design depends on exact dimensions."
                },
                new[] {
                    "Preview / Refresh builds the active design and normally reuses the existing preview frame so its position is retained.",
                    "Some native Software Inc. controls can briefly size oddly on first creation. Refresh usually resolves first-registration layout timing.",
                    "If content is cut off, increase the window height, reduce unnecessary captions, or reorganize controls with rows and sections.",
                    "If an action seems inactive, verify that it has an action name and test it in Preview. The preview title should show the action name when it fires.",
                    "If the project behaves unexpectedly after an update, save a portable export before making major changes.",
                    "Check the SI ModKit Steam Workshop page for current known issues, screenshots, examples, and workarounds."
                },
                new[] {
                    "Save Project stores an editable project inside Software Inc.'s supported settings system.",
                    "Export Project creates one portable text package containing every window, control, layout marker, action, and window setting in the project.",
                    "To back up or share a project, click the export field, press Ctrl+A, then Ctrl+C, and store the copied text somewhere safe.",
                    "Import Project accepts a complete exported package. SI ModKit validates it before replacing the current in-memory design.",
                    "Imported projects are marked unsaved until you explicitly save them into the internal Saved Projects list.",
                    "Portable project text is for reopening and editing a design. It is different from Generated C# source code."
                },
                new[] {
                    "Open Generated C# and choose Active Window or Entire Project.",
                    "Click inside the code field, press Ctrl+A, then Ctrl+C to copy the generated source.",
                    "Entire Project output contains all windows with clear logical file boundaries so you can separate them in your own source project if desired.",
                    "Generated code uses the SI ModKit wrapper API. Use the reusable source-kit files distributed with SI ModKit so those wrapper calls compile with your own source mod.",
                    "A normal workflow is: design and preview, save the SI ModKit project, generate entire-project C#, copy the source, add the wrapper source, then replace generated action stubs with real mod logic.",
                    "The Software Inc. source sandbox does not allow SI ModKit to write arbitrary .cs files directly to your computer, so generated source is intentionally copy/paste based."
                },
                new[] {
                    "The in-game Wiki is intended to cover the normal SI ModKit workflow without requiring you to understand its internal development history.",
                    "For the latest full documentation, screenshots, examples, source-kit information, known issues, and external links, check the SI ModKit Steam Workshop page.",
                    "The Workshop page is the public starting point for documentation outside the game. Links can be updated there without requiring another mod release.",
                    "When asking for help, include what you were trying to build, which control or window setting was involved, and the exact Software Inc. error message if one appeared.",
                    "For important projects, keep both an internal saved copy and a portable exported copy."
                }
            };

            editor.AddHeading("SI ModKit Wiki / How-to");
            editor.AddButton("< Back to hierarchy", () => ShowView(ViewHierarchy));
            editor.AddLabel("Choose a topic on the left. For screenshots, examples, source-kit information, and current documentation links, check the SI ModKit Steam Workshop page.");
            editor.AddList(headings, wikiPage, 154f, (index, value) => { wikiPage = index; ShowView(ViewWiki); });
            InputField article = editor.AddStretchTextArea(headings[wikiPage], 286f, 14f);
            article.readOnly = true;
            article.text = string.Join("\n\n", topics[wikiPage]);
        }

        private static void ShowDiagnostics() { ShowView(ViewDiagnostics); }
        private static void ShowCapacityTests() { ShowView(ViewCapacity); }
        private static void RunStorageProbe()
        {
            storageResult = EnvironmentProbe.TestLocalStorage();
            Debug.Log("[SI ModKit Storage Probe] " + storageResult);
            ShowView(ViewDiagnostics);
        }
        private static void RunInteropProbe()
        {
            interopResult = EnvironmentProbe.TestSettingAfterRestart();
            Debug.Log("[SI ModKit Restore Probe] " + interopResult);
            ShowView(ViewDiagnostics);
        }
        private static void RunCapacityProbe()
        {
            capacityResult = EnvironmentProbe.TestStorageCapacity();
            Debug.Log("[SI ModKit Capacity Probe] " + capacityResult);
            ShowView(ViewCapacity);
        }
        private static void VerifyCapacityProbe()
        {
            capacityResult = EnvironmentProbe.TestStorageCapacityAfterRestart();
            Debug.Log("[SI ModKit Capacity Restart Probe] " + capacityResult);
            ShowView(ViewCapacity);
        }
        private static void ClearCapacityProbe()
        {
            capacityResult = EnvironmentProbe.ClearStorageCapacityTest();
            Debug.Log("[SI ModKit Capacity Cleanup] " + capacityResult);
            ShowView(ViewCapacity);
        }

        private static void ShowInspector() { ShowView(ViewProperties); }
        private static void ShowSettings() { ShowView(ViewSettings); }
        private static void ShowCode() { generatedWholeProject = false; ShowView(ViewCode); }
        private static void ShowCurrentWindowCode() { generatedWholeProject = false; ShowView(ViewCode); }
        private static void ShowWholeProjectCode() { CaptureActiveWindow(); generatedWholeProject = true; ShowView(ViewCode); }
        private static void ShowExportProject() { CaptureActiveWindow(); ShowView(ViewExportProject); }
        private static void ShowImportProject() { importStatus = "Paste a complete SI ModKit portable project package below."; ShowView(ViewImportProject); }
        private static void ShowProject() { CaptureActiveWindow(); ShowView(ViewProject); }

        private static void ConfigurePaletteButton(Button button, string tooltip, Color tint)
        {
            if (button == null) return;
            TintButton(button, tint);
            GUIToolTipper tip = button.GetComponent<GUIToolTipper>();
            if (tip == null) tip = button.gameObject.AddComponent<GUIToolTipper>();
            tip.Localize = false;
            tip.TryLoc = false;
            tip.OnlyLocalizeDescription = false;
            tip.ToolTipValue = tooltip;
            tip.TooltipDescription = "";
        }

        private static string GetSavedSlotDescription()
        {
            ModKitRuntime owner = ModKitRuntime.ActiveRuntime;
            if (owner == null || owner.ParentMod == null) return "not available (mod runtime not initialized)";
            try
            {
                string savedId = owner.LoadSetting(ProjectIndexKey, "");
                if (string.IsNullOrEmpty(savedId)) return "(empty)";
                ProjectSnapshot snapshot;
                if (!TryDeserializeManifest(owner.LoadSetting(ManifestKey(savedId), ""), out snapshot) || snapshot == null)
                    return "unreadable project " + savedId;
                return snapshot.Name + " | " + snapshot.WindowIds.Count + " window" + (snapshot.WindowIds.Count == 1 ? "" : "s") + " | " + snapshot.Id;
            }
            catch (Exception ex)
            {
                return "unavailable: " + ex.Message;
            }
        }

        private static void UpdateMovableLabel()
        {
            if (movableButton == null) return;
            Text label = movableButton.GetComponentInChildren<Text>();
            if (label != null) label.text = "Player can move window: " + (previewMovable ? "YES" : "NO") + " (click to toggle)";
        }

        private static void UpdateResizableLabel()
        {
            if (resizableButton == null) return;
            Text label = resizableButton.GetComponentInChildren<Text>();
            if (label != null) label.text = "Player can resize window: " + (previewResizable ? "YES" : "NO") + " (click to toggle)";
        }

        private static void UpdateClosableLabel()
        {
            if (closableButton == null) return;
            Text label = closableButton.GetComponentInChildren<Text>();
            if (label != null) label.text = "Player can close window: " + (previewClosable ? "YES" : "NO") + " (click to toggle)";
        }

        private static void ToggleMovable()
        {
            RecordUndoPoint();
            previewMovable = !previewMovable;
            MarkDirty();
            UpdateMovableLabel();
            if (preview != null && preview.NativeWindow != null) preview.SetMovable(previewMovable);
            CaptureActiveWindow();
            RefreshCode();
        }

        private static void ToggleResizable()
        {
            RecordUndoPoint();
            previewResizable = !previewResizable;
            MarkDirty();
            UpdateResizableLabel();
            if (preview != null && preview.NativeWindow != null) preview.SetResizable(previewResizable);
            CaptureActiveWindow();
            RefreshCode();
        }

        private static void ToggleClosable()
        {
            RecordUndoPoint();
            previewClosable = !previewClosable;
            MarkDirty();
            UpdateClosableLabel();
            if (preview != null && preview.NativeWindow != null) preview.SetClosable(previewClosable);
            CaptureActiveWindow();
            RefreshCode();
        }

        private static void UpdateToolbarModeLabel()
        {
            if (toolbarModeButton == null) return;
            Text label = toolbarModeButton.GetComponentInChildren<Text>();
            if (label != null) label.text = "Window style: " + (previewToolbarMode ? "FLOATING TOOLBAR" : "STANDARD WINDOW") + " (click to toggle)";
        }

        private static void ToggleToolbarMode()
        {
            RecordUndoPoint();
            previewToolbarMode = !previewToolbarMode;
            if (!previewToolbarMode)
            {
                previewWidth = Math.Max(320, previewWidth);
                previewHeight = Math.Max(240, previewHeight);
            }
            MarkDirty();
            UpdateToolbarModeLabel();
            if (preview != null && preview.NativeWindow != null)
            {
                preview.SetToolbarMode(previewToolbarMode);
                preview.SetSize(previewWidth, previewHeight);
                preview.SetMovable(previewMovable);
                preview.SetResizable(previewResizable);
                preview.SetClosable(previewClosable);
            }
            CaptureActiveWindow();
            SyncEditor();
            RefreshCode();
            RefreshPreview();
        }

        private static int GetRowColumns(ControlSpec control)
        {
            if (control == null) return 2;
            string text = control.Title ?? "";
            int number = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] >= '0' && text[i] <= '9') number = number * 10 + (text[i] - '0');
                else if (number > 0) break;
            }
            return Math.Max(1, Math.Min(8, number <= 0 ? 2 : number));
        }

        private static void Add(string kind)
        {
            int number = controls.Count + 1;
            RecordUndoPoint();
            MarkDirty();
            controls.Add(new ControlSpec
            {
                Id = "control-" + nextControlId++,
                Kind = kind,
                Title = kind == "RowStart" ? "2 columns" : kind == "SectionStart" ? "New section" : kind == "ConfirmButton" ? "Confirm" : kind + " " + number,
                Placeholder = kind == "TextBox" ? "Enter text" : kind == "TextArea" ? "Enter multiple lines" : kind == "Dropdown" ? "Option 1,Option 2,Option 3" : kind == "List" ? "Item 1,Item 2,Item 3" : kind == "Table" ? "Name,Value" : kind == "ConfirmButton" ? "Are you sure?" : (kind == "Slider" || kind == "ProgressBar" || kind == "Numeric") ? "0,100" : "",
                Value = kind == "Dropdown" ? "Option 1" : kind == "List" ? "Item 1" : kind == "Table" ? "Alpha|1\nBeta|2" : kind == "ConfirmButton" ? "Confirm" : kind == "Checkbox" ? "false" : (kind == "Slider" || kind == "ProgressBar" || kind == "Numeric") ? "50" : "",
                ActionName = "",
                ActionMessage = ""
            });
            selected = controls.Count - 1;
            page = selected / VisibleRows;
            CaptureActiveWindow();
            SyncEditor();
            RefreshCode();
            RefreshPreview();
        }

        private static void SetPage(int newPage)
        {
            int maxPage = Math.Max(0, (controls.Count - 1) / VisibleRows);
            page = Math.Max(0, Math.Min(maxPage, newPage));
            SyncEditor();
        }

        private static void Select(int index)
        {
            if (index < 0 || index >= controls.Count) return;
            selected = index;
            ShowView(ViewProperties);
        }

        private static void SyncEditor()
        {
            int maxPage = Math.Max(0, (controls.Count - 1) / VisibleRows);
            if (windowTitleEditor != null) windowTitleEditor.text = previewTitle;
            if (windowWidthEditor != null) windowWidthEditor.text = previewWidth.ToString();
            if (windowHeightEditor != null) windowHeightEditor.text = previewHeight.ToString();
            if (windowMarginEditor != null) windowMarginEditor.text = previewContentMargin.ToString();
            UpdateMovableLabel();
            UpdateResizableLabel();
            UpdateClosableLabel();
            UpdateToolbarModeLabel();
            UpdateValidation();
            UpdateLayoutContext();
            if (page > maxPage) page = maxPage;
            if (hierarchyStatus != null)
                hierarchyStatus.text = "Control hierarchy (" + controls.Count + ") | page " + (page + 1) + "/" + (maxPage + 1);
            int[] depths = ComputeDepths();
            for (int i = 0; i < VisibleRows; i++)
            {
                Button button = hierarchyButtons[i];
                if (button == null) continue;
                int index = page * VisibleRows + i;
                bool available = index < controls.Count;
                button.interactable = available;
                Text label = button.GetComponentInChildren<Text>();
                if (label != null)
                    label.text = available
                        ? (index == selected ? "> " : "  ") + new string(' ', depths[index] * 2) + (index + 1) + ". " + DisplayKind(controls[index].Kind) + " - " + controls[index].Title
                        : "";
                if (available)
                {
                    string kind = controls[index].Kind;
                    Color tint = kind == "SectionStart" ? new Color(0.65f, 0.79f, 0.91f, 1f)
                        : kind == "SectionEnd" ? new Color(0.76f, 0.84f, 0.91f, 1f)
                        : kind == "RowStart" ? new Color(0.68f, 0.82f, 0.71f, 1f)
                        : kind == "RowEnd" ? new Color(0.79f, 0.87f, 0.79f, 1f)
                        : new Color(0.91f, 0.91f, 0.91f, 1f);
                    TintButton(button, index == selected ? Color.Lerp(tint, new Color(0.62f, 0.86f, 0.49f, 1f), 0.45f) : tint);
                }
            }
            bool hasSelection = selected >= 0 && selected < controls.Count;
            string activeKind = hasSelection ? controls[selected].Kind : "";
            bool isText = hasSelection && (activeKind == "TextBox" || activeKind == "TextArea");
            bool isDropdown = hasSelection && (activeKind == "Dropdown" || activeKind == "List");
            bool isRange = hasSelection && (activeKind == "Slider" || activeKind == "ProgressBar" || activeKind == "Numeric");
            bool isTable = hasSelection && activeKind == "Table";
            bool isConfirm = hasSelection && activeKind == "ConfirmButton";
            bool isCheckbox = hasSelection && activeKind == "Checkbox";
            bool supportsAction = hasSelection && (activeKind == "Button" || activeKind == "ConfirmButton" || activeKind == "TextBox" || activeKind == "TextArea" || activeKind == "Numeric" || activeKind == "Checkbox" || activeKind == "Dropdown" || activeKind == "Slider" || activeKind == "List" || activeKind == "Table");
            if (selectionStatus != null)
                selectionStatus.text = hasSelection
                    ? "Selected " + (selected + 1) + "/" + controls.Count + ": " + controls[selected].Kind
                    : "No control selected";
            if (titleEditor != null)
            {
                titleEditor.interactable = hasSelection;
                titleEditor.text = hasSelection ? controls[selected].Title : "";
            }
            if (hintEditor != null)
            {
                hintEditor.interactable = isText || isDropdown || isRange || isTable || isConfirm;
                hintEditor.text = (isText || isDropdown || isRange || isTable || isConfirm) ? controls[selected].Placeholder : "";
            }
            if (valueEditor != null)
            {
                valueEditor.interactable = isText || isDropdown || isRange || isTable || isConfirm || isCheckbox;
                valueEditor.text = (isText || isDropdown || isRange || isTable || isConfirm || isCheckbox) ? controls[selected].Value : "";
            }
            if (actionEditor != null)
                actionEditor.text = supportsAction ? controls[selected].ActionName : "";
            if (actionMessageEditor != null)
                actionMessageEditor.text = supportsAction ? controls[selected].ActionMessage : "";
        }

        // Markers represent a flat serialization of a nested layout tree. Report unclosed,
        // mismatched and unsupported row nesting before the user copies generated code.
        private static int[] ComputeDepths()
        {
            int[] depths = new int[controls.Count];
            int level = 0;
            for (int i = 0; i < controls.Count; i++)
            {
                string kind = controls[i].Kind;
                if (kind == "RowEnd" || kind == "SectionEnd") level = Math.Max(0, level - 1);
                depths[i] = Math.Min(level, 8);
                if (kind == "RowStart" || kind == "SectionStart") level++;
            }
            return depths;
        }

        private static string ValidateLayout()
        {
            Stack<string> open = new Stack<string>();
            int rowChildren = 0;
            for (int i = 0; i < controls.Count; i++)
            {
                string kind = controls[i].Kind;
                if (kind == "RowStart")
                {
                    if (open.Contains("RowStart")) return "Nested rows are not yet supported (item " + (i + 1) + ")";
                    open.Push(kind);
                    rowChildren = 0;
                }
                else if (kind == "SectionStart")
                {
                    if (open.Contains("RowStart")) return "Finish the row before starting a section (item " + (i + 1) + ")";
                    open.Push(kind);
                }
                else if (kind == "RowEnd" || kind == "SectionEnd")
                {
                    string expected = kind == "RowEnd" ? "RowStart" : "SectionStart";
                    if (open.Count == 0 || open.Peek() != expected)
                        return "Unmatched " + kind + " at item " + (i + 1);
                    if (kind == "RowEnd" && rowChildren > 2)
                        return "Row contains more than two controls (item " + (i + 1) + ")";
                    open.Pop();
                }
                else if (open.Contains("RowStart")) rowChildren++;
            }
            if (open.Count > 0) return "Unclosed " + open.Peek() + " marker";
            return null;
        }

        // Report the open layout stack at the insertion point, not just when a child exists.
        // This makes required closing markers visible as soon as a row/section begins.
        private static void UpdateLayoutContext()
        {
            if (layoutContextStatus == null) return;
            Stack<string> open = new Stack<string>();
            int rowItems = 0;
            for (int i = 0; i < controls.Count; i++)
            {
                string kind = controls[i].Kind;
                if (kind == "RowStart") { open.Push(kind); rowItems = 0; }
                else if (kind == "SectionStart") open.Push(kind);
                else if (kind == "RowEnd" || kind == "SectionEnd")
                {
                    string matching = kind == "RowEnd" ? "RowStart" : "SectionStart";
                    if (open.Count > 0 && open.Peek() == matching) open.Pop();
                }
                else if (open.Contains("RowStart")) rowItems++;
            }
            if (open.Count == 0)
            {
                layoutContextStatus.text = "Current layout: window root | no closing marker needed";
                return;
            }
            string current = open.Peek();
            if (current == "RowStart")
                layoutContextStatus.text = "INSIDE ROW (" + rowItems + "/2 controls) | End row REQUIRED";
            else
                layoutContextStatus.text = "INSIDE SECTION (depth " + open.Count + ") | End section REQUIRED";
        }

        private static void UpdateValidation()
        {
            if (validationStatus == null) return;
            string error = ValidateLayout();
            validationStatus.text = error == null ? "Layout: valid" : "Layout warning: " + error;
        }

        private static void ApplyWindowSettings()
        {
            int w, h, margin;
            if (windowWidthEditor == null || windowHeightEditor == null || windowMarginEditor == null ||
                !int.TryParse(windowWidthEditor.text, out w) ||
                !int.TryParse(windowHeightEditor.text, out h) ||
                !int.TryParse(windowMarginEditor.text, out margin))
            {
                if (validationStatus != null) validationStatus.text = "Window size and margin must contain whole numbers";
                return;
            }
            RecordUndoPoint();
            previewTitle = windowTitleEditor == null ? "" : windowTitleEditor.text;
            MarkDirty();
            int minWidth = previewToolbarMode ? 160 : 320;
            int minHeight = previewToolbarMode ? 80 : 240;
            previewWidth = Math.Max(minWidth, Math.Min(1600, w));
            previewHeight = Math.Max(minHeight, Math.Min(1200, h));
            previewContentMargin = Math.Max(4, Math.Min(64, margin));
            if (preview != null && preview.NativeWindow != null)
            {
                preview.SetToolbarMode(previewToolbarMode);
                preview.SetSize(previewWidth, previewHeight);
                preview.SetContentMargin(previewContentMargin);
                preview.SetMovable(previewMovable);
                preview.SetResizable(previewResizable);
                preview.SetClosable(previewClosable);
            }
            CaptureActiveWindow();
            SyncEditor();
            RefreshCode();
            RefreshPreview();
        }

        private static void ApplyProperties()
        {
            if (selected < 0 || selected >= controls.Count) return;
            RecordUndoPoint();
            MarkDirty();
            ControlSpec c = controls[selected];
            c.Title = titleEditor != null ? titleEditor.text : "";
            if (c.Kind == "TextBox" || c.Kind == "TextArea" || c.Kind == "Numeric" || c.Kind == "Checkbox" || c.Kind == "Dropdown" || c.Kind == "List" || c.Kind == "Table" || c.Kind == "Slider" || c.Kind == "ProgressBar" || c.Kind == "ConfirmButton")
            {
                if (c.Kind != "Checkbox") c.Placeholder = hintEditor != null ? hintEditor.text : "";
                c.Value = valueEditor != null ? valueEditor.text : "";
            }
            if (c.Kind == "Button" || c.Kind == "ConfirmButton" || c.Kind == "TextBox" || c.Kind == "TextArea" || c.Kind == "Numeric" || c.Kind == "Checkbox" || c.Kind == "Dropdown" || c.Kind == "Slider" || c.Kind == "List" || c.Kind == "Table")
            {
                c.ActionName = actionEditor != null ? actionEditor.text : "";
                c.ActionMessage = actionMessageEditor != null ? actionMessageEditor.text : "";
            }
            CaptureActiveWindow();
            SyncEditor();
            RefreshCode();
            RefreshPreview();
        }

        private static void RemoveSelected()
        {
            if (selected < 0 || selected >= controls.Count) return;
            RecordUndoPoint();
            MarkDirty();
            controls.RemoveAt(selected);
            selected = controls.Count == 0 ? -1 : Math.Min(selected, controls.Count - 1);
            if (selected >= 0) page = selected / VisibleRows;
            CaptureActiveWindow();
            ShowView(ViewHierarchy);
            RefreshPreview();
        }

        private static void MoveSelected(int delta)
        {
            int target = selected + delta;
            if (selected < 0 || target < 0 || target >= controls.Count) return;
            RecordUndoPoint();
            MarkDirty();
            ControlSpec item = controls[selected];
            controls.RemoveAt(selected);
            controls.Insert(target, item);
            selected = target;
            page = selected / VisibleRows;
            CaptureActiveWindow();
            ShowView(ViewProperties);
            RefreshPreview();
        }

        private static void DuplicateSelected()
        {
            if (selected < 0 || selected >= controls.Count) return;
            RecordUndoPoint();
            MarkDirty();
            ControlSpec source = controls[selected];
            controls.Insert(selected + 1, new ControlSpec
            {
                Id = "control-" + nextControlId++,
                Kind = source.Kind,
                Title = source.Title,
                Placeholder = source.Placeholder,
                Value = source.Value,
                ActionName = source.ActionName,
                ActionMessage = source.ActionMessage
            });
            selected++;
            page = selected / VisibleRows;
            CaptureActiveWindow();
            ShowView(ViewProperties);
            RefreshPreview();
        }

        private static ControlSpec CloneControl(ControlSpec source)
        {
            return new ControlSpec
            {
                Id = source.Id,
                Kind = source.Kind,
                Title = source.Title,
                Placeholder = source.Placeholder,
                Value = source.Value,
                ActionName = source.ActionName,
                ActionMessage = source.ActionMessage
            };
        }

        private static WindowDocument CloneWindow(WindowDocument source, string newId, string newTitle)
        {
            WindowDocument copy = new WindowDocument
            {
                Id = newId,
                Title = newTitle,
                Width = source.Width,
                Height = source.Height,
                Resizable = source.Resizable,
                Movable = source.Movable,
                Closable = source.Closable,
                ToolbarMode = source.ToolbarMode,
                ContentMargin = source.ContentMargin
            };
            for (int i = 0; i < source.Controls.Count; i++)
            {
                ControlSpec control = CloneControl(source.Controls[i]);
                control.Id = "control-" + nextControlId++;
                copy.Controls.Add(control);
            }
            return copy;
        }

        private static void EnsureProject()
        {
            if (windows.Count > 0 && currentWindow >= 0 && currentWindow < windows.Count) return;
            WindowDocument first = new WindowDocument
            {
                Id = "window-" + nextWindowId++,
                Title = previewTitle,
                Width = previewWidth,
                Height = previewHeight,
                Resizable = previewResizable,
                Movable = previewMovable,
                Closable = previewClosable,
                ToolbarMode = previewToolbarMode,
                ContentMargin = previewContentMargin
            };
            for (int i = 0; i < controls.Count; i++) first.Controls.Add(CloneControl(controls[i]));
            windows.Add(first);
            currentWindow = 0;
        }

        private static void CaptureActiveWindow()
        {
            EnsureProject();
            WindowDocument target = windows[currentWindow];
            target.Title = previewTitle;
            target.Width = previewWidth;
            target.Height = previewHeight;
            target.Resizable = previewResizable;
            target.Movable = previewMovable;
            target.Closable = previewClosable;
            target.ToolbarMode = previewToolbarMode;
            target.ContentMargin = previewContentMargin;
            target.Controls.Clear();
            for (int i = 0; i < controls.Count; i++) target.Controls.Add(CloneControl(controls[i]));
        }

        private static void ApplyWindowDocument(int index)
        {
            if (index < 0 || index >= windows.Count) return;
            WindowDocument source = windows[index];
            currentWindow = index;
            previewTitle = source.Title;
            previewWidth = source.Width;
            previewHeight = source.Height;
            previewResizable = source.Resizable;
            previewMovable = source.Movable;
            previewClosable = source.Closable;
            previewToolbarMode = source.ToolbarMode;
            previewContentMargin = source.ContentMargin <= 0 ? 14 : source.ContentMargin;
            controls.Clear();
            for (int i = 0; i < source.Controls.Count; i++) controls.Add(CloneControl(source.Controls[i]));
            selected = -1;
            page = 0;
        }

        private static void ApplyProjectName()
        {
            string candidate = projectNameEditor == null ? "" : projectNameEditor.text.Trim();
            if (!string.IsNullOrEmpty(candidate) && candidate != projectName)
            {
                RecordUndoPoint();
                projectName = candidate;
                MarkDirty();
            }
            ShowView(ViewProject);
        }

        // Native Software Inc. controls sometimes need a completed frame before
        // their final geometry settles. Window switches rebuild immediately, then
        private static void SelectProjectWindow(int index)
        {
            if (index < 0 || index >= windows.Count) return;
            CaptureActiveWindow();
            ApplyWindowDocument(index);
            RefreshPreview();
            ShowView(ViewHierarchy);
        }

        private static void SetProjectWindowPage(int newPage)
        {
            int maxPage = Math.Max(0, (windows.Count - 1) / projectWindowButtons.Length);
            projectWindowPage = Math.Max(0, Math.Min(maxPage, newPage));
            ShowView(ViewProject);
        }

        private static void CreateProjectWindow()
        {
            RecordUndoPoint();
            MarkDirty();
            CaptureActiveWindow();
            WindowDocument created = new WindowDocument
            {
                Id = "window-" + nextWindowId++,
                Title = "Window " + (windows.Count + 1),
                Width = 560,
                Height = 460,
                Resizable = true,
                Movable = true,
                Closable = true,
                ToolbarMode = false,
                ContentMargin = 14
            };
            windows.Add(created);
            projectWindowPage = (windows.Count - 1) / projectWindowButtons.Length;
            ApplyWindowDocument(windows.Count - 1);
            RefreshPreview();
            ShowView(ViewHierarchy);
        }

        private static void DuplicateProjectWindow()
        {
            RecordUndoPoint();
            MarkDirty();
            EnsureProject();
            CaptureActiveWindow();
            WindowDocument source = windows[currentWindow];
            WindowDocument copy = CloneWindow(source, "window-" + nextWindowId++, source.Title + " Copy");
            int insert = currentWindow + 1;
            windows.Insert(insert, copy);
            projectWindowPage = insert / projectWindowButtons.Length;
            ApplyWindowDocument(insert);
            RefreshPreview();
            ShowView(ViewHierarchy);
        }

        private static void RequestDeleteProjectWindow()
        {
            EnsureProject();
            CaptureActiveWindow();
            ShowView(ViewDeleteWindowConfirm);
        }

        private static void DeleteProjectWindowConfirmed()
        {
            if (windows.Count <= 1 || currentWindow < 0 || currentWindow >= windows.Count)
            {
                ShowView(ViewProject);
                return;
            }
            RecordUndoPoint();
            MarkDirty();
            windows.RemoveAt(currentWindow);
            if (currentWindow >= windows.Count) currentWindow = windows.Count - 1;
            projectWindowPage = currentWindow / projectWindowButtons.Length;
            ApplyWindowDocument(currentWindow);
            RefreshPreview();
            ShowView(ViewProject);
        }

        private static void RecordUndoPoint()
        {
            if (restoringHistory) return;
            EnsureProject();
            CaptureActiveWindow();
            string snapshot = BuildPortableProjectPackage();
            if (undoHistory.Count == 0 || undoHistory[undoHistory.Count - 1] != snapshot)
            {
                undoHistory.Add(snapshot);
                if (undoHistory.Count > MaxHistoryEntries) undoHistory.RemoveAt(0);
            }
            redoHistory.Clear();
        }

        private static void RestoreHistoryPackage(string package)
        {
            ProjectSnapshot snapshot;
            List<WindowDocument> loaded;
            string error;
            if (!TryParsePortableProjectPackage(package, out snapshot, out loaded, out error) || snapshot == null || loaded == null) return;
            restoringHistory = true;
            windows.Clear();
            for (int i = 0; i < loaded.Count; i++) windows.Add(loaded[i]);
            projectId = snapshot.Id;
            projectName = snapshot.Name;
            nextWindowId = snapshot.NextWindowId;
            nextControlId = snapshot.NextControlId;
            currentWindow = Math.Max(0, Math.Min(snapshot.CurrentWindow, windows.Count - 1));
            projectWindowPage = currentWindow / projectWindowButtons.Length;
            ApplyWindowDocument(currentWindow);
            projectDirty = true;
            persistenceStatus = "Modified by undo/redo";
            restoringHistory = false;
            RefreshPreview();
            ShowView(ViewHierarchy);
        }

        private static void Undo()
        {
            if (undoHistory.Count == 0) return;
            CaptureActiveWindow();
            string current = BuildPortableProjectPackage();
            string target = undoHistory[undoHistory.Count - 1];
            undoHistory.RemoveAt(undoHistory.Count - 1);
            redoHistory.Add(current);
            RestoreHistoryPackage(target);
        }

        private static void Redo()
        {
            if (redoHistory.Count == 0) return;
            CaptureActiveWindow();
            string current = BuildPortableProjectPackage();
            string target = redoHistory[redoHistory.Count - 1];
            redoHistory.RemoveAt(redoHistory.Count - 1);
            undoHistory.Add(current);
            RestoreHistoryPackage(target);
        }

        private static void MarkDirty()
        {
            projectDirty = true;
            if (persistenceStatus == "Saved") persistenceStatus = "Modified since last save";
        }

        private static List<string> GetSavedProjectIds()
        {
            List<string> ids = new List<string>();
            ModKitRuntime owner = ModKitRuntime.ActiveRuntime;
            if (owner == null || owner.ParentMod == null) return ids;
            string data = owner.LoadSetting(ProjectSlotsKey, "");
            if (!string.IsNullOrEmpty(data) && data.StartsWith("SIMODKIT-SLOTS-1|"))
            {
                string[] parts = data.Split('|');
                for (int i = 1; i < parts.Length; i++)
                {
                    string id;
                    if (TryDecodeText(parts[i], out id) && !string.IsNullOrEmpty(id) && !ids.Contains(id)) ids.Add(id);
                }
            }
            string legacy = owner.LoadSetting(ProjectIndexKey, "");
            if (!string.IsNullOrEmpty(legacy) && !ids.Contains(legacy)) ids.Add(legacy);
            return ids;
        }

        private static void SaveProjectIds(List<string> ids)
        {
            ModKitRuntime owner = ModKitRuntime.ActiveRuntime;
            if (owner == null || owner.ParentMod == null) return;
            StringBuilder sb = new StringBuilder("SIMODKIT-SLOTS-1");
            for (int i = 0; i < ids.Count; i++) sb.Append('|').Append(EncodeText(ids[i]));
            owner.SaveSetting(ProjectSlotsKey, sb.ToString());
        }

        private static string GetSavedProjectName(string id)
        {
            ModKitRuntime owner = ModKitRuntime.ActiveRuntime;
            if (owner == null || owner.ParentMod == null) return id;
            ProjectSnapshot snapshot;
            if (TryDeserializeManifest(owner.LoadSetting(ManifestKey(id), ""), out snapshot) && snapshot != null) return snapshot.Name;
            return id;
        }

        private static string NextProjectId()
        {
            ModKitRuntime owner = ModKitRuntime.ActiveRuntime;
            int serial = 1;
            if (owner != null && owner.ParentMod != null) int.TryParse(owner.LoadSetting(ProjectSerialKey, "1"), out serial);
            if (serial < 1) serial = 1;
            List<string> ids = GetSavedProjectIds();
            string candidate;
            do { candidate = "project-" + serial++; } while (ids.Contains(candidate));
            if (owner != null && owner.ParentMod != null) owner.SaveSetting(ProjectSerialKey, serial.ToString());
            return candidate;
        }

        private static void RequestLoadProjectById(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (projectDirty)
            {
                pendingProjectLoadId = id;
                ShowView(ViewLoadNamedProjectConfirm);
                return;
            }
            LoadProjectById(id);
        }

        private static void ConfirmLoadNamedProject()
        {
            string id = pendingProjectLoadId;
            pendingProjectLoadId = "";
            if (string.IsNullOrEmpty(id))
            {
                ShowView(ViewProjectSlots);
                return;
            }
            LoadProjectById(id);
        }

        private static void LoadProjectById(string savedId)
        {
            ModKitRuntime owner = ModKitRuntime.ActiveRuntime;
            if (owner == null || owner.ParentMod == null) return;
            ProjectSnapshot snapshot;
            if (!TryDeserializeManifest(owner.LoadSetting(ManifestKey(savedId), ""), out snapshot) || snapshot == null)
            {
                persistenceStatus = "LOAD FAILED: saved project manifest is invalid";
                ShowView(ViewProjectSlots);
                return;
            }
            List<WindowDocument> loaded = new List<WindowDocument>();
            for (int i = 0; i < snapshot.WindowIds.Count; i++)
            {
                WindowDocument parsed;
                string id = snapshot.WindowIds[i];
                if (!TryDeserializeWindow(owner.LoadSetting(WindowKey(savedId, id), ""), out parsed) || parsed == null)
                {
                    persistenceStatus = "LOAD FAILED: project window '" + id + "' is invalid";
                    ShowView(ViewProjectSlots);
                    return;
                }
                loaded.Add(parsed);
            }
            windows.Clear();
            for (int i = 0; i < loaded.Count; i++) windows.Add(loaded[i]);
            projectId = snapshot.Id;
            projectName = snapshot.Name;
            nextWindowId = snapshot.NextWindowId;
            nextControlId = snapshot.NextControlId;
            currentWindow = Math.Max(0, Math.Min(snapshot.CurrentWindow, windows.Count - 1));
            projectWindowPage = currentWindow / projectWindowButtons.Length;
            ApplyWindowDocument(currentWindow);
            projectDirty = false;
            currentProjectBackedBySavedSlot = true;
            persistenceStatus = "Loaded " + projectName;
            owner.SaveSetting(ProjectIndexKey, projectId);
            undoHistory.Clear();
            redoHistory.Clear();
            RefreshPreview();
            ShowView(ViewProject);
        }

        private static void RequestCreateProjectFromTemplate(int template)
        {
            if (projectDirty)
            {
                pendingProjectTemplate = template;
                ShowView(ViewCreateProjectConfirm);
                return;
            }
            CreateProjectFromTemplate(template);
        }

        private static void ConfirmCreateProjectFromTemplate()
        {
            int template = pendingProjectTemplate;
            pendingProjectTemplate = 0;
            CreateProjectFromTemplate(template);
        }

        private static void CreateProjectFromTemplate(int template)
        {
            windows.Clear();
            projectId = NextProjectId();
            projectName = template == 1 ? "Settings Project" : template == 2 ? "Toolbar Project" : "New Project";
            nextWindowId = 2;
            nextControlId = 1;
            WindowDocument first = new WindowDocument
            {
                Id = "window-1",
                Title = template == 1 ? "Settings" : template == 2 ? "Toolbar" : "Main Window",
                Width = template == 2 ? 420 : 560,
                Height = template == 2 ? 160 : 460,
                Resizable = template != 2,
                Movable = true,
                Closable = true,
                ToolbarMode = template == 2,
                ContentMargin = template == 2 ? 8 : 14
            };
            if (template == 1)
            {
                first.Controls.Add(new ControlSpec { Id = "control-1", Kind = "SectionStart", Title = "Settings", Placeholder = "", Value = "", ActionName = "", ActionMessage = "" });
                first.Controls.Add(new ControlSpec { Id = "control-2", Kind = "Checkbox", Title = "Enabled", Placeholder = "", Value = "true", ActionName = "", ActionMessage = "" });
                first.Controls.Add(new ControlSpec { Id = "control-3", Kind = "Button", Title = "Apply", Placeholder = "", Value = "", ActionName = "ApplySettings", ActionMessage = "" });
                first.Controls.Add(new ControlSpec { Id = "control-4", Kind = "SectionEnd", Title = "", Placeholder = "", Value = "", ActionName = "", ActionMessage = "" });
                nextControlId = 5;
            }
            else if (template == 2)
            {
                first.Controls.Add(new ControlSpec { Id = "control-1", Kind = "RowStart", Title = "3 columns", Placeholder = "", Value = "", ActionName = "", ActionMessage = "" });
                first.Controls.Add(new ControlSpec { Id = "control-2", Kind = "Button", Title = "One", Placeholder = "", Value = "", ActionName = "ToolbarOne", ActionMessage = "" });
                first.Controls.Add(new ControlSpec { Id = "control-3", Kind = "Button", Title = "Two", Placeholder = "", Value = "", ActionName = "ToolbarTwo", ActionMessage = "" });
                first.Controls.Add(new ControlSpec { Id = "control-4", Kind = "Button", Title = "Three", Placeholder = "", Value = "", ActionName = "ToolbarThree", ActionMessage = "" });
                first.Controls.Add(new ControlSpec { Id = "control-5", Kind = "RowEnd", Title = "", Placeholder = "", Value = "", ActionName = "", ActionMessage = "" });
                nextControlId = 6;
            }
            windows.Add(first);
            currentWindow = 0;
            projectWindowPage = 0;
            ApplyWindowDocument(0);
            projectDirty = true;
            currentProjectBackedBySavedSlot = false;
            persistenceStatus = "New project created from template (not saved)";
            undoHistory.Clear();
            redoHistory.Clear();
            RefreshPreview();
            ShowView(ViewProject);
        }

        private static string ManifestKey(string id)
        {
            return ProjectKeyPrefix + id + ".Manifest.v1";
        }

        private static string WindowKey(string project, string window)
        {
            return ProjectKeyPrefix + project + ".Window." + window + ".v1";
        }

        private static string EncodeText(string value)
        {
            value = value ?? "";
            StringBuilder sb = new StringBuilder(value.Length * 4);
            for (int i = 0; i < value.Length; i++) sb.Append(((int)value[i]).ToString("X4"));
            return sb.ToString();
        }

        private static bool TryDecodeText(string encoded, out string value)
        {
            value = "";
            if (encoded == null || (encoded.Length % 4) != 0) return false;
            StringBuilder sb = new StringBuilder(encoded.Length / 4);
            for (int i = 0; i < encoded.Length; i += 4)
            {
                int code = 0;
                for (int j = 0; j < 4; j++)
                {
                    char c = encoded[i + j];
                    int digit;
                    if (c >= '0' && c <= '9') digit = c - '0';
                    else if (c >= 'A' && c <= 'F') digit = c - 'A' + 10;
                    else if (c >= 'a' && c <= 'f') digit = c - 'a' + 10;
                    else return false;
                    code = code * 16 + digit;
                }
                sb.Append((char)code);
            }
            value = sb.ToString();
            return true;
        }

        private static string SerializeManifest()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SIMODKIT-PROJECT-1|");
            sb.Append(EncodeText(projectId)).Append('|');
            sb.Append(EncodeText(projectName)).Append('|');
            sb.Append(currentWindow).Append('|');
            sb.Append(nextWindowId).Append('|');
            sb.Append(nextControlId).Append('|');
            sb.Append(windows.Count);
            for (int i = 0; i < windows.Count; i++) sb.Append('|').Append(EncodeText(windows[i].Id));
            return sb.ToString();
        }

        private static bool TryDeserializeManifest(string data, out ProjectSnapshot snapshot)
        {
            snapshot = null;
            if (string.IsNullOrEmpty(data)) return false;
            string[] parts = data.Split('|');
            if (parts.Length < 7 || parts[0] != "SIMODKIT-PROJECT-1") return false;
            int active, nextWindow, nextControl, count;
            string id, name;
            if (!TryDecodeText(parts[1], out id) || !TryDecodeText(parts[2], out name) ||
                !int.TryParse(parts[3], out active) || !int.TryParse(parts[4], out nextWindow) ||
                !int.TryParse(parts[5], out nextControl) || !int.TryParse(parts[6], out count)) return false;
            if (count <= 0 || parts.Length != 7 + count) return false;
            ProjectSnapshot parsed = new ProjectSnapshot();
            parsed.Id = id;
            parsed.Name = name;
            parsed.CurrentWindow = active;
            parsed.NextWindowId = Math.Max(1, nextWindow);
            parsed.NextControlId = Math.Max(1, nextControl);
            for (int i = 0; i < count; i++)
            {
                string windowId;
                if (!TryDecodeText(parts[7 + i], out windowId) || string.IsNullOrEmpty(windowId)) return false;
                parsed.WindowIds.Add(windowId);
            }
            if (parsed.CurrentWindow < 0 || parsed.CurrentWindow >= parsed.WindowIds.Count) parsed.CurrentWindow = 0;
            snapshot = parsed;
            return true;
        }

        private static string SerializeWindow(WindowDocument window)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SIMODKIT-WINDOW-4|");
            sb.Append(EncodeText(window.Id)).Append('|');
            sb.Append(EncodeText(window.Title)).Append('|');
            sb.Append(window.Width).Append('|').Append(window.Height).Append('|');
            sb.Append(window.Resizable ? "1" : "0").Append('|');
            sb.Append(window.Movable ? "1" : "0").Append('|');
            sb.Append(window.Closable ? "1" : "0").Append('|');
            sb.Append(window.ToolbarMode ? "1" : "0").Append('|');
            sb.Append(Math.Max(4, Math.Min(64, window.ContentMargin <= 0 ? 14 : window.ContentMargin))).Append('|');
            sb.Append(window.Controls.Count);
            for (int i = 0; i < window.Controls.Count; i++)
            {
                ControlSpec c = window.Controls[i];
                sb.Append('|').Append(EncodeText(c.Id));
                sb.Append('|').Append(EncodeText(c.Kind));
                sb.Append('|').Append(EncodeText(c.Title));
                sb.Append('|').Append(EncodeText(c.Placeholder));
                sb.Append('|').Append(EncodeText(c.Value));
                sb.Append('|').Append(EncodeText(c.ActionName));
                sb.Append('|').Append(EncodeText(c.ActionMessage));
            }
            return sb.ToString();
        }

        private static bool TryDeserializeWindow(string data, out WindowDocument window)
        {
            window = null;
            if (string.IsNullOrEmpty(data)) return false;
            string[] parts = data.Split('|');
            bool v4 = parts.Length >= 11 && parts[0] == "SIMODKIT-WINDOW-4";
            bool v3 = parts.Length >= 9 && parts[0] == "SIMODKIT-WINDOW-3";
            bool v2 = parts.Length >= 7 && parts[0] == "SIMODKIT-WINDOW-2";
            bool v1 = parts.Length >= 7 && parts[0] == "SIMODKIT-WINDOW-1";
            if (!v1 && !v2 && !v3 && !v4) return false;

            string id, title;
            int width, height, count, margin = 14;
            int countIndex = v4 ? 10 : (v3 ? 8 : 6);
            if (!TryDecodeText(parts[1], out id) || !TryDecodeText(parts[2], out title) ||
                !int.TryParse(parts[3], out width) || !int.TryParse(parts[4], out height) ||
                !int.TryParse(parts[countIndex], out count)) return false;

            if (parts[5] != "0" && parts[5] != "1") return false;
            bool resizable = parts[5] == "1";
            bool movable = true;
            bool closable = true;
            bool toolbar = false;
            if (v4)
            {
                if ((parts[6] != "0" && parts[6] != "1") ||
                    (parts[7] != "0" && parts[7] != "1") ||
                    (parts[8] != "0" && parts[8] != "1") ||
                    !int.TryParse(parts[9], out margin)) return false;
                movable = parts[6] == "1";
                closable = parts[7] == "1";
                toolbar = parts[8] == "1";
            }
            else if (v3)
            {
                if ((parts[6] != "0" && parts[6] != "1") || !int.TryParse(parts[7], out margin)) return false;
                toolbar = parts[6] == "1";
                movable = true;
                closable = !toolbar;
            }

            int fieldsPerControl = (v2 || v3 || v4) ? 7 : 6;
            int headerFields = v4 ? 11 : (v3 ? 9 : 7);
            if (count < 0 || parts.Length != headerFields + count * fieldsPerControl) return false;

            int minimumWidth = toolbar ? 160 : 320;
            int minimumHeight = toolbar ? 80 : 240;
            WindowDocument parsed = new WindowDocument
            {
                Id = id,
                Title = title,
                Width = Math.Max(minimumWidth, Math.Min(1600, width)),
                Height = Math.Max(minimumHeight, Math.Min(1200, height)),
                Resizable = resizable,
                Movable = movable,
                Closable = closable,
                ToolbarMode = toolbar,
                ContentMargin = Math.Max(4, Math.Min(64, margin))
            };

            int cursor = headerFields;
            for (int i = 0; i < count; i++)
            {
                string cid, kind, ctitle, hint, value, actionName = "", action;
                if (!TryDecodeText(parts[cursor++], out cid) || !TryDecodeText(parts[cursor++], out kind) ||
                    !TryDecodeText(parts[cursor++], out ctitle) || !TryDecodeText(parts[cursor++], out hint) ||
                    !TryDecodeText(parts[cursor++], out value)) return false;
                if ((v2 || v3 || v4) && !TryDecodeText(parts[cursor++], out actionName)) return false;
                if (!TryDecodeText(parts[cursor++], out action)) return false;
                parsed.Controls.Add(new ControlSpec
                {
                    Id = cid,
                    Kind = kind,
                    Title = ctitle,
                    Placeholder = hint,
                    Value = value,
                    ActionName = actionName,
                    ActionMessage = action
                });
            }
            window = parsed;
            return true;
        }

        private static string BuildPortableProjectPackage()
        {
            EnsureProject();
            CaptureActiveWindow();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(PortableProjectSignature);
            sb.Append("MANIFEST:").AppendLine(SerializeManifest());
            sb.Append("WINDOWS:").AppendLine(windows.Count.ToString());
            for (int i = 0; i < windows.Count; i++)
                sb.Append("WINDOW:").AppendLine(SerializeWindow(windows[i]));
            sb.Append("END-SIMODKIT-PROJECT");
            return sb.ToString();
        }

        private static bool TryParsePortableProjectPackage(string text, out ProjectSnapshot snapshot, out List<WindowDocument> loaded, out string error)
        {
            snapshot = null;
            loaded = null;
            error = "";
            string normalized = (text ?? "").Replace("\r", "").Trim();
            if (string.IsNullOrEmpty(normalized)) { error = "No project package was provided."; return false; }
            string[] lines = normalized.Split('\n');
            if (lines.Length < 5 || lines[0] != PortableProjectSignature)
            {
                error = "Package signature is missing or unsupported.";
                return false;
            }
            if (!lines[1].StartsWith("MANIFEST:")) { error = "Project manifest line is missing."; return false; }
            if (!lines[2].StartsWith("WINDOWS:")) { error = "Window-count line is missing."; return false; }

            ProjectSnapshot parsedSnapshot;
            if (!TryDeserializeManifest(lines[1].Substring("MANIFEST:".Length), out parsedSnapshot) || parsedSnapshot == null)
            {
                error = "Project manifest is invalid.";
                return false;
            }
            if (string.IsNullOrEmpty(parsedSnapshot.Id) || string.IsNullOrEmpty(parsedSnapshot.Name))
            {
                error = "Project identity is incomplete.";
                return false;
            }

            int count;
            if (!int.TryParse(lines[2].Substring("WINDOWS:".Length), out count) || count <= 0)
            {
                error = "Window count is invalid.";
                return false;
            }
            if (count != parsedSnapshot.WindowIds.Count)
            {
                error = "Window count does not match the project manifest.";
                return false;
            }
            if (lines.Length != count + 4 || lines[lines.Length - 1] != "END-SIMODKIT-PROJECT")
            {
                error = "Package is truncated or contains unexpected trailing data.";
                return false;
            }

            List<WindowDocument> parsedWindows = new List<WindowDocument>();
            List<string> seenWindowIds = new List<string>();
            List<string> seenControlIds = new List<string>();
            for (int i = 0; i < count; i++)
            {
                string line = lines[3 + i];
                if (!line.StartsWith("WINDOW:")) { error = "Window record " + (i + 1) + " is missing."; return false; }
                WindowDocument window;
                if (!TryDeserializeWindow(line.Substring("WINDOW:".Length), out window) || window == null)
                {
                    error = "Window record " + (i + 1) + " is invalid.";
                    return false;
                }
                if (window.Id != parsedSnapshot.WindowIds[i])
                {
                    error = "Window order/identity does not match the manifest at record " + (i + 1) + ".";
                    return false;
                }
                if (seenWindowIds.Contains(window.Id)) { error = "Duplicate window ID: " + window.Id; return false; }
                seenWindowIds.Add(window.Id);
                for (int c = 0; c < window.Controls.Count; c++)
                {
                    string controlId = window.Controls[c].Id;
                    if (string.IsNullOrEmpty(controlId)) { error = "A control in '" + window.Title + "' has no ID."; return false; }
                    if (seenControlIds.Contains(controlId)) { error = "Duplicate control ID: " + controlId; return false; }
                    seenControlIds.Add(controlId);
                }
                parsedWindows.Add(window);
            }

            snapshot = parsedSnapshot;
            loaded = parsedWindows;
            return true;
        }

        private static void RequestPortableImport()
        {
            importBuffer = importProjectArea == null ? importBuffer : importProjectArea.text;
            ProjectSnapshot snapshot;
            List<WindowDocument> loaded;
            string error;
            if (!TryParsePortableProjectPackage(importBuffer, out snapshot, out loaded, out error))
            {
                pendingImportSnapshot = null;
                pendingImportWindows = null;
                importStatus = "IMPORT BLOCKED: " + error;
                ShowView(ViewImportProject);
                return;
            }
            pendingImportSnapshot = snapshot;
            pendingImportWindows = loaded;
            importStatus = "Validated: " + snapshot.Name + " | " + loaded.Count + " windows";
            ShowView(ViewImportConfirm);
        }

        private static void CommitPortableImport()
        {
            if (pendingImportSnapshot == null || pendingImportWindows == null || pendingImportWindows.Count == 0)
            {
                importStatus = "IMPORT BLOCKED: validated project state is no longer available.";
                ShowView(ViewImportProject);
                return;
            }

            windows.Clear();
            for (int i = 0; i < pendingImportWindows.Count; i++) windows.Add(pendingImportWindows[i]);
            projectId = pendingImportSnapshot.Id;
            projectName = pendingImportSnapshot.Name;
            nextWindowId = pendingImportSnapshot.NextWindowId;
            nextControlId = pendingImportSnapshot.NextControlId;
            currentWindow = pendingImportSnapshot.CurrentWindow;
            if (currentWindow < 0 || currentWindow >= windows.Count) currentWindow = 0;
            projectWindowPage = currentWindow / projectWindowButtons.Length;
            ApplyWindowDocument(currentWindow);
            projectDirty = true;
            currentProjectBackedBySavedSlot = false;
            persistenceStatus = "Imported portable project (not saved to internal slot)";
            pendingImportSnapshot = null;
            pendingImportWindows = null;
            importBuffer = "";
            importStatus = "Paste a complete SI ModKit portable project package below.";
            RefreshPreview();
            ShowView(ViewProject);
        }

        private static void SaveProject()
        {
            ModKitRuntime owner = ModKitRuntime.ActiveRuntime;
            if (owner == null || owner.ParentMod == null)
            {
                persistenceStatus = "NOT READY: Mod runtime is not initialized";
                ShowView(ViewProject);
                return;
            }
            EnsureProject();
            CaptureActiveWindow();
            try
            {
                List<string> existingProjects = GetSavedProjectIds();
                if (!currentProjectBackedBySavedSlot && existingProjects.Contains(projectId))
                {
                    projectId = NextProjectId();
                    persistenceStatus = "Existing saved ID protected; saving as " + projectId;
                }
                string oldManifest = owner.LoadSetting(ManifestKey(projectId), "");
                ProjectSnapshot oldSnapshot;
                bool hadOld = TryDeserializeManifest(oldManifest, out oldSnapshot);

                for (int i = 0; i < windows.Count; i++)
                {
                    string chunk = SerializeWindow(windows[i]);
                    if (chunk.Length > MaxWindowChunkCharacters)
                    {
                        persistenceStatus = "SAVE BLOCKED: window '" + windows[i].Title + "' is " + chunk.Length + " characters; safe ceiling is " + MaxWindowChunkCharacters + ".";
                        ShowView(ViewProject);
                        return;
                    }
                    owner.SaveSetting(WindowKey(projectId, windows[i].Id), chunk);
                }
                owner.SaveSetting(ManifestKey(projectId), SerializeManifest());
                owner.SaveSetting(ProjectIndexKey, projectId);
                List<string> savedProjects = GetSavedProjectIds();
                if (!savedProjects.Contains(projectId)) savedProjects.Add(projectId);
                SaveProjectIds(savedProjects);

                if (hadOld && oldSnapshot != null)
                {
                    for (int i = 0; i < oldSnapshot.WindowIds.Count; i++)
                    {
                        string oldId = oldSnapshot.WindowIds[i];
                        bool stillUsed = false;
                        for (int j = 0; j < windows.Count; j++) if (windows[j].Id == oldId) { stillUsed = true; break; }
                        if (!stillUsed) owner.DeleteSetting(WindowKey(projectId, oldId));
                    }
                }
                projectDirty = false;
                currentProjectBackedBySavedSlot = true;
                persistenceStatus = "Saved";
            }
            catch (Exception ex)
            {
                persistenceStatus = "SAVE FAILED: " + ex.Message;
            }
            ShowView(ViewProject);
        }

        private static void RequestLoadProject()
        {
            if (projectDirty) ShowView(ViewOpenProjectConfirm);
            else LoadProject();
        }

        private static void LoadProject()
        {
            ModKitRuntime owner = ModKitRuntime.ActiveRuntime;
            if (owner == null || owner.ParentMod == null)
            {
                persistenceStatus = "NOT READY: Mod runtime is not initialized";
                ShowView(ViewProject);
                return;
            }
            try
            {
                string savedId = owner.LoadSetting(ProjectIndexKey, "");
                if (string.IsNullOrEmpty(savedId))
                {
                    persistenceStatus = "No saved project found";
                    ShowView(ViewProject);
                    return;
                }
                ProjectSnapshot snapshot;
                if (!TryDeserializeManifest(owner.LoadSetting(ManifestKey(savedId), ""), out snapshot) || snapshot == null)
                {
                    persistenceStatus = "LOAD FAILED: saved project manifest is missing or invalid";
                    ShowView(ViewProject);
                    return;
                }
                List<WindowDocument> loaded = new List<WindowDocument>();
                for (int i = 0; i < snapshot.WindowIds.Count; i++)
                {
                    string id = snapshot.WindowIds[i];
                    WindowDocument parsed;
                    string chunk = owner.LoadSetting(WindowKey(savedId, id), "");
                    if (!TryDeserializeWindow(chunk, out parsed) || parsed == null || parsed.Id != id)
                    {
                        persistenceStatus = "LOAD FAILED: window chunk '" + id + "' is missing or invalid; current project was left untouched";
                        ShowView(ViewProject);
                        return;
                    }
                    loaded.Add(parsed);
                }

                windows.Clear();
                for (int i = 0; i < loaded.Count; i++) windows.Add(loaded[i]);
                projectId = snapshot.Id;
                projectName = snapshot.Name;
                nextWindowId = snapshot.NextWindowId;
                nextControlId = snapshot.NextControlId;
                currentWindow = snapshot.CurrentWindow;
                projectWindowPage = currentWindow / projectWindowButtons.Length;
                ApplyWindowDocument(currentWindow);
                projectDirty = false;
                currentProjectBackedBySavedSlot = true;
                persistenceStatus = "Loaded saved project";
                RefreshPreview();
            }
            catch (Exception ex)
            {
                persistenceStatus = "LOAD FAILED: " + ex.Message;
            }
            ShowView(ViewProject);
        }

        // Never wipe a design from the main hierarchy without explicit confirmation.
        private static void ShowClearConfirmation()
        {
            if (controls.Count == 0) return;
            ShowView(ViewClearConfirm);
        }

        private static void ClearConfirmed()
        {
            RecordUndoPoint();
            MarkDirty();
            controls.Clear();
            selected = -1;
            page = 0;
            CaptureActiveWindow();
            SyncEditor();
            RefreshCode();
            if (preview != null) { preview.Destroy(); preview = null; }
            ShowView(ViewHierarchy);
        }

        // Native button color blocks preserve hover, pressed, and disabled feedback.
        private static void TintButton(Button button, Color normal)
        {
            if (button == null) return;
            ColorBlock colors = button.colors;
            colors.normalColor = normal;
            colors.highlightedColor = Color.Lerp(normal, Color.white, 0.25f);
            colors.pressedColor = Color.Lerp(normal, Color.black, 0.12f);
            button.colors = colors;
        }

        private static string DisplayKind(string kind)
        {
            if (kind == "SectionStart") return "SECTION OPEN";
            if (kind == "SectionEnd") return "SECTION CLOSE";
            if (kind == "RowStart") return "ROW OPEN";
            if (kind == "RowEnd") return "ROW CLOSE";
            return kind;
        }

        private static string[] ParseDropdownOptions(string encoded)
        {
            string[] raw = (encoded ?? "").Split(',');
            List<string> result = new List<string>();
            for (int i = 0; i < raw.Length; i++)
            {
                string item = raw[i] == null ? "" : raw[i].Trim();
                if (!string.IsNullOrEmpty(item)) result.Add(item);
            }
            if (result.Count == 0) result.Add("Option");
            return result.ToArray();
        }

        private static int FindDropdownSelection(string[] items, string selectedValue)
        {
            if (items == null || items.Length == 0) return 0;
            for (int i = 0; i < items.Length; i++)
                if (string.Equals(items[i], selectedValue ?? "", StringComparison.Ordinal)) return i;
            return 0;
        }

        private static string[] ParseItems(string encoded)
        {
            string normalized = (encoded ?? "").Replace("\r", "\n");
            string[] raw = normalized.Split(new char[] { ',', '\n' });
            List<string> result = new List<string>();
            for (int i = 0; i < raw.Length; i++)
            {
                string item = raw[i] == null ? "" : raw[i].Trim();
                if (!string.IsNullOrEmpty(item)) result.Add(item);
            }
            if (result.Count == 0) result.Add("Item");
            return result.ToArray();
        }

        private static string[][] ParseTableRows(string encoded)
        {
            string normalized = (encoded ?? "").Replace("\r", "");
            string[] rawRows = normalized.Split(new char[] { '\n', ';' });
            List<string[]> rows = new List<string[]>();
            for (int i = 0; i < rawRows.Length; i++)
            {
                string row = rawRows[i] == null ? "" : rawRows[i].Trim();
                if (string.IsNullOrEmpty(row)) continue;
                string[] cells = row.Split('|');
                for (int c = 0; c < cells.Length; c++) cells[c] = (cells[c] ?? "").Trim();
                rows.Add(cells);
            }
            return rows.ToArray();
        }

        private static int FindSelection(string[] items, string selectedValue)
        {
            if (items == null || items.Length == 0) return 0;
            for (int i = 0; i < items.Length; i++)
                if (string.Equals(items[i], selectedValue ?? "", StringComparison.Ordinal)) return i;
            return 0;
        }

        private static bool ParseBool(string value)
        {
            bool parsed;
            return bool.TryParse((value ?? "").Trim(), out parsed) && parsed;
        }

        private static void ParseRange(string encoded, out float minValue, out float maxValue)
        {
            minValue = 0f;
            maxValue = 100f;
            string[] parts = (encoded ?? "").Split(',');
            float parsed;
            if (parts.Length > 0 && float.TryParse(parts[0].Trim(), out parsed)) minValue = parsed;
            if (parts.Length > 1 && float.TryParse(parts[1].Trim(), out parsed)) maxValue = parsed;
            if (maxValue < minValue)
            {
                float swap = minValue; minValue = maxValue; maxValue = swap;
            }
            if (Mathf.Approximately(minValue, maxValue)) maxValue = minValue + 1f;
        }

        private static float ParseRangeValue(string encoded, float minValue, float maxValue)
        {
            float value;
            if (!float.TryParse((encoded ?? "").Trim(), out value)) value = minValue;
            return Mathf.Clamp(value, minValue, maxValue);
        }

        private static string FloatLiteral(float value)
        {
            return value.ToString("0.###").Replace(',', '.');
        }

        private static void RefreshPreview()
        {
            // Keep the same GUIWindow instance. Software Inc. recenters freshly spawned
            // windows; updating only its owned controls preserves player placement/size.
            if (preview == null || preview.NativeWindow == null)
            {
                preview = UI.CreateWindow("Builder Preview", previewWidth, previewHeight);
            }
            else
            {
                preview.ClearContent();
            }
            preview.SetToolbarMode(previewToolbarMode);
            preview.SetSize(previewWidth, previewHeight);
            preview.SetContentMargin(previewContentMargin);
            preview.SetMovable(previewMovable);
            preview.SetResizable(previewResizable);
            preview.SetClosable(previewClosable);
            previewInputs.Clear();
            previewDropdowns.Clear();
            previewSliders.Clear();
            previewToggles.Clear();
            previewLists.Clear();
            previewTables.Clear();
            previewToggles.Clear();
            previewLists.Clear();
            previewTables.Clear();
            preview.SetTitle("Builder Preview");
            if (!previewToolbarMode) preview.AddHeading(previewTitle);
            foreach (ControlSpec control in controls)
            {
                if (control.Kind == "RowStart") preview.BeginRow(GetRowColumns(control));
                if (control.Kind == "RowEnd") preview.EndRow();
                if (control.Kind == "SectionStart") preview.BeginSection(control.Title);
                if (control.Kind == "SectionEnd") preview.EndSection();
                if (control.Kind == "Label") preview.AddLabel(control.Title);
                if (control.Kind == "TextBox")
                {
                    if (!string.IsNullOrEmpty(control.Title)) preview.AddLabel(control.Title);
                    string actionName = control.ActionName;
                    string message = control.ActionMessage;
                    InputField input = preview.AddTextBox(control.Placeholder, string.IsNullOrEmpty(actionName) ? (Action<string>)null : value => RunPreviewAction(actionName, message));
                    input.text = control.Value;
                    previewInputs[control.Id] = input;
                }
                if (control.Kind == "TextArea")
                {
                    if (!string.IsNullOrEmpty(control.Title)) preview.AddLabel(control.Title);
                    string actionName = control.ActionName;
                    string message = control.ActionMessage;
                    InputField input = preview.AddTextArea(control.Placeholder, control.Value, 96f, string.IsNullOrEmpty(actionName) ? (Action<string>)null : value => RunPreviewAction(actionName, message));
                    previewInputs[control.Id] = input;
                }
                if (control.Kind == "Numeric")
                {
                    if (!string.IsNullOrEmpty(control.Title)) preview.AddLabel(control.Title);
                    float minValue, maxValue;
                    ParseRange(control.Placeholder, out minValue, out maxValue);
                    float initial = ParseRangeValue(control.Value, minValue, maxValue);
                    string actionName = control.ActionName;
                    string message = control.ActionMessage;
                    InputField input = preview.AddNumberBox("Enter number", minValue, maxValue, initial, string.IsNullOrEmpty(actionName) ? (Action<float>)null : value => RunPreviewAction(actionName, message));
                    previewInputs[control.Id] = input;
                }
                if (control.Kind == "Checkbox")
                {
                    string actionName = control.ActionName;
                    string message = control.ActionMessage;
                    Toggle toggle = preview.AddCheckbox(control.Title, ParseBool(control.Value), string.IsNullOrEmpty(actionName) ? (Action<bool>)null : value => RunPreviewAction(actionName, message));
                    previewToggles[control.Id] = toggle;
                }
                if (control.Kind == "Dropdown")
                {
                    if (!string.IsNullOrEmpty(control.Title)) preview.AddLabel(control.Title);
                    string[] options = ParseDropdownOptions(control.Placeholder);
                    string actionName = control.ActionName;
                    string message = control.ActionMessage;
                    GUICombobox combo = preview.AddDropdown(options, FindDropdownSelection(options, control.Value), string.IsNullOrEmpty(actionName) ? (Action<int, string>)null : (index, value) => RunPreviewAction(actionName, message));
                    previewDropdowns[control.Id] = combo;
                }
                if (control.Kind == "Slider")
                {
                    if (!string.IsNullOrEmpty(control.Title)) preview.AddLabel(control.Title);
                    float minValue, maxValue;
                    ParseRange(control.Placeholder, out minValue, out maxValue);
                    string actionName = control.ActionName;
                    string message = control.ActionMessage;
                    Slider slider = preview.AddSlider(minValue, maxValue, ParseRangeValue(control.Value, minValue, maxValue), string.IsNullOrEmpty(actionName) ? (Action<float>)null : value => RunPreviewAction(actionName, message));
                    previewSliders[control.Id] = slider;
                }
                if (control.Kind == "ProgressBar")
                {
                    if (!string.IsNullOrEmpty(control.Title)) preview.AddLabel(control.Title);
                    float minValue, maxValue;
                    ParseRange(control.Placeholder, out minValue, out maxValue);
                    preview.AddProgressBar(minValue, maxValue, ParseRangeValue(control.Value, minValue, maxValue));
                }
                if (control.Kind == "List")
                {
                    if (!string.IsNullOrEmpty(control.Title)) preview.AddLabel(control.Title);
                    string[] items = ParseItems(control.Placeholder);
                    string actionName = control.ActionName;
                    string message = control.ActionMessage;
                    GUIListView list = preview.AddList(items, FindSelection(items, control.Value), 144f, string.IsNullOrEmpty(actionName) ? (Action<int, string>)null : (index, value) => RunPreviewAction(actionName, message));
                    previewLists[control.Id] = list;
                }
                if (control.Kind == "Table")
                {
                    if (!string.IsNullOrEmpty(control.Title)) preview.AddLabel(control.Title);
                    string[] columns = ParseItems(control.Placeholder);
                    string[][] rows = ParseTableRows(control.Value);
                    string actionName = control.ActionName;
                    string message = control.ActionMessage;
                    GUIListView table = preview.AddTable(columns, rows, 180f, string.IsNullOrEmpty(actionName) ? (Action<int, string[]>)null : (index, row) => RunPreviewAction(actionName, message));
                    previewTables[control.Id] = table;
                }
                if (control.Kind == "ConfirmButton")
                {
                    string actionName = control.ActionName;
                    string message = control.ActionMessage;
                    preview.AddConfirmButton(control.Title, control.Placeholder, string.IsNullOrEmpty(control.Value) ? "Confirm" : control.Value,
                        string.IsNullOrEmpty(actionName) ? (Action)null : () => RunPreviewAction(actionName, message));
                }
                if (control.Kind == "Button")
                {
                    string actionName = control.ActionName;
                    string message = control.ActionMessage;
                    preview.AddButton(control.Title, string.IsNullOrEmpty(actionName) ? (Action)null : () => RunPreviewAction(actionName, message));
                }
            }
            preview.EndRow();
            preview.Show();
        }

        private static void RunPreviewAction(string actionName, string message)
        {
            if (preview != null && preview.NativeWindow != null)
                preview.SetTitle("Builder Preview | Action: " + actionName);
            Debug.Log("[SI ModKit] Action: " + actionName + (string.IsNullOrEmpty(message) ? "" : " | " + message));
            foreach (KeyValuePair<string, InputField> pair in previewInputs)
                Debug.Log("[SI ModKit] " + pair.Key + " = " + (pair.Value == null ? "" : pair.Value.text));
            foreach (KeyValuePair<string, GUICombobox> pair in previewDropdowns)
                Debug.Log("[SI ModKit] " + pair.Key + " = " + (pair.Value == null ? "" : (pair.Value.SelectedItemString ?? "")));
            foreach (KeyValuePair<string, Slider> pair in previewSliders)
                Debug.Log("[SI ModKit] " + pair.Key + " = " + (pair.Value == null ? "" : pair.Value.value.ToString()));
            foreach (KeyValuePair<string, Toggle> pair in previewToggles)
                Debug.Log("[SI ModKit] " + pair.Key + " = " + (pair.Value == null ? "" : pair.Value.isOn.ToString()));
            foreach (KeyValuePair<string, GUIListView> pair in previewLists)
                Debug.Log("[SI ModKit] " + pair.Key + " = " + (pair.Value == null ? "" : (pair.Value.GetFirstSelected<string>() ?? "")));
            foreach (KeyValuePair<string, GUIListView> pair in previewTables)
            {
                string[] row = pair.Value == null ? null : pair.Value.GetFirstSelected<string[]>();
                Debug.Log("[SI ModKit] " + pair.Key + " = " + (row == null ? "" : string.Join(" | ", row)));
            }
        }

        private static string SafeIdentifier(string value, string fallback)
        {
            if (string.IsNullOrEmpty(value)) return fallback;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_' || (i > 0 && c >= '0' && c <= '9')) sb.Append(c);
                else if (i == 0 && c >= '0' && c <= '9') { sb.Append('_'); sb.Append(c); }
                else sb.Append('_');
            }
            return sb.Length == 0 ? fallback : sb.ToString();
        }

        private static string Escape(string input)
        {
            return (input ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"")
                .Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
        }

        private static string GenerateWindowCode(WindowDocument source, string symbolPrefix)
        {
            if (source == null) return "";
            string prefix = SafeIdentifier(symbolPrefix ?? "", "");
            if (!string.IsNullOrEmpty(prefix) && !prefix.EndsWith("_")) prefix += "_";
            string windowVar = "window_" + (string.IsNullOrEmpty(prefix) ? "active" : prefix.TrimEnd('_'));
            List<ControlSpec> sourceControls = source.Controls;
            StringBuilder sb = new StringBuilder();
            List<ControlSpec> textInputs = new List<ControlSpec>();
            List<ControlSpec> checkboxInputs = new List<ControlSpec>();
            List<ControlSpec> dropdownInputs = new List<ControlSpec>();
            List<ControlSpec> sliderInputs = new List<ControlSpec>();
            List<ControlSpec> listInputs = new List<ControlSpec>();
            List<ControlSpec> tableInputs = new List<ControlSpec>();
            List<string> actionNames = new List<string>();
            for (int i = 0; i < sourceControls.Count; i++)
            {
                ControlSpec c = sourceControls[i];
                if (c.Kind == "TextBox" || c.Kind == "TextArea" || c.Kind == "Numeric") textInputs.Add(c);
                if (c.Kind == "Checkbox") checkboxInputs.Add(c);
                if (c.Kind == "Dropdown") dropdownInputs.Add(c);
                if (c.Kind == "Slider") sliderInputs.Add(c);
                if (c.Kind == "List") listInputs.Add(c);
                if (c.Kind == "Table") tableInputs.Add(c);
                if (!string.IsNullOrEmpty(c.ActionName) && (c.Kind == "Button" || c.Kind == "ConfirmButton" || c.Kind == "TextBox" || c.Kind == "TextArea" || c.Kind == "Numeric" || c.Kind == "Checkbox" || c.Kind == "Dropdown" || c.Kind == "Slider" || c.Kind == "List" || c.Kind == "Table"))
                {
                    string id = SafeIdentifier(c.ActionName, "Action");
                    if (!actionNames.Contains(id)) actionNames.Add(id);
                }
            }

            sb.AppendLine("var " + windowVar + " = SIModKit.UI.CreateWindow(\"" + Escape(source.Title) + "\", " + source.Width + ", " + source.Height + ");");
            sb.AppendLine(windowVar + ".SetToolbarMode(" + (source.ToolbarMode ? "true" : "false") + ");");
            sb.AppendLine(windowVar + ".SetContentMargin(" + source.ContentMargin + ");");
            sb.AppendLine(windowVar + ".SetMovable(" + (source.Movable ? "true" : "false") + ");");
            sb.AppendLine(windowVar + ".SetResizable(" + (source.Resizable ? "true" : "false") + ");");
            sb.AppendLine(windowVar + ".SetClosable(" + (source.Closable ? "true" : "false") + ");");
            for (int i = 0; i < textInputs.Count; i++)
                sb.AppendLine("UnityEngine.UI.InputField input_" + prefix + SafeIdentifier(textInputs[i].Id, "field" + i) + " = null;");
            for (int i = 0; i < checkboxInputs.Count; i++)
                sb.AppendLine("UnityEngine.UI.Toggle toggle_" + prefix + SafeIdentifier(checkboxInputs[i].Id, "toggle" + i) + " = null;");
            for (int i = 0; i < dropdownInputs.Count; i++)
                sb.AppendLine("GUICombobox select_" + prefix + SafeIdentifier(dropdownInputs[i].Id, "select" + i) + " = null;");
            for (int i = 0; i < sliderInputs.Count; i++)
                sb.AppendLine("UnityEngine.UI.Slider slider_" + prefix + SafeIdentifier(sliderInputs[i].Id, "slider" + i) + " = null;");
            for (int i = 0; i < listInputs.Count; i++)
                sb.AppendLine("GUIListView list_" + prefix + SafeIdentifier(listInputs[i].Id, "list" + i) + " = null;");
            for (int i = 0; i < tableInputs.Count; i++)
                sb.AppendLine("GUIListView table_" + prefix + SafeIdentifier(tableInputs[i].Id, "table" + i) + " = null;");
            if (textInputs.Count > 0 || checkboxInputs.Count > 0 || dropdownInputs.Count > 0 || sliderInputs.Count > 0 || listInputs.Count > 0 || tableInputs.Count > 0) sb.AppendLine();

            for (int i = 0; i < actionNames.Count; i++)
            {
                string actionId = actionNames[i];
                string actionVar = "action_" + prefix + actionId;
                string message = "";
                for (int j = 0; j < sourceControls.Count; j++)
                    if (SafeIdentifier(sourceControls[j].ActionName, "Action") == actionId && !string.IsNullOrEmpty(sourceControls[j].ActionMessage)) { message = sourceControls[j].ActionMessage; break; }
                sb.AppendLine("System.Action " + actionVar + " = () =>");
                sb.AppendLine("{");
                sb.AppendLine("    UnityEngine.Debug.Log(\"[SI ModKit] Action: " + Escape(actionId) + (string.IsNullOrEmpty(message) ? "" : " | " + Escape(message)) + "\");");
                for (int j = 0; j < textInputs.Count; j++)
                {
                    string field = "input_" + prefix + SafeIdentifier(textInputs[j].Id, "field" + j);
                    sb.AppendLine("    UnityEngine.Debug.Log(\"" + Escape(textInputs[j].Id) + " = \" + (" + field + " == null ? \"\" : " + field + ".text));");
                }
                for (int j = 0; j < checkboxInputs.Count; j++)
                {
                    string field = "toggle_" + prefix + SafeIdentifier(checkboxInputs[j].Id, "toggle" + j);
                    sb.AppendLine("    UnityEngine.Debug.Log(\"" + Escape(checkboxInputs[j].Id) + " = \" + (" + field + " == null ? \"\" : " + field + ".isOn.ToString()));");
                }
                for (int j = 0; j < dropdownInputs.Count; j++)
                {
                    string field = "select_" + prefix + SafeIdentifier(dropdownInputs[j].Id, "select" + j);
                    sb.AppendLine("    UnityEngine.Debug.Log(\"" + Escape(dropdownInputs[j].Id) + " = \" + (" + field + " == null ? \"\" : (" + field + ".SelectedItemString ?? \"\")));");
                }
                for (int j = 0; j < sliderInputs.Count; j++)
                {
                    string field = "slider_" + prefix + SafeIdentifier(sliderInputs[j].Id, "slider" + j);
                    sb.AppendLine("    UnityEngine.Debug.Log(\"" + Escape(sliderInputs[j].Id) + " = \" + (" + field + " == null ? \"\" : " + field + ".value.ToString()));");
                }
                for (int j = 0; j < listInputs.Count; j++)
                {
                    string field = "list_" + prefix + SafeIdentifier(listInputs[j].Id, "list" + j);
                    sb.AppendLine("    UnityEngine.Debug.Log(\"" + Escape(listInputs[j].Id) + " = \" + (" + field + " == null ? \"\" : (" + field + ".GetFirstSelected<string>() ?? \"\")));");
                }
                for (int j = 0; j < tableInputs.Count; j++)
                {
                    string field = "table_" + prefix + SafeIdentifier(tableInputs[j].Id, "table" + j);
                    sb.AppendLine("    string[] row_" + j + " = " + field + " == null ? null : " + field + ".GetFirstSelected<string[]>();");
                    sb.AppendLine("    UnityEngine.Debug.Log(\"" + Escape(tableInputs[j].Id) + " = \" + (row_" + j + " == null ? \"\" : string.Join(\" | \", row_" + j + ")));");
                }
                sb.AppendLine("};");
                sb.AppendLine();
            }

            if (!source.ToolbarMode) sb.AppendLine(windowVar + ".AddHeading(\"" + Escape(source.Title) + "\");");
            foreach (ControlSpec control in sourceControls)
            {
                string value = Escape(control.Title);
                if (control.Kind == "RowStart") sb.AppendLine(windowVar + ".BeginRow(" + GetRowColumns(control) + ");");
                if (control.Kind == "RowEnd") sb.AppendLine(windowVar + ".EndRow();");
                if (control.Kind == "SectionStart") sb.AppendLine(windowVar + ".BeginSection(\"" + value + "\");");
                if (control.Kind == "SectionEnd") sb.AppendLine(windowVar + ".EndSection();");
                if (control.Kind == "Label") sb.AppendLine(windowVar + ".AddLabel(\"" + value + "\");");
                if (control.Kind == "TextBox")
                {
                    if (!string.IsNullOrEmpty(control.Title)) sb.AppendLine(windowVar + ".AddLabel(\"" + value + "\");");
                    string field = "input_" + prefix + SafeIdentifier(control.Id, "field");
                    string callback = string.IsNullOrEmpty(control.ActionName) ? "null" : "value => action_" + prefix + SafeIdentifier(control.ActionName, "Action") + "()";
                    sb.AppendLine(field + " = " + windowVar + ".AddTextBox(\"" + Escape(control.Placeholder) + "\", " + callback + ");");
                    sb.AppendLine(field + ".text = \"" + Escape(control.Value) + "\";");
                }
                if (control.Kind == "TextArea")
                {
                    if (!string.IsNullOrEmpty(control.Title)) sb.AppendLine(windowVar + ".AddLabel(\"" + value + "\");");
                    string field = "input_" + prefix + SafeIdentifier(control.Id, "field");
                    string callback = string.IsNullOrEmpty(control.ActionName) ? "null" : "value => action_" + prefix + SafeIdentifier(control.ActionName, "Action") + "()";
                    sb.AppendLine(field + " = " + windowVar + ".AddTextArea(\"" + Escape(control.Placeholder) + "\", \"" + Escape(control.Value) + "\", 96f, " + callback + ");");
                }
                if (control.Kind == "Numeric")
                {
                    if (!string.IsNullOrEmpty(control.Title)) sb.AppendLine(windowVar + ".AddLabel(\"" + value + "\");");
                    float minValue, maxValue;
                    ParseRange(control.Placeholder, out minValue, out maxValue);
                    float initial = ParseRangeValue(control.Value, minValue, maxValue);
                    string field = "input_" + prefix + SafeIdentifier(control.Id, "field");
                    string callback = string.IsNullOrEmpty(control.ActionName) ? "null" : "value => action_" + prefix + SafeIdentifier(control.ActionName, "Action") + "()";
                    sb.AppendLine(field + " = " + windowVar + ".AddNumberBox(\"Enter number\", " + FloatLiteral(minValue) + "f, " + FloatLiteral(maxValue) + "f, " + FloatLiteral(initial) + "f, " + callback + ");");
                }
                if (control.Kind == "Checkbox")
                {
                    string field = "toggle_" + prefix + SafeIdentifier(control.Id, "toggle");
                    string callback = string.IsNullOrEmpty(control.ActionName) ? "null" : "value => action_" + prefix + SafeIdentifier(control.ActionName, "Action") + "()";
                    sb.AppendLine(field + " = " + windowVar + ".AddCheckbox(\"" + value + "\", " + (ParseBool(control.Value) ? "true" : "false") + ", " + callback + ");");
                }
                if (control.Kind == "Dropdown")
                {
                    if (!string.IsNullOrEmpty(control.Title)) sb.AppendLine(windowVar + ".AddLabel(\"" + value + "\");");
                    string[] options = ParseDropdownOptions(control.Placeholder);
                    StringBuilder optionCode = new StringBuilder();
                    for (int oi = 0; oi < options.Length; oi++)
                    {
                        if (oi > 0) optionCode.Append(", ");
                        optionCode.Append("\"").Append(Escape(options[oi])).Append("\"");
                    }
                    string field = "select_" + prefix + SafeIdentifier(control.Id, "select");
                    string callback = string.IsNullOrEmpty(control.ActionName) ? "null" : "(index, selectedValue) => action_" + prefix + SafeIdentifier(control.ActionName, "Action") + "()";
                    sb.AppendLine(field + " = " + windowVar + ".AddDropdown(new string[] { " + optionCode + " }, " + FindDropdownSelection(options, control.Value) + ", " + callback + ");");
                }
                if (control.Kind == "Slider")
                {
                    if (!string.IsNullOrEmpty(control.Title)) sb.AppendLine(windowVar + ".AddLabel(\"" + value + "\");");
                    float minValue, maxValue;
                    ParseRange(control.Placeholder, out minValue, out maxValue);
                    float initial = ParseRangeValue(control.Value, minValue, maxValue);
                    string field = "slider_" + prefix + SafeIdentifier(control.Id, "slider");
                    string callback = string.IsNullOrEmpty(control.ActionName) ? "null" : "value => action_" + prefix + SafeIdentifier(control.ActionName, "Action") + "()";
                    sb.AppendLine(field + " = " + windowVar + ".AddSlider(" + FloatLiteral(minValue) + "f, " + FloatLiteral(maxValue) + "f, " + FloatLiteral(initial) + "f, " + callback + ");");
                }
                if (control.Kind == "ProgressBar")
                {
                    if (!string.IsNullOrEmpty(control.Title)) sb.AppendLine(windowVar + ".AddLabel(\"" + value + "\");");
                    float minValue, maxValue;
                    ParseRange(control.Placeholder, out minValue, out maxValue);
                    float initial = ParseRangeValue(control.Value, minValue, maxValue);
                    sb.AppendLine(windowVar + ".AddProgressBar(" + FloatLiteral(minValue) + "f, " + FloatLiteral(maxValue) + "f, " + FloatLiteral(initial) + "f);");
                }
                if (control.Kind == "List")
                {
                    if (!string.IsNullOrEmpty(control.Title)) sb.AppendLine(windowVar + ".AddLabel(\"" + value + "\");");
                    string[] items = ParseItems(control.Placeholder);
                    StringBuilder itemCode = new StringBuilder();
                    for (int ii = 0; ii < items.Length; ii++)
                    {
                        if (ii > 0) itemCode.Append(", ");
                        itemCode.Append("\"").Append(Escape(items[ii])).Append("\"");
                    }
                    string field = "list_" + prefix + SafeIdentifier(control.Id, "list");
                    string callback = string.IsNullOrEmpty(control.ActionName) ? "null" : "(index, selectedValue) => action_" + prefix + SafeIdentifier(control.ActionName, "Action") + "()";
                    sb.AppendLine(field + " = " + windowVar + ".AddList(new string[] { " + itemCode + " }, " + FindSelection(items, control.Value) + ", 144f, " + callback + ");");
                }
                if (control.Kind == "Table")
                {
                    if (!string.IsNullOrEmpty(control.Title)) sb.AppendLine(windowVar + ".AddLabel(\"" + value + "\");");
                    string[] columns = ParseItems(control.Placeholder);
                    string[][] rows = ParseTableRows(control.Value);
                    StringBuilder columnCode = new StringBuilder();
                    for (int ci = 0; ci < columns.Length; ci++)
                    {
                        if (ci > 0) columnCode.Append(", ");
                        columnCode.Append("\"").Append(Escape(columns[ci])).Append("\"");
                    }
                    StringBuilder rowCode = new StringBuilder();
                    for (int ri = 0; ri < rows.Length; ri++)
                    {
                        if (ri > 0) rowCode.Append(", ");
                        rowCode.Append("new string[] { ");
                        for (int ci = 0; ci < rows[ri].Length; ci++)
                        {
                            if (ci > 0) rowCode.Append(", ");
                            rowCode.Append("\"").Append(Escape(rows[ri][ci])).Append("\"");
                        }
                        rowCode.Append(" }");
                    }
                    string field = "table_" + prefix + SafeIdentifier(control.Id, "table");
                    string callback = string.IsNullOrEmpty(control.ActionName) ? "null" : "(index, selectedRow) => action_" + prefix + SafeIdentifier(control.ActionName, "Action") + "()";
                    sb.AppendLine(field + " = " + windowVar + ".AddTable(new string[] { " + columnCode + " }, new string[][] { " + rowCode + " }, 180f, " + callback + ");");
                }
                if (control.Kind == "ConfirmButton")
                {
                    string callback = string.IsNullOrEmpty(control.ActionName) ? "null" : "action_" + prefix + SafeIdentifier(control.ActionName, "Action");
                    sb.AppendLine(windowVar + ".AddConfirmButton(\"" + value + "\", \"" + Escape(control.Placeholder) + "\", \"" + Escape(string.IsNullOrEmpty(control.Value) ? "Confirm" : control.Value) + "\", " + callback + ");");
                }
                if (control.Kind == "Button")
                {
                    if (string.IsNullOrEmpty(control.ActionName)) sb.AppendLine(windowVar + ".AddButton(\"" + value + "\", null);");
                    else sb.AppendLine(windowVar + ".AddButton(\"" + value + "\", action_" + prefix + SafeIdentifier(control.ActionName, "Action") + ");");
                }
            }
            sb.AppendLine(windowVar + ".EndRow();");
            sb.AppendLine(windowVar + ".Show();");
            return sb.ToString();
        }

        private static string GenerateWholeProjectCode()
        {
            EnsureProject();
            CaptureActiveWindow();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("// SI ModKit complete-project generated source");
            sb.AppendLine("// Project: " + projectName);
            sb.AppendLine("// Project ID: " + projectId);
            sb.AppendLine("// Each FILE marker is a logical boundary for manual copy/integration.");
            sb.AppendLine();
            for (int i = 0; i < windows.Count; i++)
            {
                WindowDocument source = windows[i];
                string prefix = "w" + (i + 1);
                sb.AppendLine("// ============================================================");
                sb.AppendLine("// FILE: " + (i + 1).ToString("00") + "_" + SafeIdentifier(source.Title, "Window" + (i + 1)) + ".cs");
                sb.AppendLine("// WINDOW ID: " + source.Id);
                sb.AppendLine("// ============================================================");
                sb.Append(GenerateWindowCode(source, prefix));
                if (i + 1 < windows.Count) sb.AppendLine();
            }
            return sb.ToString();
        }

        private static void RefreshCode()
        {
            if (generated == null) return;
            CaptureActiveWindow();
            string text;
            if (generatedWholeProject) text = GenerateWholeProjectCode();
            else
            {
                EnsureProject();
                WindowDocument source = windows[Math.Max(0, Math.Min(currentWindow, windows.Count - 1))];
                text = GenerateWindowCode(source, "");
            }
            generated.text = text;
            Debug.Log("[SI ModKit] Generated C#:\n" + text);
        }

        public static void Destroy()
        {
            if (editor != null) editor.Destroy();
            if (preview != null) preview.Destroy();
            editor = null;
            preview = null;
            generated = null;
            portableProjectArea = null;
            importProjectArea = null;
            pendingImportSnapshot = null;
            pendingImportWindows = null;
            actionMessageEditor = null;
            previewInputs.Clear();
            previewDropdowns.Clear();
            previewSliders.Clear();
            previewToggles.Clear();
            previewLists.Clear();
            previewTables.Clear();
            resizableButton = null;
            movableButton = null;
            closableButton = null;
            previewResizable = true;
            previewMovable = true;
            previewClosable = true;
            previewToolbarMode = false;
            previewContentMargin = 14;
            wikiPage = 0;
            storageResult = "Not tested";
            interopResult = "Not tested";
            windowTitleEditor = null;
            windowWidthEditor = null;
            windowHeightEditor = null;
            windowMarginEditor = null;
            toolbarModeButton = null;
            projectNameEditor = null;
            projectStatus = null;
            validationStatus = null;
            previewTitle = "My Mod Window";
            previewWidth = 560;
            previewHeight = 460;
            titleEditor = null;
            hintEditor = null;
            valueEditor = null;
            actionEditor = null;
            selectionStatus = null;
            hierarchyStatus = null;
            layoutContextStatus = null;
            for (int i = 0; i < hierarchyButtons.Length; i++) hierarchyButtons[i] = null;
            for (int i = 0; i < projectWindowButtons.Length; i++) projectWindowButtons[i] = null;
            controls.Clear();
            windows.Clear();
            projectId = "project-1";
            projectName = "My Mod Project";
            currentWindow = -1;
            nextWindowId = 1;
            nextControlId = 1;
            projectWindowPage = 0;
            selected = -1;
            page = 0;
        }
    }
}
