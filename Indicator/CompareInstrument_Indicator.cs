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
/// Version: 1.2.2
/// -------------------------------------------------------------------------
/// Simon Pucher 2017
/// -------------------------------------------------------------------------
/// ****** Important ******
/// To compile this script without any error you also need access to the utility indicator to use these global source code elements.
/// You will find this indicator on GitHub: https://raw.githubusercontent.com/simonpucher/AgenaTrader/master/Utilities/GlobalUtilities_Utility.cs
/// -------------------------------------------------------------------------
/// Namespace holds all indicators and is required. Do not change it.
/// </summary>
namespace AgenaTrader.UserCode
{
	[Description("Compare two instruments.")]
	public class CompareInstrument_Indicator : UserIndicator
	{

        private const int endOfScale = 1;
        private const int topOfScale = 100;

        int i1 = 1;
        int i2 = 2;

        protected override void OnInit()
		{
			Add(new OutputDescriptor(Color.FromKnownColor(KnownColor.Gray), "MyComparePlot_1"));
            Add(new OutputDescriptor(Color.FromKnownColor(KnownColor.Orange), "MyComparePlot_2"));
        }

        protected override void OnBarsRequirements()
        {
            Add(Core.InstrumentManager.GetInstrument(this.Instrument.Symbol));
            Add(Core.InstrumentManager.GetInstrument(_instrument_2));

            if (this.Instrument.Symbol == _instrument_2)
            {
                i1 = 0;
                i2 = 1;
            }
        }


        protected override void OnCalculate()
		{
            MyPlot1.Set(Normalize(Closes[i1].ToList(), Closes[i1][0]));
            MyPlot2.Set(Normalize(Closes[i2].ToList(), Closes[i2][0]));
        }

        private static double Normalize(List<double> list, double currentValue)
        {
            
            double min = list.Min();
            double max = list.Max();

            return endOfScale + (currentValue - min) * (topOfScale - endOfScale) / (max - min);

        }

        #region Properties

        [Browsable(false)]
		[XmlIgnore()]
		public DataSeries MyPlot1
		{
			get { return Outputs[0]; }
		}

        [Browsable(false)]
        [XmlIgnore()]
        public DataSeries MyPlot2
        {
            get { return Outputs[1]; }
        }

       

        private string _instrument_2 = "AAPL";

        [Description("First Symbol to compare")]
        [InputParameter]
        [DisplayNameAttribute("2nd Symbol")]
        public string Symbol_2
        {
            get { return _instrument_2; }
            set { _instrument_2 = value; }
        }

        #endregion
    }
}