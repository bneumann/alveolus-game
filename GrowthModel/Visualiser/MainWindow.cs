using System;
using System.Collections.Generic;
using Cairo;
using GrowthModelLibrary;
using Gtk;

public partial class MainWindow : Window
{
	Game game;
	DrawingArea da;

	public MainWindow() : base("Bacteria Grow")
	{
		SetDefaultSize(600, 600);
		SetPosition(WindowPosition.Center);
		var hb = new HBox();
		// i = new Infection(new ModelParameter() { CellDimension = 600, BacteriaDoublingTime = 1 });
		game = new Game();
		da = new CairoGraphic(game.worldObjects);
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
			game.infection.Cough();
		}
	}

	private bool Update()
	{
		game.Update();
		this.QueueDraw();
		return true;
	}
}

public class CairoGraphic : DrawingArea
{
	HashSet<GameObject> bactList = new HashSet<GameObject>();

	public CairoGraphic(HashSet<GameObject> worldObjects)
	{
		bactList = worldObjects;
	}

	static void DrawBacteria(Context cr, double x, double y, int radius)
	{
		cr.Save();
		//cr.MoveTo(x, y);
		cr.SetSourceColor(new Color(0, 0, 0));
		cr.Arc(x, y, radius, 0.0, 2.0 * Math.PI);
		cr.LineWidth = 1;
		cr.Stroke();
		//cr.StrokePreserve();
		cr.Restore();
	}

	protected override bool OnExposeEvent(Gdk.EventExpose args)
	{
		using (Context g = Gdk.CairoHelper.Create(args.Window))
		{
			g.Save();
			g.SetSourceColor(new Color(1, 1, 1));
			int width = Allocation.Width;
			int height = Allocation.Height;

			g.Rectangle(0, 0, width, height);
			g.Stroke();
			g.Fill();
			g.Restore();

			foreach (GameObject b in bactList)
			{
				//Console.WriteLine("X {0}, Y{1}", b.X, b.Y);
				if (b.GetType() == typeof(Bacteria))
				{
					DrawBacteria(g, b.X, b.Y, 2);
				}
				else
				{
                    DrawBacteria(g, b.X, b.Y, 5);
				}
			}

		}
		return true;
	}

}
