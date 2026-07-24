using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CloudNativeDesigner.Core;

namespace CloudNativeDesigner.Controls
{
    /// <summary>
    /// 工具箱配置对话框。
    /// 左侧：已注册图形多选列表（勾选=启用，取消=禁用）
    /// 右侧：上移/下移/添加自定义工具
    /// </summary>
    public class ToolboxConfigDialog : Form
    {
        private CheckedListBox _listTools;
        private Button _btnUp;
        private Button _btnDown;
        private Button _btnAddCustom;
        private Button _btnOk;
        private Button _btnCancel;
        private List<ToolboxItem> _originalItems;
        private List<ToolboxItem> _workingItems;

        public List<ToolboxItem> ResultItems { get { return _workingItems; } }

        public ToolboxConfigDialog(List<ToolboxItem> currentItems, ShapeTypeRegistry registry)
        {
            _originalItems = currentItems;
            _workingItems = new List<ToolboxItem>();
            foreach (ToolboxItem item in currentItems)
                _workingItems.Add(item);

            this.Text = "工具箱配置";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(420, 400);
            this.Shown += new EventHandler(OnShown);

            _listTools = new CheckedListBox();
            _listTools.Location = new Point(10, 10);
            _listTools.Size = new Size(280, 340);
            _listTools.CheckOnClick = true;
            _listTools.MultiColumn = false;
            _listTools.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(_listTools);

            _btnUp = new Button();
            _btnUp.Text = "上移";
            _btnUp.Location = new Point(300, 10);
            _btnUp.Size = new Size(110, 30);
            _btnUp.Click += new EventHandler(OnMoveUp);
            this.Controls.Add(_btnUp);

            _btnDown = new Button();
            _btnDown.Text = "下移";
            _btnDown.Location = new Point(300, 48);
            _btnDown.Size = new Size(110, 30);
            _btnDown.Click += new EventHandler(OnMoveDown);
            this.Controls.Add(_btnDown);

            _btnAddCustom = new Button();
            _btnAddCustom.Text = "添加自定义工具...";
            _btnAddCustom.Location = new Point(300, 86);
            _btnAddCustom.Size = new Size(110, 30);
            _btnAddCustom.Click += new EventHandler(OnAddCustom);
            this.Controls.Add(_btnAddCustom);

            _btnOk = new Button();
            _btnOk.Text = "确定";
            _btnOk.DialogResult = DialogResult.OK;
            _btnOk.Location = new Point(230, 360);
            _btnOk.Size = new Size(80, 30);
            this.Controls.Add(_btnOk);

            _btnCancel = new Button();
            _btnCancel.Text = "取消";
            _btnCancel.DialogResult = DialogResult.Cancel;
            _btnCancel.Location = new Point(320, 360);
            _btnCancel.Size = new Size(80, 30);
            this.Controls.Add(_btnCancel);

            PopulateList();
        }

        private void OnShown(object sender, EventArgs e)
        {
            _btnOk.Focus();
        }

        private void PopulateList()
        {
            _listTools.Items.Clear();

            // 收集已注册图形中不在列表中的项，追加到末尾（未启用）
            List<string> existingNames = new List<string>();
            foreach (ToolboxItem item in _workingItems)
                existingNames.Add(item.Name);

            List<string> categories = ShapeTypeRegistry.Instance.GetCategories();
            List<ToolboxItem> allItems = new List<ToolboxItem>(_workingItems);

            foreach (string category in categories)
            {
                List<ShapeType> types = ShapeTypeRegistry.Instance.GetTypesByCategory(category);
                foreach (ShapeType type in types)
                {
                    if (!existingNames.Contains(type.Name))
                    {
                        ToolboxItem item = new ToolboxItem();
                        item.Name = type.Name;
                        item.Category = type.Category;
                        item.CreateShape = new CreateShapeHandler(type.CreateInstance);
                        item.Visible = false;
                        allItems.Add(item);
                    }
                }
            }

            _workingItems.Clear();
            foreach (ToolboxItem item in allItems)
                _workingItems.Add(item);

            // 填充列表
            foreach (ToolboxItem item in _workingItems)
            {
                string display = string.IsNullOrEmpty(item.Category) ? item.Name
                    : string.Format("[{0}] {1}", item.Category, item.Name);
                _listTools.Items.Add(display, item.Visible);
            }
        }

        private void OnMoveUp(object sender, EventArgs e)
        {
            if (_listTools.SelectedIndex <= 0) return;
            int idx = _listTools.SelectedIndex;
            SwapItems(idx, idx - 1);
            _listTools.SelectedIndex = idx - 1;
        }

        private void OnMoveDown(object sender, EventArgs e)
        {
            if (_listTools.SelectedIndex < 0 || _listTools.SelectedIndex >= _listTools.Items.Count - 1) return;
            int idx = _listTools.SelectedIndex;
            SwapItems(idx, idx + 1);
            _listTools.SelectedIndex = idx + 1;
        }

        private void SwapItems(int a, int b)
        {
            // 交换 working list
            ToolboxItem tmp = _workingItems[a];
            _workingItems[a] = _workingItems[b];
            _workingItems[b] = tmp;

            // 交换 checked list
            bool checkedA = _listTools.GetItemChecked(a);
            bool checkedB = _listTools.GetItemChecked(b);
            object textA = _listTools.Items[a];

            _listTools.Items[a] = _listTools.Items[b];
            _listTools.SetItemChecked(a, checkedB);
            _listTools.Items[b] = textA;
            _listTools.SetItemChecked(b, checkedA);
        }

        private void OnAddCustom(object sender, EventArgs e)
        {
            using (CustomShapeDialog dlg = new CustomShapeDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK && dlg.ResultShapeType != null)
                {
                    ShapeType st = dlg.ResultShapeType;

                    ToolboxItem item = new ToolboxItem();
                    item.Name = st.Name;
                    item.Category = "自定义";
                    item.CreateShape = new CreateShapeHandler(st.CreateInstance);
                    item.Visible = true;
                    item.Icon = null;

                    // 注册到全局
                    ShapeTypeRegistry.Instance.Register(st);

                    _workingItems.Add(item);
                    string display = string.Format("[自定义] {0}", st.Name);
                    _listTools.Items.Add(display, true);
                    _listTools.SelectedIndex = _listTools.Items.Count - 1;
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (this.DialogResult == DialogResult.OK)
            {
                // 同步勾选状态到 Visible
                for (int i = 0; i < _listTools.Items.Count && i < _workingItems.Count; i++)
                {
                    _workingItems[i].Visible = _listTools.GetItemChecked(i);
                }
            }
        }
    }
}
