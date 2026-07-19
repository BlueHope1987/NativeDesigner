using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CloudNativeDesigner.Config;
using CloudNativeDesigner.Core;
using CloudNativeDesigner.Serialization;
using CloudNativeDesigner.Shapes;

namespace CloudNativeDesigner.Controls
{
    public partial class DiagramEditor : UserControl
    {
        #region 菜单事件处理

        private void OnFileNew(object sender, EventArgs e)
        {
            NewDocument();
            OnDocNew(EventArgs.Empty);
        }

        private void OnFileOpen(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "XML 文件 (*.xml)|*.xml|所有文件 (*.*)|*.*";
            dlg.DefaultExt = "xml";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    LoadDocument(dlg.FileName);
                    OnDocLoaded(EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("打开失败: " + ex.Message, "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnFileSave(object sender, EventArgs e)
        {
            if (_currentFilePath.Length > 0)
            {
                try
                {
                    SaveDocument(_currentFilePath);
                    OnDocSaved(EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("保存失败: " + ex.Message, "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                OnFileSaveAs(sender, e);
            }
        }

        private void OnFileSaveAs(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "XML 文件 (*.xml)|*.xml|所有文件 (*.*)|*.*";
            dlg.DefaultExt = "xml";
            if (_currentFilePath.Length > 0)
                dlg.FileName = _currentFilePath;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    SaveDocument(dlg.FileName);
                    OnDocSaved(EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("保存失败: " + ex.Message, "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnFileExit(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            if (parentForm != null)
                parentForm.Close();
        }

        private void OnEditDelete(object sender, EventArgs e)
        {
            DeleteSelected();
        }

        private void OnEditSelectAll(object sender, EventArgs e)
        {
            SelectAll();
        }

        private void OnViewGrid(object sender, EventArgs e)
        {
            GlobalConfig.Instance.ShowGrid = !GlobalConfig.Instance.ShowGrid;
            _menuViewGrid.Checked = GlobalConfig.Instance.ShowGrid;
        }

        private void OnViewSnap(object sender, EventArgs e)
        {
            GlobalConfig.Instance.SnapToGrid = !GlobalConfig.Instance.SnapToGrid;
            _menuViewSnap.Checked = GlobalConfig.Instance.SnapToGrid;
        }

        private void OnViewZoomIn(object sender, EventArgs e)
        {
            ZoomIn();
        }

        private void OnViewZoomOut(object sender, EventArgs e)
        {
            ZoomOut();
        }

        private void OnViewReset(object sender, EventArgs e)
        {
            ZoomReset();
        }

        private void OnViewToolbar(object sender, EventArgs e)
        {
            ShowToolbar = !ShowToolbar;
        }

        private void OnViewProperty(object sender, EventArgs e)
        {
            ShowPropertyPanel = !ShowPropertyPanel;
        }

        private void OnViewToolbox(object sender, EventArgs e)
        {
            ShowToolboxPanel = !ShowToolboxPanel;
        }

        private void OnViewContextMenu(object sender, EventArgs e)
        {
            ShowContextMenu = !ShowContextMenu;
        }

        private void OnViewToolbarText(object sender, EventArgs e)
        {
            ShowToolbarText = !ShowToolbarText;
        }

        private void OnThemeLight(object sender, EventArgs e)
        {
            Theme = EditorTheme.Light;
            OnThemeChanged(EventArgs.Empty);
        }

        private void OnThemeDark(object sender, EventArgs e)
        {
            Theme = EditorTheme.Dark;
            OnThemeChanged(EventArgs.Empty);
        }

        private void OnToolSelect(object sender, EventArgs e)
        {
            SetTool(CanvasTool.Select);
        }

        private void OnToolConnect(object sender, EventArgs e)
        {
            SetTool(CanvasTool.Connect);
        }

        private void OnToolStraight(object sender, EventArgs e)
        {
            SetConnectionMode(ConnectionMode.Straight);
        }

        private void OnToolCurve(object sender, EventArgs e)
        {
            SetConnectionMode(ConnectionMode.Curve);
        }

        private void OnToolOrtho(object sender, EventArgs e)
        {
            SetConnectionMode(ConnectionMode.Orthogonal);
        }

        #endregion

        #region 工具栏事件处理

        private void OnSelectTool(object sender, EventArgs e)
        {
            SetTool(CanvasTool.Select);
        }

        private void OnConnectTool(object sender, EventArgs e)
        {
            SetTool(CanvasTool.Connect);
        }

        private void OnStraightMode(object sender, EventArgs e)
        {
            SetConnectionMode(ConnectionMode.Straight);
        }

        private void OnCurveMode(object sender, EventArgs e)
        {
            SetConnectionMode(ConnectionMode.Curve);
        }

        private void OnOrthoMode(object sender, EventArgs e)
        {
            SetConnectionMode(ConnectionMode.Orthogonal);
        }

        private void OnZoomIn(object sender, EventArgs e)
        {
            ZoomIn();
        }

        private void OnZoomOut(object sender, EventArgs e)
        {
            ZoomOut();
        }

        private void OnZoomFit(object sender, EventArgs e)
        {
            ZoomReset();
        }

        private void OnBringToFront(object sender, EventArgs e)
        {
            BringToFront();
        }

        private void OnSendToBack(object sender, EventArgs e)
        {
            SendToBack();
        }

        private void OnDeleteSelected(object sender, EventArgs e)
        {
            DeleteSelected();
        }

        #endregion

        #region 画布事件处理

        private void OnCanvasSelectionChanged(object sender, EventArgs e)
        {
            List<ShapeBase> shapes = _canvas.Document.GetSelectedShapes();
            List<Connection> conns = _canvas.Document.GetSelectedConnections();

            if (shapes.Count == 1 && conns.Count == 0)
            {
                _propertyGrid.SelectedObject = shapes[0];
            }
            else if (shapes.Count == 0 && conns.Count == 1)
            {
                _propertyGrid.SelectedObject = conns[0];
            }
            else if (shapes.Count > 1)
            {
                object[] arr = new object[shapes.Count];
                for (int i = 0; i < shapes.Count; i++)
                    arr[i] = shapes[i];
                _propertyGrid.SelectedObjects = arr;
            }
            else
            {
                _propertyGrid.SelectedObject = GlobalConfig.Instance;
            }

            _statusLabel.Text = string.Format("选中: {0} 个实体, {1} 条连线",
                shapes.Count, conns.Count);
        }

        private void OnDocumentModified(object sender, EventArgs e)
        {
            _statusLabel.Text = "文档已修改";
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            PointF world = _canvas.ScreenToWorld(e.Location);
            _zoomLabel.Text = string.Format("缩放: {0:0}% | 坐标: ({1:0}, {2:0})",
                _canvas.Zoom * 100, world.X, world.Y);
        }

        private void OnCanvasMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && _contextMenuEnabled)
            {
                ShowContextMenu(e.Location);
            }
        }

        #endregion

        #region 右键菜单

        private void ShowContextMenu(Point location)
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            List<ShapeBase> selectedShapes = _canvas.Document.GetSelectedShapes();
            bool hasSingleGenericShape = false;
            bool supportsMembers = false;

            if (selectedShapes.Count == 1)
            {
                GenericShape gs = selectedShapes[0] as GenericShape;
                if (gs != null)
                {
                    hasSingleGenericShape = true;
                    ShapeType st = ShapeTypeRegistry.Instance.GetShapeType(gs.ShapeTypeName);
                    if (st != null && st.SupportsMembers)
                    {
                        supportsMembers = true;
                    }
                }
            }

            if (hasSingleGenericShape && supportsMembers)
            {
                ToolStripMenuItem itemAddMember = new ToolStripMenuItem("添加成员",
                    null, new EventHandler(OnCtxAddMember));
                ToolStripMenuItem itemSwitchState = new ToolStripMenuItem("切换状态",
                    null, new EventHandler(OnCtxSwitchState));
                menu.Items.Add(itemAddMember);
                menu.Items.Add(itemSwitchState);
                menu.Items.Add(new ToolStripSeparator());
            }

            ToolStripMenuItem itemDelete = new ToolStripMenuItem("删除",
                null, new EventHandler(OnCtxDelete));
            itemDelete.ShortcutKeyDisplayString = "Delete";
            menu.Items.Add(itemDelete);

            if (selectedShapes.Count > 1)
            {
                ToolStripMenuItem itemFront = new ToolStripMenuItem("置顶",
                    null, new EventHandler(OnCtxFront));
                ToolStripMenuItem itemBack = new ToolStripMenuItem("置底",
                    null, new EventHandler(OnCtxBack));
                menu.Items.Add(itemFront);
                menu.Items.Add(itemBack);
            }

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem itemProps = new ToolStripMenuItem("属性...",
                null, new EventHandler(OnCtxProps));
            menu.Items.Add(itemProps);

            menu.Show(_canvas, location);
        }

        private void OnCtxDelete(object sender, EventArgs e)
        {
            DeleteSelected();
        }

        private void OnCtxFront(object sender, EventArgs e)
        {
            BringToFront();
        }

        private void OnCtxBack(object sender, EventArgs e)
        {
            SendToBack();
        }

        private void OnCtxProps(object sender, EventArgs e)
        {
            _propertyGrid.Focus();
        }

        private void OnCtxAddMember(object sender, EventArgs e)
        {
            List<ShapeBase> shapes = _canvas.Document.GetSelectedShapes();
            if (shapes.Count == 1)
            {
                GenericShape genericShape = shapes[0] as GenericShape;
                if (genericShape != null)
                {
                    ShapeMember m = new ShapeMember();
                    m.Name = "NewMember";
                    m.TypeName = "string";
                    genericShape.Members.Add(m);
                    _canvas.Invalidate();
                }
            }
        }

        private void OnCtxSwitchState(object sender, EventArgs e)
        {
            List<ShapeBase> shapes = _canvas.Document.GetSelectedShapes();
            if (shapes.Count == 1)
            {
                GenericShape genericShape = shapes[0] as GenericShape;
                if (genericShape != null && genericShape.States.Count > 1)
                {
                    int idx = 0;
                    for (int i = 0; i < genericShape.States.Count; i++)
                    {
                        if (genericShape.States[i].Name == genericShape.CurrentStateName)
                        {
                            idx = i;
                            break;
                        }
                    }
                    idx = (idx + 1) % genericShape.States.Count;
                    genericShape.CurrentStateName = genericShape.States[idx].Name;
                    _canvas.Invalidate();
                }
            }
        }

        #endregion
    }
}
