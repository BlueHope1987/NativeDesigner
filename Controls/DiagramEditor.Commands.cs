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

        private void OnViewStatusBar(object sender, EventArgs e)
        {
            ShowStatusBar = !ShowStatusBar;
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

            UpdateMenuAvailability();
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
                ShowCanvasContextMenu(e.Location);
            }
        }

        #endregion

        private void UpdateMenuAvailability()
        {
            List<ShapeBase> selectedShapes = _canvas.Document.GetSelectedShapes();
            bool hasSelection = (selectedShapes.Count > 0);

            if (_menuEditDelete != null)
                _menuEditDelete.Enabled = hasSelection;

            if (_menuShapeToFront != null)
                _menuShapeToFront.Enabled = hasSelection;
            if (_menuShapeToBack != null)
                _menuShapeToBack.Enabled = hasSelection;

            bool canAddMember = false;
            bool canSwitchState = false;

            if (selectedShapes.Count == 1)
            {
                GenericShape gs = selectedShapes[0] as GenericShape;
                if (gs != null)
                {
                    ShapeType st = ShapeTypeRegistry.Instance.GetShapeType(gs.ShapeTypeName);
                    if (st != null && st.SupportsMembers)
                    {
                        canAddMember = true;
                        canSwitchState = (gs.States.Count > 1);
                    }
                }
            }

            if (_menuShapeAddMember != null)
                _menuShapeAddMember.Enabled = canAddMember;
            if (_menuShapeSwitchState != null)
                _menuShapeSwitchState.Enabled = canSwitchState;
        }

        #region 右键菜单

        private void ShowCanvasContextMenu(Point location)
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
                    LoadIcon("add_member.png"), new EventHandler(OnCtxAddMember));
                ToolStripMenuItem itemSwitchState = new ToolStripMenuItem("切换状态",
                    LoadIcon("switch_state.png"), new EventHandler(OnCtxSwitchState));
                menu.Items.Add(itemAddMember);
                menu.Items.Add(itemSwitchState);
                menu.Items.Add(new ToolStripSeparator());
            }

            ToolStripMenuItem itemDelete = new ToolStripMenuItem("删除",
                LoadIcon("delete.png"), new EventHandler(OnCtxDelete));
            itemDelete.ShortcutKeyDisplayString = "Delete";
            menu.Items.Add(itemDelete);

            if (selectedShapes.Count > 1)
            {
                ToolStripMenuItem itemFront = new ToolStripMenuItem("置顶",
                    LoadIcon("to_front.png"), new EventHandler(OnCtxFront));
                ToolStripMenuItem itemBack = new ToolStripMenuItem("置底",
                    LoadIcon("to_back.png"), new EventHandler(OnCtxBack));
                menu.Items.Add(itemFront);
                menu.Items.Add(itemBack);
            }

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem itemProps = new ToolStripMenuItem("属性...",
                LoadIcon("properties.png"), new EventHandler(OnCtxProps));
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
            ShowPropertyPanel = true;
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
