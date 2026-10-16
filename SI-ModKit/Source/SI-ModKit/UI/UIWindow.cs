using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SIModKit
{
    /// <summary>A native Software Inc. window with beginner-friendly automatic form layout.</summary>
    public sealed class UIWindow
    {
        public GUIWindow NativeWindow { get; private set; }
        private float nextY = 12f;
        private readonly List<GameObject> ownedControls = new List<GameObject>();
        private const float Height = 32f;
        private const float Gap = 8f;
        private float contentMargin = 14f;
        private float fieldWidth;
        private bool toolbarMode;
        private bool resizableRequested = true;
        private bool movableRequested = true;
        private bool closableRequested = true;
        private string requestedTitle = "";
        private Sprite originalPanelSprite;
        private Color originalPanelColor;
        private int rowColumns;
        private int rowSlot;
        private float rowHeight;
        private int sectionDepth;
        private const float SectionIndent = 16f;

        internal UIWindow(string title, int width, int height)
        {
            NativeWindow = WindowManager.SpawnWindow();
            if (NativeWindow == null) throw new InvalidOperationException("SI ModKit: SpawnWindow returned null.");
            requestedTitle = title ?? "";
            NativeWindow.InitialTitle = NativeWindow.NonLocTitle = NativeWindow.TitleText.text = requestedTitle;
            // Keep the requested dimensions as the registration minimum until first show.
            // Software Inc. otherwise clamps new windows to its UI-scaled screen bounds, which
            // can make the native frame smaller than the toolkit layout was built for.
            NativeWindow.MinSize = new Vector2(width, height);
            NativeWindow.ShowCentered = true;
            NativeWindow.OnlyHide = true;
            if (NativeWindow.rectTransform != null)
                NativeWindow.rectTransform.sizeDelta = new Vector2(width, height);
            Image panelImage = NativeWindow.MainPanel == null ? null : NativeWindow.MainPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                originalPanelSprite = panelImage.sprite;
                originalPanelColor = panelImage.color;
            }
            RecalculateFieldWidth();
        }
        /// <summary>Updates the native title without replacing the window.</summary>
        public void SetTitle(string title)
        {
            if (NativeWindow == null) return;
            requestedTitle = title ?? "";
            NativeWindow.InitialTitle = NativeWindow.NonLocTitle = requestedTitle;
            if (NativeWindow.TitleText != null) NativeWindow.TitleText.text = toolbarMode ? "" : requestedTitle;
        }
        /// <summary>Sets dimensions and minimum size without spawning a new window.
        /// Existing controls are not reflowed; call ClearContent and rebuild when needed.</summary>
        public void SetSize(int width, int height)
        {
            if (NativeWindow == null) return;
            int minimumWidth = toolbarMode ? 160 : 320;
            int minimumHeight = toolbarMode ? 80 : 240;
            width = Mathf.Clamp(width, minimumWidth, 1600);
            height = Mathf.Clamp(height, minimumHeight, 1200);
            NativeWindow.MinSize = new Vector2(minimumWidth, minimumHeight);
            if (NativeWindow.rectTransform != null)
                NativeWindow.rectTransform.sizeDelta = new Vector2(width, height);
            Image panelImage = NativeWindow.MainPanel == null ? null : NativeWindow.MainPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                originalPanelSprite = panelImage.sprite;
                originalPanelColor = panelImage.color;
            }
            RecalculateFieldWidth();
        }
        /// <summary>Shows or hides Software Inc.'s native resize handle.
        /// Disabling it prevents drag-resizing but does not lock programmatic SetSize.
        /// </summary>
        public void SetResizable(bool enabled)
        {
            resizableRequested = enabled;
            if (NativeWindow == null || NativeWindow.SizeButton == null) return;
            NativeWindow.SizeButton.SetActive(enabled);
        }

        /// <summary>Enables or disables player dragging from the native top strip.</summary>
        public void SetMovable(bool enabled)
        {
            movableRequested = enabled;
            if (NativeWindow == null) return;
            if (NativeWindow.TopPanel != null) NativeWindow.TopPanel.raycastTarget = enabled;
            if (NativeWindow.TitleText != null) NativeWindow.TitleText.raycastTarget = enabled;
        }

        /// <summary>Shows or hides Software Inc.'s native close control.</summary>
        public void SetClosable(bool enabled)
        {
            closableRequested = enabled;
            if (NativeWindow == null || NativeWindow.CloseButton == null) return;
            NativeWindow.CloseButton.SetActive(enabled);
        }

        /// <summary>Sets equal left/right content padding for toolkit-managed controls.</summary>
        public void SetContentMargin(int pixels)
        {
            contentMargin = Mathf.Clamp(pixels, 4, 64);
            RecalculateFieldWidth();
        }

        /// <summary>Enables a compact floating-toolbar presentation. Presentation is independent
        /// of movement, resizing and closing policies. The native top strip remains available
        /// for dragging when SetMovable(true) is in effect.</summary>
        public void SetToolbarMode(bool enabled)
        {
            if (NativeWindow == null) return;
            toolbarMode = enabled;
            NativeWindow.IgnoreWindowBorder = enabled;
            if (NativeWindow.TitleText != null)
            {
                NativeWindow.TitleText.gameObject.SetActive(true);
                NativeWindow.TitleText.text = enabled ? "" : requestedTitle;
            }
            if (NativeWindow.CloseButton != null) NativeWindow.CloseButton.SetActive(closableRequested);
            if (NativeWindow.SizeButton != null) NativeWindow.SizeButton.SetActive(resizableRequested);
            if (NativeWindow.TopPanel != null) NativeWindow.TopPanel.raycastTarget = movableRequested;
            if (NativeWindow.TitleText != null) NativeWindow.TitleText.raycastTarget = movableRequested;
            Image panelImage = NativeWindow.MainPanel == null ? null : NativeWindow.MainPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                if (enabled)
                {
                    panelImage.sprite = null;
                    panelImage.color = new Color(originalPanelColor.r, originalPanelColor.g, originalPanelColor.b, 0.86f);
                }
                else
                {
                    panelImage.sprite = originalPanelSprite;
                    panelImage.color = originalPanelColor;
                }
            }
            int minimumWidth = enabled ? 160 : 320;
            int minimumHeight = enabled ? 80 : 240;
            NativeWindow.MinSize = new Vector2(minimumWidth, minimumHeight);
            if (NativeWindow.rectTransform != null)
            {
                Vector2 size = NativeWindow.rectTransform.sizeDelta;
                NativeWindow.rectTransform.sizeDelta = new Vector2(Mathf.Max(minimumWidth, size.x), Mathf.Max(minimumHeight, size.y));
            }
            RecalculateFieldWidth();
        }

        private void RecalculateFieldWidth()
        {
            float width = NativeWindow != null && NativeWindow.rectTransform != null ? NativeWindow.rectTransform.sizeDelta.x : 560f;
            fieldWidth = Mathf.Max(100f, width - contentMargin * 2f);
        }

        private void Attach(GameObject gameObject, float height)
        {
            RecalculateFieldWidth();
            float inset = sectionDepth * SectionIndent;
            float y = nextY;
            Rect position;
            Rect anchors;
            if (rowColumns > 0)
            {
                // Responsive horizontal cells: anchor each control to its fractional share of
                // the window width, while margins/spacing remain pixel offsets.
                const float spacing = 6f;
                float aMin = (float)rowSlot / rowColumns;
                float aMax = (float)(rowSlot + 1) / rowColumns;
                float left = rowSlot == 0 ? contentMargin + inset : spacing * 0.5f;
                float right = rowSlot == rowColumns - 1 ? -contentMargin : -spacing * 0.5f;
                position = new Rect(left, y, right - left, height);
                anchors = new Rect(aMin, 0f, aMax - aMin, 0f);
                rowHeight = Mathf.Max(rowHeight, height);
                rowSlot++;
                if (rowSlot >= rowColumns) { rowSlot = 0; nextY += rowHeight + Gap; rowHeight = 0f; }
            }
            else
            {
                // Full-width controls stretch horizontally with the native window instead of
                // preserving the width that happened to exist when they were spawned.
                float left = contentMargin + inset;
                float right = -contentMargin;
                position = new Rect(left, y, right - left, height);
                anchors = new Rect(0f, 0f, 1f, 0f);
                nextY += height + Gap;
            }
            WindowManager.AddElementToWindow(gameObject, NativeWindow, position, anchors);
            ownedControls.Add(gameObject);
        }
        /// <summary>Start a fixed-width row. Add exactly 'columns' controls, then EndRow.
        /// Existing controls remain in place when the row is finished.</summary>
        public void BeginRow(int columns)
        {
            EndRow();
            rowColumns = Mathf.Clamp(columns, 1, 8);
            rowSlot = 0;
            rowHeight = 0f;
        }
        /// <summary>Alias for BeginRow; describes the responsive horizontal-container intent.</summary>
        public void BeginHorizontal(int columns) { BeginRow(columns); }

        /// <summary>Practical responsive grid: each subsequent control occupies one equal-width cell.</summary>
        public void BeginGrid(int columns) { BeginRow(columns); }

        /// <summary>Ends a horizontal/grid container.</summary>
        public void EndHorizontal() { EndRow(); }

        public void EndRow()
        {
            if (rowColumns > 0 && rowSlot > 0) nextY += rowHeight + Gap;
            rowColumns = 0;
            rowSlot = 0;
            rowHeight = 0f;
        }
        /// <summary>Insert a native heading and indent subsequent controls until EndSection.</summary>
        public void BeginSection(string title)
        {
            EndRow();
            AddHeading(title);
            sectionDepth = Mathf.Min(sectionDepth + 1, 4);
        }
        public void EndSection()
        {
            EndRow();
            sectionDepth = Mathf.Max(0, sectionDepth - 1);
        }
        public Text AddLabel(string value)
        {
            Text t = WindowManager.SpawnLabel();
            t.text = value ?? "";
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.alignment = TextAnchor.UpperLeft;
            // Estimate required lines at the current usable width, allowing labels to
            // grow rather than clipping. The text's own Unity generator gives a more
            // accurate height when its font is available.
            float availableWidth = Mathf.Max(100f, fieldWidth - sectionDepth * SectionIndent);
            if (rowColumns > 0) availableWidth = Mathf.Max(80f, (availableWidth - (rowColumns - 1) * 6f) / rowColumns);
            float labelHeight = 26f;
            if (t.font != null)
            {
                TextGenerationSettings settings = t.GetGenerationSettings(new Vector2(availableWidth, 10000f));
                float preferred = t.cachedTextGeneratorForLayout.GetPreferredHeight(t.text, settings);
                labelHeight = Mathf.Max(26f, Mathf.Min(600f, preferred / t.pixelsPerUnit + 4f));
            }
            Attach(t.gameObject, labelHeight);
            return t;
        }
        public Text AddHeading(string value)
        {
            Text t = AddLabel(value);
            t.fontStyle = FontStyle.Bold;
            return t;
        }
        public InputField AddTextBox(string placeholder = "", Action<string> callback = null)
        {
            InputField input = WindowManager.SpawnInputbox();
            if (input.placeholder is Text) ((Text)input.placeholder).text = placeholder;
            if (callback != null) input.onEndEdit.AddListener(value => callback(value ?? ""));
            Attach(input.gameObject, Height);
            return input;
        }

        /// <summary>Creates a numeric input with optional minimum/maximum clamping.
        /// The callback fires when editing ends and receives the parsed/clamped value.</summary>
        public InputField AddNumberBox(string placeholder, float minValue, float maxValue, float value, Action<float> callback = null)
        {
            InputField input = WindowManager.SpawnInputbox();
            if (maxValue < minValue)
            {
                float swap = minValue;
                minValue = maxValue;
                maxValue = swap;
            }
            if (Mathf.Approximately(minValue, maxValue)) maxValue = minValue + 1f;
            input.contentType = InputField.ContentType.DecimalNumber;
            if (input.placeholder is Text) ((Text)input.placeholder).text = placeholder ?? "";
            input.text = Mathf.Clamp(value, minValue, maxValue).ToString("0.###");
            input.onEndEdit.AddListener(text =>
            {
                float parsed;
                if (!float.TryParse((text ?? "").Trim(), out parsed)) parsed = minValue;
                parsed = Mathf.Clamp(parsed, minValue, maxValue);
                input.text = parsed.ToString("0.###");
                if (callback != null) callback(parsed);
            });
            Attach(input.gameObject, Height);
            return input;
        }

        /// <summary>Creates a fixed-height multiline text editor. Long text wraps and the
        /// native InputField viewport scrolls as the caret moves through the content.</summary>
        public InputField AddTextArea(string placeholder, string initialText = "", float height = 96f, Action<string> callback = null)
        {
            InputField input = WindowManager.SpawnInputbox();
            input.lineType = InputField.LineType.MultiLineNewline;
            if (input.placeholder is Text) ((Text)input.placeholder).text = placeholder ?? "";
            if (input.textComponent != null)
            {
                input.textComponent.alignment = TextAnchor.UpperLeft;
                input.textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
                input.textComponent.verticalOverflow = VerticalWrapMode.Overflow;
            }
            input.text = initialText ?? "";
            if (callback != null) input.onEndEdit.AddListener(value => callback(value ?? ""));
            Attach(input.gameObject, Mathf.Clamp(height, 64f, 360f));
            return input;
        }

        /// <summary>Creates a native one-column Software Inc. list. The callback fires when
        /// the selection changes and receives the selected row index and value.</summary>
        public GUIListView AddList(string[] items, int selectedIndex = 0, float height = 144f, Action<int, string> callback = null)
        {
            GUIListView list = WindowManager.SpawnList();
            if (list == null) throw new InvalidOperationException("SI ModKit: SpawnList returned null.");
            list.IgnoreTranslation = true;
            float columnWidth = Mathf.Max(110f, fieldWidth - 24f);
            list.AddColumn(new GUIListView.ColumnDefinition<string>("Item", delegate(string x) { return x ?? ""; }, false, columnWidth, false, true));
            EventList<object> data = new EventList<object>();
            string[] safeItems = items == null || items.Length == 0 ? new[] { "Item" } : items;
            for (int i = 0; i < safeItems.Length; i++) data.Add(safeItems[i] ?? "");
            list.Items = data;
            if (callback != null)
            {
                list.OnSelectChange = delegate(bool direct)
                {
                    string selected = list.GetFirstSelected<string>();
                    int index = selected == null ? -1 : list.ActualItems.IndexOf(selected);
                    callback(index, selected ?? "");
                };
            }
            Attach(list.gameObject, Mathf.Clamp(height, 96f, 420f));
            if (safeItems.Length > 0) list.Select(Mathf.Clamp(selectedIndex, 0, safeItems.Length - 1));
            return list;
        }

        /// <summary>Creates a native Software Inc. table from string columns/rows. This is a
        /// deliberately simple data table; callers can use Native GUIListView APIs for advanced columns.</summary>
        public GUIListView AddTable(string[] columns, string[][] rows, float height = 180f, Action<int, string[]> callback = null)
        {
            GUIListView table = WindowManager.SpawnList();
            if (table == null) throw new InvalidOperationException("SI ModKit: SpawnList returned null.");
            table.IgnoreTranslation = true;
            string[] safeColumns = columns == null || columns.Length == 0 ? new[] { "Value" } : columns;
            float width = Mathf.Max(72f, (fieldWidth - 16f) / safeColumns.Length);
            for (int i = 0; i < safeColumns.Length; i++)
            {
                int columnIndex = i;
                string header = string.IsNullOrEmpty(safeColumns[i]) ? "Column " + (i + 1) : safeColumns[i];
                table.AddColumn(new GUIListView.ColumnDefinition<string[]>(header,
                    delegate(string[] row) { return row != null && columnIndex < row.Length ? (row[columnIndex] ?? "") : ""; },
                    false, width, false, true));
            }
            EventList<object> data = new EventList<object>();
            if (rows != null)
                for (int i = 0; i < rows.Length; i++) data.Add(rows[i] ?? new string[0]);
            table.Items = data;
            if (callback != null)
            {
                table.OnSelectChange = delegate(bool direct)
                {
                    string[] selected = table.GetFirstSelected<string[]>();
                    int index = selected == null ? -1 : table.ActualItems.IndexOf(selected);
                    callback(index, selected ?? new string[0]);
                };
            }
            Attach(table.gameObject, Mathf.Clamp(height, 110f, 480f));
            return table;
        }
        /// <summary>Multi-line editor anchored to all four window edges; its bottom expands on resize.
        /// top is measured from the top of MainPanel; bottom is inset from its bottom.</summary>
        public InputField AddStretchTextArea(string placeholder, float top, float bottom = 14f)
        {
            InputField input = WindowManager.SpawnInputbox();
            input.lineType = InputField.LineType.MultiLineNewline;
            if (input.placeholder is Text) ((Text)input.placeholder).text = placeholder;
            if (input.textComponent != null)
            {
                input.textComponent.alignment = TextAnchor.UpperLeft;
                input.textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
                input.textComponent.verticalOverflow = VerticalWrapMode.Overflow;
            }
            // Anchors in WindowManager use top-down coordinates, while offsets use edge positions.
            WindowManager.AddElementToWindow(input.gameObject, NativeWindow,
                new Rect(contentMargin, top, -contentMargin * 2f, -bottom - top), new Rect(0f, 0f, 1f, 1f));
            ownedControls.Add(input.gameObject);
            return input;
        }

        /// <summary>Creates a native Software Inc. combobox with string items.
        /// selectedIndex is clamped to the available items. Optional callback receives
        /// both the selected index and current display string.
        /// </summary>
        public GUICombobox AddDropdown(string[] items, int selectedIndex = 0, Action<int, string> callback = null)
        {
            GUICombobox combo = WindowManager.SpawnComboBox();
            if (combo == null) throw new InvalidOperationException("SI ModKit: SpawnComboBox returned null.");
            string[] safeItems = items == null || items.Length == 0 ? new[] { "Option" } : items;
            combo.LocalizeContent = false;
            combo.UpdateContent(safeItems);
            combo.Selected = Mathf.Clamp(selectedIndex, 0, safeItems.Length - 1);
            if (callback != null)
            {
                combo.OnSelectedChangedRemoveAllListeners();
                combo.OnSelectedChanged.AddListener(() => callback(combo.Selected, combo.SelectedItemString ?? ""));
            }
            Attach(combo.gameObject, Height);
            return combo;
        }

        /// <summary>Creates a native Software Inc. slider. Values are clamped to the supplied range.
        /// The optional callback receives the live slider value whenever it changes.</summary>
        public Slider AddSlider(float minValue, float maxValue, float value, Action<float> callback = null)
        {
            Slider slider = WindowManager.SpawnSlider();
            if (slider == null) throw new InvalidOperationException("SI ModKit: SpawnSlider returned null.");
            if (maxValue < minValue)
            {
                float swap = minValue;
                minValue = maxValue;
                maxValue = swap;
            }
            if (Mathf.Approximately(minValue, maxValue)) maxValue = minValue + 1f;
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = Mathf.Clamp(value, minValue, maxValue);
            slider.onValueChanged.RemoveAllListeners();
            if (callback != null) slider.onValueChanged.AddListener(v => callback(v));
            Attach(slider.gameObject, Height);
            return slider;
        }

        /// <summary>Creates a native Software Inc. progress bar. The supplied value is
        /// normalized from minValue/maxValue into the game's 0..1 progress range.</summary>
        public GUIProgressBar AddProgressBar(float minValue, float maxValue, float value)
        {
            GUIProgressBar progress = WindowManager.SpawnProgressbar();
            if (progress == null) throw new InvalidOperationException("SI ModKit: SpawnProgressbar returned null.");
            if (maxValue < minValue)
            {
                float swap = minValue;
                minValue = maxValue;
                maxValue = swap;
            }
            float normalized = Mathf.Approximately(minValue, maxValue) ? 0f : Mathf.InverseLerp(minValue, maxValue, value);
            progress.Value = normalized;
            Attach(progress.gameObject, Height);
            return progress;
        }

        public Toggle AddCheckbox(string title, bool defaultValue = false, Action<bool> callback = null)
        {
            Toggle toggle = WindowManager.SpawnCheckbox();
            Text label = toggle.GetComponentInChildren<Text>();
            if (label != null) label.text = title;
            toggle.isOn = defaultValue;
            if (callback != null) toggle.onValueChanged.AddListener(value => callback(value));
            Attach(toggle.gameObject, Height);
            return toggle;
        }

        /// <summary>Creates a button that opens a native Software Inc. confirmation dialog.</summary>
        public Button AddConfirmButton(string title, string message, string confirmLabel, Action onConfirm, Action onCancel = null)
        {
            return AddButton(title, () => UI.ShowConfirmation(message, onConfirm, onCancel, confirmLabel));
        }
        public Button AddButton(string title, Action callback)
        {
            Button button = WindowManager.SpawnButton();
            Text label = button.GetComponentInChildren<Text>();
            if (label != null) label.text = title;
            if (callback != null) button.onClick.AddListener(() => callback());
            Attach(button.gameObject, Height);
            return button;
        }
        /// <summary>Creates a compact row of equally sized buttons at the next layout position.</summary>
        public Button[] AddButtonRow(string[] titles, Action[] callbacks)
        {
            EndRow();
            if (titles == null || titles.Length == 0) return new Button[0];
            Button[] result = new Button[titles.Length];
            float spacing = 6f;
            float width = (fieldWidth - (titles.Length - 1) * spacing) / titles.Length;
            for (int i = 0; i < titles.Length; i++)
            {
                Button b = WindowManager.SpawnButton();
                Text label = b.GetComponentInChildren<Text>();
                if (label != null) label.text = titles[i] ?? "";
                Action callback = callbacks != null && i < callbacks.Length ? callbacks[i] : null;
                if (callback != null) b.onClick.AddListener(() => callback());
                float aMin = (float)i / titles.Length;
                float aMax = (float)(i + 1) / titles.Length;
                float left = i == 0 ? contentMargin : spacing * 0.5f;
                float right = i == titles.Length - 1 ? -contentMargin : -spacing * 0.5f;
                WindowManager.AddElementToWindow(b.gameObject, NativeWindow,
                    new Rect(left, nextY, right - left, Height),
                    new Rect(aMin, 0f, aMax - aMin, 0f));
                ownedControls.Add(b.gameObject);
                result[i] = b;
            }
            nextY += Height + Gap;
            return result;
        }
        /// <summary>Rebuild toolkit-owned content without destroying or re-centering its native window.
        /// Do not call on a window containing unmanaged controls.</summary>
        public void ClearContent()
        {
            foreach (GameObject control in ownedControls)
                if (control != null)
                {
                    control.SetActive(false);
                    UnityEngine.Object.Destroy(control);
                }
            ownedControls.Clear();
            nextY = 12f;
            rowColumns = 0;
            rowSlot = 0;
            rowHeight = 0f;
            sectionDepth = 0;
        }
        public void Show()
        {
            if (NativeWindow == null || NativeWindow.Shown) return;

            // Register the window at the dimensions the toolkit actually laid out for.
            // WindowManager.RegisterWindow clamps against both MinSize and the UI-scaled
            // screen bounds; temporarily using the requested size as MinSize prevents the
            // native frame from becoming narrower than its controls on first show.
            Vector2 requestedSize = NativeWindow.rectTransform == null
                ? NativeWindow.MinSize
                : NativeWindow.rectTransform.sizeDelta;
            int minimumWidth = toolbarMode ? 160 : 320;
            int minimumHeight = toolbarMode ? 80 : 240;
            NativeWindow.MinSize = new Vector2(
                Mathf.Max(minimumWidth, requestedSize.x),
                Mathf.Max(minimumHeight, requestedSize.y));

            NativeWindow.Toggle();

            // Once registered, restore the behavioral resize minimum. The current dimensions
            // stay unchanged; this only controls how far a resizable window may later shrink.
            NativeWindow.MinSize = new Vector2(minimumWidth, minimumHeight);
            SetMovable(movableRequested);
            SetResizable(resizableRequested);
            SetClosable(closableRequested);
        }
        public void Hide() { if (NativeWindow != null && NativeWindow.Shown) NativeWindow.Toggle(); }
        public void Destroy()
        {
            if (NativeWindow != null)
            {
                UnityEngine.Object.Destroy(NativeWindow.gameObject);
                NativeWindow = null;
            }
            UI.Forget(this);
        }
    }
}
