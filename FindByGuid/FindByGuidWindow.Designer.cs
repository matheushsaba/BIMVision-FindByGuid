namespace FindByGuid
{
    partial class FindByGuidWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._findButton = new System.Windows.Forms.Button();
            this._guidTextBox = new System.Windows.Forms.TextBox();
            this._guidLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _findButton
            // 
            this._findButton.Location = new System.Drawing.Point(214, 12);
            this._findButton.Name = "_findButton";
            this._findButton.Size = new System.Drawing.Size(75, 23);
            this._findButton.TabIndex = 0;
            this._findButton.Text = "Find";
            this._findButton.UseVisualStyleBackColor = true;
            this._findButton.Click += new System.EventHandler(this.FindGuid_Click);
            // 
            // _guidTextBox
            // 
            this._guidTextBox.Location = new System.Drawing.Point(53, 14);
            this._guidTextBox.Name = "_guidTextBox";
            this._guidTextBox.Size = new System.Drawing.Size(155, 20);
            this._guidTextBox.TabIndex = 1;
            this._guidTextBox.TextChanged += new System.EventHandler(this.GuidTextBox_TextChanged);
            // 
            // _guidLaBel
            // 
            this._guidLabel.AutoSize = true;
            this._guidLabel.Location = new System.Drawing.Point(12, 17);
            this._guidLabel.Name = "_guidLaBel";
            this._guidLabel.Size = new System.Drawing.Size(34, 13);
            this._guidLabel.TabIndex = 2;
            this._guidLabel.Text = "GUID";
            // 
            // FindGuidWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(303, 49);
            this.Controls.Add(this._guidLabel);
            this.Controls.Add(this._guidTextBox);
            this.Controls.Add(this._findButton);
            this.Name = "FindGuidWindow";
            this.Text = "Find Guid";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FindGuidWindow_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _findButton;
        private System.Windows.Forms.TextBox _guidTextBox;
        private System.Windows.Forms.Label _guidLabel;
    }
}