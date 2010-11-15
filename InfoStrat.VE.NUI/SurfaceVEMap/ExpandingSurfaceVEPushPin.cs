using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using InfoStrat.VE;
using InfoStrat.VE.Utilities;

namespace InfoStrat.VE.NUI
{
    public enum ExpandingSurfaceVEPushPinDetailState
    {
        Mini,
        Full,
        Growing,
        Shrinking
    };

    public class ExpandingSurfaceVEPushPin : SurfaceVEPushPin
    {
        #region Fields

        ExpandingSurfaceVEPushPinDetailState currentDetailState;

        ContentPresenter cpContent;
        ContentPresenter cpDetails;
        Panel pnlContainer;

        #endregion

        #region Details DP

        public object Details
        {
            get { return (object)GetValue(DetailsProperty); }
            set { SetValue(DetailsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Details.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DetailsProperty =
            DependencyProperty.Register("Details", typeof(object), typeof(ExpandingSurfaceVEPushPin), new UIPropertyMetadata(null));

        #endregion

        #region Properties

        public ExpandingSurfaceVEPushPinDetailState DetailState
        {
            get
            {
                return currentDetailState;
            }
        }

        #endregion
        
        #region Events

        #region Expanding Events

        #region Expanding Event

        public event EventHandler Expanding;

        protected void SendOnExpanding()
        {
            if (Expanding == null)
                return;

            Expanding(this, EventArgs.Empty);
        }

        #endregion

        #region Expanded Event

        public event EventHandler Expanded;

        protected void SendOnExpanded()
        {
            if (Expanded == null)
                return;

            Expanded(this, EventArgs.Empty);
        }
        #endregion

        #region Collapsing Event

        public event EventHandler Collapsing;

        protected void SendOnCollapsing()
        {
            if (Collapsing == null)
                return;

            Collapsing(this, EventArgs.Empty);
        }

        #endregion

        #region Collapsed Event

        public event EventHandler Collapsed;

        protected void SendOnCollapsed()
        {
            if (Collapsed == null)
                return;

            Collapsed(this, EventArgs.Empty);
        }

        #endregion

        #endregion

        #endregion
        
        #region Constructors

        static ExpandingSurfaceVEPushPin()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ExpandingSurfaceVEPushPin), new FrameworkPropertyMetadata(typeof(ExpandingSurfaceVEPushPin)));
        }

        #endregion

        #region Public Methods

        public void CloseDetails()
        {
            //Fake it out, set to the opposite of what we want then toggle
            currentDetailState = ExpandingSurfaceVEPushPinDetailState.Growing;
            ToggleDetailState();
        }

        public void OpenDetails()
        {
            //Fake it out, set to the opposite of what we want then toggle
            currentDetailState = ExpandingSurfaceVEPushPinDetailState.Shrinking;
            ToggleDetailState();
        }

        public void ToggleDetails()
        {
            ToggleDetailState();
        }

        #endregion

        #region Private Methods

        protected override void Initialize()
        {
            base.Initialize();

            this.cpContent = null;
            this.cpDetails = null;
            this.pnlContainer = null;

            this.currentDetailState = ExpandingSurfaceVEPushPinDetailState.Mini;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.Click += new EventHandler<VEPushPinClickedEventArgs>(ExpandingSurfaceVEPushPin_Click);

            this.cpContent = (ContentPresenter)this.Template.FindName("PART_cpContent", this);
            this.cpDetails = (ContentPresenter)this.Template.FindName("PART_cpDetails", this);
            this.pnlContainer = (Panel)this.Template.FindName("PART_pnlContainer", this);
        }

        void ExpandingSurfaceVEPushPin_Click(object sender, VEPushPinClickedEventArgs e)
        {
            e.Handled = true;
            ToggleDetailState();
        }

        void ToggleDetailState()
        {
            cpContent.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size sizeTitle = cpContent.DesiredSize;

            Visibility original = cpDetails.Visibility;
            cpDetails.Visibility = Visibility.Visible;

            cpDetails.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size sizeDetails = cpDetails.DesiredSize;
            cpDetails.Visibility = original;

            pnlContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size currentSize = pnlContainer.DesiredSize;

            AnimateUtility.StopAnimation(pnlContainer, Panel.WidthProperty);
            AnimateUtility.StopAnimation(pnlContainer, Panel.HeightProperty);

            pnlContainer.Width = currentSize.Width;
            pnlContainer.Height = currentSize.Height;

            Size expandedSize = new Size(sizeTitle.Width, sizeTitle.Height + sizeDetails.Height);
            if (sizeDetails.Width > sizeTitle.Width)
            {
                expandedSize.Width = sizeDetails.Width;
            }


            if (currentDetailState == ExpandingSurfaceVEPushPinDetailState.Full ||
                currentDetailState == ExpandingSurfaceVEPushPinDetailState.Growing)
            {

                // Hide pp details.
                if (Map != null)
                    Map.SendToBack(this);

                currentDetailState = ExpandingSurfaceVEPushPinDetailState.Shrinking;

                AnimateUtility.AnimateElementDouble(cpDetails, Panel.OpacityProperty, 0, 0, 1);
                AnimateUtility.AnimateElementDouble(pnlContainer, Panel.WidthProperty, sizeTitle.Width, 0, 1);
                AnimationClock shrinkClock = AnimateUtility.AnimateElementDouble(pnlContainer, Panel.HeightProperty, sizeTitle.Height, 0, 1);

                shrinkClock.CurrentTimeInvalidated += new EventHandler(currentTimeInvalidated);
                shrinkClock.Completed += new EventHandler(shrinkClock_Completed);
                SendOnCollapsing();
            }
            else
            {
                // Show pp details.
                if (Map != null)
                    Map.SendToFront(this);
                currentDetailState = ExpandingSurfaceVEPushPinDetailState.Growing;

                cpDetails.Visibility = Visibility.Visible;

                AnimateUtility.AnimateElementDouble(cpDetails, Panel.OpacityProperty, 1, 0, 1);
                AnimateUtility.AnimateElementDouble(pnlContainer, Panel.WidthProperty, expandedSize.Width, 0, 1);
                AnimationClock growClock = AnimateUtility.AnimateElementDouble(pnlContainer, Panel.HeightProperty, expandedSize.Height, 0, 1);

                growClock.CurrentTimeInvalidated += new EventHandler(currentTimeInvalidated);
                growClock.Completed += new EventHandler(growClock_Completed);
                SendOnExpanding();
            }
        }

        void currentTimeInvalidated(object sender, EventArgs e)
        {
            if (Map != null)
                Map.ForceUpdateItemPosition(this);
        }

        void growClock_Completed(object sender, EventArgs e)
        {
            currentDetailState = ExpandingSurfaceVEPushPinDetailState.Full;
            SendOnExpanded();
        }

        void shrinkClock_Completed(object sender, EventArgs e)
        {
            currentDetailState = ExpandingSurfaceVEPushPinDetailState.Mini;
            cpDetails.Visibility = Visibility.Collapsed;
            SendOnCollapsed();
        }

        #endregion

    }
}
