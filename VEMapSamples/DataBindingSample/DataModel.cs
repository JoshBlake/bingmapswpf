using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DataBindingSample
{
    public class DataModel : INotifyPropertyChanged
    {
        #region Properties

        private ObservableCollection<LocationModel> _locations;
        public ObservableCollection<LocationModel> Locations
        {
            get
            {
                return _locations;
            }
        }

        private bool _isGroup1Visible;
        public bool IsGroup1Visible
        {
            get
            {
                return _isGroup1Visible;
            }
            set
            {
                _isGroup1Visible = value;
                NotifyPropertyChanged("isGroup1Visible");
                UpdateVisibility();
            }
        }

        private bool _isGroup2Visible;
        public bool IsGroup2Visible
        {
            get
            {
                return _isGroup2Visible;
            }
            set
            {
                _isGroup2Visible = value;
                NotifyPropertyChanged("isGroup2Visible");
                UpdateVisibility();
            }
        }

        private bool _isGroup3Visible;
        public bool IsGroup3Visible
        {
            get
            {
                return _isGroup3Visible;
            }
            set
            {
                _isGroup3Visible = value;
                NotifyPropertyChanged("isGroup3Visible");
                UpdateVisibility();
            }
        }

        private bool _isGroup4Visible;
        public bool IsGroup4Visible
        {
            get
            {
                return _isGroup4Visible;
            }
            set
            {
                _isGroup4Visible = value;
                NotifyPropertyChanged("isGroup4Visible");
                UpdateVisibility();
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        #endregion

        #region Constructors

        public DataModel()
        {
            _isGroup1Visible = true;
            _isGroup2Visible = true;
            _isGroup3Visible = true;
            _isGroup4Visible = true;

            InitData();

            UpdateVisibility();
        }

        public void InitData()
        {
            ObservableCollection<LocationModel> newLocations = new ObservableCollection<LocationModel>();

            double centerLat = 38.9444195081574;
            double centerLong = -77.0630161230201;
            double spacingLat = 0.002;
            double spacingLong = 0.0015;

            bool currentAlternate = false;
            int currentGroup = 1;

            int size = 5;
            for (int i = -size; i < size; i++)
            {
                currentAlternate = !currentAlternate;
                for (int j = -size; j < size; j++)
                {
                    LocationModel loc = new LocationModel(centerLat + i*spacingLat, centerLong + j * spacingLong, 0, i.ToString() + ", " + j.ToString());
                    loc.AlternateItem = currentAlternate;
                    currentAlternate = !currentAlternate;
                    loc.GroupNumber = currentGroup;

                    newLocations.Add(loc);

                    currentGroup++;
                    if (currentGroup > 4)
                        currentGroup = 1;
                }
            }
            _locations = newLocations;
            NotifyPropertyChanged("Locations");
        }
        #endregion

        #region Methods

        private void UpdateVisibility()
        {
            Locations.ToList().ForEach(loc => loc.IsVisible = IsGroupVisible(loc.GroupNumber));
        }

        private bool IsGroupVisible(int groupNumber)
        {
            switch (groupNumber)
            {
                case 1:
                    return IsGroup1Visible;
                case 2:
                    return IsGroup2Visible;
                case 3:
                    return IsGroup3Visible;
                case 4:
                    return IsGroup4Visible;
                default:
                    return false;
            }
        }

        #endregion

    }
}
