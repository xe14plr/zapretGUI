using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ZapretGUI.App.Controls;

/// <summary>
/// A loose homage to the PS2 boot screen: a few soft glowing orbs drift around the window,
/// bounce off the edges, and pick a new color on each bounce (DVD-screensaver style). Runs
/// behind the Обзор content via a Canvas in MainWindow. WPF is a 2D framework, so this is a
/// stylized approximation rather than a real 3D scene - Start()/Stop() keep it cheap to swap
/// out later if a real video/GIF asset replaces it.
/// </summary>
public sealed class AnimatedBackground
{
    private sealed class Orb
    {
        public required Ellipse Shape { get; init; }
        public required RadialGradientBrush GradientBrush { get; init; }
        public double X;
        public double Y;
        public double Vx;
        public double Vy;
        public double Radius;
    }

    private static readonly Color[] Palette =
    [
        Color.FromRgb(0x8B, 0x5C, 0xF6),
        Color.FromRgb(0x3B, 0x9E, 0xFF),
        Color.FromRgb(0xF4, 0x72, 0xB6),
        Color.FromRgb(0x22, 0xD3, 0xB5),
        Color.FromRgb(0xF5, 0x9E, 0x0B)
    ];

    private readonly Canvas _canvas;
    private readonly Random _rng = new();
    private readonly List<Orb> _orbs = [];
    private DateTime _lastTick;
    private bool _running;

    public AnimatedBackground(Canvas canvas)
    {
        _canvas = canvas;
    }

    public void Start()
    {
        if (_running)
        {
            return;
        }

        _running = true;

        for (var i = 0; i < 3; i++)
        {
            AddOrb();
        }

        _lastTick = DateTime.Now;
        CompositionTarget.Rendering += OnRendering;
    }

    public void Stop()
    {
        if (!_running)
        {
            return;
        }

        _running = false;
        CompositionTarget.Rendering -= OnRendering;
    }

    private void AddOrb()
    {
        var radius = 110 + _rng.NextDouble() * 90;
        var color = Palette[_rng.Next(Palette.Length)];

        var gradientBrush = new RadialGradientBrush
        {
            GradientStops =
            {
                new GradientStop(Color.FromArgb(80, color.R, color.G, color.B), 0),
                new GradientStop(Color.FromArgb(0, color.R, color.G, color.B), 1)
            }
        };

        var ellipse = new Ellipse
        {
            Width = radius * 2,
            Height = radius * 2,
            Fill = gradientBrush
        };
        _canvas.Children.Add(ellipse);

        var startWidth = Math.Max(_canvas.ActualWidth, 800);
        var startHeight = Math.Max(_canvas.ActualHeight, 500);

        var orb = new Orb
        {
            Shape = ellipse,
            GradientBrush = gradientBrush,
            Radius = radius,
            X = _rng.NextDouble() * Math.Max(startWidth - radius * 2, 1),
            Y = _rng.NextDouble() * Math.Max(startHeight - radius * 2, 1),
            Vx = (_rng.NextDouble() - 0.5) * 70,
            Vy = (_rng.NextDouble() - 0.5) * 70
        };

        Canvas.SetLeft(ellipse, orb.X);
        Canvas.SetTop(ellipse, orb.Y);
        _orbs.Add(orb);
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        var dt = (now - _lastTick).TotalSeconds;
        _lastTick = now;

        if (dt <= 0 || dt > 0.25)
        {
            return;
        }

        var width = _canvas.ActualWidth;
        var height = _canvas.ActualHeight;
        if (width <= 0 || height <= 0)
        {
            return;
        }

        foreach (var orb in _orbs)
        {
            orb.X += orb.Vx * dt;
            orb.Y += orb.Vy * dt;

            var diameter = orb.Radius * 2;
            var bounced = false;

            if (orb.X < 0)
            {
                orb.X = 0;
                orb.Vx = Math.Abs(orb.Vx);
                bounced = true;
            }
            else if (orb.X + diameter > width)
            {
                orb.X = width - diameter;
                orb.Vx = -Math.Abs(orb.Vx);
                bounced = true;
            }

            if (orb.Y < 0)
            {
                orb.Y = 0;
                orb.Vy = Math.Abs(orb.Vy);
                bounced = true;
            }
            else if (orb.Y + diameter > height)
            {
                orb.Y = height - diameter;
                orb.Vy = -Math.Abs(orb.Vy);
                bounced = true;
            }

            if (bounced)
            {
                var color = Palette[_rng.Next(Palette.Length)];
                orb.GradientBrush.GradientStops[0].Color = Color.FromArgb(80, color.R, color.G, color.B);
                orb.GradientBrush.GradientStops[1].Color = Color.FromArgb(0, color.R, color.G, color.B);
            }

            Canvas.SetLeft(orb.Shape, orb.X);
            Canvas.SetTop(orb.Shape, orb.Y);
        }
    }
}
