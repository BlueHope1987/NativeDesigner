using System;
using System.Drawing;
using System.Windows.Forms;
using CloudNativeDesigner.Core;

namespace CloudNativeDesigner.Controls
{
    /// <summary>
    /// 图形状态编辑对话框。用于添加或编辑单个 ShapeState。
    /// </summary>
    public class ShapeStateEditDialog : Form
    {
        private TextBox _txtName;
        private Button _btnFillColor;
        private Button _btnBorderColor;
        private Button _btnTextColor;
        private Button _btnHeaderColor;
        private NumericUpDown _numPriority;
        private Button _btnOk;
        private Button _btnCancel;
        private ColorDialog _colorDialog;

        private Color _fillColor;
        private Color _borderColor;
        private Color _textColor;
        private Color _headerColor;

        public ShapeState ResultState { get; private set; }

        public ShapeStateEditDialog() : this(null) { }

        public ShapeStateEditDialog(ShapeState editState)
        {
            this.Text = (editState == null) ? "添加状态" : "编辑状态";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(320, 260);

            _colorDialog = new ColorDialog();

            int y = 12;
            int lblW = 70;
            int xVal = 85;

            // 名称
            Label lblName = new Label();
            lblName.Text = "名称：";
            lblName.Location = new Point(10, y);
            lblName.Size = new Size(lblW, 20);
            lblName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Controls.Add(lblName);

            _txtName = new TextBox();
            _txtName.Location = new Point(xVal, y);
            _txtName.Size = new Size(210, 22);
            _txtName.Text = (editState != null) ? editState.Name : "Normal";
            this.Controls.Add(_txtName);
            y += 34;

            // 填充颜色
            Label lblFill = new Label();
            lblFill.Text = "填充颜色：";
            lblFill.Location = new Point(10, y);
            lblFill.Size = new Size(lblW, 24);
            lblFill.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Controls.Add(lblFill);

            _btnFillColor = new Button();
            _btnFillColor.Location = new Point(xVal, y);
            _btnFillColor.Size = new Size(80, 24);
            _btnFillColor.Click += new EventHandler(OnPickFillColor);
            this.Controls.Add(_btnFillColor);
            y += 34;

            // 边框颜色
            Label lblBorder = new Label();
            lblBorder.Text = "边框颜色：";
            lblBorder.Location = new Point(10, y);
            lblBorder.Size = new Size(lblW, 24);
            lblBorder.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Controls.Add(lblBorder);

            _btnBorderColor = new Button();
            _btnBorderColor.Location = new Point(xVal, y);
            _btnBorderColor.Size = new Size(80, 24);
            _btnBorderColor.Click += new EventHandler(OnPickBorderColor);
            this.Controls.Add(_btnBorderColor);
            y += 34;

            // 文字颜色
            Label lblText = new Label();
            lblText.Text = "文字颜色：";
            lblText.Location = new Point(10, y);
            lblText.Size = new Size(lblW, 24);
            lblText.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Controls.Add(lblText);

            _btnTextColor = new Button();
            _btnTextColor.Location = new Point(xVal, y);
            _btnTextColor.Size = new Size(80, 24);
            _btnTextColor.Click += new EventHandler(OnPickTextColor);
            this.Controls.Add(_btnTextColor);
            y += 34;

            // 标题颜色
            Label lblHeader = new Label();
            lblHeader.Text = "标题颜色：";
            lblHeader.Location = new Point(10, y);
            lblHeader.Size = new Size(lblW, 24);
            lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Controls.Add(lblHeader);

            _btnHeaderColor = new Button();
            _btnHeaderColor.Location = new Point(xVal, y);
            _btnHeaderColor.Size = new Size(80, 24);
            _btnHeaderColor.Click += new EventHandler(OnPickHeaderColor);
            this.Controls.Add(_btnHeaderColor);
            y += 34;

            // 优先级
            Label lblPriority = new Label();
            lblPriority.Text = "优先级：";
            lblPriority.Location = new Point(10, y);
            lblPriority.Size = new Size(lblW, 22);
            lblPriority.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Controls.Add(lblPriority);

            _numPriority = new NumericUpDown();
            _numPriority.Location = new Point(xVal, y);
            _numPriority.Size = new Size(80, 22);
            _numPriority.Minimum = 0;
            _numPriority.Maximum = 100;
            _numPriority.Value = (editState != null) ? editState.Priority : 0;
            this.Controls.Add(_numPriority);

            // 初始化颜色
            if (editState != null)
            {
                _fillColor = editState.FillColor.ToColor();
                _borderColor = editState.BorderColor.ToColor();
                _textColor = editState.TextColor.ToColor();
                _headerColor = editState.HeaderColor.ToColor();
            }
            else
            {
                _fillColor = Color.FromArgb(230, 240, 255);
                _borderColor = Color.FromArgb(80, 120, 180);
                _textColor = Color.FromArgb(40, 40, 40);
                _headerColor = Color.FromArgb(80, 130, 180);
            }
            UpdateColorButtons();

            // 确定/取消
            _btnOk = new Button();
            _btnOk.Text = "确定";
            _btnOk.DialogResult = DialogResult.OK;
            _btnOk.Location = new Point(130, 220);
            _btnOk.Size = new Size(80, 28);
            this.Controls.Add(_btnOk);

            _btnCancel = new Button();
            _btnCancel.Text = "取消";
            _btnCancel.DialogResult = DialogResult.Cancel;
            _btnCancel.Location = new Point(220, 220);
            _btnCancel.Size = new Size(80, 28);
            this.Controls.Add(_btnCancel);

            this.AcceptButton = _btnOk;
            this.CancelButton = _btnCancel;
        }

        private void UpdateColorButtons()
        {
            _btnFillColor.BackColor = _fillColor;
            _btnBorderColor.BackColor = _borderColor;
            _btnTextColor.BackColor = _textColor;
            _btnHeaderColor.BackColor = _headerColor;

            // 根据亮度决定按钮文字颜色
            _btnFillColor.ForeColor = GetContrastColor(_fillColor);
            _btnBorderColor.ForeColor = GetContrastColor(_borderColor);
            _btnTextColor.ForeColor = GetContrastColor(_textColor);
            _btnHeaderColor.ForeColor = GetContrastColor(_headerColor);
        }

        private Color GetContrastColor(Color c)
        {
            float brightness = c.R * 0.299f + c.G * 0.587f + c.B * 0.114f;
            return brightness > 128 ? Color.Black : Color.White;
        }

        private void OnPickFillColor(object sender, EventArgs e)
        {
            _colorDialog.Color = _fillColor;
            if (_colorDialog.ShowDialog() == DialogResult.OK)
            {
                _fillColor = _colorDialog.Color;
                UpdateColorButtons();
            }
        }

        private void OnPickBorderColor(object sender, EventArgs e)
        {
            _colorDialog.Color = _borderColor;
            if (_colorDialog.ShowDialog() == DialogResult.OK)
            {
                _borderColor = _colorDialog.Color;
                UpdateColorButtons();
            }
        }

        private void OnPickTextColor(object sender, EventArgs e)
        {
            _colorDialog.Color = _textColor;
            if (_colorDialog.ShowDialog() == DialogResult.OK)
            {
                _textColor = _colorDialog.Color;
                UpdateColorButtons();
            }
        }

        private void OnPickHeaderColor(object sender, EventArgs e)
        {
            _colorDialog.Color = _headerColor;
            if (_colorDialog.ShowDialog() == DialogResult.OK)
            {
                _headerColor = _colorDialog.Color;
                UpdateColorButtons();
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            if (this.DialogResult == DialogResult.OK)
            {
                string name = _txtName.Text.Trim();
                if (string.IsNullOrEmpty(name))
                    name = "Normal";

                ResultState = new ShapeState();
                ResultState.Name = name;
                ResultState.FillColor = new XmlColor(_fillColor);
                ResultState.BorderColor = new XmlColor(_borderColor);
                ResultState.TextColor = new XmlColor(_textColor);
                ResultState.HeaderColor = new XmlColor(_headerColor);
                ResultState.Priority = (int)_numPriority.Value;
            }
        }
    }
}
