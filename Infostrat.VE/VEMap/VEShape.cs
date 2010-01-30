using System;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace InfoStrat.VE
{
	public abstract class VEShape : ContentControl, ILocationProvider
	{
        #region Class Members

	    //TODO move _points to VEPolyLine
        private readonly Collection<VELatLong> _points;

	    private VEMap _map;

        #endregion

        #region Public Properties

        public VEMap Map
        {
            get
            {
                return _map;
            }
        }

	    public string CustomIconHtml { get; set; }

	    public Collection<VELatLong> Points 
        {   
            get
            {
                return this._points;
            }
        }

	    public VEShapeType ShapeType { get; set; }

	    #endregion

        #region Constructors

	    protected VEShape()
        {
            this._points = new Collection<VELatLong>();
        }
        
        #endregion

        #region Public Methods

        public virtual Point? UpdatePosition()
        {
            return UpdatePosition(this._map);
        }

        public virtual Point? UpdatePosition(VEMap map)
        {
            this._map = map;
            return null;
        }

        #endregion

    }
}

