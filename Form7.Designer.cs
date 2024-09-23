namespace PDFiumTest
{
	partial class Form7
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
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.comboBox2 = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.textBox6 = new System.Windows.Forms.TextBox();
			this.textBox4 = new System.Windows.Forms.TextBox();
			this.button2 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// checkBox1
			// 
			this.checkBox1.AutoSize = true;
			this.checkBox1.Location = new System.Drawing.Point(118, 99);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(106, 16);
			this.checkBox1.TabIndex = 26;
			this.checkBox1.Text = "マルチページTIFF";
			this.checkBox1.UseVisualStyleBackColor = true;
			// 
			// comboBox2
			// 
			this.comboBox2.FormattingEnabled = true;
			this.comboBox2.Items.AddRange(new object[] {
            "100",
            "200",
            "300",
            "400",
            "600"});
			this.comboBox2.Location = new System.Drawing.Point(80, 6);
			this.comboBox2.Name = "comboBox2";
			this.comboBox2.Size = new System.Drawing.Size(147, 20);
			this.comboBox2.TabIndex = 18;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(11, 33);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(59, 12);
			this.label2.TabIndex = 19;
			this.label2.Text = "画像形式：";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(56, 58);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(112, 12);
			this.label8.TabIndex = 21;
			this.label8.Text = "JPEG品質（0～100）：";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(21, 80);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(147, 12);
			this.label11.TabIndex = 23;
			this.label11.Text = "BMP白黒しきい値（0～255）：";
			// 
			// comboBox1
			// 
			this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Items.AddRange(new object[] {
            "Bitmap（フルカラー）",
            "Bitmap（グレースケール）",
            "Bitmap（白黒2値）",
            "Jpeg",
            "PNG",
            "Tiff（フルカラー）",
            "Tiff（白黒2値）"});
			this.comboBox1.Location = new System.Drawing.Point(80, 30);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(147, 20);
			this.comboBox1.TabIndex = 20;
			this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 9);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(47, 12);
			this.label3.TabIndex = 17;
			this.label3.Text = "解像度：";
			// 
			// textBox6
			// 
			this.textBox6.Location = new System.Drawing.Point(172, 77);
			this.textBox6.Name = "textBox6";
			this.textBox6.Size = new System.Drawing.Size(55, 19);
			this.textBox6.TabIndex = 24;
			// 
			// textBox4
			// 
			this.textBox4.Location = new System.Drawing.Point(172, 55);
			this.textBox4.Name = "textBox4";
			this.textBox4.Size = new System.Drawing.Size(55, 19);
			this.textBox4.TabIndex = 22;
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(47, 122);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(86, 24);
			this.button2.TabIndex = 25;
			this.button2.Text = "OK";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// button1
			// 
			this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button1.Location = new System.Drawing.Point(141, 121);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(86, 24);
			this.button1.TabIndex = 25;
			this.button1.Text = "キャンセル";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// Form7
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(238, 153);
			this.Controls.Add(this.checkBox1);
			this.Controls.Add(this.comboBox2);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.label11);
			this.Controls.Add(this.comboBox1);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.textBox6);
			this.Controls.Add(this.textBox4);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.button2);
			this.Name = "Form7";
			this.Text = "画像変換";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form7_FormClosing);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.ComboBox comboBox2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textBox6;
		private System.Windows.Forms.TextBox textBox4;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button1;
	}
}