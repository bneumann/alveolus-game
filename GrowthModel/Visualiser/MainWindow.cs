using System;
using System.Collections.Generic;
using System.Threading;
using Cairo;
using GrowthModelLibrary;
using Gtk;

public partial class MainWindow : Window
{
	Game game;
	DrawingArea da;

	public MainWindow() : base("Bacteria Grow")
	{
		SetPosition(WindowPosition.Center);
		var hb = new HBox();
		// i = new Infection(new ModelParameter() { CellDimension = 600, BacteriaDoublingTime = 1 });
		game = new Game();
		SetDefaultSize(game.Width, game.Height);
		da = new CairoGraphic(game);
		hb.Add(da);
		Add(hb);
		Thread gThread = new Thread(game.MainThread);
		GLib.Timeout.Add(10, Update);
		gThread.IsBackground = true;
		gThread.Start();
		DeleteEvent += delegate { Application.Quit(); };
		KeyPressEvent += new KeyPressEventHandler(keypress_event);
		ShowAll();
	}

	void keypress_event(object obj, KeyPressEventArgs args)
	{
		System.Console.WriteLine("Keypress: {0}", args.Event.Key);
		if (args.Event.Key == Gdk.Key.C)
		{
			//game.infection.Cough();
		}
	}

	private bool Update()
	{
		//game.Update();
		this.QueueDraw();
		return true;
	}
}

public class CairoGraphic : DrawingArea
{
	private Game mGame;

	public CairoGraphic(Game game)
	{
		mGame = game;
	}

	static void DrawBacteria(Context cr, double x, double y, int radius, Color color)
	{
		cr.Save();
		//cr.MoveTo(x, y);
		cr.SetSourceColor(color);
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
			lock(mGame.worldObjects)
			{
				foreach (GameObject b in mGame.worldObjects)
				{
					//Console.WriteLine("X {0}, Y{1}", b.X, b.Y);
					if (b.GetType() == typeof(Bacteria))
					{
						Color color = b.isDead ? new Color(1, 0, 0) : new Color(0, 0, 0);
						DrawBacteria(g, b.X, b.Y, 2, color);
					}
					else
					{
						DrawBacteria(g, b.X, b.Y, 5, new Color(0, 0, 0));
					}
				}
			}

		}
		return true;
	}

}
