using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using InfoStrat.VE.Utilities;

namespace InfoStrat.VE
{
    public class VEMapItem : ContentControl
    {
        #region Class Members

        ContentPresenter presenter;

        ILocationProvider provider;

        #endregion

        #region Constructors

        static VEMapItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VEMapItem), new FrameworkPropertyMetadata(typeof(VEMapItem)));
        }

        public VEMapItem()
        {
            presenter = null;
            provider = null;
        }

        #endregion

        #region Overridden Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            presenter = this.Template.FindName("PART_ContentPresenter", this) as ContentPresenter;
        }

        #endregion

        #region Internal Methods

        internal void UpdatePosition(VEMap map)
        {
            if (provider == null)
            {
                if (this.ContentTemplate != null)
                {
                    DependencyObject depObj = VisualUtility.GetChildByType(this, typeof(ILocationProvider));
                    provider = depObj as ILocationProvider;
                }

                if (provider == null && this.Content != null)
                {
                    provider = this.Content as ILocationProvider;
                }

                if (provider == null)
                {
                    return;
                }
            }
            Point? position = provider.UpdatePosition(map);

            if (!position.HasValue)
                return;

            Canvas.SetLeft(this, position.Value.X);
            Canvas.SetTop(this, position.Value.Y);
        }


        #endregion
    }
}
