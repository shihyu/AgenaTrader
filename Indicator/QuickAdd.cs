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
/// Version: 1.0
/// -------------------------------------------------------------------------
/// Simon Pucher 2016
/// -------------------------------------------------------------------------
/// Namespace holds all indicators and is required. Do not change it.
/// </summary>
namespace AgenaTrader.UserCode
{
	[Description("Adds the instrument to a static list.")]
	public class QuickAdd : UserIndicator
	{
		#region Variables

		    private string _name_of_list = String.Empty;
            private IInstrumentsList _list = null;
            private Rectangle rect;
            private Pen pen;

		#endregion

        protected override void Initialize()
        {
            Add(new Plot(Color.FromKnownColor(KnownColor.Orange), "MyPlot1"));
            Overlay = true;
        }


        protected override void OnStartUp()
        {
            //base.OnStartUp();


            // Add event listener
            if (ChartControl != null)
                ChartControl.ChartPanelMouseDown += OnChartPanelMouseDown;

            if (this.Instrument != null)
            {
                if (!String.IsNullOrEmpty(Name_of_list))
                {

                    this.Root.Core.InstrumentManager.GetInstrumentLists();
                    _list = this.Root.Core.InstrumentManager.GetInstrumentsListStatic(this.Name_of_list);
                    if (_list == null)
                    {
                        _list = this.Root.Core.InstrumentManager.GetInstrumentsListDynamic(this.Name_of_list);
                    }
                    if (_list == null)
                    {
                        Log("The list " + this.Name_of_list + " does not exist.", InfoLogLevel.Warning);
                    }
                }
                else
                {
                    Log("You need to specify a name for the list.", InfoLogLevel.Warning);
                }
            }



            //if (this.Instrument != null)
            //{
            //     if (!String.IsNullOrEmpty(Name_of_list))
            //    {

            //        this.Root.Core.InstrumentManager.GetInstrumentLists();
            //        _list = this.Root.Core.InstrumentManager.GetInstrumentsListStatic(this.Name_of_list);
            //        if (liste == null)
            //        {
            //            liste = this.Root.Core.InstrumentManager.GetInstrumentsListDynamic(this.Name_of_list);
            //        }
            //        if (liste != null)
            //        {
            //            //Get instrument
            //            // Instrument instrument = this.Root.Core.InstrumentManager.GetInstrument("SKB.DE");

            //            // liste.Add(instrument);

            //            //If you want to clear instruments from the list
            //            //liste.Clear();

            //            if (!this.Root.Core.InstrumentManager.IsInstrumentExists(this.Instrument.Symbol))
            //            {
            //                this.Root.Core.InstrumentManager.AddInstrument2List(this.Instrument, this.Name_of_list);
            //            }

            //        }
            //        else
            //        {
            //            Log("The list " + this.Name_of_list + " does not exist.", InfoLogLevel.Warning);
            //        }

            //    }
            //    else
            //    {
            //        Log("You need to specify a name for the list.", InfoLogLevel.Warning);
            //    }
            //}


        }







		protected override void OnBarUpdate()
		{
			MyPlot1.Set(Input[0]);

            if (this.IsCurrentBarLast && _list != null)
            {
                if (_list.Contains((Instrument)this.Instrument))
                {
                    //DrawTextFixed("MyText", "is in list", TextPosition.TopRight);
                    //DrawText("MyText2", "Das ist ein Beispieltext.", 10, 60, Color.Black);
                }
                else
                {
                    //DrawTextFixed("MyText", "is not list", TextPosition.TopRight);
                    //DrawText("MyText2", "Das ist ein Beispieltext.", 10, 60, Color.Black);
                }

                DrawTextFixed("MyText", "Currentbar: " + CurrentBar + " - " + DateTime.Now.ToString(), TextPosition.TopRight);

            }

		}

        protected override void OnTermination()
        {
            // Remove event listener
            if (ChartControl != null)
                ChartControl.ChartPanelMouseDown -= OnChartPanelMouseDown;
        }


        public override string DisplayName
        {
            get
            {
                return "QA";
            }
        }


        public override string ToString()
        {
            return "QA";
        }


        #region Events



        public override void Plot(Graphics g, Rectangle r, double min, double max)
        {
            if (Bars == null || ChartControl == null) return;

          
            pen = Pens.Black;
           rect = new Rectangle(r.Width - 100, 20 , 80, 60);
            g.DrawRectangle(pen, rect);

            //// Eigenschaften von ChartControl
            //string s;
            //s = "bounds: " + r.X.ToString() + "   " + r.Y.ToString() + "   " + r.Height.ToString() + "   " + r.Width.ToString();
            //g.DrawString(s, font, brush, 10, 50, sf);

            //s = "min: " + Instrument.Round2TickSize(min).ToString() + "   max: " + Instrument.Round2TickSize(max).ToString();
            //g.DrawString(s, font, brush, 10, 70, sf);

            //s = "BarSpace: " + ChartControl.BarSpace.ToString() + "   BarWidth: " + ChartControl.BarWidth.ToString();
            //g.DrawString(s, font, brush, 10, 90, sf);

            //s = "Bars.Count: " + Bars.Count.ToString();
            //g.DrawString(s, font, brush, 10, 110, sf);

            //s = "BarsPainted: " + ChartControl.BarsPainted.ToString() + "   FirstBarPainted: " + ChartControl.FirstBarPainted.ToString() + "   LastBarPainted: " + ChartControl.LastBarPainted.ToString();
            //g.DrawString(s, font, brush, 10, 130, sf);

            //s = "BarsVisible: " + ChartControl.BarsVisible.ToString() + "   FirstBarVisible: " + ChartControl.FirstBarVisible.ToString() + "   LastBarVisible: " + ChartControl.LastBarVisible.ToString();
            //g.DrawString(s, font, brush, 10, 150, sf);



        }


            private void OnChartPanelMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
            {
                Print("X = {0}, Y = {1}", e.X, e.Y);
                Print("X = {0}, Y = {1}", ChartControl.GetDateTimeByX(e.X), ChartControl.GetPriceByY(e.Y));

                //ITextFixed txt = (ITextFixed)DrawObjects["MyText"];
                //IText bubu;
                

                //Point cursorPos = new Point(e.X, e.Y);
                //if (rect.Contains(cursorPos))
                //{
                //    DrawText("MyText", "Das ist ein Beispieltext.", 10, 3, Color.Black);
                //    pen = Pens.Red;
                //}
                //else
                //{
                //    DrawText("MyText", "Das ist ein Beispieltext.", 10, 3, Color.Black);
                //    pen = Pens.Black;
                //}


                this.OnBarUpdate();
                
              

                //if (e.Button == System.Windows.Forms.MouseButtons.Left)
                //{
                //    //Get the last order where IsProposal is true.
                //    if (this.TradingManager != null && olf != null && this.Instrument != null)
                //    {
                //        IEnumerable<OrdersLogRecord> olren = this.TradingManager.GetOrdersLog(olf).Where(x => x.Instrument.Id == this.Instrument.Id).Where(x => x.IsProposal == true).Where(x => x.State == OrderState.PendingSubmit);
                //        List<OrdersLogRecord> hhh = olren.ToList();
                //        OrdersLogRecord olr = olren.LastOrDefault();
                //        if (olr != null)
                //        {
                //            //Cast the order
                //            Order ord = (Order)this.TradingManager.GetOrder(olr.OrderId);
                //            if (ord != null && ord.State == OrderState.PendingSubmit)
                //            {
                //                //Change quantity
                //                double clickprice = ChartControl.GetPriceByY(e.Y);
                //                if (clickprice >= ord.Price + (ord.Price / 100 * 1))
                //                {
                //                    ord.Quantity = ord.Quantity + 1;
                //                    this.TradingManager.EditOrder(ord);
                //                }
                //                else if (clickprice <= ord.Price - (ord.Price / 100 * 1))
                //                {
                //                    if (ord.Quantity > 1)
                //                    {
                //                        ord.Quantity = ord.Quantity - 1;
                //                        this.TradingManager.EditOrder(ord);
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}
            }

        #endregion



        #region Properties

        #region Output

        [Browsable(false)]
            [XmlIgnore()]
            public DataSeries MyPlot1
            {
                get { return Values[0]; }
            }

            #endregion

            #region Input


            [Description("The name of the list to which you would like to add instruments.")]
            //[Category("Values")]
            [DisplayName("Name of the list")]
            public string Name_of_list
            {
                get { return _name_of_list; }
                set { _name_of_list = value; }
            }
            #endregion

  

		#endregion

        
    }
}

#region AgenaTrader Automaticaly Generated Code. Do not change it manualy

namespace AgenaTrader.UserCode
{
	#region Indicator

	public partial class UserIndicator : Indicator
	{
		/// <summary>
		/// Adds the instrument to a static list.
		/// </summary>
		public QuickAdd QuickAdd()
        {
			return QuickAdd(Input);
		}

		/// <summary>
		/// Adds the instrument to a static list.
		/// </summary>
		public QuickAdd QuickAdd(IDataSeries input)
		{
			var indicator = CachedCalculationUnits.GetCachedIndicator<QuickAdd>(input);

			if (indicator != null)
				return indicator;

			indicator = new QuickAdd
						{
							BarsRequired = BarsRequired,
							CalculateOnBarClose = CalculateOnBarClose,
							Input = input
						};
			indicator.SetUp();

			CachedCalculationUnits.AddIndicator2Cache(indicator);

			return indicator;
		}
	}

	#endregion

	#region Strategy

	public partial class UserStrategy
	{
		/// <summary>
		/// Adds the instrument to a static list.
		/// </summary>
		public QuickAdd QuickAdd()
		{
			return LeadIndicator.QuickAdd(Input);
		}

		/// <summary>
		/// Adds the instrument to a static list.
		/// </summary>
		public QuickAdd QuickAdd(IDataSeries input)
		{
			if (InInitialize && input == null)
				throw new ArgumentException("You only can access an indicator with the default input/bar series from within the 'Initialize()' method");

			return LeadIndicator.QuickAdd(input);
		}
	}

	#endregion

	#region Column

	public partial class UserColumn
	{
		/// <summary>
		/// Adds the instrument to a static list.
		/// </summary>
		public QuickAdd QuickAdd()
		{
			return LeadIndicator.QuickAdd(Input);
		}

		/// <summary>
		/// Adds the instrument to a static list.
		/// </summary>
		public QuickAdd QuickAdd(IDataSeries input)
		{
			return LeadIndicator.QuickAdd(input);
		}
	}

	#endregion

	#region Scripted Condition

	public partial class UserScriptedCondition
	{
		/// <summary>
		/// Adds the instrument to a static list.
		/// </summary>
		public QuickAdd QuickAdd()
		{
			return LeadIndicator.QuickAdd(Input);
		}

		/// <summary>
		/// Adds the instrument to a static list.
		/// </summary>
		public QuickAdd QuickAdd(IDataSeries input)
		{
			return LeadIndicator.QuickAdd(input);
		}
	}

	#endregion

}

#endregion
