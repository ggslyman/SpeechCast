using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Media;

namespace SpeechCast
{
    public partial class FormSettings : Form
    {
        public int VoiceVolme;
        public FormSettings()
        {
            InitializeComponent();
            System.Drawing.Text.InstalledFontCollection ifc =
                 new System.Drawing.Text.InstalledFontCollection();

            FontFamily[] ffs = ifc.Families;
            int i = 0;
            int index = 0;
            foreach (FontFamily ff in ffs)
            {
                ComboBoxFontName.Items.Add(ff.Name);
                comboBoxAAFontName.Items.Add(ff.Name);
                if (ff.Name == "MS UI Gothic")
                {
                    index = i;
                }
                i++;
            }

            ComboBoxFontName.SelectedIndex = index;
            comboBoxAAFontName.SelectedIndex = index;
        }

        public static string StringsToText(IEnumerable<string> strings)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string line in strings)
            {
                sb.AppendLine(line);
            }
            return sb.ToString();
        }

        public void SetUserConfig(UserConfig userConfig)
        {
            try
            {
                this.NumericUpDownMaxSpeakingCharacterCount.Value = userConfig.MaxSpeakingCharacterCount;
            }
            catch //(Exception ex)
            {
            }
            try
            {
                this.numericUpDownTurboThreshold.Value = userConfig.TurboThreshold;
            }
            catch //(Exception ex)
            {
            }
            try
            {
                this.NumericUpDownAutoGettingWebInverval.Value = userConfig.AutoGettingWebInvervalMillsec;
            }
            catch //(Exception ex)
            {
            }

            try
            {
                this.numericUpDownAAModeTextLength.Value = userConfig.AAModeTextLength;
            }
            catch //(Exception ex)
            {
            }

            try
            {
                this.numericUpDownAAModeInvervalMillsec.Value = userConfig.AAModeInvervalMillsec;
            }
            catch //(Exception ex)
            {
            }

            foreach(string NewResponseSoundFilePath in userConfig.NewResponseSoundFilePathes){
                this.listBoxNewResponseSoundFilePathes.Items.Add(NewResponseSoundFilePath);
            }
            this.NumericUpDownSpeakingInvervalMillsec.Value = userConfig.SpeakingInvervalMillsec;
            this.NumericUpDownTurboSpeakingInvervalMillsec.Value = userConfig.TurboSpeakingInvervalMillsec;
            this.TrackBarSpeakingRate.Value = userConfig.SpeakingRate;
            this.TrackBarTurboSpeakingRate.Value = userConfig.TurboSpeakingRate;
            this.labelTurboSpeakingRate.Text = userConfig.TurboSpeakingRate.ToString();
            this.TextBoxNGWords.Text = StringsToText(userConfig.NGWords);
            this.TextBoxPronounciations.Text = StringsToText(userConfig.Pronounciations);
            this.textBoxAAModeConditions.Text = StringsToText(userConfig.AAModeConditions);
            this.checkBoxSpeaksResNumber.Checked = userConfig.SpeaksResNumber;
            this.checkBoxGZipCompression.Checked = userConfig.GZipCompressionEnabled;
            this.checkBoxUseProxy.Checked = userConfig.UseDefaultProxy;
            this.checkBoxShowCaptionImmediately.Checked = userConfig.ShowCaptionImmediately;
            this.CaptionFont = userConfig.CaptionFont;
            this.AACaptionFont = userConfig.AACaptionFont;
            this.checkBoxReduceFontSizeByWindowWidth.Checked = userConfig.ReduceFontSizeByWindowWidth;
            this.checkBoxReduceFontSizeByWindowHeight.Checked = userConfig.ReduceFontSizeByWindowHeight;
            this.checkBoxSpeakTextBetweenBraces.Checked = userConfig.SpeakTextBetweenBracesEvenIfAAMode;
            this.checkBoxPlaySoundNewResponse.Checked = userConfig.PlaySoundNewResponse;
            this.checkBoxPlaySoundSync.Checked = userConfig.PlaySoundNewResponseSync;
            //this.textBoxSoundFilePath.Text = userConfig.NewResponseSoundFilePathes;
            this.textBoxEditorFilePath.Text = userConfig.EditorFilePath;
            this.checkBoxSpeaksResNumberWhenAAMode.Checked = userConfig.SpeaksResNumberWhenAAMode;
            this.textBoxSpeakingTextWhenAAMode.Text = userConfig.SpeakingTextWhenAAMode;
            this.checkBoxHideCaptionTitle.Checked = userConfig.HideCaptionTitle;
            this.VoiceVolme = userConfig.SpeakingVolume;
            this.checkBoxDebug.Checked = userConfig.OutputDebugLog;
            UpdateUI();
        }

        private CaptionFont.CaptionColors fontColors;
        private CaptionFont.CaptionColors aAFontColors;

        private Regex regexAllSpaces = new Regex(@"^\s*$");

        public void GetUserConfig(UserConfig userConfig)
        {
            userConfig.SpeakingInvervalMillsec = System.Convert.ToInt32(this.NumericUpDownSpeakingInvervalMillsec.Value);
            userConfig.TurboSpeakingInvervalMillsec = System.Convert.ToInt32(this.NumericUpDownTurboSpeakingInvervalMillsec.Value);
            userConfig.SpeakingRate = this.TrackBarSpeakingRate.Value;
            userConfig.TurboSpeakingRate = this.TrackBarTurboSpeakingRate.Value;
            userConfig.TurboThreshold = System.Convert.ToInt32(this.numericUpDownTurboThreshold.Value);
            userConfig.MaxSpeakingCharacterCount = System.Convert.ToInt32(this.NumericUpDownMaxSpeakingCharacterCount.Value);
            userConfig.CaptionFont = this.CaptionFont;
            userConfig.AACaptionFont = this.AACaptionFont;
            userConfig.AutoGettingWebInvervalMillsec = System.Convert.ToInt32(this.NumericUpDownAutoGettingWebInverval.Value);
            userConfig.AAModeTextLength = System.Convert.ToInt32(this.numericUpDownAAModeTextLength.Value);
            userConfig.SpeaksResNumber = this.checkBoxSpeaksResNumber.Checked;
            userConfig.NGWords.Clear();
            foreach (string ngword in this.TextBoxNGWords.Lines)
            {
                if (!regexAllSpaces.IsMatch(ngword))
                {
                    userConfig.NGWords.Add(ngword);
                }
            }
            userConfig.Pronounciations.Clear();
            foreach (string pron in this.TextBoxPronounciations.Lines)
            {
                if (!regexAllSpaces.IsMatch(pron))
                {
                    userConfig.Pronounciations.Add(pron);
                }
            }
            userConfig.AAModeConditions.Clear();
            foreach (string cond in this.textBoxAAModeConditions.Lines)
            {
                if (!regexAllSpaces.IsMatch(cond))
                {
                    userConfig.AAModeConditions.Add(cond);
                }
            }
            userConfig.NewResponseSoundFilePathes.Clear();
            foreach (string NewResponseSoundFilePath in listBoxNewResponseSoundFilePathes.Items)
            {
                if (!regexAllSpaces.IsMatch(NewResponseSoundFilePath))
                {
                    userConfig.NewResponseSoundFilePathes.Add(NewResponseSoundFilePath);
                }
            }

            userConfig.UpdatePronounciations();
            userConfig.GZipCompressionEnabled = checkBoxGZipCompression.Checked;
            userConfig.UseDefaultProxy = checkBoxUseProxy.Checked;
            userConfig.ShowCaptionImmediately = checkBoxShowCaptionImmediately.Checked;
            userConfig.ReduceFontSizeByWindowWidth = checkBoxReduceFontSizeByWindowWidth.Checked;
            userConfig.ReduceFontSizeByWindowHeight = checkBoxReduceFontSizeByWindowHeight.Checked;
            userConfig.SpeakTextBetweenBracesEvenIfAAMode = checkBoxSpeakTextBetweenBraces.Checked;
            userConfig.AAModeInvervalMillsec = System.Convert.ToInt32(this.numericUpDownAAModeInvervalMillsec.Value);
            userConfig.PlaySoundNewResponse = this.checkBoxPlaySoundNewResponse.Checked;
            userConfig.PlaySoundNewResponseSync = this.checkBoxPlaySoundSync.Checked;
            //userConfig.NewResponseSoundFilePath = this.textBoxSoundFilePath.Text;
            userConfig.EditorFilePath = this.textBoxEditorFilePath.Text;
            userConfig.SpeaksResNumberWhenAAMode = this.checkBoxSpeaksResNumberWhenAAMode.Checked;
            userConfig.SpeakingTextWhenAAMode = this.textBoxSpeakingTextWhenAAMode.Text;
            userConfig.AACaptionFont.Colors = this.aAFontColors;
            userConfig.HideCaptionTitle = this.checkBoxHideCaptionTitle.Checked;
            userConfig.OutputDebugLog = this.checkBoxDebug.Checked;
        }
        

        public CaptionFont CaptionFont
        {
            get
            {
                CaptionFont captionFont = new CaptionFont();

                captionFont.Name = ComboBoxFontName.Text;

                try
                {
                    captionFont.Size = System.Convert.ToSingle(ComboBoxFontSize.Text);
                }
                catch //(Exception e)
                {
                }
                captionFont.Bold = CheckBoxFontBold.Checked;
                captionFont.Italic = CheckBoxFontItalic.Checked;
                captionFont.BorderSize = System.Convert.ToInt32(NumericUpDownFontBorderSize.Value);
                captionFont.IsDirectionVertical = checkBoxFontVertical.Checked;
                captionFont.Colors = this.fontColors;

                return captionFont;
            }

            set
            {
                int idx = ComboBoxFontName.Items.IndexOf(value.Name);

                if (idx >= 0)
                {
                    ComboBoxFontName.SelectedIndex = idx;
                }

                ComboBoxFontSize.Text = value.Size.ToString();

                idx = ComboBoxFontSize.Items.IndexOf(value.Size.ToString());
                if (idx >= 0)
                {
                    ComboBoxFontSize.SelectedIndex = idx;
                }
                CheckBoxFontBold.Checked = value.Bold;
                CheckBoxFontItalic.Checked = value.Italic;
                checkBoxFontVertical.Checked = value.IsDirectionVertical;
                NumericUpDownFontBorderSize.Value = value.BorderSize;
                fontColors = value.Colors;
            }
        }

        public CaptionFont AACaptionFont
        {
            get
            {
                CaptionFont captionFont = new CaptionFont();

                captionFont.Name = comboBoxAAFontName.Text;

                try
                {
                    captionFont.Size = System.Convert.ToSingle(comboBoxAAFontSize.Text);
                }
                catch //(Exception e)
                {
                }
                captionFont.BorderSize = System.Convert.ToInt32(numericUpDownAAFontBorderSize.Value);
                captionFont.Colors = aAFontColors;
                return captionFont;
            }

            set
            {
                int idx = comboBoxAAFontName.Items.IndexOf(value.Name);

                if (idx >= 0)
                {
                    comboBoxAAFontName.SelectedIndex = idx;
                }

                comboBoxAAFontSize.Text = value.Size.ToString();

                idx = comboBoxAAFontSize.Items.IndexOf(value.Size.ToString());
                if (idx >= 0)
                {
                    comboBoxAAFontSize.SelectedIndex = idx;
                }
                numericUpDownAAFontBorderSize.Value = value.BorderSize;
                aAFontColors = value.Colors;
            }
        }


        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void trackBarSpeakingRate_ValueChanged(object sender, EventArgs e)
        {
            labelSpeakingRate.Text = TrackBarSpeakingRate.Value.ToString();
        }

        private void TrackBarTurboSpeakingRate_Scroll(object sender, EventArgs e)
        {
            labelTurboSpeakingRate.Text = TrackBarTurboSpeakingRate.Value.ToString();
        }

        private void panelPreview_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = panelPreview.ClientRectangle;

            rect.Inflate(-10, -10);

            FormCaption.DrawCaption(e.Graphics, rect, "プレビュー 123 ABC", CaptionFont, false);
        }

        private void ComboBoxFontName_SelectedIndexChanged(object sender, EventArgs e)
        {
            panelPreview.Refresh();
        }


        private void UpdateUI()
        {
            panelPlaySound.Enabled = checkBoxPlaySoundNewResponse.Checked;

            //foreach (Control control in panelPlaySound.Controls)
            //{
            //    control.Enabled = checkBoxPlaySoundNewResponse.Checked;
            //}
        }

        private void checkBoxPlaySoundNewResponse_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void buttonRefernceEditor_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Title = "エディターのファイルパスの指定";
            dialog.DefaultExt = "exe";
            dialog.Filter = "プログラム(*.exe)|*.exe";
            dialog.CheckFileExists = true;
            dialog.FileName = textBoxEditorFilePath.Text;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBoxEditorFilePath.Text = dialog.FileName;
            }

        }

        private void buttonRunEditor_GetEditorFilePath(object sender, SpeechCast.Controls.EventEditorArgs e)
        {
            e.EditorFilePath = textBoxEditorFilePath.Text;
        }

        private void buttonFontColor_Click(object sender, EventArgs e)
        {
            FormEditFontColors form = new FormEditFontColors();

            form.Colors = fontColors;

            if (form.ShowDialog() == DialogResult.OK)
            {
                fontColors = form.Colors;
                panelPreview.Refresh();
            }
        }

        private void buttonAAFontColor_Click(object sender, EventArgs e)
        {
            FormEditFontColors form = new FormEditFontColors();

            form.Colors = aAFontColors;

            if (form.ShowDialog() == DialogResult.OK)
            {
                aAFontColors = form.Colors;
            }
        }

        private void buttonVoiceTest_Click(object sender, EventArgs e)
        {
            FormMain.Instance.synthesizer.Volume = this.VoiceVolme;
            FormMain.Instance.synthesizer.Rate = TrackBarSpeakingRate.Value;
            FormMain.Instance.synthesizer.SpeakAsync(textTestSentence.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FormMain.Instance.synthesizer.Volume = this.VoiceVolme;
            FormMain.Instance.synthesizer.Rate = TrackBarTurboSpeakingRate.Value;
            FormMain.Instance.synthesizer.SpeakAsync(textTestSentence.Text);
        }

        private void buttonNewResponseSoundFilePathesAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Title = "サウンドファイルパスの指定";
            dialog.DefaultExt = "wav";
            //dialog.Filter = "サウンドファイル(*.wav,*.mp3)|*.wav;*.mp3";
            dialog.Filter = "サウンドファイル(*.wav)|*.wav";
            dialog.CheckFileExists = true;
            //dialog.FileName = textBoxSoundFilePath.Text;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                listBoxNewResponseSoundFilePathes.Items.Add(dialog.FileName);
            }
        }

        private void buttonNewResponseSoundFilePathesRemove_Click(object sender, EventArgs e)
        {
            listBoxNewResponseSoundFilePathes.Items.Remove(listBoxNewResponseSoundFilePathes.SelectedItem);
        }

        private void buttonCaptionIndentPaddingLeft_Click(object sender, EventArgs e)
        {
            FormMain.UserConfig.CaptionIndentLeftPadding--;
            FormCaption.Instance.Refresh();
        }

        private void buttonCaptionIndentPaddingRight_Click(object sender, EventArgs e)
        {
            FormMain.UserConfig.CaptionIndentLeftPadding++;
            FormCaption.Instance.Refresh();
        }

        private void buttonCaptionIndentPaddingUp_Click(object sender, EventArgs e)
        {
            FormMain.UserConfig.CaptionIndentTopPadding--;
            FormCaption.Instance.Refresh();
        }

        private void buttonCaptionIndentPaddingDown_Click(object sender, EventArgs e)
        {
            FormMain.UserConfig.CaptionIndentTopPadding++;
            FormCaption.Instance.Refresh();
        }

     }
}
