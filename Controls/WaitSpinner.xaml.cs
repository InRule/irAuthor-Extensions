using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace InRuleLabs.AuthoringExtensions.FieldsInUse.Controls
{
    /// <summary>
    /// Interaction logic for WaitSpinner.xaml
    /// </summary>
    public partial class WaitSpinner : UserControl
    {
        public static readonly DependencyProperty FilledColorProperty = DependencyProperty.Register("FilledColor", typeof(Color), typeof(WaitSpinner));
        public static readonly DependencyProperty UnfilledColorProperty = DependencyProperty.Register("UnfilledColor", typeof(Color), typeof(WaitSpinner));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(WaitSpinner));

        public WaitSpinner()
        {
            
            FilledColor = Color.FromArgb(255,155,155,155);
            FilledColor = Colors.Blue;
            AutoFillUnFilledColor();
            InitializeComponent();
            Text = "";
            StartSpinning();
            DataContext = this;
        }
        

        public void StartSpinning()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(StartSpinning);
            }
            else
            {
                if (!IsSpinning)
                {
                    EnsureAnimationColors();
                    Storyboards.ToList().ForEach(i => i.Begin());
                    IsSpinning = true;
                    this.Visibility = Visibility.Visible;
                }
            }
            
        }
        public void StopSpinning()
        {
            if (IsSpinning)
            {
                Storyboards.ToList().ForEach(i => i.Stop());
                this.Visibility = Visibility.Hidden;
                IsSpinning = false;
            }
        }
        private IEnumerable<Storyboard> Storyboards
        {
            get
            {
                for (var i = 0; i < 8; i++)
                {
                    yield return this.Resources[$"Animation{i}"] as Storyboard;
               }
            }
        }

      
        private void EnsureAnimationColors()
        {
            foreach (var storyBoard in Storyboards)
            {
                var child = storyBoard.Children.First() as ColorAnimationUsingKeyFrames;
                var frame0 = (SplineColorKeyFrame) child.KeyFrames[0];
                var frame1 = (SplineColorKeyFrame) child.KeyFrames[1];
                frame0.Value = this.FilledColor;
                frame1.Value = this.UnfilledColor;
            }
        }

        public bool IsSpinning { get; private set; }
        
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public Color FilledColor
        {
            get { return (Color)GetValue(FilledColorProperty); }
            set { SetValue(FilledColorProperty, value); }
        }

        public Color UnfilledColor
        {
            get { return (Color)GetValue(UnfilledColorProperty); }
            set { SetValue(UnfilledColorProperty, value); }
        }

        public void AutoFillUnFilledColor()
        {
            UnfilledColor = Color.FromArgb(0, FilledColor.R, FilledColor.G, FilledColor.B);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            StartSpinning();
        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            StopSpinning();

        }
    }
}
