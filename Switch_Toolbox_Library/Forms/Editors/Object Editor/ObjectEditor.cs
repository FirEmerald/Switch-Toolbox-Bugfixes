﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GL_EditorFramework.Interfaces;
using GL_EditorFramework.EditorDrawables;
using System.Text.RegularExpressions;
using Switch_Toolbox.Library.Animations;
using Switch_Toolbox.Library.IO;

namespace Switch_Toolbox.Library.Forms
{
    public partial class ObjectEditor : STForm
    {
        private ObjectEditorTree ObjectTree;
        private ObjectEditorList ObjectList; //Optionally usable for archives

        private TreeView _fieldsTreeCache;

        public void BeginUpdate() { ObjectTree.BeginUpdate(); }
        public void EndUpdate() { ObjectTree.EndUpdate(); }

        public void AddNodeCollection (TreeNodeCollection nodes, bool ClearNodes)
        {
            if (ObjectTree != null) {
                ObjectTree.AddNodeCollection(nodes,ClearNodes);
            }
        }

        public TreeNodeCollection GetNodes() { return ObjectTree.GetNodes(); }

        public void AddNode(TreeNode node, bool ClearAllNodes = false)
        {
            if (ObjectTree != null) {
                ObjectTree.AddNode(node, ClearAllNodes);
            }
        }

        private void AddNodes(TreeNode node, bool ClearAllNodes = false)
        {
            if (ObjectTree != null) {
                ObjectTree.AddNode(node, ClearAllNodes);
            }
        }

        public void ClearNodes()
        {
            if (ObjectTree != null) {
                ObjectTree.ClearNodes();
            }
        }

        public bool AddFilesToActiveEditor
        {
            get
            {
                return ObjectTree.AddFilesToActiveEditor;
            }
            set
            {
                ObjectTree.AddFilesToActiveEditor = value;
            }
        }

        public bool UseListView = true;

        public ObjectEditor()
        {
            InitializeComponent();

            ObjectTree = new ObjectEditorTree(this);
            ObjectTree.Dock = DockStyle.Fill;
            stPanel1.Controls.Add(ObjectTree);
        }

        public ObjectEditor(IFileFormat FileFormat)
        {
            InitializeComponent();

            if (FileFormat is IArchiveFile)
            {
                /* ObjectList = new ObjectEditorList();
                 ObjectList.Dock = DockStyle.Fill;
                 stPanel1.Controls.Add(ObjectList);
                 ObjectList.FillList((IArchiveFile)FileFormat);*/

                ObjectTree = new ObjectEditorTree(this);
                ObjectTree.Dock = DockStyle.Fill;
                stPanel1.Controls.Add(ObjectTree);
                AddIArchiveFile(FileFormat);
            }
            else
            {
                ObjectTree = new ObjectEditorTree(this);
                ObjectTree.Dock = DockStyle.Fill;
                stPanel1.Controls.Add(ObjectTree);
                AddNode((TreeNode)FileFormat);
            }
        }

        public void AddIArchiveFile(IFileFormat FileFormat)
        {
            TreeNode FileRoot = new ArchiveRootNodeWrapper(FileFormat.FileName, (IArchiveFile)FileFormat);
            FillTreeNodes(FileRoot, (IArchiveFile)FileFormat);
            AddNode(FileRoot);
        }

        void FillTreeNodes(TreeNode root, IArchiveFile archiveFile)
        {
            var rootText = root.Text;
            var rootTextLength = rootText.Length;
            var nodeFiles = archiveFile.Files;
            if (nodeFiles.Count() > 400)
            {
                foreach (var node in nodeFiles)
                {
                    ArchiveFileWrapper wrapperFile = new ArchiveFileWrapper(node.FileName, node, archiveFile);
                    root.Nodes.Add(wrapperFile);
                }
            }
            else
            {
                foreach (var node in nodeFiles)
                {
                    string nodeString = node.FileName;

                    var roots = nodeString.Split(new char[] { '/' },
                        StringSplitOptions.RemoveEmptyEntries);

                    // The initial parent is the root node
                    var parentNode = root;
                    var sb = new StringBuilder(rootText, nodeString.Length + rootTextLength);
                    for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
                    {
                        // Build the node name
                        var parentName = roots[rootIndex];
                        sb.Append("/");
                        sb.Append(parentName);
                        var nodeName = sb.ToString();

                        // Search for the node
                        var index = parentNode.Nodes.IndexOfKey(nodeName);
                        if (index == -1)
                        {
                            // Node was not found, add it

                            var folder = new ArchiveFolderNodeWrapper(parentName, archiveFile);

                            if (rootIndex == roots.Length - 1)
                            {
                                ArchiveFileWrapper wrapperFile = new ArchiveFileWrapper(parentName, node, archiveFile);
                                wrapperFile.Name = nodeName;
                                parentNode.Nodes.Add(wrapperFile);
                                parentNode = wrapperFile;
                            }
                            else
                            {
                                folder.Name = nodeName;
                                parentNode.Nodes.Add(folder);
                                parentNode = folder;
                            }
                        }
                        else
                        {
                            // Node was found, set that as parent and continue
                            parentNode = parentNode.Nodes[index];
                        }
                    }
                }
            }
        }


        public Viewport GetViewport() => viewport;

        //Attatch a viewport instance here if created.
        //If the editor gets switched, we can keep the previous viewed area when switched back
        Viewport viewport = null;

        bool IsLoaded = false;
        public void LoadViewport(Viewport Viewport)
        {
            viewport = Viewport;

            IsLoaded = true;
        }

        public List<DrawableContainer> DrawableContainers = new List<DrawableContainer>();

        public static List<DrawableContainer> GetDrawableContainers()
        {
            var editor = LibraryGUI.Instance.GetObjectEditor();
            if (editor == null)
                return new List<DrawableContainer>();

           return editor.DrawableContainers;
        }

        public static void AddContainer(DrawableContainer drawable)
        {
            var editor = LibraryGUI.Instance.GetObjectEditor();
            if (editor == null)
                return;

            editor.DrawableContainers.Add(drawable);
        }

        public static void RemoveContainer(DrawableContainer drawable)
        {
            var editor = LibraryGUI.Instance.GetObjectEditor();
            if (editor == null)
                return;

            editor.DrawableContainers.Remove(drawable);
        }

        public List<Control> GetEditors()
        {
            if (ObjectTree != null)
                return ObjectTree.GetEditors();
            else
                return new List<Control>();
        }

        public IFileFormat GetActiveFile()
        {
            if (ObjectTree != null)
                return ObjectTree.GetActiveFile();
            else
                return ObjectList.GetActiveFile();
        }

        public void LoadEditor(Control control)
        {
            ObjectTree.LoadEditor(control);
        }

        private void ObjectEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            Viewport viewport = LibraryGUI.Instance.GetActiveViewport();

            if (viewport != null)
                viewport.FormClosing();

            if (ObjectTree != null)
                ObjectTree.FormClosing();
        }

        public void RemoveFile(TreeNode File)
        {
            if (File is IFileFormat) {
                ((IFileFormat)File).Unload();
            }

            ObjectTree.RemoveFile(File);
        }

        public void ResetControls()
        {
            ObjectTree.ResetControls();
            Text = "";
        }
    }
}
