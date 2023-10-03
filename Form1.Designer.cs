namespace A_Scout_D
{
    partial class Form1
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.btLiveView = new System.Windows.Forms.Button();
            this.btStop = new System.Windows.Forms.Button();
            this.btRecord = new System.Windows.Forms.Button();
            this.btPlay = new System.Windows.Forms.Button();
            this.btSave = new System.Windows.Forms.Button();
            this.lbState = new System.Windows.Forms.Label();
            this.tb1 = new System.Windows.Forms.TrackBar();
            this.tb2 = new System.Windows.Forms.TrackBar();
            this.lbCam1FPS = new System.Windows.Forms.Label();
            this.lbCam2FPS = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btCamSetSave = new System.Windows.Forms.Button();
            this.cbFocusMode = new System.Windows.Forms.CheckBox();
            this.tbExposure = new System.Windows.Forms.TrackBar();
            this.lbExposure = new System.Windows.Forms.Label();
            this.tbISO = new System.Windows.Forms.TrackBar();
            this.lbISO = new System.Windows.Forms.Label();
            this.tbFPS = new System.Windows.Forms.TrackBar();
            this.lbFPS = new System.Windows.Forms.Label();
            this.btExchange = new System.Windows.Forms.Button();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.tb1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tb2)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbExposure)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbISO)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbFPS)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btLiveView
            // 
            this.btLiveView.Location = new System.Drawing.Point(26, 33);
            this.btLiveView.Name = "btLiveView";
            this.btLiveView.Size = new System.Drawing.Size(153, 31);
            this.btLiveView.TabIndex = 2;
            this.btLiveView.Text = "Live View";
            this.btLiveView.UseVisualStyleBackColor = true;
            this.btLiveView.Click += new System.EventHandler(this.btLiveView_Click);
            // 
            // btStop
            // 
            this.btStop.Location = new System.Drawing.Point(26, 81);
            this.btStop.Name = "btStop";
            this.btStop.Size = new System.Drawing.Size(153, 31);
            this.btStop.TabIndex = 3;
            this.btStop.Text = "Live View Stop";
            this.btStop.UseVisualStyleBackColor = true;
            this.btStop.Click += new System.EventHandler(this.btStop_Click);
            // 
            // btRecord
            // 
            this.btRecord.Location = new System.Drawing.Point(26, 129);
            this.btRecord.Name = "btRecord";
            this.btRecord.Size = new System.Drawing.Size(153, 31);
            this.btRecord.TabIndex = 4;
            this.btRecord.Text = "Record 3 Sec";
            this.btRecord.UseVisualStyleBackColor = true;
            this.btRecord.Click += new System.EventHandler(this.btRecord_Click);
            // 
            // btPlay
            // 
            this.btPlay.Location = new System.Drawing.Point(26, 177);
            this.btPlay.Name = "btPlay";
            this.btPlay.Size = new System.Drawing.Size(153, 31);
            this.btPlay.TabIndex = 5;
            this.btPlay.Text = "Play";
            this.btPlay.UseVisualStyleBackColor = true;
            this.btPlay.Click += new System.EventHandler(this.btPlay_Click);
            // 
            // btSave
            // 
            this.btSave.Location = new System.Drawing.Point(26, 225);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(153, 31);
            this.btSave.TabIndex = 6;
            this.btSave.Text = "Save";
            this.btSave.UseVisualStyleBackColor = true;
            this.btSave.Click += new System.EventHandler(this.btSave_Click);
            // 
            // lbState
            // 
            this.lbState.AutoSize = true;
            this.lbState.Font = new System.Drawing.Font("Arial Black", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbState.Location = new System.Drawing.Point(39, 32);
            this.lbState.Name = "lbState";
            this.lbState.Size = new System.Drawing.Size(215, 30);
            this.lbState.TabIndex = 8;
            this.lbState.Text = "Camera State";
            // 
            // tb1
            // 
            this.tb1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tb1.Location = new System.Drawing.Point(244, 674);
            this.tb1.Name = "tb1";
            this.tb1.Size = new System.Drawing.Size(800, 69);
            this.tb1.TabIndex = 9;
            this.tb1.Scroll += new System.EventHandler(this.tb1_Scroll);
            // 
            // tb2
            // 
            this.tb2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tb2.Location = new System.Drawing.Point(1077, 674);
            this.tb2.Name = "tb2";
            this.tb2.Size = new System.Drawing.Size(800, 69);
            this.tb2.TabIndex = 10;
            this.tb2.Scroll += new System.EventHandler(this.tb2_Scroll);
            // 
            // lbCam1FPS
            // 
            this.lbCam1FPS.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbCam1FPS.AutoSize = true;
            this.lbCam1FPS.Font = new System.Drawing.Font("Arial Black", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbCam1FPS.Location = new System.Drawing.Point(239, 735);
            this.lbCam1FPS.Name = "lbCam1FPS";
            this.lbCam1FPS.Size = new System.Drawing.Size(202, 30);
            this.lbCam1FPS.TabIndex = 11;
            this.lbCam1FPS.Text = "Cam1 FPS : ";
            // 
            // lbCam2FPS
            // 
            this.lbCam2FPS.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbCam2FPS.AutoSize = true;
            this.lbCam2FPS.Font = new System.Drawing.Font("Arial Black", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbCam2FPS.Location = new System.Drawing.Point(1072, 735);
            this.lbCam2FPS.Name = "lbCam2FPS";
            this.lbCam2FPS.Size = new System.Drawing.Size(202, 30);
            this.lbCam2FPS.TabIndex = 12;
            this.lbCam2FPS.Text = "Cam2 FPS : ";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btSave);
            this.groupBox1.Controls.Add(this.btPlay);
            this.groupBox1.Controls.Add(this.btRecord);
            this.groupBox1.Controls.Add(this.btStop);
            this.groupBox1.Controls.Add(this.btLiveView);
            this.groupBox1.Location = new System.Drawing.Point(12, 96);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(209, 279);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Operation";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btCamSetSave);
            this.groupBox2.Controls.Add(this.cbFocusMode);
            this.groupBox2.Controls.Add(this.tbExposure);
            this.groupBox2.Controls.Add(this.lbExposure);
            this.groupBox2.Controls.Add(this.tbISO);
            this.groupBox2.Controls.Add(this.lbISO);
            this.groupBox2.Controls.Add(this.tbFPS);
            this.groupBox2.Controls.Add(this.lbFPS);
            this.groupBox2.Location = new System.Drawing.Point(12, 394);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(208, 406);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Camera Control";
            // 
            // btCamSetSave
            // 
            this.btCamSetSave.Location = new System.Drawing.Point(26, 358);
            this.btCamSetSave.Name = "btCamSetSave";
            this.btCamSetSave.Size = new System.Drawing.Size(153, 33);
            this.btCamSetSave.TabIndex = 7;
            this.btCamSetSave.Text = "Camera Save";
            this.btCamSetSave.UseVisualStyleBackColor = true;
            this.btCamSetSave.Click += new System.EventHandler(this.btCamSetSave_Click);
            // 
            // cbFocusMode
            // 
            this.cbFocusMode.AutoSize = true;
            this.cbFocusMode.Location = new System.Drawing.Point(26, 302);
            this.cbFocusMode.Name = "cbFocusMode";
            this.cbFocusMode.Size = new System.Drawing.Size(136, 22);
            this.cbFocusMode.TabIndex = 6;
            this.cbFocusMode.Text = "Focus Mode";
            this.cbFocusMode.UseVisualStyleBackColor = true;
            this.cbFocusMode.CheckedChanged += new System.EventHandler(this.cbFocusMode_CheckedChanged);
            // 
            // tbExposure
            // 
            this.tbExposure.Location = new System.Drawing.Point(19, 237);
            this.tbExposure.Name = "tbExposure";
            this.tbExposure.Size = new System.Drawing.Size(168, 69);
            this.tbExposure.TabIndex = 5;
            this.tbExposure.Scroll += new System.EventHandler(this.tbExposure_Scroll);
            // 
            // lbExposure
            // 
            this.lbExposure.AutoSize = true;
            this.lbExposure.Location = new System.Drawing.Point(24, 213);
            this.lbExposure.Name = "lbExposure";
            this.lbExposure.Size = new System.Drawing.Size(86, 18);
            this.lbExposure.TabIndex = 4;
            this.lbExposure.Text = "Exposure";
            // 
            // tbISO
            // 
            this.tbISO.Location = new System.Drawing.Point(20, 153);
            this.tbISO.Name = "tbISO";
            this.tbISO.Size = new System.Drawing.Size(168, 69);
            this.tbISO.TabIndex = 3;
            this.tbISO.Scroll += new System.EventHandler(this.tbISO_Scroll);
            // 
            // lbISO
            // 
            this.lbISO.AutoSize = true;
            this.lbISO.Location = new System.Drawing.Point(25, 129);
            this.lbISO.Name = "lbISO";
            this.lbISO.Size = new System.Drawing.Size(35, 18);
            this.lbISO.TabIndex = 2;
            this.lbISO.Text = "ISO";
            // 
            // tbFPS
            // 
            this.tbFPS.Location = new System.Drawing.Point(18, 65);
            this.tbFPS.Name = "tbFPS";
            this.tbFPS.Size = new System.Drawing.Size(168, 69);
            this.tbFPS.TabIndex = 1;
            this.tbFPS.Scroll += new System.EventHandler(this.tbFPS_Scroll);
            // 
            // lbFPS
            // 
            this.lbFPS.AutoSize = true;
            this.lbFPS.Location = new System.Drawing.Point(23, 41);
            this.lbFPS.Name = "lbFPS";
            this.lbFPS.Size = new System.Drawing.Size(100, 18);
            this.lbFPS.TabIndex = 0;
            this.lbFPS.Text = "Frame Rate";
            // 
            // btExchange
            // 
            this.btExchange.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btExchange.Image = global::A_Scout_D.Properties.Resources.exchange_3;
            this.btExchange.Location = new System.Drawing.Point(1031, 32);
            this.btExchange.Name = "btExchange";
            this.btExchange.Size = new System.Drawing.Size(69, 67);
            this.btExchange.TabIndex = 15;
            this.btExchange.UseVisualStyleBackColor = true;
            this.btExchange.Click += new System.EventHandler(this.btExchange_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox2.Location = new System.Drawing.Point(1077, 105);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(800, 550);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 1;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(244, 105);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(800, 550);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1898, 812);
            this.Controls.Add(this.btExchange);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lbCam2FPS);
            this.Controls.Add(this.lbCam1FPS);
            this.Controls.Add(this.tb2);
            this.Controls.Add(this.tb1);
            this.Controls.Add(this.lbState);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Name = "Form1";
            this.Text = "A-Scout-D";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.tb1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tb2)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbExposure)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbISO)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbFPS)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button btLiveView;
        private System.Windows.Forms.Button btStop;
        private System.Windows.Forms.Button btRecord;
        private System.Windows.Forms.Button btPlay;
        private System.Windows.Forms.Button btSave;
        private System.Windows.Forms.Label lbState;
        private System.Windows.Forms.TrackBar tb1;
        private System.Windows.Forms.TrackBar tb2;
        private System.Windows.Forms.Label lbCam1FPS;
        private System.Windows.Forms.Label lbCam2FPS;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TrackBar tbFPS;
        private System.Windows.Forms.Label lbFPS;
        private System.Windows.Forms.Button btCamSetSave;
        private System.Windows.Forms.CheckBox cbFocusMode;
        private System.Windows.Forms.TrackBar tbExposure;
        private System.Windows.Forms.Label lbExposure;
        private System.Windows.Forms.TrackBar tbISO;
        private System.Windows.Forms.Label lbISO;
        private System.Windows.Forms.Button btExchange;
    }
}

