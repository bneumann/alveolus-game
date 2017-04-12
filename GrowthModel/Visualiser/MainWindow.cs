using System;
using System.Collections.Generic;
using Cairo;
using GrowthModelLibrary;
using Gtk;

public partial class MainWindow : Window
{
	Infection i;
	DrawingArea da;

	public MainWindow() : base("Bacteria Grow")
	{
		SetDefaultSize(600, 600);
		SetPosition(WindowPosition.Center);
		var hb = new HBox();
		i = new Infection(new ModelParameter() { CellDimension = 600, BacteriaDoublingTime = 1 });
		da = new CairoGraphic();
		hb.Add(da);
		Add(hb);
		GLib.Timeout.Add(10, Update);
		DeleteEvent += delegate { Application.Quit(); };
		KeyPressEvent += new KeyPressEventHandler(keypress_event); 
		ShowAll();
	}

	void keypress_event(object obj, KeyPressEventArgs args)
	{
		System.Console.WriteLine("Keypress: {0}", args.Event.Key);
		if (args.Event.Key == Gdk.Key.C)
		{
			i.Cough();
		}
	}

	private bool Update()
	{
		i.Grow();
		((CairoGraphic)da).UpdateList(i);
		this.QueueDraw();
		return true;
	}
}

public class CairoGraphic : DrawingArea
{
	List<Bacteria> bactList = new List<Bacteria>();

	public void UpdateList(List<Bacteria> list)
	{
		bactList = list;
	}

	static void DrawBacteria(Context cr, double x, double y)
	{
		cr.Save();
		//cr.MoveTo(x, y);
		cr.SetSourceColor(new Color(0, 0, 0));
		cr.Arc(x, y, 2, 0.0, 2.0 * Math.PI);
		cr.LineWidth = 1;
      	cr.Stroke ();
		//cr.StrokePreserve();
	    cr.Restore ();
	}

	protected override bool OnExposeEvent(Gdk.EventExpose args)
	{
		using (Context g = Gdk.CairoHelper.Create(args.Window))
		{
			g.Save();
			g.SetSourceColor(new Color(1, 1, 1));
			int width = Allocation.Width;
          	int height = Allocation.Height;

			g.Rectangle (0, 0, width, height);
			g.Stroke();
		    g.Fill ();
			g.Restore();

			foreach (Bacteria b in bactList)
			{
				//Console.WriteLine("X {0}, Y{1}", b.X, b.Y);
				DrawBacteria(g, b.X, b.Y);
			}

		}
		return true;
	}

}
