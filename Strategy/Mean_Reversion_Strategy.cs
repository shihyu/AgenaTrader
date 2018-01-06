using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using AgenaTrader.API;
using AgenaTrader.Custom;
using AgenaTrader.Plugins;
using AgenaTrader.Helper;


/// <summary>
/// Version: 1.1
/// -------------------------------------------------------------------------
/// Simon Pucher 2016
/// Christian Kovar 2016
/// -------------------------------------------------------------------------
/// Description: https://en.wikipedia.org/wiki/Algorithmic_trading#Mean_reversion 
/// -------------------------------------------------------------------------
/// ****** Important ******
/// To compile this indicator without any error you also need access to the utility indicator to use these global source code elements.
/// You will find this indicator on GitHub: https://raw.githubusercontent.com/simonpucher/AgenaTrader/master/Utilities/GlobalUtilities_Utility.cs
/// -------------------------------------------------------------------------
/// Namespace holds all indicators and is required. Do not change it.
/// </summary>
namespace AgenaTrader.UserCode
{
    [Description("The mean reversion is the theory suggesting that prices and returns eventually move back towards the mean or average.")]
    public class Mean_Reversion_Strategy : UserStrategy, IMean_Reversion
	{
        //interface 
        private bool _IsShortEnabled = false;
        private bool _IsLongEnabled = true;
        private bool _WarningOccured = false;
        private bool _ErrorOccured = false;
        private int _Bollinger_Period = 20;
        private double _Bollinger_Standard_Deviation = 2;
        private int _Momentum_Period = 100;
        private int _RSI_Period = 14;
        private int _RSI_Smooth = 3;
        private int _RSI_Level_Low = 30;
        private int _RSI_Level_High = 70;
        private int _Momentum_Level_Low = -1;
        private int _Momentum_Level_High = 1;

        //input
        private bool _send_email = false;
        private bool _autopilot = true;
        //todo
        //private bool _statisticbacktesting = false;

        //output

        //internal
        private IOrder _orderenterlong;
        private IOrder _orderentershort;
        private Mean_Reversion_Indicator _Mean_Reversion_Indicator = null;
        //todo
        //private StatisticContainer _StatisticContainer = null;

		protected override void OnInit()
		{
            //We need at least xy bars
            this.RequiredBarsCount = 130;
		}

        protected override void OnStart()
        {
            base.OnStart();

            //Print("OnStartUp" + Bars[0].Timestamp);

            //Init our indicator to get code access
            this._Mean_Reversion_Indicator = new Mean_Reversion_Indicator();

            ////todo Initalize statistic data list if this feature is enabled
            //if (this.StatisticBacktesting)
            //{
            //    this._StatisticContainer = new StatisticContainer();
            //}

            this.ErrorOccured = false;
            this.WarningOccured = false;
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            ////Print("OnTermination" + Bars[0].Timestamp);
            //IAccount account = this.Core.AccountManager.GetAccount(this.Instrument, true);

            //double counti = this.Core.TradingManager.GetExecutions(account, this.Backtesting.Settings.DateTimeRange.Lower, this.Backtesting.Settings.DateTimeRange.Upper).Count();
            //IEnumerable<ExecutionHistory> data  = this.Backtesting.TradingProcessor.GetExecutions(this.Backtesting.TradingProcessor.Accounts.First(), this.Backtesting.Settings.DateTimeRange.Lower, this.Backtesting.Settings.DateTimeRange.Upper);
            //foreach (ExecutionHistory item in data)
            //{
            //    item.Ex
            //}
            
            ////todo Close statistic data list if this feature is enabled
            //if (this.StatisticBacktesting)
            //{
            //    //get the statistic data
            //    this._StatisticContainer.copyToClipboard();
            //}
        }

		protected override void OnCalculate()
		{
            //Set automated during configuration in input dialog at strategy escort in chart
            this.IsAutoConfirmOrder = this.Autopilot;

            //calculate data
            ResultValue returnvalue = this._Mean_Reversion_Indicator.calculate(InSeries, Open, High, _orderenterlong, _orderentershort, this.Bollinger_Period, this.Bollinger_Standard_Deviation, this.Momentum_Period, this.RSI_Period, this.RSI_Smooth, this.RSI_Level_Low, this.RSI_Level_High, this.Momentum_Level_Low, this.Momentum_Level_High);

            //If the calculate method was not finished we need to stop and show an alert message to the user.
            if (returnvalue.ErrorOccured)
            {
                //Display error just one time
                if (!this.ErrorOccured)
                {
                    Log(this.DisplayName + ": " + Const.DefaultStringErrorDuringCalculation, InfoLogLevel.AlertLog);
                    this.ErrorOccured = true;
                }
                return;
            }

            //Entry
            if (returnvalue.Entry.HasValue)
            {
                switch (returnvalue.Entry)
                {
                    case OrderDirection.Buy:
                        this.DoEnterLong();
                        break;
                    case OrderDirection.Sell:
                        this.DoEnterShort();
                        break;
                }
            }

            //Exit
            if (returnvalue.Exit.HasValue)
            {
                switch (returnvalue.Exit)
                {
                    case OrderDirection.Buy:
                        this.DoExitShort();
                        break;
                    case OrderDirection.Sell:
                        this.DoExitLong();
                        break;
                }
            }

            //todo move stop of long order 
            // SetUpTrailStop(CalculationMode.Ticks, 30);
    
		}





        /// <summary>
        /// OnExecution of orders
        /// </summary>
        /// <param name="execution"></param>
        protected override void OnOrderExecution(IExecution execution)
        {
            ////todo Create statistic for execution
            //if (this.StatisticBacktesting)
            //{
            //    this._StatisticContainer.Add(this.Root.Core.TradingManager, this.DisplayName, execution);
            //}

            //send email
            if (this.Send_email)
            {
                this.SendEmail(Core.Settings.MailDefaultFromAddress, Core.PreferenceManager.DefaultEmailAddress,
                        GlobalUtilities.GetEmailSubject(execution), GlobalUtilities.GetEmailText(execution, this.GetType().Name));
            }
        }



        /// <summary>
        /// Create LONG order.
        /// </summary>
        private void DoEnterLong()
        {
            if (_orderenterlong == null)
            {
                _orderenterlong = SubmitOrder(new StrategyOrderParameters {Direction = OrderDirection.Buy, Type = OrderType.Market, Quantity = GlobalUtilities.AdjustPositionToRiskManagement(this.Root.Core.AccountManager, this.Root.Core.PreferenceManager, this.Instrument, Bars[0].Close), SignalName =  this.DisplayName + "_" + OrderDirection.Buy + "_" + this.Instrument.Symbol + "_" + Bars[0].Time.Ticks.ToString(), Instrument =  this.Instrument, TimeFrame =  this.TimeFrame});
                SetUpStopLoss(_orderenterlong.Name, CalculationMode.Price, Bars[0].Close * 0.97, false);
                //SetUpProfitTarget(_orderenterlong.Name, CalculationMode.Price, this._orb_indicator.TargetLong); 
            }
        }

        /// <summary>
        /// Create SHORT order.
        /// </summary>
        private void DoEnterShort()
        {
            if (_orderentershort == null)
            {
                _orderentershort = SubmitOrder(new StrategyOrderParameters {Direction = OrderDirection.Sell, Type = OrderType.Market, Quantity = GlobalUtilities.AdjustPositionToRiskManagement(this.Root.Core.AccountManager, this.Root.Core.PreferenceManager, this.Instrument, Bars[0].Close), SignalName =  this.DisplayName + "_" + OrderDirection.Sell + "_" + this.Instrument.Symbol + "_" + Bars[0].Time.Ticks.ToString(), Instrument =  this.Instrument, TimeFrame =  this.TimeFrame});
                //SetUpStopLoss(_orderentershort.Name, CalculationMode.Price, this._orb_indicator.RangeHigh, false);
                //SetUpProfitTarget(_orderentershort.Name, CalculationMode.Price, this._orb_indicator.TargetShort);
            }
        }

        /// <summary>
        /// Exit the LONG position.
        /// </summary>
        private void DoExitLong()
        {
            if (this._orderenterlong != null)
            {
                CloseLongTrade(new StrategyOrderParameters {Type = OrderType.Market, Quantity = this._orderenterlong.Quantity });
                this._orderenterlong = null;
            }
        }

        /// <summary>
        /// Fill the SHORT position.
        /// </summary>
        private void DoExitShort()
        {
            if (this._orderentershort != null)
            {
                CloseShortTrade(new StrategyOrderParameters {Type = OrderType.Market, Quantity = this._orderentershort.Quantity});
                this._orderentershort = null;
            }
        }


        #region Properties


        #region Interface


        /// <summary>
        /// </summary>
        [Description("If true it is allowed to create long positions.")]
        [Category("Parameters")]
        [DisplayName("Allow Long")]
        public bool IsLongEnabled
        {
            get { return _IsLongEnabled; }
            set { _IsLongEnabled = value; }
        }


        /// <summary>
        /// </summary>
        [Description("If true it is allowed to create short positions.")]
        [Category("Parameters")]
        [DisplayName("Allow Short")]
        public bool IsShortEnabled
        {
            get { return _IsShortEnabled; }
            set { _IsShortEnabled = value; }
        }

        [Browsable(false)]
        [XmlIgnore()]
        public bool ErrorOccured
        {
            get { return _ErrorOccured; }
            set { _ErrorOccured = value; }
        }

        [Browsable(false)]
        [XmlIgnore()]
        public bool WarningOccured
        {
            get { return _WarningOccured; }
            set { _WarningOccured = value; }
        }




        [Description("Period of the Bollinger Band.")]
        [Category("Parameters")]
        [DisplayName("BB Period")]
        public int Bollinger_Period
        {
            get { return _Bollinger_Period; }
            set { _Bollinger_Period = value; }
        }

        [Description("Standard Deviation of the Bollinger Band.")]
        [Category("Parameters")]
        [DisplayName("BB StdDev")]
        public double Bollinger_Standard_Deviation
        {
            get { return _Bollinger_Standard_Deviation; }
            set { _Bollinger_Standard_Deviation = value; }
        }

        [Description("Period of the Momentum.")]
        [Category("Parameters")]
        [DisplayName("MOM Period")]
        public int Momentum_Period
        {
            get { return _Momentum_Period; }
            set { _Momentum_Period = value; }
        }

        [Description("Period of the RSI.")]
        [Category("Parameters")]
        [DisplayName("RSI Period")]
        public int RSI_Period
        {
            get { return _RSI_Period; }
            set { _RSI_Period = value; }
        }

        [Description("Smooth Period of the RSI.")]
        [Category("Parameters")]
        [DisplayName("RSI Smooth Period")]
        public int RSI_Smooth
        {
            get { return _RSI_Smooth; }
            set { _RSI_Smooth = value; }
        }

        [Description("We trade long below this RSI level.")]
        [Category("Parameters")]
        [DisplayName("RSI Level Low")]
        public int RSI_Level_Low
        {
            get { return _RSI_Level_Low; }
            set { _RSI_Level_Low = value; }
        }

        [Description("We trade short above this RSI level.")]
        [Category("Parameters")]
        [DisplayName("RSI Level High")]
        public int RSI_Level_High
        {
            get { return _RSI_Level_High; }
            set { _RSI_Level_High = value; }
        }

        [Description("We trade long if momentum is above this level.")]
        [Category("Parameters")]
        [DisplayName("MOM Level Low")]
        public int Momentum_Level_Low
        {
            get { return _Momentum_Level_Low; }
            set { _Momentum_Level_Low = value; }
        }

        [Description("We trade short if momentum is below this level.")]
        [Category("Parameters")]
        [DisplayName("MOM Level High")]
        public int Momentum_Level_High
        {
            get { return _Momentum_Level_High; }
            set { _Momentum_Level_High = value; }
        }
        #endregion

        #region Internal

        [Description("If true an email will be send on order execution and on other important issues")]
        [Category("Safety first!")]
        [DisplayName("Send email")]
        public bool Send_email
        {
            get { return _send_email; }
            set { _send_email = value; }
        }


        [Description("If true the strategy will handle everything. It will create buy orders, sell orders, stop loss orders, targets fully automatically")]
        [Category("Safety first!")]
        [DisplayName("Autopilot")]
        public bool Autopilot
        {
            get { return _autopilot; }
            set { _autopilot = value; }
        }

        //todo
        //[Description("If true the strategy will create statistic data during the backtesting process")]
        //[Category("Safety first!")]
        //[DisplayName("Statistic Backtesting")]
        //public bool StatisticBacktesting
        //{
        //    get { return _statisticbacktesting; }
        //    set { _statisticbacktesting = value; }
        //}

        #endregion


        #endregion
    }
}
