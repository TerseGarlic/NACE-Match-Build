using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using NACE_Match_Builder.Builders;
using NACE_Match_Builder.Models;

namespace NACE_Match_Builder
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _coinSideTimer;
        private bool _showingHeads = true;
        private string _selectedSide = "NACE"; // Default selection
        private Random _random = new Random();
        private CallOfDutyMatchBuilder _codMatchBuilder;
        private ValorantMatchBuilder _valorantMatchBuilder;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.MatchCreationViewModel();

            // Initialize the match builders
            _codMatchBuilder = new CallOfDutyMatchBuilder();
            _valorantMatchBuilder = new ValorantMatchBuilder();

            // Set up timer to toggle coin sides
            _coinSideTimer = new DispatcherTimer();
            _coinSideTimer.Interval = TimeSpan.FromMilliseconds(80); // Faster flipping (was 100ms)
            _coinSideTimer.Tick += CoinSideTimer_Tick;

            // Set up side selection radio buttons
            NACERadio.Checked += (s, e) => _selectedSide = "NACE";
            CODRadio.Checked += (s, e) => _selectedSide = "COD";
            ValorantRadio.Checked += (s, e) => _selectedSide = "Valorant";

            // Initialize game selection
            GameTabControl.SelectedIndex = 0;
            UpdateGameSelection();
        }

        private void GameTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateGameSelection();
        }

        private void UpdateGameSelection()
        {
            // Update the view model with the selected game
            var vm = (ViewModels.MatchCreationViewModel)DataContext;
            if (GameTabControl.SelectedItem is TabItem selectedTab && selectedTab.Tag is GameTitle gameTitle)
            {
                vm.SelectedGame = gameTitle;
            }

            // Update UI based on selected game
            if (GameTabControl.SelectedIndex == 0) // Call of Duty
            {
                GameTitleText.Text = "Call of Duty Match Builder";
                GameTitleText.Foreground = (SolidColorBrush)FindResource("AccentColor");
                CODRadio.Visibility = Visibility.Visible;
                ValorantRadio.Visibility = Visibility.Collapsed;
                HeadsSideText.Text = "NACE";
                TailsSideText.Text = "COD";
            }
            else // Valorant
            {
                GameTitleText.Text = "Valorant Match Builder";
                GameTitleText.Foreground = (SolidColorBrush)FindResource("ValorantRed");
                CODRadio.Visibility = Visibility.Collapsed;
                ValorantRadio.Visibility = Visibility.Visible;
                HeadsSideText.Text = "NACE";
                TailsSideText.Text = "Valorant";
            }
        }

        private void HideCoinAfterDelay()
        {
            // Hide the coin after 3 seconds from when the result is shown
            DispatcherTimer hideCoinTimer = new DispatcherTimer();
            hideCoinTimer.Interval = TimeSpan.FromMilliseconds(3000);
            hideCoinTimer.Tick += (s, args) =>
            {
                // Create a fade-out animation for smooth disappearance
                var fadeOutAnimation = new Storyboard();
                var opacityAnimation = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(500));
                Storyboard.SetTarget(opacityAnimation, CoinEllipse);
                Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("(UIElement.Opacity)"));

                fadeOutAnimation.Children.Add(opacityAnimation);

                // After fade out, hide the coin completely
                fadeOutAnimation.Completed += (s2, e2) =>
                {
                    CoinEllipse.Visibility = Visibility.Hidden;
                    CoinEllipse.Opacity = 1.0; // Reset opacity for next use
                };

                try
                {
                    fadeOutAnimation.Begin();
                }
                catch (Exception ex)
                {
                    // Fallback: just hide it immediately if animation fails
                    CoinEllipse.Visibility = Visibility.Hidden;
                    CoinEllipse.Opacity = 1.0;
                }

                ((DispatcherTimer)s).Stop();
            };
            hideCoinTimer.Start();
        }

        private void CoinFlipButton_Click(object sender, RoutedEventArgs e)
        {
            // Disable the button during animation to prevent multiple clicks
            CoinFlipButton.IsEnabled = false;

            // Reset to show heads initially
            HeadsSide.Visibility = Visibility.Visible;
            TailsSide.Visibility = Visibility.Hidden;
            _showingHeads = true;

            // Display initial animation message
            CoinFlipStatus.Text = "Flipping...";

            // Start the coin flip animation when button is clicked
            Storyboard coinFlipAnimation = new Storyboard();

            // Create the angle rotation animation (improved for more realistic physics)
            var rotateAnimation = new DoubleAnimationUsingKeyFrames();
            Storyboard.SetTarget(rotateAnimation, CoinEllipse);
            Storyboard.SetTargetProperty(rotateAnimation,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[2].(RotateTransform.Angle)"));

            // Add keyframes to the rotation with improved physics
            var keyFrame1 = new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero));
            var keyFrame2 = new EasingDoubleKeyFrame(720, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(600)));
            keyFrame2.EasingFunction = new PowerEase { EasingMode = EasingMode.EaseOut, Power = 2 };
            var keyFrame3 = new EasingDoubleKeyFrame(1080, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1200)));
            keyFrame3.EasingFunction = new PowerEase { EasingMode = EasingMode.EaseOut, Power = 3 };
            var keyFrame4 = new EasingDoubleKeyFrame(2160 + _random.Next(-30, 30), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2000)));
            keyFrame4.EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 };

            rotateAnimation.KeyFrames.Add(keyFrame1);
            rotateAnimation.KeyFrames.Add(keyFrame2);
            rotateAnimation.KeyFrames.Add(keyFrame3);
            rotateAnimation.KeyFrames.Add(keyFrame4);

            // Create vertical jump animation
            var jumpAnimation = new DoubleAnimationUsingKeyFrames();
            Storyboard.SetTarget(jumpAnimation, CoinEllipse);
            Storyboard.SetTargetProperty(jumpAnimation,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[3].(TranslateTransform.Y)"));

            jumpAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            jumpAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(-80, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300))));
            jumpAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(-100, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500))));
            jumpAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(-70, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(700))));
            jumpAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(-30, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(900))));
            jumpAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1300))));
            jumpAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(-20, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1600))));
            jumpAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2000))));

            // Create scale animations for flipping effect
            var scaleXAnimation1 = new DoubleAnimation(1, 0.2, TimeSpan.FromMilliseconds(150));
            Storyboard.SetTarget(scaleXAnimation1, CoinEllipse);
            Storyboard.SetTargetProperty(scaleXAnimation1,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
            scaleXAnimation1.BeginTime = TimeSpan.FromMilliseconds(100);
            scaleXAnimation1.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };

            var scaleXAnimation2 = new DoubleAnimation(0.2, 1, TimeSpan.FromMilliseconds(150));
            Storyboard.SetTarget(scaleXAnimation2, CoinEllipse);
            Storyboard.SetTargetProperty(scaleXAnimation2,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
            scaleXAnimation2.BeginTime = TimeSpan.FromMilliseconds(250);
            scaleXAnimation2.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };

            var scaleXAnimation3 = new DoubleAnimation(1, 0.2, TimeSpan.FromMilliseconds(150));
            Storyboard.SetTarget(scaleXAnimation3, CoinEllipse);
            Storyboard.SetTargetProperty(scaleXAnimation3,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
            scaleXAnimation3.BeginTime = TimeSpan.FromMilliseconds(500);
            scaleXAnimation3.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };

            var scaleXAnimation4 = new DoubleAnimation(0.2, 1, TimeSpan.FromMilliseconds(150));
            Storyboard.SetTarget(scaleXAnimation4, CoinEllipse);
            Storyboard.SetTargetProperty(scaleXAnimation4,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
            scaleXAnimation4.BeginTime = TimeSpan.FromMilliseconds(650);
            scaleXAnimation4.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };

            // Create enhanced shimmer effect
            if (CoinShimmer != null)
            {
                var opacityPulse = new DoubleAnimationUsingKeyFrames();
                Storyboard.SetTarget(opacityPulse, CoinShimmer);
                Storyboard.SetTargetProperty(opacityPulse, new PropertyPath("(UIElement.Opacity)"));

                opacityPulse.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
                opacityPulse.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.8, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300))));
                opacityPulse.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.3, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400))));
                opacityPulse.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.7, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(700))));
                opacityPulse.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.2, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(900))));
                opacityPulse.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.6, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1200))));
                opacityPulse.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1600))));
                opacityPulse.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.9, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1900))));
                opacityPulse.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2200))));

                coinFlipAnimation.Children.Add(opacityPulse);
            }

            // Add coin glow effect animation
            if (CoinGlowEffect != null)
            {
                var glowAnimation = new DoubleAnimationUsingKeyFrames();
                Storyboard.SetTarget(glowAnimation, CoinGlowEffect);
                Storyboard.SetTargetProperty(glowAnimation, new PropertyPath("(UIElement.Opacity)"));

                glowAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
                glowAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1800))));
                glowAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.8, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1900))));
                glowAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.9, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2000))));
                glowAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.7, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2300))));
                glowAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(3000))));

                coinFlipAnimation.Children.Add(glowAnimation);
            }

            // Create visibility animations
            var coinVisibilityAnimation = new ObjectAnimationUsingKeyFrames();
            Storyboard.SetTarget(coinVisibilityAnimation, CoinEllipse);
            Storyboard.SetTargetProperty(coinVisibilityAnimation, new PropertyPath("(UIElement.Visibility)"));
            coinVisibilityAnimation.KeyFrames.Add(
                new DiscreteObjectKeyFrame(Visibility.Visible, KeyTime.FromTimeSpan(TimeSpan.Zero)));

            // Add all animations to storyboard
            coinFlipAnimation.Children.Add(rotateAnimation);
            coinFlipAnimation.Children.Add(jumpAnimation);
            coinFlipAnimation.Children.Add(scaleXAnimation1);
            coinFlipAnimation.Children.Add(scaleXAnimation2);
            coinFlipAnimation.Children.Add(scaleXAnimation3);
            coinFlipAnimation.Children.Add(scaleXAnimation4);
            coinFlipAnimation.Children.Add(coinVisibilityAnimation);

            // Start the side toggling
            _coinSideTimer.Start();

            // Start the animation
            try
            {
                coinFlipAnimation.Begin();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Animation error: {ex.Message}");
                // Ensure button is re-enabled if animation fails
                CoinFlipButton.IsEnabled = true;
                return;
            }

            // Determine which team gets the selected side
            var vm = (ViewModels.MatchCreationViewModel)DataContext;
            if (vm.Teams.Count == 2)
            {
                bool winnerGetsSelectedSide = _random.Next(2) == 0; // 50% chance
                string winnerName;
                Team winnerTeam;
                Team loserTeam;

                if (winnerGetsSelectedSide)
                {
                    // First school gets selected side
                    winnerTeam = vm.Teams[0];
                    loserTeam = vm.Teams[1];
                    winnerName = winnerTeam.Name;

                    // End animation showing the appropriate side
                    bool endOnHeads = _selectedSide == "NACE";
                    SetFinalCoinSide(endOnHeads);
                }
                else
                {
                    // Second school gets selected side
                    winnerTeam = vm.Teams[1];
                    loserTeam = vm.Teams[0];
                    winnerName = winnerTeam.Name;

                    // End animation showing the opposite side
                    bool endOnHeads = _selectedSide != "NACE";
                    SetFinalCoinSide(endOnHeads);
                }

                // Initialize the appropriate match builder with the teams based on selected game
                if (vm.SelectedGame == GameTitle.CallOfDuty)
                {
                    _codMatchBuilder = new CallOfDutyMatchBuilder()
                        .AddTeam(winnerTeam)
                        .AddTeam(loserTeam)
                        .WithBestOf(5)
                        .WithScheduled(DateTimeOffset.Now.AddDays(1));
                }
                else // Valorant
                {
                    _valorantMatchBuilder = new ValorantMatchBuilder()
                        .AddTeam(winnerTeam)
                        .AddTeam(loserTeam)
                        .WithBestOf(3)
                        .WithScheduled(DateTimeOffset.Now.AddDays(1));
                }

                // Update the winner through the view model (after a delay)
                DispatcherTimer winnerTimer = new DispatcherTimer();
                winnerTimer.Interval = TimeSpan.FromMilliseconds(2200);
                winnerTimer.Tick += (s, args) =>
                {
                    vm.CoinFlipWinner = winnerName;
                    string winnerSide = winnerGetsSelectedSide ? _selectedSide : (_selectedSide == "NACE" ?
                        (vm.SelectedGame == GameTitle.CallOfDuty ? "COD" : "Valorant") : "NACE");
                    CoinFlipStatus.Text = $"{winnerName} won and gets {winnerSide}!";
                    ((DispatcherTimer)s).Stop();

                    // Add a dramatic reveal animation for the result text
                    var resultAnimation = new Storyboard();
                    var scaleTransform = new ScaleTransform(0.7, 0.7);
                    CoinFlipStatus.RenderTransform = scaleTransform;

                    var scaleAnim = new DoubleAnimation(0.7, 1.2, TimeSpan.FromMilliseconds(200));
                    Storyboard.SetTarget(scaleAnim, CoinFlipStatus);
                    Storyboard.SetTargetProperty(scaleAnim, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
                    var scaleAnimY = new DoubleAnimation(0.7, 1.2, TimeSpan.FromMilliseconds(200));
                    Storyboard.SetTarget(scaleAnimY, CoinFlipStatus);
                    Storyboard.SetTargetProperty(scaleAnimY, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

                    resultAnimation.Children.Add(scaleAnim);
                    resultAnimation.Children.Add(scaleAnimY);

                    var scaleAnim2 = new DoubleAnimation(1.2, 1.0, TimeSpan.FromMilliseconds(200));
                    scaleAnim2.BeginTime = TimeSpan.FromMilliseconds(200);
                    Storyboard.SetTarget(scaleAnim2, CoinFlipStatus);
                    Storyboard.SetTargetProperty(scaleAnim2, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
                    var scaleAnimY2 = new DoubleAnimation(1.2, 1.0, TimeSpan.FromMilliseconds(200));
                    scaleAnimY2.BeginTime = TimeSpan.FromMilliseconds(200);
                    Storyboard.SetTarget(scaleAnimY2, CoinFlipStatus);
                    Storyboard.SetTargetProperty(scaleAnimY2, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

                    resultAnimation.Children.Add(scaleAnim2);
                    resultAnimation.Children.Add(scaleAnimY2);

                    try
                    {
                        resultAnimation.Begin();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Result animation error: {ex.Message}");
                    }

                    // Start the coin hide timer (NEW ADDITION)
                    HideCoinAfterDelay();
                };
                winnerTimer.Start();
            }

            // Stop the timer after animation completes
            DispatcherTimer stopTimer = new DispatcherTimer();
            stopTimer.Interval = TimeSpan.FromMilliseconds(3000); // Animation duration
            stopTimer.Tick += (s, args) =>
            {
                _coinSideTimer.Stop();
                CoinFlipButton.IsEnabled = true; // Re-enable the button
                ((DispatcherTimer)s).Stop();
            };
            stopTimer.Start();
        }

        // Method to build the Call of Duty match with the current rotation
        public CallOfDutyMatch BuildCodMatchFromCurrentRotation(List<MapMode> rotation)
        {
            try
            {
                return _codMatchBuilder
                    .WithRotation(rotation)
                    .WithRules(CdlRuleSet.Default)
                    .Build();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Failed to build COD match: {ex.Message}");
                return null;
            }
        }

        // Method to build the Valorant match with the current map selection
        public ValorantMatch BuildValorantMatchFromCurrentMaps(List<string> maps)
        {
            try
            {
                return _valorantMatchBuilder
                    .WithMaps(maps)
                    .WithRules(ValorantRuleSet.Default)
                    .Build();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Failed to build Valorant match: {ex.Message}");
                return null;
            }
        }

        private void SetFinalCoinSide(bool showHeads)
        {
            // This will be called to ensure the final side shown matches the winner
            DispatcherTimer finalSideTimer = new DispatcherTimer();
            finalSideTimer.Interval = TimeSpan.FromMilliseconds(1900); // Set before animation ends
            finalSideTimer.Tick += (s, args) =>
            {
                // Stop the regular flipping
                _coinSideTimer.Stop();

                // Show the final side
                HeadsSide.Visibility = showHeads ? Visibility.Visible : Visibility.Hidden;
                TailsSide.Visibility = showHeads ? Visibility.Hidden : Visibility.Visible;

                ((DispatcherTimer)s).Stop();
            };
            finalSideTimer.Start();
        }

        private void CoinSideTimer_Tick(object sender, EventArgs e)
        {
            // Toggle between heads and tails with smoother transitions
            if (_showingHeads)
            {
                HeadsSide.Visibility = Visibility.Hidden;
                TailsSide.Visibility = Visibility.Visible;
            }
            else
            {
                HeadsSide.Visibility = Visibility.Visible;
                TailsSide.Visibility = Visibility.Hidden;
            }
            _showingHeads = !_showingHeads;
        }
    }
}