using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace SpeechCast.Controls
{
    class ToolStripTrackBar : ToolStripControlHost
    {
        public ToolStripTrackBar()
            : base(new TrackBar())
        {
            this.AutoSize = false;
            this.TrackBar.AutoSize = false;
            this.TrackBar.Orientation = Orientation.Horizontal;
        }


        public TrackBar TrackBar
        {
            get
            {
                return (TrackBar)Control;
            }
        }

        public int Value
        {
            get
            {
                return this.TrackBar.Value;
            }
            set
            {
                this.TrackBar.Value = value;
            }
        }

        public int Maximum
        {
            get
            {
                return this.TrackBar.Maximum;
            }
            set
            {
                this.TrackBar.Maximum = value;
            }
        }

        public int Minimum
        {
            get
            {
                return this.TrackBar.Minimum;
            }
            set
            {
                this.TrackBar.Minimum = value;
            }
        }

        public int LargeChange
        {
            get
            {
                return this.TrackBar.LargeChange;
            }
            set
            {
                this.TrackBar.LargeChange = value;
            }
        }

        public int SmallChange
        {
            get
            {
                return this.TrackBar.SmallChange;
            }
            set
            {
                this.TrackBar.SmallChange = value;
            }
        }


        protected override void OnSubscribeControlEvents(Control control)
        {
            base.OnSubscribeControlEvents(control);
            TrackBar.ValueChanged += new EventHandler(TrackBar_ValueChanged);
        }


        protected override void OnUnsubscribeControlEvents(Control control)
        {
            base.OnUnsubscribeControlEvents(control);
            TrackBar.ValueChanged -= new EventHandler(TrackBar_ValueChanged);
        }

        public event EventHandler ValueChanged;

        void TrackBar_ValueChanged(object sender, EventArgs e)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, e);
            }
        }
    }
}
