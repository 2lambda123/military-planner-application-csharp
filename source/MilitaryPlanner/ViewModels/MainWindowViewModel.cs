using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MilitaryPlanner.Helpers;
using MilitaryPlanner.Models;
using System.Windows.Data;
using Microsoft.Win32;

namespace MilitaryPlanner.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        #region Properties

        #region MyDateTime

        private Mission _mission = new Mission("Default Mission");

        private DateTime _myDateTime;
        public DateTime MyDateTime
        {
            get { return _myDateTime; }
            set
            {
                if (_myDateTime != value)
                {
                    _myDateTime = value;
                    RaisePropertyChanged(() => MyDateTime);
                }
            }
        }

        #endregion

        private int _sliderMinimum = 0;
        public int SliderMinimum
        {
            get
            {
                return _sliderMinimum;
            }

            set
            {
                if (_sliderMinimum != value)
                {
                    _sliderMinimum = value;
                    RaisePropertyChanged(() => SliderMinimum);
                }
            }
        }

        private int _sliderMaximum = 0;
        public int SliderMaximum
        {
            get
            {
                return _sliderMaximum;
            }

            set
            {
                if (_sliderMaximum != value)
                {
                    _sliderMaximum = value;
                    RaisePropertyChanged(() => SliderMaximum);
                }
            }
        }

        private int _sliderValue = 0;
        public int SliderValue
        {
            get
            {
                return _sliderValue;
            }

            set
            {
                if (_sliderValue != value)
                {
                    if (value > _sliderValue)
                    {
                        // next
                        Mediator.NotifyColleagues(Constants.ACTION_PHASE_NEXT, value);
                    }
                    else
                    {
                        // back
                        Mediator.NotifyColleagues(Constants.ACTION_PHASE_BACK, value);
                    }

                    _sliderValue = value;
                    RaisePropertyChanged(() => SliderValue);
                }
            }
        }

        private MilitaryPlanner.Views.MapView _mapView;
        public MilitaryPlanner.Views.MapView MapView
        {
            get { return _mapView; }
            set{
                if (_mapView != value)
                {
                    _mapView = value;
                    RaisePropertyChanged(() => MapView);
                }
            }
        }

        private MilitaryPlanner.Views.OrderOfBattleView _OOBView;
        public MilitaryPlanner.Views.OrderOfBattleView OOBView
        {
            get { return _OOBView; }
            set
            {
                if (_OOBView != value)
                {
                    _OOBView = value;
                    RaisePropertyChanged(() => OOBView);
                }
            }
        }

        private MilitaryPlanner.Views.MissionTimeLineView _MTLView;
        public MilitaryPlanner.Views.MissionTimeLineView MTLView
        {
            get { return _MTLView; }
            set
            {
                if (_MTLView != value)
                {
                    _MTLView = value;
                    RaisePropertyChanged(() => MTLView);
                }
            }
        }

        #endregion

        #region Commands

        public RelayCommand CancelCommand { get; set; }
        public RelayCommand DeleteCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }
        public RelayCommand OpenCommand { get; set; }

        #endregion

        #region Ctor

        public MainWindowViewModel()
        {
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ClientId = "sloy45Jis4XaPxFd";

            try
            {
                Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.Initialize();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to initialize the ArcGIS Runtime with the client id provided: " + ex.Message);
            }

            //Mediator.Register(Constants.ACTION_MSG_LAYER_ADDED, _mission.DoMessageLayerAdded);
            Mediator.Register(Constants.ACTION_MSG_LAYER_ADDED, DoMessageLayerAdded);
            //Mediator.Register(Constants.ACTION_MSG_PROCESSED, _mission.DoMessageProcessed);

            CancelCommand = new RelayCommand(OnCancelCommand);
            DeleteCommand = new RelayCommand(OnDeleteCommand);
            SaveCommand = new RelayCommand(OnSaveCommand);
            OpenCommand = new RelayCommand(OnOpenCommand);
            
            MapView = new Views.MapView();
            OOBView = new Views.OrderOfBattleView();
            MTLView = new Views.MissionTimeLineView();
        }

        private void DoMessageLayerAdded(object obj)
        {
            _mission.DoMessageLayerAdded(obj);

            SliderMaximum = _mission.PhaseList.Count - 1;
            SliderValue = SliderMaximum;
        }

        #endregion

        #region Command Handlers

        private void OnCancelCommand(object obj)
        {
            Mediator.NotifyColleagues(Constants.ACTION_CANCEL, obj);
        }

        private void OnDeleteCommand(object obj)
        {
            Mediator.NotifyColleagues(Constants.ACTION_DELETE, obj);
        }

        private void OnSaveCommand(object obj)
        {
            // file dialog
            var sfd = new SaveFileDialog();

            sfd.Filter = "xml files (*.xml)|*.xml";
            sfd.RestoreDirectory = true;

            if (sfd.ShowDialog() == true)
            {
                Mediator.NotifyColleagues(Constants.ACTION_MISSION_HYDRATE, _mission);

                _mission.Save(sfd.FileName);
            }
        }

        private void OnOpenCommand(object obj)
        {
            var ofd = new OpenFileDialog();

            ofd.Filter = "xml files (*.xml)|*.xml";
            ofd.RestoreDirectory = true;
            ofd.Multiselect = false;

            if (ofd.ShowDialog() == true)
            {
                //_mission = Mission.Load(ofd.FileName);
                OnLoadMission(ofd.FileName);
                Mediator.NotifyColleagues(Constants.ACTION_MISSION_LOADED, _mission);
            }

            InitializeUI(_mission);
        }

        private void InitializeUI(Mission _mission)
        {
            SliderMinimum = 0;
            SliderMaximum = _mission.PhaseList.Count - 1;
            SliderValue = 0;
        }

        private void OnLoadMission(string filename)
        {
            if (_mission != null)
            {
                Mediator.Unregister(Constants.ACTION_MSG_LAYER_ADDED, _mission.DoMessageLayerAdded);
            }

            _mission = Mission.Load(filename);

            if (_mission != null)
            {
                Mediator.Register(Constants.ACTION_MSG_LAYER_ADDED, _mission.DoMessageLayerAdded);
            }
        }

        private void OnSaveMission(string filename)
        {
            Mediator.NotifyColleagues(Constants.ACTION_MISSION_HYDRATE, _mission);

            _mission.Save(filename);
        }

        #endregion

    }
}