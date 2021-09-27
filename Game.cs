// Jednostavan primjer animacije + kostur najjednostavnije moguće igre.
// Tretirati kao ilustraciju koncepta i ništa više od toga.
// Autor: Marko Čupić - FER

using System;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace DemoGame
{

  // Osnovni model pokretnog objekta:
  public abstract class Objekt {
    public System.Windows.Point position;
    public Vector speed;
    public double r = 10;
    public static double MaxSpeed = 50;
    public static int SpeedDelta = 5;
    
    public Objekt(int x, int y, int vx, int vy) 
    {
      position = new System.Windows.Point(x,y);
      speed = new Vector(vx,vy);
    }
    
    public abstract void NakonVremena(double t, bool leftPressed, bool rightPressed, bool upPressed, bool downPressed);
    public abstract void Paint(System.Windows.Forms.PaintEventArgs e);
  }
  
  // Neprijatelj je pokretni objekt:
  public class Neprijatelj : Objekt
  {
    // Ovo je konstruktor:
    public Neprijatelj(int x, int y, int vx, int vy) : base(x,y,vx,vy)
    {
    }
    
    public override void NakonVremena(double t, bool leftPressed, bool rightPressed, bool upPressed, bool downPressed)
    {
      position = speed + position;
      if(position.X < 20 || position.X > 400) speed.X = -speed.X;
      if(position.Y < 20 || position.Y > 400) speed.Y = -speed.Y;
    }
    
    public override void Paint(System.Windows.Forms.PaintEventArgs e)
    {
      Pen blackPen = new Pen(Color.Black, 3);
      e.Graphics.DrawArc(blackPen, (float)(position.X-r), (float)(position.Y-r), (float)(2*r), (float)(2*r), 0, 360);
    }
  }

  // Igrac je pokretni objekt:
  public class Igrac : Objekt
  {
    // Ovo je konstruktor:
    public Igrac(int x, int y, int vx, int vy) : base(x,y,vx,vy)
    {
    }
    
    public override void NakonVremena(double t, bool leftPressed, bool rightPressed, bool upPressed, bool downPressed)
    {
      if(leftPressed) speed += new Vector(-SpeedDelta,0);
      if(rightPressed) speed += new Vector(SpeedDelta,0);
      if(upPressed) speed += new Vector(0,-SpeedDelta);
      if(downPressed) speed += new Vector(0,SpeedDelta);
      if(speed.X > MaxSpeed) speed.X=MaxSpeed;
      if(speed.Y > MaxSpeed) speed.Y=MaxSpeed;
      if(speed.X < -MaxSpeed) speed.X=-MaxSpeed;
      if(speed.Y < -MaxSpeed) speed.Y=-MaxSpeed;
      
      position = speed + position;
      if(position.X < 20 || position.X>400) speed.X = -speed.X;
      if(position.Y < 20 || position.Y>400) speed.Y = -speed.Y;
    }
    
    public override void Paint(System.Windows.Forms.PaintEventArgs e)
    {
      Pen redPen = new Pen(Color.Red, 3);
      double r = 10;
      e.Graphics.DrawArc(redPen, (float)(position.X-r), (float)(position.Y-r), (float)(2*r), (float)(2*r), 0, 360);
    }
  }


// Ovo je prozor koji predstavlja igru. Sadrzi timer, popis svih pokretnih objekata, brine za iscrtavanje i detekciju kolizija.
// Puno toga je "hardkodirano" (primjerice, što se događa kada se igrač i neprijatelj susretnu), što bi u općenitijem razvojnom okruženju
// bilo definirano kroz odgovarajuću skriptu.
public class HelloWorld : Form
{
    private Panel panel;
    private Font fnt = new Font("Arial",10);
    private Timer timer;
    private bool GameStarted = false;
    private long LastTimeMillis = 0;
    private List<Objekt> objekti = new List<Objekt>();
    private bool leftPressed = false;
    private bool rightPressed = false;
    private bool upPressed = false;
    private bool downPressed = false;
    private Igrac igrac;
    private Random rand = new Random();
    private int score = 0;
    
    static public void Main ()
    {
        Application.Run (new HelloWorld ());
    }

    public HelloWorld ()
    {
        Width = 430;
        Height = 450;
        Text = "Hello Mono World";

        panel = new Panel();
        panel.Dock = DockStyle.Fill;
        panel.BackColor = Color.White;
        panel.Paint += new PaintEventHandler(panel_Paint);
        this.Controls.Add(panel);
        
        this.KeyDown += new KeyEventHandler(form_KeyDown);
        this.KeyUp += new KeyEventHandler(form_KeyUp);

        igrac = new Igrac(200,200,0,0);
        
        objekti.Add(new Neprijatelj(20,20,Objekt.SpeedDelta,0));
        objekti.Add(new Neprijatelj(50,20,Objekt.SpeedDelta,0));
        objekti.Add(igrac);
        
        timer = new Timer();
        timer.Interval = 50;
        timer.Tick += new EventHandler(timer_EventProcessor);
        timer.Start();
    }
    
    private void timer_EventProcessor(Object myObject, EventArgs myEventArgs) {
      long Milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
      if(!GameStarted) {
        GameStarted = true;
        LastTimeMillis = Milliseconds;
      } 
      else 
      {
        double delta = (Milliseconds - LastTimeMillis)/1000.0;
        LastTimeMillis = Milliseconds;
        foreach(Objekt o in objekti)
        {
          o.NakonVremena(delta, leftPressed, rightPressed, upPressed, downPressed);
        }

        List<Objekt> zaMaknuti = new List<Objekt>();        
        foreach(Objekt o in objekti)
        {
          if(o==igrac) continue;
          Vector dif = o.position - igrac.position;
          if(dif.Length < igrac.r+o.r)  // kolizija!
          {
            zaMaknuti.Add(o);
            score++;
          }
        }
        
        foreach(Objekt o in zaMaknuti)
        {
          objekti.Remove(o);
          objekti.Add(new Neprijatelj(
            (int)Math.Round(rand.NextDouble()*300+50),
            (int)Math.Round(rand.NextDouble()*300+50),
            (int)Math.Round(rand.NextDouble()*2*Objekt.SpeedDelta-Objekt.SpeedDelta),
            (int)Math.Round(rand.NextDouble()*2*Objekt.SpeedDelta-Objekt.SpeedDelta)));
        }
      }
      
      panel.Invalidate();
    }
    
    private void panel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
    {
      // Dohvati grafički objekt za crtanje po panelu
      Graphics g = e.Graphics;

      // Napiši poruku na panelu
      g.DrawString("Bodovi: "+score, fnt, System.Drawing.Brushes.Blue, new System.Drawing.Point(0,10));
      
      // Nacrtaj liniju
      g.DrawRectangle(System.Drawing.Pens.Red, 10, 10, 400, 400);
      
      foreach(Objekt o in objekti)
      {
        o.Paint(e);
      }
    }
    
    // Sljedece dvije metode brinu o praćenju statusa navigacijskih tipki (kursorskih tipki).
    
    private void form_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {
      if(e.KeyCode == Keys.Left) {
       leftPressed = true;
      }
      if(e.KeyCode == Keys.Right) {
       rightPressed = true;
      }
      if(e.KeyCode == Keys.Up) {
       upPressed = true;
      }
      if(e.KeyCode == Keys.Down) {
       downPressed = true;
      }
    }
    
    private void form_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
    {
      if(e.KeyCode == Keys.Left) {
       leftPressed = false;
      }
      if(e.KeyCode == Keys.Right) {
       rightPressed = false;
      }
      if(e.KeyCode == Keys.Up) {
       upPressed = false;
      }
      if(e.KeyCode == Keys.Down) {
       downPressed = false;
      }
    }
    
}

}
