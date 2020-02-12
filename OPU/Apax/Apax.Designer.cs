namespace Apax
{
    partial class Apax
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.onesec = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.отладкаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DebugOn = new System.Windows.Forms.ToolStripMenuItem();
            this.DebugOff = new System.Windows.Forms.ToolStripMenuItem();
            this.LoopStop = new System.Windows.Forms.ToolStripMenuItem();
            this.LoopStart = new System.Windows.Forms.ToolStripMenuItem();
            this.ShowServer = new System.Windows.Forms.ToolStripMenuItem();
            this.устройстваToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StopDevices = new System.Windows.Forms.ToolStripMenuItem();
            this.StartDevices = new System.Windows.Forms.ToolStripMenuItem();
            this.ShowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.message = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 24);
            this.splitter1.Margin = new System.Windows.Forms.Padding(2);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(1122, 2);
            this.splitter1.TabIndex = 3;
            this.splitter1.TabStop = false;
            // 
            // onesec
            // 
            this.onesec.Enabled = true;
            this.onesec.Interval = 1000;
            this.onesec.Tick += new System.EventHandler(this.onesec_Tick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.отладкаToolStripMenuItem,
            this.устройстваToolStripMenuItem,
            this.ShowToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1122, 24);
            this.menuStrip1.TabIndex = 7;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip1_ItemClicked);
            // 
            // отладкаToolStripMenuItem
            // 
            this.отладкаToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.DebugOn,
            this.DebugOff,
            this.LoopStop,
            this.LoopStart,
            this.ShowServer});
            this.отладкаToolStripMenuItem.Name = "отладкаToolStripMenuItem";
            this.отладкаToolStripMenuItem.Size = new System.Drawing.Size(64, 20);
            this.отладкаToolStripMenuItem.Text = "Отладка";
            // 
            // DebugOn
            // 
            this.DebugOn.Name = "DebugOn";
            this.DebugOn.Size = new System.Drawing.Size(185, 22);
            this.DebugOn.Text = "Включить отладку";
            this.DebugOn.Click += new System.EventHandler(this.DebugOn_Click);
            // 
            // DebugOff
            // 
            this.DebugOff.Name = "DebugOff";
            this.DebugOff.Size = new System.Drawing.Size(185, 22);
            this.DebugOff.Text = "Отключить отладку";
            this.DebugOff.Click += new System.EventHandler(this.DebugOff_Click);
            // 
            // LoopStop
            // 
            this.LoopStop.Name = "LoopStop";
            this.LoopStop.Size = new System.Drawing.Size(185, 22);
            this.LoopStop.Text = "Остановить расчет";
            this.LoopStop.Click += new System.EventHandler(this.LoopStop_Click);
            // 
            // LoopStart
            // 
            this.LoopStart.Name = "LoopStart";
            this.LoopStart.Size = new System.Drawing.Size(185, 22);
            this.LoopStart.Text = "Возобновить расчет";
            this.LoopStart.Click += new System.EventHandler(this.LoopStart_Click);
            // 
            // ShowServer
            // 
            this.ShowServer.Name = "ShowServer";
            this.ShowServer.Size = new System.Drawing.Size(185, 22);
            this.ShowServer.Text = "Показать сервер";
            this.ShowServer.Click += new System.EventHandler(this.ShowServer_Click);
            // 
            // устройстваToolStripMenuItem
            // 
            this.устройстваToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StopDevices,
            this.StartDevices});
            this.устройстваToolStripMenuItem.Name = "устройстваToolStripMenuItem";
            this.устройстваToolStripMenuItem.Size = new System.Drawing.Size(81, 20);
            this.устройстваToolStripMenuItem.Text = "Устройства";
            // 
            // StopDevices
            // 
            this.StopDevices.Name = "StopDevices";
            this.StopDevices.Size = new System.Drawing.Size(223, 22);
            this.StopDevices.Text = "Остановить все устройства";
            this.StopDevices.Click += new System.EventHandler(this.остановитьВсеToolStripMenuItem_Click);
            // 
            // StartDevices
            // 
            this.StartDevices.Name = "StartDevices";
            this.StartDevices.Size = new System.Drawing.Size(223, 22);
            this.StartDevices.Text = "Запустить все заново";
            this.StartDevices.Click += new System.EventHandler(this.StartDevices_Click);
            // 
            // ShowToolStripMenuItem
            // 
            this.ShowToolStripMenuItem.Name = "ShowToolStripMenuItem";
            this.ShowToolStripMenuItem.Size = new System.Drawing.Size(73, 20);
            this.ShowToolStripMenuItem.Text = "Обновить";
            this.ShowToolStripMenuItem.Click += new System.EventHandler(this.ShowToolStripMenuItem_Click);
            // 
            // message
            // 
            this.message.Enabled = true;
            this.message.Tick += new System.EventHandler(this.message_Tick);
            // 
            // Apax
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1122, 293);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Apax";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Сервер Apax";
            this.Load += new System.EventHandler(this.Apax_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Splitter splitter1;
        public System.Windows.Forms.Timer onesec;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem отладкаToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem DebugOn;
        private System.Windows.Forms.ToolStripMenuItem DebugOff;
        private System.Windows.Forms.ToolStripMenuItem LoopStop;
        private System.Windows.Forms.ToolStripMenuItem LoopStart;
        private System.Windows.Forms.ToolStripMenuItem ShowServer;
        private System.Windows.Forms.Timer message;
        private System.Windows.Forms.ToolStripMenuItem устройстваToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem StopDevices;
        private System.Windows.Forms.ToolStripMenuItem StartDevices;
        private System.Windows.Forms.ToolStripMenuItem ShowToolStripMenuItem;
    }
}

