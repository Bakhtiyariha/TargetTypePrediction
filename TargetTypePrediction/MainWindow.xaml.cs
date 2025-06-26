using Microsoft.ML;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TargetTypePrediction.Annotations;
using static TargetTypePrediction.TargetTypeModel;

namespace TargetTypePrediction
{
    public partial class MainWindow : Window
    {
        // Radio button section
        string? selectedTargetIntentOption = string.Empty;
        string? selectedFlightPathPredictabilityOption = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void ChkDegree_OnUnchecked(object sender, RoutedEventArgs e)
        {
            sldHeight.Minimum = 0;
            sldHeight.Maximum = 30;
            sldHeight.Value = 4;
            lblchk.Foreground = Brushes.DarkGray;
        }

        private void ChkDegree_OnChecked(object sender, RoutedEventArgs e)
        {
            sldHeight.Minimum = 30;
            sldHeight.Maximum = 60;
            sldHeight.Value = 45;
            lblchk.Foreground = Brushes.DarkSlateBlue;
        }

        private void BtnPrediction_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the selected options from radio buttons
                GetSelectedRadioButtonInTargetIntent();
                GetSelectedRadioButtonInFlightPathPredictability();

                // List of TextBoxes with their respective labels
                var inputControls = new List<(TextBlock, string)>
                {
                    (txbShowRange, "Range"),
                    (txbShowRCS, "RCS"),
                    (txbShowSpeed, "Speed"),
                    (txbShowAzimuth, "Azimuth"),
                    (txbShowHeight, "Height"),
                    (txbShowAltitude, "Altitude"),
                    (txbShowCrossParameter, "Cross Parameter"),
                    (txbShowSize, "Size"),
                    (txbShowManeuverability, "Maneuverability"),
                    (txbShowEmissionSignature, "Emission Signature")
                };

                var inputs = new List<float>();

                // Safely parse all inputs and add them to the list
                foreach (var (textBox, label) in inputControls)
                {
                    if (!float.TryParse(textBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                    {
                        MessageBox.Show($"After pressing the reset command, the values must change!!!");
                        return;
                    }
                    inputs.Add(value);
                }

                // Get the prediction and score
                var (predictedType, confidencePercent) = PredictTargetType(
                    inputs[0], inputs[1], inputs[2], inputs[3],
                    inputs[4], inputs[5], inputs[6], inputs[7],
                    inputs[8], inputs[9], selectedTargetIntentOption, selectedFlightPathPredictabilityOption
                );

                // Display the predicted value and confidence in the TextBlock
                UpdateTextBlockColor(predictedType);
                lblProgressBar.Content = predictedType;
                progressBarConfidence.Visibility = Visibility.Visible;
                progressBarConfidence.Value = confidencePercent;

                btnPrediction.IsEnabled = false;
                btnreset.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        // Update the PredictTargetType method to return both predicted type and confidence percentage
        private (string predictedType, double confidencePercent) PredictTargetType(float range, float radarCrossSection, float speed,
            float direction, float height, float altitude, float crossParameter, float size,
            float maneuverability, float emissionSignature, string targetIntent, string flightPathPredictability)
        {
            var input = new ModelInput
            {
                Range_m_ = range,
                RadarCrossSection_sq_m_ = radarCrossSection,
                Speed_m_s_ = speed,
                Direction_deg_ = direction,
                Height_deg_ = height,
                Altitude_m_ = altitude,
                CrossParameter_m_ = crossParameter,
                Size_m_ = size,
                Maneuverability = maneuverability,
                EmissionSignature = emissionSignature,
                TargetIntent = targetIntent,
                FlightPathPredictability = flightPathPredictability
            };

            // Predict using the ML.NET model
            ModelOutput result = TargetTypeModel.Predict(input);

            // Get the score of the predicted label
            var predictedLabel = result.PredictedLabel;

            // Normalize scores using softmax to get confidence percentages
            var sumExpScores = result.Score.Sum(score => Math.Exp(score));
            var softmaxScores = result.Score.Select(score => Math.Exp(score) / sumExpScores).ToArray();

            // Find confidence for the predicted label
            var predictedIndex = Array.IndexOf(result.Score, result.Score.Max());

            var confidencePercent = softmaxScores[predictedIndex] * 100;

            return (predictedLabel, confidencePercent);
        }

        // Method to change TextBlock color based on the predicted type
        private void UpdateTextBlockColor(string predictedType)
        {
            switch (predictedType)
            {
                case "Passenger Aircraft":
                    txbAirPlane.Content = "Result";
                    txbAirPlane.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                case "Fighter Aircraft":
                    txbFighter.Content = "Result";
                    txbFighter.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                case "Cruise Missile":
                    txbCruise.Content = "Result";
                    txbCruise.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                case "Guided Bomb":
                    txbBomb.Content = "Result";
                    txbBomb.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                case "Helicopter":
                    txbHelicopter.Content = "Result";
                    txbHelicopter.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                case "Drone":
                    txbDrone.Content = "Result";
                    txbDrone.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                default:
                    txbAirPlane.Content = "Result";
                    txbAirPlane.Foreground = new SolidColorBrush(Colors.Red);
                    break;
            }
        }

        // Ensure selected target intent and flight path predictability options are retrieved properly
        private void GetSelectedRadioButtonInTargetIntent()
        {
            if (rdbCivilian.IsChecked == true)
                selectedTargetIntentOption = rdbCivilian.Content.ToString();
            else if (rdbMilitary.IsChecked == true)
                selectedTargetIntentOption = rdbMilitary.Content.ToString();
        }

        private void GetSelectedRadioButtonInFlightPathPredictability()
        {
            if (rdbStraight.IsChecked == true)
                selectedFlightPathPredictabilityOption = rdbStraight.Content.ToString();
            else if (rdbEvasive.IsChecked == true)
                selectedFlightPathPredictabilityOption = rdbEvasive.Content.ToString();
            else if (rdbErratic.IsChecked == true)
                selectedFlightPathPredictabilityOption = rdbErratic.Content.ToString();
            else
            {
                selectedFlightPathPredictabilityOption = rdbPredictable.Content.ToString();
            }
        }

        private void Btnreset_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Change the values ​​and try again\n\n مقادیر را تغییر داده و دوباره امتحان کنید", "Information توجه کنید...", MessageBoxButton.OK, MessageBoxImage.Information);

            // Reset Slider values
            sldRange.Value = 5000;
            sldRCS.Value = 0.05;
            sldSpeed.Value = 150;
            sldAzimuth.Value = 150;
            sldHeight.Value = 4; // Adjusted for altitude constraints
            sldAltitude.Value = 500; // Keep a reasonable default
            sldCrossParameter.Value = 400;
            sldSize.Value = 10;
            sldManeuverability.Value = 2;
            sldEmissionSignature.Value = 2;

            // Reset Radio Buttons
            rdbMilitary.IsChecked = true;
            rdbEvasive.IsChecked = true;

            // Reset TextBlock values to reflect current slider values
            UpdateTextBlockValues();

            txbAirPlane.Background = Brushes.Transparent;
            txbAirPlane.Content = "";

            txbFighter.Background = Brushes.Transparent;
            txbFighter.Content = "";

            txbCruise.Background = Brushes.Transparent;
            txbCruise.Content = "";

            txbBomb.Background = Brushes.Transparent;
            txbBomb.Content = "";

            txbHelicopter.Background = Brushes.Transparent;
            txbHelicopter.Content = "";

            txbDrone.Background = Brushes.Transparent;
            txbDrone.Content = "";

            lblProgressBar.Content = "";
            progressBarConfidence.Value = 0;

            btnPrediction.IsEnabled = true;
            btnreset.IsEnabled = false;
            progressBarConfidence.Visibility = Visibility.Hidden;

        }

        // Method to update TextBlock values based on current slider values
        private void UpdateTextBlockValues()
        {
            txbShowRange.Text = sldRange.Value.ToString("F2");
            txbShowRCS.Text = sldRCS.Value.ToString("F2");
            txbShowSpeed.Text = sldSpeed.Value.ToString("F2");
            txbShowAzimuth.Text = sldAzimuth.Value.ToString("F2");
            txbShowHeight.Text = sldHeight.Value.ToString("F2");
            txbShowAltitude.Text = sldAltitude.Value.ToString("F2");
            txbShowCrossParameter.Text = sldCrossParameter.Value.ToString("F2");
            txbShowSize.Text = sldSize.Value.ToString("F2");
            txbShowManeuverability.Text = sldManeuverability.Value.ToString("F2");
            txbShowEmissionSignature.Text = sldEmissionSignature.Value.ToString("F2");
        }

        // Handle Slider Value Changed Events to update TextBlocks
        private void Sld_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateTextBlockValues();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Note that the input information should be as close to the reality as possible, otherwise it is possible that the output of the prediction program will not be correctly predicted.\n\n\n توجه داشته باشید اطلاعات ورودی تا حد امکان بایستی به واقعیت نزدیک باشد در غیر این صورت احتمال دارد در خروجی برنامه پیش بینی صحیح انجام نگیرد", "Information توجه کنید...", MessageBoxButton.OK, MessageBoxImage.Information);

        }
    }
}
