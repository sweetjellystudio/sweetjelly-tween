using System;
using System.Collections.Generic;
using SWeetJelly.Tween.App;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Framework.Unity;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SWeetJelly.Tween.Editor
{
    public sealed class UITKTweenEditor : EditorWindow
    {
        [SerializeField] private UITKTweenAsset asset;
        private UITKTweenNode selected;
        private ScrollView elements, nodes, inspector;
        private UITKTweenEditorLayout layout;
        private Toolbar graphToolbar;
        private Label addDestination;
        private readonly HashSet<UITKTweenNode> collapsed = new();
        private UITKTweenPreviewViewport canvas;
        private VisualElement previewRoot;
        private Label message;
        private Slider scrub;
        private ObjectField assetField;
        private TweenService service;
        private UITKTweenPlayer player;
        private UITKTweenTimeline timeline;
        private UITKTweenControl control;
        private double lastTime;
        private bool changingScrub;

        [MenuItem("Window/SWeetJelly/Tween UI Toolkit")]
        public static UITKTweenEditor Open() => GetWindow<UITKTweenEditor>("UITK Tween");
        public static UITKTweenEditor Open(UITKTweenAsset asset)
        {
            var window = Open(); window.SetAsset(asset); window.Show(); return window;
        }
        [OnOpenAsset] private static bool OnOpenAsset(int id, int line)
        {
            if (EditorUtility.InstanceIDToObject(id) is not UITKTweenAsset tween) return false;
            Open(tween); return true;
        }

        private void OnEnable()
        {
            minSize = new Vector2(1000, 650);
            Undo.undoRedoPerformed += OnUndo;
            EditorApplication.projectChanged += OnProjectChanged;
            EditorApplication.update += Tick;
        }
        private void OnDisable()
        {
            StopPreview();
            Undo.undoRedoPerformed -= OnUndo;
            EditorApplication.projectChanged -= OnProjectChanged;
            EditorApplication.update -= Tick;
        }

        public void CreateGUI()
        {
            rootVisualElement.Clear();
            var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.sweetjelly.tween/Editor/UITKTweenEditor.uss");
            if (sheet != null) rootVisualElement.styleSheets.Add(sheet);
            rootVisualElement.AddToClassList("tween-editor");
            // Restore before descendants receive detach and invalidate their runtime binding.
            rootVisualElement.UnregisterCallback<DetachFromPanelEvent>(OnPanelDetaching, TrickleDown.TrickleDown);
            rootVisualElement.RegisterCallback<DetachFromPanelEvent>(OnPanelDetaching, TrickleDown.TrickleDown);
            var toolbar = new Toolbar();
            assetField = new ObjectField("Tween") { name = "tween-asset", objectType = typeof(UITKTweenAsset), allowSceneObjects = false, value = asset };
            assetField.RegisterValueChangedCallback(evt => SetAsset(evt.newValue as UITKTweenAsset));
            toolbar.Add(assetField);
            toolbar.Add(new ToolbarButton(Save) { text = "Save", name = "save-tween" });
            toolbar.Add(new ToolbarButton(LoadDocument) { text = "Reload UXML", name = "reload-document" });
            toolbar.Add(new ToolbarButton(() => layout.Reset()) { text = "Reset Layout", name = "reset-layout", tooltip = "Restore panel sizes. Canvas and Tween settings are retained." });
            rootVisualElement.Add(toolbar);
            canvas = new UITKTweenPreviewViewport(ResetPreview);
            rootVisualElement.Add(canvas.Controls);
            elements = new ScrollView(ScrollViewMode.VerticalAndHorizontal) { name = "element-tree" }; elements.AddToClassList("elements");
            inspector = new ScrollView { name = "node-inspector" }; inspector.AddToClassList("inspector");
            var playback = new Toolbar();
            playback.Add(new ToolbarButton(Play) { text = "Play", name = "preview-play" });
            playback.Add(new ToolbarButton(() => Try(() => control?.Pause())) { text = "Pause", name = "preview-pause" });
            playback.Add(new ToolbarButton(() => Try(() => { EnsureControl(); control.ResumeAsync(false); lastTime = EditorApplication.timeSinceStartup; })) { text = "Reverse", name = "preview-reverse" });
            playback.Add(new ToolbarButton(ResetPreview) { text = "Reset / Restore", name = "preview-reset" });
            scrub = new Slider("Seconds", 0, 1) { name = "preview-time" };
            scrub.RegisterValueChangedCallback(evt => { if (!changingScrub) Try(() => { EnsureControl(); control.Sample(evt.newValue); }); });
            playback.Add(scrub);
            graphToolbar = new Toolbar(); graphToolbar.AddToClassList("graph-toolbar");
            graphToolbar.Add(new ToolbarButton(() => AddNode(UITKTweenNodeKind.Property)) { text = "+ Property", name = "add-property" });
            graphToolbar.Add(new ToolbarButton(() => AddNode(UITKTweenNodeKind.Sequence)) { text = "+ Sequence", name = "add-sequence" });
            graphToolbar.Add(new ToolbarButton(() => AddNode(UITKTweenNodeKind.Group)) { text = "+ Group", name = "add-group" });
            graphToolbar.Add(new ToolbarButton(() => Move(-1)) { text = "Up", name = "move-up" });
            graphToolbar.Add(new ToolbarButton(() => Move(1)) { text = "Down", name = "move-down" });
            graphToolbar.Add(new ToolbarButton(RemoveNode) { text = "Remove", name = "remove-node" });
            addDestination = new Label { name = "add-destination" }; addDestination.AddToClassList("help-text");
            nodes = new ScrollView(ScrollViewMode.VerticalAndHorizontal) { name = "tween-nodes" }; nodes.AddToClassList("nodes");
            var hierarchy = Pane("hierarchy-pane", "Tween Hierarchy", "Group: parallel. Sequence: sequential.");
            hierarchy.Add(graphToolbar); hierarchy.Add(addDestination); hierarchy.Add(nodes);
            var left = Pane("elements-pane", "UXML Elements", "Select a Property tween, then choose its UXML target."); left.Add(elements);
            var preview = Pane("preview-pane", "Preview", null); preview.Add(canvas);
            var right = Pane("inspector-pane", "Tween Inspector", null); right.Add(inspector);
            layout = new UITKTweenEditorLayout(left, preview, right, hierarchy, playback);
            rootVisualElement.Add(layout);
            message = new Label("Select a Tween asset to begin.") { name = "tween-diagnostics" }; message.AddToClassList("message"); rootVisualElement.Add(message);
            selected = asset != null ? asset.Root : null;
            RefreshNodes(); BuildInspector(); LoadDocument();
        }

        private static Label Help(string text)
        {
            var label = new Label(text); label.AddToClassList("help-text"); return label;
        }
        private static Label Section(string text)
        {
            var label = new Label(text); label.AddToClassList("section-title"); return label;
        }
        private static VisualElement Pane(string name, string title, string help)
        {
            var pane = new VisualElement { name = name }; pane.AddToClassList("editor-pane");
            var heading = new Label(title); heading.AddToClassList("pane-title"); pane.Add(heading);
            if (help != null) pane.Add(Help(help));
            return pane;
        }
        private static void SizeListText(Button button)
        {
            button.RegisterCallback<AttachToPanelEvent>(_ => button.schedule.Execute(() =>
                button.style.minWidth = button.MeasureTextSize(button.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined).x + 24));
        }

        private void SetAsset(UITKTweenAsset next)
        {
            StopPreview(); collapsed.Clear(); asset = next; selected = asset != null ? asset.Root : null;
            if (assetField == null) return;
            assetField.SetValueWithoutNotify(asset);
            RefreshNodes(); BuildInspector(); LoadDocument();
        }

        private void BeginEdit()
        {
            StopPreview();
            Undo.RecordObject(asset, "Edit UITK Tween");
        }
        private void Changed() { EditorUtility.SetDirty(asset); RefreshNodes(); message.text = "Edited. Save to persist; Play or scrub to preview."; }
        private void Save()
        {
            if (asset == null) return;
            AssetDatabase.SaveAssetIfDirty(asset); message.text = "Saved. Preview values are never saved to UXML or USS.";
        }

        private void LoadDocument()
        {
            if (canvas == null) return;
            StopPreview(); canvas.Document.Clear(); elements.Clear(); previewRoot = null;
            if (asset == null || asset.AuthoringDocument == null) { message.text = "Assign a production UXML document in the inspector."; return; }
            var template = asset.AuthoringDocument.Instantiate(); template.AddToClassList("preview-template");
            canvas.Document.Add(template);
            previewRoot = template.childCount == 1 ? template[0] : template;
            previewRoot.RegisterCallback<PointerDownEvent>(evt =>
            {
                var target = evt.target as VisualElement;
                while (target != null && target != previewRoot && string.IsNullOrEmpty(target.name)) target = target.parent;
                if (target != null) SelectTarget(target);
                evt.StopPropagation();
            }, TrickleDown.TrickleDown);
            void AddElement(VisualElement element, int depth)
            {
                var target = element;
                var button = new Button(() => SelectTarget(target))
                { text = new string(' ', depth * 2) + (element == previewRoot ? "(root) " : "") + element.GetType().Name + " #" + element.name };
                button.tooltip = button.text; SizeListText(button); elements.Add(button);
                foreach (var child in element.Children()) AddElement(child, depth + 1);
            }
            AddElement(previewRoot, 0);
            message.text = "Choose a property node, then select a target in the element tree or preview.";
        }

        private void SelectTarget(VisualElement element)
        {
            if (selected == null || selected.Kind != UITKTweenNodeKind.Property) { message.text = "Select or add a property node first."; return; }
            if (element != previewRoot && string.IsNullOrEmpty(element.name)) { message.text = "Give this element a unique name in its UXML first."; return; }
            BeginEdit(); selected.TargetName = element == previewRoot ? "" : element.name; Changed(); BuildInspector();
        }

        private void RefreshNodes()
        {
            if (nodes == null) return;
            nodes.Clear(); graphToolbar.SetEnabled(asset != null);
            var destination = AddParent();
            addDestination.text = asset == null ? "Select a Tween asset." : destination != null
                ? "Add to: " + destination.Label + " / " + KindText(destination)
                : asset.Root == null ? "Add a root tween." : "Add beside the root in a new Sequence.";
            var selectedParent = asset != null && selected != null ? ParentOf(selected) : null;
            int selectedIndex = selectedParent?.Children.IndexOf(selected) ?? -1;
            graphToolbar.Q<Button>("move-up").SetEnabled(selectedIndex > 0);
            graphToolbar.Q<Button>("move-down").SetEnabled(selectedIndex >= 0 && selectedIndex + 1 < selectedParent.Children.Count);
            graphToolbar.Q<Button>("remove-node").SetEnabled(selected != null);
            if (asset == null || asset.Root == null) return;
            var visited = new HashSet<UITKTweenNode>();
            void Add(UITKTweenNode node, VisualElement container)
            {
                if (node == null) { container.Add(Help("Missing node")); return; }
                if (!visited.Add(node)) { container.Add(Help("Cycle/shared node: remove the invalid reference.")); return; }
                var item = node;
                var row = new VisualElement(); row.AddToClassList("node-row"); container.Add(row);
                var children = new VisualElement(); children.AddToClassList("node-children");
                var button = new Button(() => SelectNode(item)) { text = NodeText(node), userData = node, tooltip = NodeText(node) };
                button.AddToClassList("node-select");
                SizeListText(button);
                if (node.Kind != UITKTweenNodeKind.Property)
                {
                    bool closed = collapsed.Contains(node);
                    var disclosure = new Button { name = "toggle-children", text = closed ? "+" : "−", userData = node, tooltip = "Expand or collapse children" };
                    disclosure.AddToClassList("node-disclosure");
                    children.EnableInClassList("collapsed-children", closed);
                    disclosure.clicked += () =>
                    {
                        if (!collapsed.Add(item)) collapsed.Remove(item);
                        bool isClosed = collapsed.Contains(item); disclosure.text = isClosed ? "+" : "−";
                        children.EnableInClassList("collapsed-children", isClosed);
                    };
                    row.Add(disclosure);
                }
                else row.Add(new VisualElement { name = "leaf-indent" });
                if (ReferenceEquals(selected, node)) button.AddToClassList("selected-node");
                row.Add(button); container.Add(children);
                if (node.Children != null) foreach (var child in node.Children) Add(child, children);
            }
            Add(asset.Root, nodes);
        }

        private static string KindText(UITKTweenNode node) => node.Kind switch
        {
            UITKTweenNodeKind.Group => "Group (parallel)",
            UITKTweenNodeKind.Sequence => "Sequence (sequential)",
            _ => "Property"
        };
        private static string NodeText(UITKTweenNode node) => node.Label + " · " + KindText(node) +
            (node.Kind == UITKTweenNodeKind.Property ? " / " + node.Property + " / " + (string.IsNullOrEmpty(node.TargetName) ? "(root)" : node.TargetName) : " (" + (node.Children?.Count ?? 0) + ")");
        private void SelectNode(UITKTweenNode node)
        {
            selected = node;
            var seen = new HashSet<UITKTweenNode>();
            for (var parent = ParentOf(node); parent != null && seen.Add(parent); parent = ParentOf(parent)) collapsed.Remove(parent);
            RefreshNodes(); BuildInspector();
        }
        private UITKTweenNode AddParent() => selected != null && selected.Kind != UITKTweenNodeKind.Property ? selected : asset != null ? ParentOf(selected) : null;

        private void BuildInspector()
        {
            if (inspector == null) return;
            inspector.Clear();
            if (asset == null) { inspector.Add(Help("Select a Tween asset to edit its settings.")); return; }
            inspector.Add(Section("Asset Settings"));
            var document = new ObjectField("Production UXML") { name = "authoring-document", objectType = typeof(VisualTreeAsset), allowSceneObjects = false, value = asset.AuthoringDocument };
            document.RegisterValueChangedCallback(evt => { BeginEdit(); asset.AuthoringDocument = evt.newValue as VisualTreeAsset; Changed(); LoadDocument(); });
            inspector.Add(document);
            var lane = new EnumField("Default lane", asset.UpdateMethod) { name = "default-lane" };
            lane.RegisterValueChangedCallback(evt => { BeginEdit(); asset.UpdateMethod = (UpdateMethod)evt.newValue; Changed(); }); inspector.Add(lane);
            var unscaled = new Toggle("Default unscaled") { name = "default-unscaled", value = asset.UnscaledTime };
            unscaled.RegisterValueChangedCallback(evt => { BeginEdit(); asset.UnscaledTime = evt.newValue; Changed(); }); inspector.Add(unscaled);
            if (selected == null) return;
            var node = selected;
            inspector.Add(Section("Selected Tween"));
            inspector.Add(Help(KindText(node)));
            var parent = ParentOf(node);
            inspector.Add(Help(parent == null ? "Root tween" : "Parent: " + parent.Label + " / " + KindText(parent)));
            var label = new TextField("Label") { name = "node-label", value = node.Label, isDelayed = true };
            label.RegisterValueChangedCallback(evt => { BeginEdit(); node.Label = evt.newValue; Changed(); }); inspector.Add(label);
            BuildMoveTo(node);
            if (node.Kind != UITKTweenNodeKind.Property)
            {
                inspector.Add(Help(node.Kind == UITKTweenNodeKind.Group
                    ? "Children play together. Group duration is the longest child duration."
                    : "Children play in order, from top to bottom. Sequence duration is the sum of child durations."));
                inspector.Add(Section("Add Child"));
                inspector.Add(Help("Add to: " + node.Label));
                var actions = new VisualElement(); actions.AddToClassList("child-actions");
                foreach (var kind in new[] { UITKTweenNodeKind.Property, UITKTweenNodeKind.Sequence, UITKTweenNodeKind.Group })
                {
                    var childKind = kind;
                    actions.Add(new Button(() => AddNode(childKind, node)) { name = "add-child-" + kind.ToString().ToLowerInvariant(), text = kind.ToString() });
                }
                inspector.Add(actions);
                inspector.Add(Section("Children (" + (node.Children?.Count ?? 0) + ")"));
                inspector.Add(Help("Select a child here or in Tween Hierarchy. Use Up/Down to reorder siblings, or Remove to delete the selected subtree."));
                if (node.Children == null || node.Children.Count == 0) inspector.Add(Help("No children. Use Add Child above."));
                else foreach (var child in node.Children)
                {
                    if (child == null) { inspector.Add(Help("Missing node")); continue; }
                    var item = child;
                    var childButton = new Button(() => SelectNode(item)) { text = NodeText(child), userData = child, tooltip = NodeText(child) };
                    childButton.AddToClassList("child-select"); inspector.Add(childButton);
                }
                return;
            }
            inspector.Add(new TextField("Target") { name = "node-target", value = string.IsNullOrEmpty(node.TargetName) ? "(root)" : node.TargetName, isReadOnly = true });
            var property = new EnumField("Property", node.Property) { name = "node-property" };
            property.RegisterValueChangedCallback(evt => { BeginEdit(); node.Property = (UITKTweenProperty)evt.newValue; Changed(); BuildInspector(); }); inspector.Add(property);
            var current = new Toggle("Capture current start") { name = "from-current", value = node.FromCurrent };
            current.RegisterValueChangedCallback(evt => { BeginEdit(); node.FromCurrent = evt.newValue; Changed(); }); inspector.Add(current);
            if (node.Property == UITKTweenProperty.Opacity)
            {
                Float("Start opacity", "from-opacity", node.From.x, value => node.From = new Vector2(value, 0));
                Float("End opacity", "to-opacity", node.To.x, value => node.To = new Vector2(value, 0));
            }
            else
            {
                Vector("Start " + (node.Property == UITKTweenProperty.Translate ? "px" : "factor"), "from-vector", node.From, value => node.From = value);
                Vector("End " + (node.Property == UITKTweenProperty.Translate ? "px" : "factor"), "to-vector", node.To, value => node.To = value);
            }
            Float("Duration (s)", "duration", node.Duration, value => node.Duration = value);
            Float("Initial delay (s)", "delay", node.Delay, value => node.Delay = value);
            var repeat = new IntegerField("Repetitions (total)") { name = "repetitions", value = node.Repetitions, isDelayed = true };
            repeat.RegisterValueChangedCallback(evt => { BeginEdit(); node.Repetitions = evt.newValue; Changed(); }); inspector.Add(repeat);
            var loop = new EnumField("Loop", node.Loop) { name = "loop", value = node.Loop };
            loop.RegisterValueChangedCallback(evt => { BeginEdit(); node.Loop = (UITKTweenLoop)evt.newValue; Changed(); }); inspector.Add(loop);
            var easing = new EnumField("Easing", node.Easing) { name = "easing" };
            easing.RegisterValueChangedCallback(evt => { BeginEdit(); node.Easing = (EaseType)evt.newValue; Changed(); }); inspector.Add(easing);
            var useCurve = new Toggle("Use custom curve") { name = "use-curve", value = node.UseCurve };
            useCurve.RegisterValueChangedCallback(evt => { BeginEdit(); node.UseCurve = evt.newValue; Changed(); }); inspector.Add(useCurve);
            var curve = new CurveField("Curve") { name = "curve", value = node.Curve };
            curve.RegisterValueChangedCallback(evt => { BeginEdit(); node.Curve = evt.newValue; Changed(); }); inspector.Add(curve);
        }

        private void BuildMoveTo(UITKTweenNode node)
        {
            var candidates = new List<UITKTweenNode>();
            var paths = new List<string> { "Choose parent..." };
            var seen = new HashSet<UITKTweenNode>();
            var currentParent = ParentOf(node);
            void Collect(UITKTweenNode candidate, string path)
            {
                if (candidate == null || !seen.Add(candidate)) return;
                if (candidate.Kind != UITKTweenNodeKind.Property && candidate != currentParent && !ContainsNode(node, candidate))
                { candidates.Add(candidate); paths.Add(path + " / " + KindText(candidate)); }
                if (candidate.Children != null) for (int i = 0; i < candidate.Children.Count; i++)
                {
                    var child = candidate.Children[i];
                    Collect(child, path + " / " + (i + 1) + ": " + (child?.Label ?? "Missing"));
                }
            }
            Collect(asset.Root, asset.Root?.Label ?? "Root");
            var row = new VisualElement(); row.AddToClassList("move-parent");
            var field = new DropdownField("Move To", paths, 0) { name = "move-to-parent", tooltip = "Move the selected subtree to the end of another Group or Sequence." };
            var move = new Button(() =>
            {
                int index = field.index - 1;
                if (index >= 0 && index < candidates.Count) MoveTo(candidates[index]);
            }) { name = "move-to-apply", text = "Move" };
            field.SetEnabled(currentParent != null && candidates.Count > 0); move.SetEnabled(false);
            field.RegisterValueChangedCallback(_ => move.SetEnabled(field.index > 0));
            row.Add(field); row.Add(move); inspector.Add(row);
        }

        private static bool ContainsNode(UITKTweenNode root, UITKTweenNode target)
        {
            if (root == null || target == null) return false;
            var pending = new Stack<UITKTweenNode>(); var visited = new HashSet<UITKTweenNode>(); pending.Push(root);
            while (pending.Count > 0)
            {
                var node = pending.Pop(); if (node == null || !visited.Add(node)) continue;
                if (ReferenceEquals(node, target)) return true;
                if (node.Children != null) foreach (var child in node.Children) pending.Push(child);
            }
            return false;
        }

        private void MoveTo(UITKTweenNode parent)
        {
            var previous = ParentOf(selected);
            if (previous == null || parent == null || parent == previous || parent.Kind == UITKTweenNodeKind.Property ||
                !ContainsNode(asset.Root, parent) || ContainsNode(selected, parent)) return;
            BeginEdit(); previous.Children.Remove(selected); parent.Children ??= new(); parent.Children.Add(selected);
            collapsed.Remove(parent); Changed(); SelectNode(selected);
        }

        private void Float(string label, string name, float value, Action<float> apply)
        {
            var field = new FloatField(label) { name = name, value = value, isDelayed = true };
            field.RegisterValueChangedCallback(evt => { BeginEdit(); apply(evt.newValue); Changed(); }); inspector.Add(field);
        }
        private void Vector(string label, string name, Vector2 value, Action<Vector2> apply)
        {
            var field = new Vector2Field(label) { name = name, value = value };
            field.Query<FloatField>().ForEach(input => input.isDelayed = true);
            field.RegisterValueChangedCallback(evt => { BeginEdit(); apply(evt.newValue); Changed(); }); inspector.Add(field);
        }

        private UITKTweenNode ParentOf(UITKTweenNode target)
        {
            if (asset == null || target == null) return null;
            var seen = new HashSet<UITKTweenNode>();
            UITKTweenNode Find(UITKTweenNode node)
            {
                if (node == null || !seen.Add(node) || node.Children == null) return null;
                if (node.Children.Contains(target)) return node;
                foreach (var child in node.Children) { var parent = Find(child); if (parent != null) return parent; }
                return null;
            }
            return Find(asset.Root);
        }
        private void AddNode(UITKTweenNodeKind kind) => AddNode(kind, AddParent());
        private void AddNode(UITKTweenNodeKind kind, UITKTweenNode parent)
        {
            if (asset == null) return;
            if (parent != null && (parent.Kind == UITKTweenNodeKind.Property || !ContainsNode(asset.Root, parent))) return;
            BeginEdit();
            var node = new UITKTweenNode { Kind = kind, Label = kind.ToString() };
            if (asset.Root == null) asset.Root = node;
            else if (parent != null) { parent.Children ??= new(); parent.Children.Add(node); collapsed.Remove(parent); }
            else asset.Root = new UITKTweenNode { Kind = UITKTweenNodeKind.Sequence, Label = "Sequence", Children = new() { asset.Root, node } };
            selected = node; Changed(); BuildInspector();
        }
        private void RemoveNode()
        {
            if (asset == null || selected == null) return;
            BeginEdit(); var parent = ParentOf(selected);
            if (parent != null) parent.Children.Remove(selected);
            else asset.Root = new UITKTweenNode { Kind = UITKTweenNodeKind.Sequence, Label = "Sequence" };
            selected = parent ?? asset.Root; Changed(); BuildInspector();
        }
        private void Move(int direction)
        {
            if (asset == null || selected == null) return;
            var parent = ParentOf(selected); if (parent == null) return;
            int before = parent.Children.IndexOf(selected), after = before + direction;
            if (after < 0 || after >= parent.Children.Count) return;
            BeginEdit(); parent.Children.RemoveAt(before); parent.Children.Insert(after, selected); Changed(); BuildInspector();
        }

        private void EnsureControl()
        {
            if (asset == null || previewRoot == null) throw new InvalidOperationException("Assign the production UXML first.");
            if (!canvas.IsLayoutReady) throw new InvalidOperationException("Wait for the preview canvas layout to resolve after changing its size.");
            if (player == null)
            {
                try
                {
                    service = new TweenService(); player = asset.Bind(previewRoot, service, UpdateMethod.Update, true);
                    timeline = player.PrepareTimeline();
                }
                catch { StopPreview(); throw; }
                changingScrub = true; scrub.highValue = timeline.Duration; changingScrub = false;
            }
            if (control == null || !control.IsActive) control = timeline.TakeControl();
        }
        private void Play() => Try(() => { EnsureControl(); control.ResumeAsync(); lastTime = EditorApplication.timeSinceStartup; });
        private void ResetPreview() { StopPreview(); changingScrub = true; scrub.SetValueWithoutNotify(0); changingScrub = false; message.text = "Restored original inline/USS state."; }
        private void StopPreview()
        {
            try { if (timeline != null) timeline.Restore(); }
            catch (InvalidOperationException) { /* A reimport may already have discarded the disposable tree. */ }
            finally { control?.Dispose(); player?.Dispose(); service?.Dispose(); control = null; player = null; service = null; timeline = null; }
        }
        private void Tick()
        {
            double now = EditorApplication.timeSinceStartup;
            float delta = (float)Math.Max(0, now - lastTime); lastTime = now;
            if (service == null || control == null || !control.IsAutomatic) return;
            Try(() =>
            {
                service.Tick(UpdateMethod.Update, delta, delta, true);
                changingScrub = true; scrub.SetValueWithoutNotify(control.Position); changingScrub = false;
                message.text = $"Preview {control.Position:0.000} / {control.Duration:0.000} seconds";
            });
        }
        private void Try(Action action)
        {
            try { action(); }
            catch (Exception error) { if (message != null) message.text = error.Message; }
        }
        private void OnUndo() { StopPreview(); collapsed.Clear(); selected = asset != null ? asset.Root : null; RefreshNodes(); BuildInspector(); LoadDocument(); }
        private void OnProjectChanged() { if (canvas != null) LoadDocument(); }
        private void OnPanelDetaching(DetachFromPanelEvent evt)
        {
            if (ReferenceEquals(evt.target, rootVisualElement)) StopPreview();
        }
    }

    [CustomEditor(typeof(UITKTweenAsset))]
    internal sealed class UITKTweenAssetInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.Add(new HelpBox("Edit targets, composition and preview in the UITK Tween window. The consumer supplies the runtime root.", HelpBoxMessageType.Info));
            root.Add(new Button(() => UITKTweenEditor.Open((UITKTweenAsset)target)) { text = "Open UITK Tween Editor" });
            return root;
        }
    }
}
