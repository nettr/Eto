using System;
using swc = System.Windows.Controls;
using sw = System.Windows;
using swm = System.Windows.Media;
using Eto.Forms;
using Eto.Drawing;

namespace Eto.Wpf.Forms.Controls
{
	public class ScrollableHandler : WpfPanel<swc.Border, Scrollable, Scrollable.ICallback>, Scrollable.IHandler
	{
		BorderType borderType;
		bool expandContentWidth = true;
		bool expandContentHeight = true;
		readonly EtoScrollViewer scroller;
		sw.Size preferredContentSize;

		public sw.FrameworkElement ContentControl { get { return scroller; } }

		protected override bool UseContentSize { get { return false; } }

		public class EtoScrollViewer : swc.ScrollViewer
		{
			public ScrollableHandler Handler { get; set; }

			public swc.Primitives.IScrollInfo GetScrollInfo() => ScrollInfo;

			protected override sw.Size MeasureOverride(sw.Size constraint)
			{
				var content = (sw.FrameworkElement)Content;

				// reset to preferred size to calculate scroll sizes initially based on that
				var desiredSize = Handler.preferredContentSize;
				content.Width = desiredSize.Width;
				content.Height = desiredSize.Height;

				return base.MeasureOverride(constraint);
			}

			protected override sw.Size ArrangeOverride(sw.Size arrangeSize)
			{
				var content = (sw.FrameworkElement)Content;

				// expand to width or height of viewport, now that we know which scrollbars are mandatory
				var desiredSize = content.DesiredSize;
				if (Handler.ExpandContentWidth)
					content.Width = Math.Max(ScrollInfo.ViewportWidth, desiredSize.Width);
				if (Handler.ExpandContentHeight)
					content.Height = Math.Max(ScrollInfo.ViewportHeight, desiredSize.Height);

				return base.ArrangeOverride(arrangeSize);
			}
		}

		public override Color BackgroundColor
		{
			get { return scroller.Background.ToEtoColor(); }
			set { scroller.Background = value.ToWpfBrush(scroller.Background); }
		}

		public ScrollableHandler()
		{
			Control = new swc.Border
			{
				SnapsToDevicePixels = true,
				Focusable = false,
			};
			scroller = new EtoScrollViewer
			{
				Handler = this,
				VerticalScrollBarVisibility = swc.ScrollBarVisibility.Auto,
				HorizontalScrollBarVisibility = swc.ScrollBarVisibility.Auto,
				CanContentScroll = true,
				SnapsToDevicePixels = true,
				Focusable = false
			};
			scroller.SizeChanged += HandleSizeChanged;
			scroller.Loaded += HandleSizeChanged;

			Control.Child = scroller;
			this.Border = BorderType.Bezel;
		}

		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateSizes();
		}

		void HandleSizeChanged(object sender, EventArgs e)
		{
			if (Widget.Loaded)
				UpdateSizes();
		}

		void UpdateSizes()
		{
			preferredContentSize = Content.GetPreferredSize(new sw.Size(double.MaxValue, double.MaxValue));

			scroller.InvalidateMeasure();
		}

		public override void UpdatePreferredSize()
		{
			UpdateSizes();
			base.UpdatePreferredSize();
		}

		public void UpdateScrollSizes()
		{
			Control.InvalidateMeasure();
			UpdateSizes();
		}

		protected override void SetContentScale(bool xscale, bool yscale)
		{
			base.SetContentScale(ExpandContentWidth, ExpandContentHeight);
		}

		public Point ScrollPosition
		{
			get
			{
				EnsureLoaded();
				return new Point((int)scroller.HorizontalOffset, (int)scroller.VerticalOffset);
			}
			set
			{
				scroller.ScrollToVerticalOffset(value.Y);
				scroller.ScrollToHorizontalOffset(value.X);
			}
		}

		public Size ScrollSize
		{
			get
			{
				EnsureLoaded();
				return new Size((int)scroller.ExtentWidth, (int)scroller.ExtentHeight);
			}
			set
			{
				var content = (swc.Border)Control.Child;
				content.MinHeight = value.Height;
				content.MinWidth = value.Width;
				UpdateSizes();
			}
		}

		public BorderType Border
		{
			get { return borderType; }
			set
			{
				borderType = value;
				switch (value)
				{
					case BorderType.Bezel:
						Control.BorderBrush = sw.SystemColors.ControlDarkDarkBrush;
						Control.BorderThickness = new sw.Thickness(1);
						break;
					case BorderType.Line:
						Control.BorderBrush = sw.SystemColors.ControlDarkDarkBrush;
						Control.BorderThickness = new sw.Thickness(1);
						break;
					case BorderType.None:
						Control.BorderBrush = null;
                        Control.BorderThickness = new sw.Thickness(0);
                        break;
					default:
						throw new NotSupportedException();
				}
			}
		}

		public override Size ClientSize
		{
			get
			{
				if (!Widget.Loaded)
					return Size;
				EnsureLoaded();
				var info = scroller.GetScrollInfo();
				return info != null ? new Size((int)info.ViewportWidth, (int)info.ViewportHeight) : Size.Empty;
			}
			set
			{
				Size = value;
			}
		}

		public Rectangle VisibleRect
		{
			get { return new Rectangle(ScrollPosition, ClientSize); }
		}

		public override void SetContainerContent(sw.FrameworkElement content)
		{
			content.HorizontalAlignment = sw.HorizontalAlignment.Left;
			content.VerticalAlignment = sw.VerticalAlignment.Top;
			content.SizeChanged += HandleSizeChanged;
			scroller.Content = content;
		}

		public override void AttachEvent(string id)
		{
			switch (id)
			{
				case Scrollable.ScrollEvent:
					scroller.ScrollChanged += (sender, e) =>
					{
						Callback.OnScroll(Widget, new ScrollEventArgs(new Point((int)e.HorizontalOffset, (int)e.VerticalOffset)));
					};
					break;
				default:
					base.AttachEvent(id);
					break;
			}
		}


		public bool ExpandContentWidth
		{
			get { return expandContentWidth; }
			set
			{
				if (expandContentWidth != value)
				{
					expandContentWidth = value;
					UpdateSizes();
				}
			}
		}

		public bool ExpandContentHeight
		{
			get { return expandContentHeight; }
			set
			{
				if (expandContentHeight != value)
				{
					expandContentHeight = value;
					UpdateSizes();
				}
			}
		}

		public float MaximumZoom { get { return 1f; } set { } }

		public float MinimumZoom { get { return 1f; } set { } }

		public float Zoom { get { return 1f; } set { } }
	}
}
