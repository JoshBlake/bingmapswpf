using System;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace InfoStrat.VE
{
	public abstract class VEShape : ContentControl
	{
        #region Class Members
        
        private VEShapeType shapeType;
        
        //TODO move points to VEPolyLine
        private Collection<VELatLong> points;
        private string customIconHtml;

        private VEMap map;

        #endregion

        #region Public Properties

        public VEMap Map
        {
            get
            {
                return map;
            }
        }

        public string CustomIconHtml 
        { 
            get
            {
                return customIconHtml;
            }
            set
            {
                customIconHtml = value;
            }
        }

        public Collection<VELatLong> Points 
        {   
            get
            {
                return this.points;
            }
        }

        public VEShapeType ShapeType 
        {
            get
            {
                return this.shapeType;
            }
            //TODO: set is temporary, delete soon
            set
            {
                this.shapeType = value;
            }
        }

        #endregion

        #region Constructors

        public VEShape()
        {
            this.points = new Collection<VELatLong>();
        }
        
        #endregion

        #region Public Methods

        public virtual void UpdatePosition()
        {
            UpdatePosition(this.map);
        }

        public virtual void UpdatePosition(VEMap map)
        {
            this.map = map;
        }

        #endregion

    }
}

