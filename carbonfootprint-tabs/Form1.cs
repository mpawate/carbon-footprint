            using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using static System.Net.WebRequestMethods;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Drawing.Layout;
namespace carbonfootprint_tabs
{
    public partial class Form1 : Form
    {
        // Global variables to store emission values
        private string totalLedEmission = "";
        private string totalFanEmission = "";
        private string totalKettleEmission = "";
        private string totalWaterEmission = "";
        private string totalElectricHeaterEmission = "";
        private string totalCustomEntryEmission = "";

        private string totalLeisureTravelCarEmission = "";
        private string totalLeisureTravelBikeEmission = "";
        private string totalHotelStayEmission = "";

        private string totalCommuteTravelCarEmission = "";
        private string totalCommuteTravelTrainEmission = "";
        private string totalCommuteTravelBusEmission = "";
        private string totalWorkHoursEmission = "";
        
        private string totalOrganicGardenWasteEmission = "";
        private string totalHouseholdResidualWasteEmission = "";
        private string totalOrganicFoodWasteEmission = "";
        private string selectedYear = "";

        // Define number of days in a year and working days for commute
        int daysInYear = 365;
        int workingDaysInYear = 254; //https://timetastic.co.uk/blog/how-many-working-days-are-in-a-year/

        // Boolean flags to track error state
        private bool isWattKettleErrorSet = false;
        private bool isHoursKettleErrorSet = false;
        private bool isQtyKettleErrorSet = false;

        private bool isWattLEDErrorSet = false;
        private bool isHoursLEDErrorSet = false;
        private bool isQtyLEDErrorSet = false;

        private bool isWattFanErrorSet = false;
        private bool isHoursFanErrorSet = false;
        private bool isQtyFanErrorSet = false;

        private bool isWattHeaterErrorSet = false;
        private bool isHoursHeaterErrorSet = false;
        private bool isQtyHeaterErrorSet = false;

        private bool isWattCustomErrorSet = false;
        private bool isHoursCustomErrorSet = false;
        private bool isQtyCustomErrorSet = false;

        private bool isWattWaterErrorSet = false;
        private bool isHoursWaterErrorSet = false;
        private bool isNumnerPersonWaterErrorSet = false;

        private bool isHotelStayErrorSet = false;
        private bool isCarLeisureMilesErrorSet = false;
        private bool isBikeLeisureMilesErrorSet = false;

        private bool isCommuteMilesErrorSet = false;
        private bool isHomeOfficeWorkHoursErrorSet = false;
        private bool isCarCommuteMilesErrorSet = false;

        private bool isWasteConsumptionErrorSet = false;
        private bool isNumberPersonWasteErrorSet = false;
        private bool isTrainCommuteMilesErrorSet = false;
        private bool isBusCommuteMilesErrorSet = false;

        string dbPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\conversion_factors.db";
        private Random random = new Random();

        //Unique functions
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckDatabaseConnection();
            OptimizeDatabase();
            // Add items to the ComboBox
            database_list_combobox.Items.Add("Year 2024");
            database_list_combobox.Items.Add("Year 2023");

            // Optionally set the default selected item
            database_list_combobox.SelectedIndex = 0; // This selects the first item, "Year 2024"
            selectedYear = "2024";
        }
        private void OptimizeDatabase()
        {
            try
            {
                string connectionString = $"Data Source={dbPath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand("VACUUM;", connection))
                    {
                        command.ExecuteNonQuery();
                        Debug.WriteLine("Database vacuumed successfully.");
                    }
                    using (SQLiteCommand command = new SQLiteCommand("ANALYZE;", connection))
                    {
                        command.ExecuteNonQuery();
                        Debug.WriteLine("Database analyzed successfully.");
                    }

                    // Delete data older than 6 months (or adjust the months parameter as needed)
                    DeleteOldData(6); // Deletes data older than 6 months
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Database optimization failed: {ex.Message}");
            }
        }
        private void DeleteOldData(int months)
        {
            try
            {
                string connectionString = $"Data Source={dbPath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Assuming each table has a 'date' or 'timestamp' column to track record creation or update time.
                    // This example assumes the table name is 'user_data' and the date column is named 'timestamp'.
                    string query = "DELETE FROM user_data WHERE timestamp < @OlderThanDate";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        DateTime olderThanDate = DateTime.Now.AddMonths(-months);
                        command.Parameters.AddWithValue("@OlderThanDate", olderThanDate.ToString("yyyy-MM-dd HH:mm:ss"));

                        int rowsAffected = command.ExecuteNonQuery();
                        Debug.WriteLine($"{rowsAffected} rows older than {months} months deleted.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to delete old data: {ex.Message}");
            }
        }
        private void btnPrivacyPolicy_Click(object sender, EventArgs e)
                {
                    string privacyPolicy = @"This application is designed to collect data related to the usage of household items, office commutes, leisure activities, and similar behaviors. The data collected is solely for the purpose of tracking and managing carbon footprints. No personal identifiers are collected, only the data regarding the usage patterns of the items and activities.

        User login credentials and all other logged data are securely encrypted using Advanced Encryption Standard (AES) with a 256-bit key. This encryption ensures that the data stored in the application's database is protected at rest, making it unreadable without proper decryption keys. Since the application is desktop-based and does not transmit data over the internet, encryption during data transmission is not required. All logged data is automatically deleted from the database after three months and is not recoverable after deletion.

        All scaling factors used in the application are stored in the database and are sourced from the official UK Government resources: 'ghg-conversion-factors-2023-condensed-set-update.xlsx' and 'ghg-conversion-factors-2024-condensed-set-update.xlsx'. These factors are provided by the UK Government for greenhouse gas (GHG) reporting and are suitable for use by UK-based organisations of all sizes, as well as international organisations reporting on UK operations. The scope of these factors is defined to be relevant to emissions reporting. They may also be used for other purposes, but users do so at their own risk.

        The application is focused on maintaining data security and protecting user privacy. No data is shared with third parties, and it is utilized exclusively to aid users in understanding and reducing their carbon footprints. By using this application, these practices are acknowledged and accepted.";

                    MessageBox.Show(privacyPolicy, "Privacy Policy", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }


        public class EnergyReport
        {
            public string Category { get; set; } // HomeEnergy, Commute, Waste, Leisure
            public string Item { get; set; } // LED, Fan, Kettle, Heater, etc.
            public double Usage { get; set; }
            public double AverageUsage { get; set; }
            public string Unit { get; set; }  // Added Unit property
            public string Feedback { get; set; }
            public string ImprovementTips { get; set; }
            public string YouTubeLink { get; set; }
        }
        private List<EnergyReport> energyReports = new List<EnergyReport>();
        public void AppendReport(string category, string item, double usage, double averageUsage, string feedback, string improvementTips, string youTubeLink, string unit)
        {
            var report = new EnergyReport
            {
                Category = category,
                Item = item,
                Usage = usage,
                AverageUsage = averageUsage,
                Feedback = feedback,
                ImprovementTips = improvementTips,
                YouTubeLink = youTubeLink,
                Unit = unit  // Set the unit
            };

            energyReports.Add(report);
        }

        // Updated method to show badge and random phrase based on energy usage
        private void database_list_combobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckDatabaseConnection();

            // Check if the selected item is not null
            if (database_list_combobox.SelectedItem != null)
            {
                // Get the selected year
                string selectedYearString = database_list_combobox.SelectedItem.ToString();

                // Update the selectedYear variable based on the selected year
                if (selectedYearString == "Year 2023")
                {
                    selectedYear = "2023";
                }
                else if (selectedYearString == "Year 2024")
                {
                    selectedYear = "2024";
                }

                // Recalculate carbon emissions using the selected year
                RecalculateCarbonEmissions(sender, e);

            }
            else
            {
                // Handle the case where no item is selected, if necessary
                Console.WriteLine("No year selected");
            }

        }
        private void RecalculateCarbonEmissions(object sender, EventArgs e)
        {
            OrganicGardenWaste_CalculateCarbon(sender, e);
            HouseholdResidualWaste_CalculateCarbon(sender, e);
            OrganicFoodWaste_CalculateCarbon(sender, e);
            OfficeCommute_CalculateCarbon(sender, e);
            CalculateHomeOfficeCarbon(sender, e);

            BikeLeisureTravel_CalculateBikeCarbon(sender, e);
            CarLeisureTravel_CalculateCarCarbon(sender, e);
            LeisureTravel_CalculateHotelRoomCarbon(sender, e);

            HomeEnergy_CalculateWaterCarbon(sender, e);
            Kettle_HomeEnergy_Carbon_Calculation(sender, e);
            Fan_HomeEnergy_Carbon_Calculation(sender, e);
            LED_HomeEnergy_Carbon_Calculation(sender, e);
            Heater_HomeEnergy_Carbon_Calculation(sender, e);
            CustomEntry_HomeEnergy_Carbon_Calculation(sender, e);
        }

        private void CheckDatabaseConnection()
        {
            bool isConnected = false;
            string dbPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\conversion_factors.db";
            string connectionString = $"Data Source={dbPath};Version=3;";

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    isConnected = true;
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions related to the connection check
                Console.WriteLine($"Database connection failed: {ex.Message}");
            }

            // Update the button based on connection status
            if (isConnected)
            {
                database_status_button.Text = "DB Connected";
                //database_status_button.BackColor = Color.Green;
                //database_status_button.ForeColor = Color.White; // Optional: To make the text readable
            }
            else
            {
                database_status_button.Text = "DB Disconnected";
                //database_status_button.BackColor = Color.Red;
                //database_status_button.ForeColor = Color.White; // Optional: To make the text readable
            }
        }
        /*
        private void CheckDatabaseConnection()
        {
            bool isConnected = false;
            string dbPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\conversion_factors.db";
            string connectionString = $"Data Source={dbPath};Version=3;";

            // Display the database path and connection string for debugging
            MessageBox.Show($"Attempting to connect to database at: {dbPath}", "Debug Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            MessageBox.Show($"Connection String: {connectionString}", "Debug Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    isConnected = true;
                    MessageBox.Show("Database connection successful!", "Debug Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions related to the connection check
                MessageBox.Show($"Database connection failed: {ex.Message}", "Debug Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Update the button based on connection status
            if (isConnected)
            {
                database_status_button.Text = "DB Connected";
                database_status_button.BackColor = Color.Green;
                database_status_button.ForeColor = Color.White; // Optional: To make the text readable
            }
            else
            {
                database_status_button.Text = "DB Disconnected";
                database_status_button.BackColor = Color.Red;
                database_status_button.ForeColor = Color.White; // Optional: To make the text readable
            }
        }*/

        private void ExitApp_button_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //Homeoffice commute
        private string HomeOfficeGetCarType()
        {
            if (CommuteTravel_CarType_Small_RadioButton.Checked)
            {
                return "small";
            }
            else if (CommuteTravel_CarType_Medium_RadioButton.Checked)
            {
                return "medium";
            }
            else if (CommuteTravel_CarType_Large_RadioButton.Checked)
            {
                return "large";
            }
            else
            {
                return "unknown";
            }
        }
        private string HomeOfficeGetFuelType()
        {
            if (CommuteTravel_FuelType_Petrol_RadioButton.Checked)
            {
                return "petrol";
            }
            else if (CommuteTravel_FuelType_Diesel_RadioButton.Checked)
            {
                return "diesel";
            }
            else if (CommuteTravel_FuelType_EV_RadioButton.Checked)
            {
                return "EV";
            }
            else
            {
                return "unknown";
            }
        }
        private bool TryGetMilesTravelledCommute(out double milesTravelled)
        {
            milesTravelled = 0;

            // Default feedback and UI reset
            CommuteTravel_emission_label.Text = "Emission"; // Assign default value
            feedback_officeCommute_Leisure_label.Text = "Feedback"; // Assign default value

            // Validate Miles Travelled
            if (string.IsNullOrWhiteSpace(CommuteTravel_MilesTravelled_Textbox.Text))
            {
                if (isCommuteMilesErrorSet)
                {
                    CommuteTravel_errorProvider.SetError(CommuteTravel_MilesTravelled_Textbox, string.Empty);
                    isCommuteMilesErrorSet = false;
                }
                totalCommuteTravelCarEmission = "";
                feedback_officeCommute_Leisure_label.Text = "Feedback"; // Assign default value

                updateGlobalLabel(this, EventArgs.Empty);
                return false;
            }
            else if (!double.TryParse(CommuteTravel_MilesTravelled_Textbox.Text, out double miles) || miles < 1 || miles > 100)
            {
                if (!isCommuteMilesErrorSet)
                {
                    CommuteTravel_errorProvider.SetError(CommuteTravel_MilesTravelled_Textbox,
                        "Please enter a valid number of miles between 1 and 100.\n" +
                        "   - Car: The average one-way distance is approximately 19.5 miles.\n" +
                        "   - Train: The average one-way distance is approximately 36.3 miles.\n" +
                        "   - Bus: The average one-way distance is approximately 9.7 miles.");
                    isCommuteMilesErrorSet = true;
                }
                totalCommuteTravelCarEmission = "";
                feedback_officeCommute_Leisure_label.Text = "Feedback"; // Assign default value

                updateGlobalLabel(this, EventArgs.Empty);
                return false;
            }
            else
            {
                if (isCommuteMilesErrorSet)
                {
                    CommuteTravel_errorProvider.SetError(CommuteTravel_MilesTravelled_Textbox, string.Empty);
                    isCommuteMilesErrorSet = false;
                }
                milesTravelled = miles;
                return true;
            }

        }

        void HandleCarSelection()
        {
            // Default feedback and UI reset
            CommuteTravel_emission_label.Text = "Emission"; // Assign default value
            feedback_Car_Leisure_label.Text = "Feedback"; // Assign default value

            // Clear the picturebox and label (if applicable)
            Award_Car_Leisure_picturebox.Image = null;
            Award_Car_Leisure_picturebox.Visible = false; // Hide the picturebox

            Award_Car_Leisure_label.Text = string.Empty;
            Award_Car_Leisure_label.Visible = false; // Hide the label
            feedback_Car_Leisure_label.Text = string.Empty;
            feedback_Car_Leisure_label.Visible = false; // Assign default value

            // Validate Inputs
            if (!TryGetMilesTravelledCommute(out double milesTravelled))
            {
                if (isCarCommuteMilesErrorSet)
                {
                    Car_CommuteTravel_errorProvider.SetError(CommuteTravel_MilesTravelled_Textbox, string.Empty);
                    isCarCommuteMilesErrorSet = false;
                }
                totalCommuteTravelCarEmission = "";
                feedback_Car_Leisure_label.Text = "Feedback"; // Assign default value

                updateGlobalLabel(this, EventArgs.Empty);
                return; // Exit the method if the input is invalid
            }
            else if (milesTravelled < 1 || milesTravelled > 100)
            {
                if (!isCarCommuteMilesErrorSet)
                {
                    Car_CommuteTravel_errorProvider.SetError(CommuteTravel_MilesTravelled_Textbox,
                        "Please enter a valid one-way travel distance between 1 and 100 miles. The average is 19.5 miles. Click 'Help' for more information."
                    );
                    isCarCommuteMilesErrorSet = true;
                }
                totalCommuteTravelCarEmission = "";
                feedback_Car_Leisure_label.Text = "Feedback"; // Assign default value

                updateGlobalLabel(this, EventArgs.Empty);
                return;
            }
            else
            {
                if (isCarCommuteMilesErrorSet)
                {
                    Car_CommuteTravel_errorProvider.SetError(CommuteTravel_MilesTravelled_Textbox, string.Empty);
                    isCarCommuteMilesErrorSet = false;
                }
            }

            // Validate Car Type and Fuel Type
            string carType = HomeOfficeGetCarType();
            string fuelType = HomeOfficeGetFuelType();

            if (carType == "unknown" || fuelType == "unknown")
            {
                Debug.WriteLine("Invalid car type or fuel type.");
                totalCommuteTravelCarEmission = "";
                feedback_Car_Leisure_label.Text = "Feedback"; // Assign default value

                updateGlobalLabel(this, EventArgs.Empty);
                return; // Exit the method if car type or fuel type is unknown
            }

            // Perform the calculation only if all inputs are valid
            if (milesTravelled >= 1 && milesTravelled <= 100 &&
                carType != "unknown" && fuelType != "unknown")
            {
                milesTravelled = milesTravelled * 2;//doubling to get roundtrip miles.

                // Use the carType, fuelType, and milesTravelled variables as needed
                string emissionFactor = GetEmissionFactor(carType, fuelType);
                string extractedEmissionFactor = ExtractEmissionFactorsValue(emissionFactor);
                double totalEmission = (milesTravelled)* Convert.ToDouble(extractedEmissionFactor);
                CommuteTravel_emission_label.Text = $"Total Emission: {totalEmission:F6} kg CO2e";
                totalCommuteTravelCarEmission = $"Total Emission: {totalEmission:F6} kg CO2e";
                updateGlobalLabel(this, EventArgs.Empty);

                // Provide feedback based on average commute mileage
                string improvementTips = "";
                string youTubeLink = "";
                double averageMiles = 19.5;

                if (milesTravelled > averageMiles)
                {
                    feedback_officeCommute_Leisure_label.Text = $"Your daily commute travel of {milesTravelled} miles is higher than the expected average of {averageMiles} miles for commuting purposes.";
                    improvementTips = "Consider carpooling, using public transportation, or switching to a more fuel-efficient vehicle to reduce your carbon footprint.";
                    youTubeLink = "https://www.youtube.com/watch?v=aQrzTrAh_bg";
                }
                else
                {
                    feedback_officeCommute_Leisure_label.Text = $"Your daily commute travel of {milesTravelled} miles is within the expected average of {averageMiles} miles for commuting purposes.";
                    improvementTips = "Great job! Keep up the efficient commuting practices.";
                    youTubeLink = "No suggestions";
                }
                feedback_officeCommute_Leisure_label.Visible = true;

                UpdateCarCommuteBadge(milesTravelled, averageMiles);
                // Append the report to the HomeEnergy category
                // Conditionally append the report data
                if (shouldAppend)
                {
                    AppendReport("HomeOffice/Commute", "OfficeCarCommute", milesTravelled, averageMiles, feedback_officeCommute_Leisure_label.Text, improvementTips, youTubeLink, "miles");
                }

            }

            Debug.WriteLine($"Total emission: {totalCommuteTravelCarEmission} kg CO2e");
        }
        private void UpdateCarCommuteBadge(double milesTravelled, double averageMiles)
        {
            // Define arrays for the images
            Bitmap[] goodPerformanceImages = {
                Properties.Resources.crown1,
                Properties.Resources.crown2,
                Properties.Resources.trophy_star,
                Properties.Resources.award,
                Properties.Resources.trophy,
                Properties.Resources.ribbon
            };

            Bitmap[] improvementImages = {
                Properties.Resources.target,
                Properties.Resources.person,
                Properties.Resources.business,
                Properties.Resources.fail
            };

            // Define arrays for the phrases (shortened to two words)
            string[] goodPerformancePhrases = {
                "Eco Star",
                "Great Job",
                "Top Performer",
                "Keep Going",
                "Well Done"
            };

            string[] improvementPhrases = {
                "Try Harder",
                "Improve More",
                "Keep Going",
                "Almost There",
                "Step Up"
            };

            // Generate random indexes for each array separately
            int goodImageIndex = random.Next(goodPerformanceImages.Length);
            int improvementImageIndex = random.Next(improvementImages.Length);

            int goodPhraseIndex = random.Next(goodPerformancePhrases.Length);
            int improvementPhraseIndex = random.Next(improvementPhrases.Length);

            if (milesTravelled <= averageMiles)
            {
                // Show the "Eco Warrior" badge
                Award_officeCommute_Leisure_pictureBox.Image = goodPerformanceImages[goodImageIndex];
                Award_officeCommute_Leisure_label.Text = goodPerformancePhrases[goodPhraseIndex];
            }
            else
            {
                // Show the "You Can Do Better" feedback
                Award_officeCommute_Leisure_pictureBox.Image = improvementImages[improvementImageIndex];
                Award_officeCommute_Leisure_label.Text = improvementPhrases[improvementPhraseIndex];
            }

            // Set the PictureBox's SizeMode to StretchImage to ensure the image covers the entire PictureBox
            Award_officeCommute_Leisure_pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

            // Make sure the PictureBox and Label are visible
            Award_officeCommute_Leisure_pictureBox.Visible = true;
            Award_officeCommute_Leisure_label.Visible = true;
        }
        void HandleTrainSelection()
        {
            // Default feedback and UI reset
            CommuteTravel_emission_label.Text = "Emission"; // Assign default value
            feedback_officeCommute_Leisure_label.Text = "Feedback"; // Assign default value

            // Clear the picturebox and label (if applicable)
            Award_officeCommute_Leisure_pictureBox.Image = null;
            Award_officeCommute_Leisure_pictureBox.Visible = false; // Hide the picturebox

            Award_officeCommute_Leisure_label.Text = string.Empty;
            Award_officeCommute_Leisure_label.Visible = false; // Hide the label
            feedback_officeCommute_Leisure_label.Text = string.Empty;
            feedback_officeCommute_Leisure_label.Visible = false; // Assign default value

            // Validate Inputs
            if (!TryGetMilesTravelledCommute(out double milesTravelled))
            {
                if (isTrainCommuteMilesErrorSet)
                {
                    Train_CommuteTravel_errorProvider.SetError(CommuteTravel_MilesTravelled_Textbox, string.Empty);
                    isTrainCommuteMilesErrorSet = false;
                }
                totalCommuteTravelTrainEmission = "";
                feedback_officeCommute_Leisure_label.Text = "Feedback"; // Assign default value

                updateGlobalLabel(this, EventArgs.Empty);
                return; // Exit the method if the input is invalid
            }
            else if (milesTravelled < 1 || milesTravelled > 100)
            {
                if (!isTrainCommuteMilesErrorSet)
                {
                    Train_CommuteTravel_errorProvider.SetError(CommuteTravel_MilesTravelled_Textbox,
                        "Please enter a valid number of miles for one-way travel between 1 and 100 miles. The average one-way distance is approximately 36.3 miles for train travel. Click Help for more information."
                    );
                    isTrainCommuteMilesErrorSet = true;
                }
                totalCommuteTravelTrainEmission = "";
                feedback_officeCommute_Leisure_label.Text = "Feedback"; // Assign default value

                updateGlobalLabel(this, EventArgs.Empty);
                return;
            }
            else
            {
                if (isTrainCommuteMilesErrorSet)
                {
                    Train_CommuteTravel_errorProvider.SetError(CommuteTravel_MilesTravelled_Textbox, string.Empty);
                    isTrainCommuteMilesErrorSet = false;
                }
            }

            // Perform the calculation only if all inputs are valid
            if (milesTravelled >= 1 && milesTravelled <= 100)
            {
                //milesTravelled = (milesTravelled * 2) * workingDaysInYear; // Calculate the total annual miles
                                                                           // Use the milesTravelled variable as needed
                string emissionFactorTrain = GetEmissionFactorTrain();
                string extractedEmissionFactor = ExtractEmissionFactorsValue(emissionFactorTrain);
                milesTravelled = milesTravelled * 2;//doubling to get roundtrip miles.
                double totalEmission = milesTravelled * Convert.ToDouble(extractedEmissionFactor);
                CommuteTravel_emission_label.Text = $"Total Emission: {totalEmission:F6} kg CO2e";
                totalCommuteTravelTrainEmission = $"Total Emission: {totalEmission:F6} kg CO2e";
                updateGlobalLabel(this, EventArgs.Empty);

                // Provide feedback based on average mileage
                string improvementTips = "";
                string youTubeLink = "";
                double averageMiles = 36.3; // Corrected average miles per person per year for train travel

                if (milesTravelled > averageMiles)
                {
                    feedback_officeCommute_Leisure_label.Text = $"Feedback: Your mileage of {milesTravelled} miles/day is higher than the average of {averageMiles} miles.";
                    improvementTips = "Consider reducing train travel frequency or exploring remote work options.";
                    youTubeLink = "https://www.youtube.com/watch?v=eco_travel_tips";
                }
                else
                {
                    feedback_officeCommute_Leisure_label.Text = $"Feedback: Your mileage of {milesTravelled} miles/day is within the average range of {averageMiles} miles.";
                    improvementTips = "Great job! Keep up the efficient travel practices.";
                    youTubeLink = "No suggestions";
                }

                feedback_officeCommute_Leisure_label.Visible = true;

                UpdateTrainCommuteBadge(milesTravelled, averageMiles);
                if (shouldAppend)
                {
                    AppendReport("HomeOffice/Commute", "OfficeTrainCommute", milesTravelled, averageMiles, feedback_officeCommute_Leisure_label.Text, improvementTips, youTubeLink, "miles");
                }
            }

            Debug.WriteLine($"Total emission: {totalCommuteTravelTrainEmission} kg CO2e");
        }
        private void UpdateTrainCommuteBadge(double milesTravelled, double averageMiles)
        {
            // Define arrays for the images
            Bitmap[] goodPerformanceImages = {
                Properties.Resources.crown1,
                Properties.Resources.crown2,
                Properties.Resources.trophy_star,
                Properties.Resources.award,
                Properties.Resources.trophy,
                Properties.Resources.ribbon
            };

                    Bitmap[] improvementImages = {
                Properties.Resources.target,
                Properties.Resources.person,
                Properties.Resources.business,
                Properties.Resources.fail
            };

            // Define arrays for the phrases (shortened to two words)
            string[] goodPerformancePhrases = {
                "Eco Star",
                "Great Job",
                "Top Performer",
                "Keep Going",
                "Well Done"
            };

            string[] improvementPhrases = {
                "Try Harder",
                "Improve More",
                "Keep Going",
                "Almost There",
                "Step Up"
            };

            // Generate random indexes for each array separately
            int goodImageIndex = random.Next(goodPerformanceImages.Length);
            int improvementImageIndex = random.Next(improvementImages.Length);

            int goodPhraseIndex = random.Next(goodPerformancePhrases.Length);
            int improvementPhraseIndex = random.Next(improvementPhrases.Length);

            if (milesTravelled <= averageMiles)
            {
                // Show the "Eco Warrior" badge
                Award_officeCommute_Leisure_pictureBox.Image = goodPerformanceImages[goodImageIndex];
                Award_officeCommute_Leisure_label.Text = goodPerformancePhrases[goodPhraseIndex];
            }
            else
            {
                // Show the "You Can Do Better" feedback
                Award_officeCommute_Leisure_pictureBox.Image = improvementImages[improvementImageIndex];
                Award_officeCommute_Leisure_label.Text = improvementPhrases[improvementPhraseIndex];
            }

            // Set the PictureBox's SizeMode to StretchImage to ensure the image covers the entire PictureBox
            Award_officeCommute_Leisure_pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

            // Make sure the PictureBox and Label are visible
            Award_officeCommute_Leisure_pictureBox.Visible = true;
            Award_officeCommute_Leisure_label.Visible = true;
        }
        void HandleBusSelection()
        {
            // Default feedback and UI reset
            CommuteTravel_emission_label.Text = "Emission"; // Assign default value
            feedback_officeCommute_Leisure_label.Text = "Feedback"; // Assign default value

            // Clear the picturebox and label (if applicable)
            Award_officeCommute_Leisure_pictureBox.Image = null;
            Award_officeCommute_Leisure_pictureBox.Visible = false; // Hide the picturebox

            Award_officeCommute_Leisure_label.Text = string.Empty;
            Award_officeCommute_Leisure_label.Visible = false; // Hide the label
            feedback_officeCommute_Leisure_label.Text = string.Empty;
            feedback_officeCommute_Leisure_label.Visible = false; // Assign default value

            // Validate Inputs
            if (!TryGetMilesTravelledCommute(out double milesTravelled))
            {
                if (isBusCommuteMilesErrorSet)
                {
                    Bus_CommuteTravel_errorProvider.SetError(CommuteTravel_MilesTravelled_Textbox, string.Empty);
                    isBusCommuteMilesErrorSet = false;
                }
                totalCommuteTravelBusEmission = "";
                feedback_officeCommute_Leisure_label.Text = "Feedback"; // Assign default value

                updateGlobalLabel(this, EventArgs.Empty);
                return; // Exit the method if the input is invalid
            }
            else if (milesTravelled < 1 || milesTravelled > 100)
            {
                if (!isBusCommuteMilesErrorSet)
                {
                    Bus_CommuteTravel_errorProvider.SetError(CommuteTravel_MilesTravelled_Textbox,
                        "Please enter a valid number of miles for one-way travel between 1 and 100 miles. The average one-way distance is approximately 9.7 miles for bus travel."
                    );
                    isBusCommuteMilesErrorSet = true;
                }
                totalCommuteTravelBusEmission = "";
                feedback_officeCommute_Leisure_label.Text = "Feedback"; // Assign default value

                updateGlobalLabel(this, EventArgs.Empty);
                return;
            }
            else
            {
                if (isBusCommuteMilesErrorSet)
                {
                    Bus_CommuteTravel_errorProvider.SetError(CommuteTravel_MilesTravelled_Textbox, string.Empty);
                    isBusCommuteMilesErrorSet = false;
                }
            }

            // Perform the calculation only if all inputs are valid
            if (milesTravelled >= 1 && milesTravelled <= 100)
            {
                string emissionFactorBus = GetEmissionFactorBus();
                string extractedEmissionFactor = ExtractEmissionFactorsValue(emissionFactorBus);
                milesTravelled = milesTravelled * 2; //doubling to get too and fro data for roundtrip.
                double totalEmission = milesTravelled * Convert.ToDouble(extractedEmissionFactor);
                CommuteTravel_emission_label.Text = $"Total Emission: {totalEmission:F6} kg CO2e";
                totalCommuteTravelBusEmission = $"Total Emission: {totalEmission:F6} kg CO2e";
                updateGlobalLabel(this, EventArgs.Empty);

                // Provide feedback based on average mileage
                string improvementTips = "";
                string youTubeLink = "";
                double averageMiles = 9.7; // Example average miles per person per year for bus travel

                if (milesTravelled > averageMiles)
                {
                    feedback_officeCommute_Leisure_label.Text = $"Feedback: Your mileage of {milesTravelled} miles/day is higher than the average of {averageMiles} miles.";
                    improvementTips = "Consider reducing bus travel frequency or exploring more sustainable alternatives.";
                    youTubeLink = "https://www.youtube.com/watch?v=SI_XW-1Hwjc";
                }
                else
                {
                    feedback_officeCommute_Leisure_label.Text = $"Feedback: Your mileage of {milesTravelled} miles/day is within the average range of {averageMiles} miles.";
                    improvementTips = "Great job! Keep up the efficient travel practices.";
                    youTubeLink = "No suggestions";
                }

                feedback_officeCommute_Leisure_label.Visible = true;

                UpdateBusCommuteBadge(milesTravelled, averageMiles);
                if (shouldAppend)
                {
                    AppendReport("HomeOffice/Commute", "OfficeBusCommute", milesTravelled, averageMiles, feedback_officeCommute_Leisure_label.Text, improvementTips, youTubeLink, "miles");
                }

            }

            Debug.WriteLine($"Total emission: {totalCommuteTravelBusEmission} kg CO2e");
        }
        private void UpdateBusCommuteBadge(double milesTravelled, double averageMiles)
        {
            // Define arrays for the images
            Bitmap[] goodPerformanceImages = {
                Properties.Resources.crown1,
                Properties.Resources.crown2,
                Properties.Resources.trophy_star,
                Properties.Resources.award,
                Properties.Resources.trophy,
                Properties.Resources.ribbon
            };

            Bitmap[] improvementImages = {
                Properties.Resources.target,
                Properties.Resources.person,
                Properties.Resources.business,
                Properties.Resources.fail
            };

            // Define arrays for the phrases (shortened to two words)
            string[] goodPerformancePhrases = {
                "Eco Star",
                "Great Job",
                "Top Performer",
                "Keep Going",
                "Well Done"
            };

            string[] improvementPhrases = {
                "Try Harder",
                "Improve More",
                "Keep Going",
                "Almost There",
                "Step Up"
            };

            // Generate random indexes for each array separately
            int goodImageIndex = random.Next(goodPerformanceImages.Length);
            int improvementImageIndex = random.Next(improvementImages.Length);

            int goodPhraseIndex = random.Next(goodPerformancePhrases.Length);
            int improvementPhraseIndex = random.Next(improvementPhrases.Length);

            if (milesTravelled <= averageMiles)
            {
                // Show the "Eco Warrior" badge
                Award_officeCommute_Leisure_pictureBox.Image = goodPerformanceImages[goodImageIndex];
                Award_officeCommute_Leisure_label.Text = goodPerformancePhrases[goodPhraseIndex];
            }
            else
            {
                // Show the "You Can Do Better" feedback
                Award_officeCommute_Leisure_pictureBox.Image = improvementImages[improvementImageIndex];
                Award_officeCommute_Leisure_label.Text = improvementPhrases[improvementPhraseIndex];
            }

            // Set the PictureBox's SizeMode to StretchImage to ensure the image covers the entire PictureBox
            Award_officeCommute_Leisure_pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

            // Make sure the PictureBox and Label are visible
            Award_officeCommute_Leisure_pictureBox.Visible = true;
            Award_officeCommute_Leisure_label.Visible = true;
        }
        private void OfficeCommute_CalculateCarbon(object sender, EventArgs e)
        {
            if (Commute_Car.Checked)
            {
                totalCommuteTravelTrainEmission = "";
                totalCommuteTravelBusEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);
                // Clear car type and fuel type radio buttons
                carType_groupBox.Enabled = true;  // Disable the car type group box
                fuelType_groupBox.Enabled = true;  // Disable the car type group box

                // Default feedback and UI reset
                CommuteTravel_emission_label.Text = "Total Emission:"; // Assign default value

                // Clear the picturebox and label (if applicable)
                Award_officeCommute_Leisure_pictureBox.Image = null;
                Award_officeCommute_Leisure_pictureBox.Visible = false; // Hide the picturebox

                Award_officeCommute_Leisure_label.Text = string.Empty;
                Award_officeCommute_Leisure_label.Visible = false; // Hide the label
                feedback_officeCommute_Leisure_label.Text = string.Empty;
                feedback_officeCommute_Leisure_label.Visible = false; // Assign default value


                HandleCarSelection();
            }
            else if (Commute_Train.Checked)
            {
                totalCommuteTravelCarEmission = "";
                totalCommuteTravelBusEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                // Clear car type and fuel type radio buttons
                CommuteTravel_CarType_Small_RadioButton.Checked = false;
                CommuteTravel_CarType_Medium_RadioButton.Checked = false;
                CommuteTravel_CarType_Large_RadioButton.Checked = false;
                CommuteTravel_FuelType_Petrol_RadioButton.Checked = false;
                CommuteTravel_FuelType_Diesel_RadioButton.Checked = false;
                CommuteTravel_FuelType_EV_RadioButton.Checked = false;

                // Default feedback and UI reset
                CommuteTravel_emission_label.Text = "Total Emission:"; // Assign default value

                // Clear the picturebox and label (if applicable)
                Award_officeCommute_Leisure_pictureBox.Image = null;
                Award_officeCommute_Leisure_pictureBox.Visible = false; // Hide the picturebox

                Award_officeCommute_Leisure_label.Text = string.Empty;
                Award_officeCommute_Leisure_label.Visible = false; // Hide the label
                feedback_officeCommute_Leisure_label.Text = string.Empty;
                feedback_officeCommute_Leisure_label.Visible = false; // Assign default value


                carType_groupBox.Enabled = false;  // Disable the car type group box
                fuelType_groupBox.Enabled = false;  // Disable the car type group box
                HandleTrainSelection();
            }
            else if (Commute_Bus.Checked)
            {
                totalCommuteTravelCarEmission = "";
                totalCommuteTravelTrainEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                // Clear car type and fuel type radio buttons
                CommuteTravel_CarType_Small_RadioButton.Checked = false;
                CommuteTravel_CarType_Medium_RadioButton.Checked = false;
                CommuteTravel_CarType_Large_RadioButton.Checked = false;
                CommuteTravel_FuelType_Petrol_RadioButton.Checked = false;
                CommuteTravel_FuelType_Diesel_RadioButton.Checked = false;
                CommuteTravel_FuelType_EV_RadioButton.Checked = false;

                // Default feedback and UI reset
                CommuteTravel_emission_label.Text = "Total Emission:"; // Assign default value

                // Clear the picturebox and label (if applicable)
                Award_officeCommute_Leisure_pictureBox.Image = null;
                Award_officeCommute_Leisure_pictureBox.Visible = false; // Hide the picturebox

                Award_officeCommute_Leisure_label.Text = string.Empty;
                Award_officeCommute_Leisure_label.Visible = false; // Hide the label
                feedback_officeCommute_Leisure_label.Text = string.Empty;
                feedback_officeCommute_Leisure_label.Visible = false; // Assign default value

                carType_groupBox.Enabled = false;  // Disable the car type group box
                fuelType_groupBox.Enabled = false;  // Disable the car type group box
                HandleBusSelection();
            }
        }
        private void HelpClickMe_CommuteTravel_button_Click(object sender, EventArgs e)
        {
            // Show detailed help message for commute travel
            MessageBox.Show(
                "Daily Commute Travel Data:\n\n" +
                "1. **One-Way Distance (Miles):**\n" +
                "   - Enter the one-way distance (in miles) for your daily commute to work.\n" +
                "   - Example: The average one-way distance is approximately:\n" +
                "       - Car: 19.5 miles\n" +
                "       - Train: 36.3 miles\n" +
                "       - Bus: 9.7 miles\n" +
                "   - Valid range: 1 to 100 miles.\n\n" +
                "2. **Annual Commute Calculation:**\n" +
                "   - The one-way distance will be doubled to account for round-trip travel.\n" +
                "   - It will then be multiplied by the number of working days in a year (typically 254 days) to calculate your total annual commuting distance.\n\n" +
                "3. **Carbon Emission Calculation:**\n" +
                "   - This data will be used to calculate your annual carbon emission for commute travel based on the mode of transport you select.\n\n" +
                "Note: Accurate and realistic data entry is crucial for calculating your carbon footprint. Commuting can significantly contribute to your carbon footprint. Understanding the impact of different transport modes can help you make more sustainable choices.\n\n" +
                "Source: The average one-way distances for different modes of transport are derived from the Commuter Census 2022 report. For more details, refer to the source: https://cdn.asp.events/CLIENT_Innovati_94A26F7C_B3C0_752F_CC179EFAFD17992A/sites/Innovation-Zero-2023/media/Reports/Commuter-Census-2022.pdf.\n\n" +
                "The number of working days (254) is based on the analysis provided by Timetastic. For more details, refer to the source: https://timetastic.co.uk/blog/how-many-working-days-are-in-a-year/.",
                "Help Information - Commute Travel",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }


        //WorkFromHome carbon emission calculation
        private void CalculateHomeOfficeCarbon(object sender, EventArgs e)
        {
            double totalworkhours = 0;

            // Validate inputs
            bool isValid = true;

            // Default feedback and UI reset
            HomeOffice_Emission_label.Text = "Emission"; // Assign default value
            feedback_homeOffice_Commute_label.Text = "Feedback"; // Assign default value

            // Clear the picturebox and label
            Award_homeOffice_commute_picturebox.Image = null;
            Award_homeOffice_commute_picturebox.Visible = false; // Hide the picturebox

            Award_homeOffice_Commute_label.Text = string.Empty;
            Award_homeOffice_Commute_label.Visible = false; // Hide the label
            feedback_homeOffice_Commute_label.Text = string.Empty;
            feedback_homeOffice_Commute_label.Visible = false; // Hide the label

            // Validate Working Hours
            if (string.IsNullOrWhiteSpace(HomeOffice_WorkingHours_Textbox.Text))
            {
                if (isHomeOfficeWorkHoursErrorSet)
                {
                    homeOffice_errorProvider.SetError(HomeOffice_WorkingHours_Textbox, string.Empty);
                    isHomeOfficeWorkHoursErrorSet = false;
                }
                totalWorkHoursEmission = "";
                feedback_homeOffice_Commute_label.Text = "Feedback"; // Assign default value

                updateGlobalLabel(this, EventArgs.Empty);
                return;
            }
            else if (!double.TryParse(HomeOffice_WorkingHours_Textbox.Text, out totalworkhours) || totalworkhours < 1 || totalworkhours > 8)
            {
                isValid = false;
                if (!isHomeOfficeWorkHoursErrorSet)
                {
                    homeOffice_errorProvider.SetError(HomeOffice_WorkingHours_Textbox, "Please enter a valid number of work hours between 1 and 8.");
                    isHomeOfficeWorkHoursErrorSet = true;
                }
                totalWorkHoursEmission = "";
                feedback_homeOffice_Commute_label.Text = "Feedback"; // Assign default value

                updateGlobalLabel(this, EventArgs.Empty);
                return;
            }
            else
            {
                if (isHomeOfficeWorkHoursErrorSet)
                {
                    homeOffice_errorProvider.SetError(HomeOffice_WorkingHours_Textbox, string.Empty);
                    isHomeOfficeWorkHoursErrorSet = false;
                }
            }

            // If validation fails, return
            if (!isValid)
            {
                totalWorkHoursEmission = "";
                feedback_homeOffice_Commute_label.Text = "Feedback"; // Assign default value

                updateGlobalLabel(this, EventArgs.Empty);
                return;
            }

            // Perform the calculation only if all inputs are valid
            if (!string.IsNullOrWhiteSpace(HomeOffice_WorkingHours_Textbox.Text))
            {
                totalWorkHoursEmission = CalculateTotalCarbonEmissionWorkHours(totalworkhours);

                HomeOffice_Emission_label.Text = $"Emission: {ExtractEmissionValue(totalWorkHoursEmission):F6} kg CO2e";
                updateGlobalLabel(this, EventArgs.Empty);

                // Provide feedback based on average usage or thresholds
                string improvementTips = "";
                string youTubeLink = "";
                double averageWorkingHours = 7; // Average number of working hours per day

                if (totalworkhours > averageWorkingHours)
                {
                    feedback_homeOffice_Commute_label.Text = $"Your daily working hours of {totalworkhours} exceed the average of {averageWorkingHours} hours.";
                    improvementTips = "Consider managing your work schedule to balance your working hours and reduce energy consumption during extended hours.";
                    youTubeLink = "https://www.youtube.com/watch?v=8cF442d-EdQ";
                }
                else
                {
                    feedback_homeOffice_Commute_label.Text = $"Your daily working hours of {totalworkhours} are within the average range of {averageWorkingHours} hours.";
                    improvementTips = "Good job on maintaining a balanced work schedule! Continue practicing efficient energy use during your working hours.";
                    youTubeLink = "No suggestions";
                }
                feedback_homeOffice_Commute_label.Visible = true;

                UpdateHomeOfficeBadge(totalworkhours, averageWorkingHours); // Update UI with badges or rewards based on user input

                // Example of using these variables further, like adding to a report or displaying elsewhere
                if (shouldAppend)
                {
                    AppendReport("HomeOffice", "WorkingHours", totalworkhours, averageWorkingHours, feedback_homeOffice_Commute_label.Text, improvementTips, youTubeLink, "Hours");
                }

            }
        }
        private void UpdateHomeOfficeBadge(double userWorkHours, double averageWorkHours)
        {
            // Define arrays for the images
            Bitmap[] goodPerformanceImages = {
                Properties.Resources.crown1,
                Properties.Resources.crown2,
                Properties.Resources.trophy_star,
                Properties.Resources.award,
                Properties.Resources.trophy,
                Properties.Resources.ribbon
            };

            Bitmap[] improvementImages = {
                Properties.Resources.target,
                Properties.Resources.person,
                Properties.Resources.business,
                Properties.Resources.fail
            };

            // Define arrays for the phrases (shortened to two words)
            string[] goodPerformancePhrases = {
                "Eco Star",
                "Great Job",
                "Top Performer",
                "Keep Going",
                "Well Done"
            };

            string[] improvementPhrases = {
                "Try Harder",
                "Improve More",
                "Keep Going",
                "Almost There",
                "Step Up"
            };

            // Generate random indexes for each array separately
            int goodImageIndex = random.Next(goodPerformanceImages.Length);
            int improvementImageIndex = random.Next(improvementImages.Length);

            int goodPhraseIndex = random.Next(goodPerformancePhrases.Length);
            int improvementPhraseIndex = random.Next(improvementPhrases.Length);

            if (userWorkHours <= averageWorkHours)
            {
                // Show the "Eco Warrior" badge
                Award_homeOffice_commute_picturebox.Image = goodPerformanceImages[goodImageIndex];
                Award_homeOffice_Commute_label.Text = goodPerformancePhrases[goodPhraseIndex];
            }
            else
            {
                // Show the "You Can Do Better" feedback
                Award_homeOffice_commute_picturebox.Image = improvementImages[improvementImageIndex];
                Award_homeOffice_Commute_label.Text = improvementPhrases[improvementPhraseIndex];
            }

            // Set the PictureBox's SizeMode to StretchImage to ensure the image covers the entire PictureBox
            Award_homeOffice_commute_picturebox.SizeMode = PictureBoxSizeMode.StretchImage;

            // Make sure the PictureBox and Label are visible
            Award_homeOffice_commute_picturebox.Visible = true;
            Award_homeOffice_Commute_label.Visible = true;
        }
        private string CalculateTotalCarbonEmissionWorkHours(double totalworkhours)
        {
            double homeworkingEmissionFactor = 0;
            string connectionString = $"Data Source={dbPath};Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                //string query = "SELECT * FROM conversion_factor WHERE Unit = @Unit";
                string query = "SELECT* FROM conversion_factor WHERE Activity = @Activity AND Type = @Type AND Year = @Year AND Unit = @Unit";
                //string query = input;
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Activity", "Homeworking (office equipment + heating)");
                    command.Parameters.AddWithValue("@Type", "NA");
                    command.Parameters.AddWithValue("@Unit", "per FTE Working Hour");
                    command.Parameters.AddWithValue("@Year", selectedYear);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Carbon emission factors per kWh for electricity generation in the UK
                            homeworkingEmissionFactor = reader.GetDouble(reader.GetOrdinal("kg CO2e"));
                        }
                    }
                }
            }
            // Emission factor for homeworking (office equipment + heating)
            //double homeworkingEmissionFactor = 0.33378; // kg CO2e per FTE Working Hour

            // Calculate total carbon emissions from generation
            double totalGenerationEmission = totalworkhours * homeworkingEmissionFactor;

            // Output or use these values as needed
            Debug.WriteLine($"Total Carbon Emission for Work Hours: {totalGenerationEmission} kg CO2e");

            // Optionally update UI or store these values
            // resultLabel.Text = $"Total Carbon Emission: {overallTotalEmission} kg CO2e";
            //led_op_Total_KWh_label.Text = $"Total Emission: {overallTotalEmission} kg CO2e (CO2: {overallCO2Emission}, CH4: {overallCH4Emission}, N2O: {overallN2OEmission})";
            // Create the result string
            //string result = $"Total Emission: {overallTotalEmission} kg CO2e (CO2: {overallCO2Emission}, CH4: {overallCH4Emission}, N2O: {overallN2OEmission})";
            string result = $"Total Emission: {totalGenerationEmission:F6} kg CO2e (CO2: {0:F6}, CH4: {0:F6}, N2O: {0:F6})";

            // Output for debugging purposes
            Debug.WriteLine(result);

            // Return the result string
            return result;
        }
        private void HelpClickMe_WorkingHours_button_Click(object sender, EventArgs e)
        {
            // Show detailed help message for Home Office Working Hours
            MessageBox.Show(
                "Home Office Working Hours Data:\n\n" +
                "1. **Daily Working Hours:**\n" +
                "   - Enter the total number of hours you work from home each day.\n" +
                "   - Example: 7 hours is a typical value.\n" +
                "   - Valid range: 4 hours to 8 hours.\n\n" +
                "Note: A standard full-time workday typically lasts 8 hours. Entering accurate data will allow for a precise calculation of your daily carbon emissions from working at home, considering energy usage during these hours.",
                "Help Information - Home Office Working Hours",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        //Leisure Car Emission
        private string LeisureTravelGetCarType()
        {
            if (LeisureTravel_CarType_Small_RadioButton.Checked)
            {
                return "small";
            }
            else if (LeisureTravel_CarType_Medium_RadioButton.Checked)
            {
                return "medium";
            }
            else if (LeisureTravel_CarType_Large_RadioButton.Checked)
            {
                return "large";
            }
            else
            {
                return "unknown";
            }
        }
        private string LeisureTravelGetFuelType()
        {
            if (LeisureTravel_FuelType_Petrol_RadioButton.Checked)
            {
                return "petrol";
            }
            else if (LeisureTravel_FuelType_Diesel_RadioButton.Checked)
            {
                return "diesel";
            }
            else if (LeisureTravel_FuelType_EV_RadioButton.Checked)
            {
                return "EV";
            }
            else
            {
                return "unknown";
            }
        }
        private void CarLeisureTravel_CalculateCarCarbon(object sender, EventArgs e)
        {
            double milesTravelled = 0;

            // Validate inputs
            bool isValid = true;

            // Default feedback and UI reset
            leisuretravel_car_emission_label.Text = "Emission"; // Assign default value
            feedback_Car_Leisure_label.Text = "Feedback"; // Assign default value

            // Clear the picturebox and label
            Award_Car_Leisure_picturebox.Image = null;
            Award_Car_Leisure_picturebox.Visible = false; // Hide the picturebox

            Award_Car_Leisure_label.Text = string.Empty;
            Award_Car_Leisure_label.Visible = false; // Hide the label
            feedback_Car_Leisure_label.Text = string.Empty;
            feedback_Car_Leisure_label.Visible = false; // Assign default value

            // Validate Miles Travelled
            if (string.IsNullOrWhiteSpace(MilesTravelled_Car_LeisureTravel_Textbox.Text))
            {
                if (isCarLeisureMilesErrorSet)
                {
                    Car_LeisureTravel_errorProvider.SetError(MilesTravelled_Car_LeisureTravel_Textbox, string.Empty);
                    isCarLeisureMilesErrorSet = false;
                }
                totalLeisureTravelCarEmission = "";
                feedback_Car_Leisure_label.Text = "Feedback"; // Assign default value

                updateGlobalLabel(this, EventArgs.Empty);
                return;
            }
            else if (!double.TryParse(MilesTravelled_Car_LeisureTravel_Textbox.Text, out milesTravelled) || milesTravelled < 1 || milesTravelled > 5000)
            {
                isValid = false;
                if (!isCarLeisureMilesErrorSet)
                {
                    Car_LeisureTravel_errorProvider.SetError(MilesTravelled_Car_LeisureTravel_Textbox, "Enter a value between 100 miles and 5000 miles. Example: 1053 miles (average). Click for Help..");
                    isCarLeisureMilesErrorSet = true;
                }
                totalLeisureTravelCarEmission = "";
                feedback_Car_Leisure_label.Text = "Feedback"; // Assign default value

                updateGlobalLabel(this, EventArgs.Empty);
                return;
            }
            else
            {
                if (isCarLeisureMilesErrorSet)
                {
                    Car_LeisureTravel_errorProvider.SetError(MilesTravelled_Car_LeisureTravel_Textbox, string.Empty);
                    isCarLeisureMilesErrorSet = false;
                }
            }

            // Validate Car Type and Fuel Type
            string carType = LeisureTravelGetCarType();
            string fuelType = LeisureTravelGetFuelType();

            if (carType == "unknown" || fuelType == "unknown")
            {
                isValid = false;
                //MessageBox.Show("Please select a valid car type and fuel type.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                totalLeisureTravelCarEmission = "";
                feedback_Car_Leisure_label.Text = "Feedback"; // Assign default value

                updateGlobalLabel(this, EventArgs.Empty);
                return;
            }

            // If validation fails, return
            if (!isValid)
            {
                totalLeisureTravelCarEmission = "";
                feedback_Car_Leisure_label.Text = "Feedback"; // Assign default value

                updateGlobalLabel(this, EventArgs.Empty);
                return;
            }

            // Perform the calculation only if all inputs are valid
            if (!string.IsNullOrWhiteSpace(MilesTravelled_Car_LeisureTravel_Textbox.Text) &&
                carType != "unknown" &&
                fuelType != "unknown")
            {
                // Use the carType, fuelType, and milesTravelled variables as needed
                string emissionFactor = GetEmissionFactor(carType, fuelType);
                string extractedEmissionFactor = ExtractEmissionFactorsValue(emissionFactor);

                double totalEmission = milesTravelled * Convert.ToDouble(extractedEmissionFactor);
                leisuretravel_car_emission_label.Text = $"Total Emission: {totalEmission:F6} kg CO2e";
                totalLeisureTravelCarEmission = $"Total Emission: {totalEmission:F6} kg CO2e";
                updateGlobalLabel(this, EventArgs.Empty);

                // Provide feedback based on average mileage
                string improvementTips = "";
                string youTubeLink = "";
                double averageMiles = 1053; // Average distance in miles per person per year for leisure purposes

                if (milesTravelled > averageMiles)
                {
                    feedback_Car_Leisure_label.Text = $"Your annual leisure travel of {milesTravelled} miles is higher than the expected average of {averageMiles} miles/year for leisure purposes.";
                    improvementTips = "Consider reducing car trips or using more fuel-efficient vehicles.";
                    youTubeLink = "https://www.youtube.com/watch?v=coecYbPfKuk&t=573s";
                }
                else
                {
                    feedback_Car_Leisure_label.Text = $"Your annual leisure travel of {milesTravelled} miles is within the expected average of {averageMiles} miles/year for leisure purposes.";
                    improvementTips = "Great job! Continue practicing efficient travel methods.";
                    youTubeLink = "No suggestions";
                }
                feedback_Car_Leisure_label.Visible = true;

                UpdateCarLeisureBadge(milesTravelled, averageMiles);
                // Append the report to the HomeEnergy category
                // Conditionally append the report data
                if (shouldAppend)
                {
                    AppendReport("Leisure", "Car", milesTravelled, averageMiles, feedback_Car_Leisure_label.Text, improvementTips, youTubeLink, "Miles/Year");
                }
            }
        }
        private void UpdateCarLeisureBadge(double userMileage, double averageMileage)
        {
            // Define arrays for the images
            Bitmap[] goodPerformanceImages = {
                Properties.Resources.crown1,
                Properties.Resources.crown2,
                Properties.Resources.trophy_star,
                Properties.Resources.award,
                Properties.Resources.trophy,
                Properties.Resources.ribbon
            };

            Bitmap[] improvementImages = {
                Properties.Resources.target,
                Properties.Resources.person,
                Properties.Resources.business,
                Properties.Resources.fail
            };

            // Define arrays for the phrases (shortened to two words)
            string[] goodPerformancePhrases = {
                "Eco Star",
                "Great Job",
                "Top Performer",
                "Keep Going",
                "Well Done"
            };

            string[] improvementPhrases = {
                "Try Harder",
                "Improve More",
                "Keep Going",
                "Almost There",
                "Step Up"
            };

            // Generate random indexes for each array separately
            int goodImageIndex = random.Next(goodPerformanceImages.Length);
            int improvementImageIndex = random.Next(improvementImages.Length);

            // Generate random indexes for each phrase array separately
            int goodPhraseIndex = random.Next(goodPerformancePhrases.Length);
            int improvementPhraseIndex = random.Next(improvementPhrases.Length);

            if (userMileage < averageMileage)
            {
                // Show the "Eco Warrior" badge
                Award_Car_Leisure_picturebox.Image = goodPerformanceImages[goodImageIndex];
                Award_Car_Leisure_label.Text = goodPerformancePhrases[goodPhraseIndex];
            }
            else
            {
                // Show the "You Can Do Better" feedback
                Award_Car_Leisure_picturebox.Image = improvementImages[improvementImageIndex];
                Award_Car_Leisure_label.Text = improvementPhrases[improvementPhraseIndex];
            }

            // Set the PictureBox's SizeMode to StretchImage to ensure the image covers the entire PictureBox
            Award_Car_Leisure_picturebox.SizeMode = PictureBoxSizeMode.StretchImage;

            // Make sure the PictureBox and Label are visible
            Award_Car_Leisure_picturebox.Visible = true;
            Award_Car_Leisure_label.Visible = true;
        }
        private void HelpClickMe_CarLeisureTravel_button_Click(object sender, EventArgs e)
        {
            // Show detailed help message for Leisure Travel by Car
            MessageBox.Show(
                "Annual Leisure Travel Data (Car):\n\n" +
                "1. **Distance Travelled (Miles):**\n" +
                "   - Enter the total distance you travel by car annually for leisure purposes.\n" +
                "   - Example: 1053 miles is a typical value.\n" +
                "   - Valid range: 100 miles to 5000 miles.\n\n" +
                "Note: The average annual distance travelled by car for leisure purposes in the UK is approximately 1,053 miles per person, according to Statista. For more details, refer to the source: https://www.statista.com/statistics/467325/average-distance-travelled-for-leisure-purposes-by-mode-england/.\n\n" +
                "Accurate data entry will help calculate your annual carbon emissions related to leisure travel.",
                "Help Information - Leisure Travel (Car)",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }


        //Leisure Bike Emission:
        private string LeisureTravelGetBikeType()
        {
            if (LeisureTravel_BikeType_Small_RadioButton.Checked)
            {
                return "small";
            }
            else if (LeisureTravel_BikeType_Medium_RadioButton.Checked)
            {
                return "medium";
            }
            else if (LeisureTravel_BikeType_Large_RadioButton.Checked)
            {
                return "large";
            }
            else
            {
                return "unknown";
            }
        }
        private void UpdateBikeLeisureBadge(double userMileage, double averageMileage)
        {
            // Define arrays for the images
            Bitmap[] goodPerformanceImages = {
                Properties.Resources.crown1,
                Properties.Resources.crown2,
                Properties.Resources.trophy_star,
                Properties.Resources.award,
                Properties.Resources.trophy,
                Properties.Resources.ribbon
            };

            Bitmap[] improvementImages = {
                Properties.Resources.target,
                Properties.Resources.person,
                Properties.Resources.business,
                Properties.Resources.fail
            };

            // Define arrays for the phrases (shortened to two words)
            string[] goodPerformancePhrases = {
                "Eco Star",
                "Great Job",
                "Top Performer",
                "Keep Going",
                "Well Done"
            };

            string[] improvementPhrases = {
                "Try Harder",
                "Improve More",
                "Keep Going",
                "Almost There",
                "Step Up"
            };

            // Generate random indexes for each array separately
            int goodImageIndex = random.Next(goodPerformanceImages.Length);
            int improvementImageIndex = random.Next(improvementImages.Length);

            // Generate random indexes for each phrase array separately
            int goodPhraseIndex = random.Next(goodPerformancePhrases.Length);
            int improvementPhraseIndex = random.Next(improvementPhrases.Length);

            if (userMileage < averageMileage)
            {
                // Show the "Eco Warrior" badge
                Award_Bike_Leisure_picturebox.Image = goodPerformanceImages[goodImageIndex];
                Award_Bike_Leisure_label.Text = goodPerformancePhrases[goodPhraseIndex];
            }
            else
            {
                // Show the "You Can Do Better" feedback
                Award_Bike_Leisure_picturebox.Image = improvementImages[improvementImageIndex];
                Award_Bike_Leisure_label.Text = improvementPhrases[improvementPhraseIndex];
            }

            // Set the PictureBox's SizeMode to StretchImage to ensure the image covers the entire PictureBox
            Award_Bike_Leisure_picturebox.SizeMode = PictureBoxSizeMode.StretchImage;

            // Make sure the PictureBox and Label are visible
            Award_Bike_Leisure_picturebox.Visible = true;
            Award_Bike_Leisure_label.Visible = true;
        }
        private void BikeLeisureTravel_CalculateBikeCarbon(object sender, EventArgs e)
        {
            double milesTravelled = 0;

            // Validate inputs
            bool isValid = true;

            // Default feedback and UI reset
            leisuretravel_bike_emission_label.Text = "Emission"; // Assign default value
            feedback_Bike_Leisure_label.Text = "Feedback"; // Assign default value

            // Clear the picturebox and label
            Award_Bike_Leisure_picturebox.Image = null;
            Award_Bike_Leisure_picturebox.Visible = false; // Hide the picturebox

            Award_Bike_Leisure_label.Text = string.Empty;
            Award_Bike_Leisure_label.Visible = false; // Hide the label
            feedback_Bike_Leisure_label.Text = string.Empty;
            feedback_Bike_Leisure_label.Visible = false; // Assign default value

            // Validate Miles Travelled
            if (string.IsNullOrWhiteSpace(MilesTravelled_Bike_LeisureTravel_Textbox.Text))
            {
                if (isBikeLeisureMilesErrorSet)
                {
                    Bike_LeisureTravel_errorProvider2.SetError(MilesTravelled_Bike_LeisureTravel_Textbox, string.Empty);
                    isBikeLeisureMilesErrorSet = false;
                }
                totalLeisureTravelBikeEmission = "";
                feedback_Bike_Leisure_label.Text = "Feedback"; // Assign default value

                updateGlobalLabel(this, EventArgs.Empty);
                return;
            }
            else if (!double.TryParse(MilesTravelled_Bike_LeisureTravel_Textbox.Text, out milesTravelled) || milesTravelled < 100 || milesTravelled > 5000)
            {
                isValid = false;
                if (!isBikeLeisureMilesErrorSet)
                {
                    Bike_LeisureTravel_errorProvider2.SetError(MilesTravelled_Bike_LeisureTravel_Textbox, "Enter a value between 100 miles and 5000 miles. Example: 1053 miles (average). Click for Help..");
                    isBikeLeisureMilesErrorSet = true;
                }
                totalLeisureTravelBikeEmission = "";
                feedback_Bike_Leisure_label.Text = "Feedback"; // Assign default value

                updateGlobalLabel(this, EventArgs.Empty);
                return;
            }
            else
            {
                if (isBikeLeisureMilesErrorSet)
                {
                    Bike_LeisureTravel_errorProvider2.SetError(MilesTravelled_Bike_LeisureTravel_Textbox, string.Empty);
                    isBikeLeisureMilesErrorSet = false;
                }
            }

            // Validate Car Type and Fuel Type
            string bikeType = LeisureTravelGetBikeType();

            if (bikeType == "unknown")
            {
                isValid = false;
                //MessageBox.Show("Please select a valid car type and fuel type.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                totalLeisureTravelBikeEmission = "";
                feedback_Bike_Leisure_label.Text = "Feedback"; // Assign default value

                updateGlobalLabel(this, EventArgs.Empty);
                return;
            }

            // If validation fails, return
            if (!isValid)
            {
                totalLeisureTravelBikeEmission = "";
                feedback_Bike_Leisure_label.Text = "Feedback"; // Assign default value

                updateGlobalLabel(this, EventArgs.Empty);
                return;
            }

            // Perform the calculation only if all inputs are valid
            if (!string.IsNullOrWhiteSpace(MilesTravelled_Bike_LeisureTravel_Textbox.Text) &&
                bikeType != "unknown")
            {
                // Use the carType, fuelType, and milesTravelled variables as needed
                string emissionFactor = GetEmissionFactorBike(bikeType);
                string extractedEmissionFactor = ExtractEmissionFactorsValue(emissionFactor);

                double totalEmission = milesTravelled * Convert.ToDouble(extractedEmissionFactor);
                leisuretravel_bike_emission_label.Text = $"Total Emission: {totalEmission:F6} kg CO2e";
                totalLeisureTravelBikeEmission = $"Total Emission: {totalEmission:F6} kg CO2e";
                updateGlobalLabel(this, EventArgs.Empty);

                // Provide feedback based on average mileage
                string improvementTips = "";
                string youTubeLink = "";
                double averageMiles = 1053; // Average distance in miles per person per year for leisure purposes

                if (milesTravelled > averageMiles)
                {
                    feedback_Bike_Leisure_label.Text = $"Your annual leisure travel of {milesTravelled} miles is higher than the expected average of {averageMiles} miles/year for leisure purposes.";
                    improvementTips = "Consider reducing car trips or using more fuel-efficient vehicles.";
                    youTubeLink = "https://www.youtube.com/watch?v=coecYbPfKuk&t=573s";
                }
                else
                {
                    feedback_Bike_Leisure_label.Text = $"Your annual leisure travel of {milesTravelled} miles is within the expected average of {averageMiles} miles/year for leisure purposes.";
                    improvementTips = "Great job! Continue practicing efficient travel methods.";
                    youTubeLink = "No suggestions";
                }
                feedback_Bike_Leisure_label.Visible = true;

                UpdateBikeLeisureBadge(milesTravelled, averageMiles);
                // Append the report to the HomeEnergy category
                // Conditionally append the report data
                if (shouldAppend)
                {
                    AppendReport("Leisure", "Bike", milesTravelled, averageMiles, feedback_Bike_Leisure_label.Text, improvementTips, youTubeLink, "Miles/Year");
                }
            }
        }
        private void LeisureTravel_CalculateMotorHotelCarbon(object sender, EventArgs e)
        {

        }
        private void HelpClickMe_LeisureTravel_Bike_button_Click(object sender, EventArgs e)
        {
            // Show detailed help message for Leisure Travel by Motorbike
            MessageBox.Show(
                "Annual Leisure Travel Data (Motorbike):\n\n" +
                "1. **Distance Travelled (Miles):**\n" +
                "   - Enter the total distance you travel by motorbike annually for leisure purposes.\n" +
                "   - Example: 4,000 miles is a typical value.\n" +
                "   - Valid range: 500 miles to 10,000 miles.\n\n" +
                "Note: The average annual distance traveled by motorbike for leisure purposes in the UK is assumed to be similar to car travel, around 4,000 miles per person. This assumption is based on similar patterns of leisure travel as observed for cars.\n\n" +
                "Accurate data entry will help calculate your annual carbon emissions related to motorbike leisure travel.",
                "Help Information - Leisure Travel (Motorbike)",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }


        // Leisure hotel room carbon emission calculation
        private void LeisureTravel_CalculateHotelRoomCarbon(object sender, EventArgs e)
        {
            double totalNights = 0;

            // Validate inputs
            bool isValid = true;

            // Validate Number of Nights
            if (string.IsNullOrWhiteSpace(LeisureTravel_HotelStay_Textbox.Text))
            {
                leisuretravel_HotelStay_emission_label.Text = "Emission"; // Assign default value
                                                                          // Clear the picturebox and label
                Award_HotelStay_Leisure_picturebox.Image = null;
                Award_HotelStay_Leisure_picturebox.Visible = false; // Hide the picturebox

                feedback_HotelStay_Leisure_label.Text = string.Empty;
                feedback_HotelStay_Leisure_label.Visible = false; // Hide the label

                totalHotelStayEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                if (isHotelStayErrorSet)
                {
                    hotelStay_LeisureTravel_errorProvider.SetError(LeisureTravel_HotelStay_Textbox, string.Empty);
                    isHotelStayErrorSet = false;
                }
                return;
            }
            else if (!double.TryParse(LeisureTravel_HotelStay_Textbox.Text, out totalNights) || totalNights < 1 || totalNights > 30)
            {
                isValid = false;
                if (!isHotelStayErrorSet)
                {
                    hotelStay_LeisureTravel_errorProvider.SetError(LeisureTravel_HotelStay_Textbox, $"Enter a value between 1 and 30 nights. The average hotel stay for a single leisure trip is around 6.5 nights. Click for Help.");
                    isHotelStayErrorSet = true;
                }
                leisuretravel_HotelStay_emission_label.Text = "Emission"; // Assign default value

                // Clear the picturebox and label
                Award_HotelStay_Leisure_picturebox.Image = null;
                Award_HotelStay_Leisure_picturebox.Visible = false; // Hide the picturebox

                feedback_HotelStay_Leisure_label.Text = string.Empty;
                feedback_HotelStay_Leisure_label.Visible = false; // Hide the label

                totalHotelStayEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);
                return; // Exit the method if the input is invalid
            }
            else
            {
                if (isHotelStayErrorSet)
                {
                    hotelStay_LeisureTravel_errorProvider.SetError(LeisureTravel_HotelStay_Textbox, string.Empty);
                    isHotelStayErrorSet = false;
                }
            }

            // If validation fails, return
            if (!isValid)
            {
                leisuretravel_HotelStay_emission_label.Text = "Emission"; // Assign default value
                                                                          // Clear the picturebox and label
                Award_HotelStay_Leisure_picturebox.Image = null;
                Award_HotelStay_Leisure_picturebox.Visible = false; // Hide the picturebox

                feedback_HotelStay_Leisure_label.Text = string.Empty;
                feedback_HotelStay_Leisure_label.Visible = false; // Hide the label

                totalHotelStayEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);
                return;
            }

            // Perform the calculation only if all inputs are valid
            if (!string.IsNullOrWhiteSpace(LeisureTravel_HotelStay_Textbox.Text))
            {
                totalHotelStayEmission = CalculateTotalCarbonEmissionHotel(totalNights);

                leisuretravel_HotelStay_emission_label.Text = $"Emission: {ExtractEmissionValue(totalHotelStayEmission):F6} kg CO2e";
                updateGlobalLabel(this, EventArgs.Empty);

                // Provide feedback based on average usage or thresholds
                string improvementTips = "";
                string youTubeLink = "";
                double averageNights = 7; // Average number of nights per stay

                if (totalNights > averageNights)
                {
                    leisuretravel_HotelStay_emission_label.Text = $"Your stay of {totalNights} nights exceeds the average of {averageNights} nights per visit.";
                    improvementTips = "Consider planning shorter trips or combining activities to reduce the number of nights you spend in hotels.";
                    youTubeLink = "https://www.youtube.com/watch?v=z4lCMXVfEL8";
                }
                else
                {
                    leisuretravel_HotelStay_emission_label.Text = $"Your stay of {totalNights} nights is within the average range of {averageNights} nights per visit.";
                    improvementTips = "Great job on managing your hotel stays efficiently! Keep sharing your sustainable travel habits.";
                    youTubeLink = "No suggestions";
                }


                UpdateHotelStayBadge(totalNights, averageNights); // Update UI with badges or rewards based on user input
                // Example of using these variables further, like adding to a report or displaying elsewhere
                if (shouldAppend)
                {
                    AppendReport("Leisure", "HotelStay", totalNights, averageNights, feedback_Bike_Leisure_label.Text, improvementTips, youTubeLink, "Nights");
                }
            }
        }
        private void UpdateHotelStayBadge(double userNights, double averageNights)
        {
            // Define arrays for the images
            Bitmap[] goodPerformanceImages = {
                Properties.Resources.crown1,
                Properties.Resources.crown2,
                Properties.Resources.trophy_star,
                Properties.Resources.award,
                Properties.Resources.trophy,
                Properties.Resources.ribbon
            };

            Bitmap[] improvementImages = {
                Properties.Resources.target,
                Properties.Resources.person,
                Properties.Resources.business,
                Properties.Resources.fail
            };

            // Define arrays for the phrases (shortened to two words)
            string[] goodPerformancePhrases = {
                "Eco Star",
                "Great Job",
                "Top Performer",
                "Keep Going",
                "Well Done"
            };

            string[] improvementPhrases = {
                "Try Harder",
                "Improve More",
                "Keep Going",
                "Almost There",
                "Step Up"
            };

            // Generate random indexes for each array separately
            int goodImageIndex = random.Next(goodPerformanceImages.Length);
            int improvementImageIndex = random.Next(improvementImages.Length);

            int goodPhraseIndex = random.Next(goodPerformancePhrases.Length);
            int improvementPhraseIndex = random.Next(improvementPhrases.Length);

            if (userNights <= averageNights)
            {
                // Show the "Eco Warrior" badge
                Award_HotelStay_Leisure_picturebox.Image = goodPerformanceImages[goodImageIndex];
                feedback_HotelStay_Leisure_label.Text = goodPerformancePhrases[goodPhraseIndex];
            }
            else
            {
                // Show the "You Can Do Better" feedback
                Award_HotelStay_Leisure_picturebox.Image = improvementImages[improvementImageIndex];
                feedback_HotelStay_Leisure_label.Text = improvementPhrases[improvementPhraseIndex];
            }

            // Set the PictureBox's SizeMode to StretchImage to ensure the image covers the entire PictureBox
            Award_HotelStay_Leisure_picturebox.SizeMode = PictureBoxSizeMode.StretchImage;

            // Make sure the PictureBox and Label are visible
            Award_HotelStay_Leisure_picturebox.Visible = true;
            feedback_HotelStay_Leisure_label.Visible = true;
        }
        private string CalculateTotalCarbonEmissionHotel(double totalnights)
        {
            double ukRoomPerNightEmissionFactor = 0;
            string connectionString = $"Data Source={dbPath};Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                //string query = "SELECT * FROM conversion_factor WHERE Unit = @Unit";
                string query = "SELECT* FROM conversion_factor WHERE Activity = @Activity AND Type = @Type AND Year = @Year AND Unit = @Unit";
                //string query = input;
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Activity", "Hotel stay");
                    command.Parameters.AddWithValue("@Type", "NA");
                    command.Parameters.AddWithValue("@Unit", "Room per night");
                    command.Parameters.AddWithValue("@Year", selectedYear);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Carbon emission factors per kWh for electricity generation in the UK
                            ukRoomPerNightEmissionFactor = reader.GetDouble(reader.GetOrdinal("kg CO2e"));
                        }
                    }
                }
            }
            // Emission factor per room per night in the UK
            //double ukRoomPerNightEmissionFactor = 10.4; // kg CO2e per room per night

            // Calculate total carbon emissions from generation
            double totalGenerationEmission = totalnights * ukRoomPerNightEmissionFactor;

            // Output or use these values as needed
            Debug.WriteLine($"Total Carbon Emission for Hotel Stay: {totalGenerationEmission} kg CO2e");

            // Optionally update UI or store these values
            // resultLabel.Text = $"Total Carbon Emission: {overallTotalEmission} kg CO2e";
            //led_op_Total_KWh_label.Text = $"Total Emission: {overallTotalEmission} kg CO2e (CO2: {overallCO2Emission}, CH4: {overallCH4Emission}, N2O: {overallN2OEmission})";
            // Create the result string
            //string result = $"Total Emission: {overallTotalEmission} kg CO2e (CO2: {overallCO2Emission}, CH4: {overallCH4Emission}, N2O: {overallN2OEmission})";
            string result = $"Total Emission: {totalGenerationEmission:F6} kg CO2e (CO2: {0:F6}, CH4: {0:F6}, N2O: {0:F6})";

            // Output for debugging purposes
            Debug.WriteLine(result);

            // Return the result string
            return result;
        }
        private void HelpClickMe_HotelStay_button_Click(object sender, EventArgs e)
        {
            // Show detailed help message for hotel stay
            MessageBox.Show(
                "Annual Leisure Hotel Stay Data:\n\n" +
                "1. **Number of Nights Stayed:**\n" +
                "   - Enter the total number of nights stayed at the hotel for leisure purposes in a year.\n" +
                "   - Example: 7 nights per year.\n" +
                "   - Valid range: 1 to 30 nights per stay.\n" +
                "   - The average hotel stay in London is approximately 7 nights per visit, according to Statista.\n\n" +
                "2. **Realistic Values:**\n" +
                "   - Ensure that the entered value reflects a realistic estimate of your stays.\n\n" +
                "Note: This data will be used to calculate your annual carbon emissions related to leisure hotel stays, using UK-specific data. For more details, refer to the source: https://www.statista.com/statistics/487772/average-length-overseas-visits-by-purpose-london-uk/#:~:text=The%20average%20length%20of%20an,and%20relatives%2C%20averaging%20nine%20nights.",
                "Help Information - Leisure Hotel Stay",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }


        // Function to get emission factor based on car type and fuel type
        private string GetEmissionFactor(string carType, string fuelType)
        {
            double CarTotalFactor = 0;
            double CarCO2Factor = 0;
            double CarCH4Factor = 0;
            double CarN2OFactor = 0;

            string activityParam = "Cars";
            string typeParam = "";
            string fuelParam = "Petrol";
            string unitParam = "miles";

            if (carType == "small")
            {
                typeParam = "Small";
            }
            else if (carType == "medium")
            {
                typeParam = "Medium";
            }
            else if (carType == "large")
            {
                typeParam = "Large";
            }
            else
            {
                typeParam = "";
            }

            if (fuelType == "diesel")
            {
                fuelParam = "Diesel";
            }
            else if (fuelType == "petrol")
            {
                fuelParam = "Petrol";
            }
            else if (fuelType == "EV")
            {
                fuelParam = "EV";
            }
            else
            {
                fuelParam = "";
            }

            string connectionString = $"Data Source={dbPath};Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT* FROM conversion_factor WHERE Activity = @Activity AND Type = @Type AND Fuel = @Fuel AND Year = @Year AND Unit = @Unit";
                //string query = input;
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Activity", activityParam);
                    command.Parameters.AddWithValue("@Type", typeParam);
                    command.Parameters.AddWithValue("@Unit", unitParam);
                    command.Parameters.AddWithValue("@Year", selectedYear);
                    command.Parameters.AddWithValue("@Fuel", fuelParam);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Carbon emission factors per kWh for electricity generation in the UK
                            CarTotalFactor = reader.GetDouble(reader.GetOrdinal("kg CO2e"));
                            CarCO2Factor = reader.GetDouble(reader.GetOrdinal("kg CO2e of CO2 per unit"));
                            CarCH4Factor = reader.GetDouble(reader.GetOrdinal("kg CO2e of CH4 per unit"));
                            CarN2OFactor = reader.GetDouble(reader.GetOrdinal("kg CO2e of N2O per unit"));
                        }
                    }
                }
            }
            string emission_factors = $"Emission Factors: {CarTotalFactor:F6} kg CO2e (CO2: {CarCO2Factor:F6}, CH4: {CarCH4Factor:F6}, N2O: {CarN2OFactor:F6})";
            return emission_factors; // Small car, petrol, miles

        }
        private string GetEmissionFactorBike(string bikeType)
        {
            double BikeTotalFactor = 0;
            double BikeCO2Factor = 0;
            double BikeCH4Factor = 0;
            double BikeN2OFactor = 0;

            string activityParam = "Motorbike";
            string typeParam = "";
            string fuelParam = "Petrol";
            string unitParam = "miles";

            if (bikeType == "small")
            {
                typeParam = "Small";
            }
            else if (bikeType == "medium")
            {
                typeParam = "Medium";
            }
            else if (bikeType == "large")
            {
                typeParam = "Large";
            }


            string connectionString = $"Data Source={dbPath};Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                //string query = "SELECT * FROM conversion_factor WHERE Unit = @Unit";
                string query = "SELECT* FROM conversion_factor WHERE Activity = @Activity AND Type = @Type AND Fuel = @Fuel AND Year = @Year AND Unit = @Unit";
                //string query = input;
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Activity", activityParam);
                    command.Parameters.AddWithValue("@Type", typeParam);
                    command.Parameters.AddWithValue("@Unit", unitParam);
                    command.Parameters.AddWithValue("@Year", selectedYear);
                    command.Parameters.AddWithValue("@Fuel", fuelParam);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Carbon emission factors per kWh for electricity generation in the UK
                            BikeTotalFactor = reader.GetDouble(reader.GetOrdinal("kg CO2e"));
                            BikeCO2Factor = reader.GetDouble(reader.GetOrdinal("kg CO2e of CO2 per unit"));
                            BikeCH4Factor = reader.GetDouble(reader.GetOrdinal("kg CO2e of CH4 per unit"));
                            BikeN2OFactor = reader.GetDouble(reader.GetOrdinal("kg CO2e of N2O per unit"));
                        }
                    }
                }
            }
            string emission_factors = $"Emission Factors: {BikeTotalFactor:F6} kg CO2e (CO2: {BikeCO2Factor:F6}, CH4: {BikeCH4Factor:F6}, N2O: {BikeN2OFactor:F6})";
            return emission_factors; // Small car, petrol, miles
        }
        private string GetEmissionFactorTrain()
        {
            // Emission factors for national rail per passenger.km
            double nationalRailCO2Factor = 0; // kg CO2e of CO2 per unit
            double nationalRailCH4Factor = 0; // kg CO2e of CH4 per unit
            double nationalRailN2OFactor = 0; // kg CO2e of N2O per unit
            double nationalRailTotalFactor = 0; // kg CO2e per passenger.km

            string connectionString = $"Data Source={dbPath};Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                //string query = "SELECT * FROM conversion_factor WHERE Unit = @Unit";
                string query = "SELECT* FROM conversion_factor WHERE Activity = @Activity AND Type = @Type AND Year = @Year AND Unit = @Unit";
                //string query = input;
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Activity", "Rail");
                    command.Parameters.AddWithValue("@Type", "National rail");
                    command.Parameters.AddWithValue("@Unit", "passenger.km");
                    command.Parameters.AddWithValue("@Year", selectedYear);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Emission factors for national rail per passenger.km
                            nationalRailTotalFactor = reader.GetDouble(reader.GetOrdinal("kg CO2e")); // total kg CO2e per mile
                            nationalRailCO2Factor = reader.GetDouble(reader.GetOrdinal("kg CO2e of CO2 per unit")); // kg CO2e of CO2 per mile
                            nationalRailCH4Factor = reader.GetDouble(reader.GetOrdinal("kg CO2e of CH4 per unit")); // kg CO2e of CH4 per mile
                            nationalRailN2OFactor = reader.GetDouble(reader.GetOrdinal("kg CO2e of N2O per unit")); // kg CO2e of N2O per mile
                        }
                    }
                }
            }
            string emission_factors = $"Emission Factors: {nationalRailTotalFactor:F6} kg CO2e (CO2: {nationalRailCO2Factor:F6}, CH4: {nationalRailCH4Factor:F6}, N2O: {nationalRailN2OFactor:F6})";
            return emission_factors; // Small car, petrol, miles
        }
        private string GetEmissionFactorBus()
        {
            // Emission factors for average local bus per passenger.km
            double localBusTotalFactor = 0; // kg CO2e per passenger.km
            double localBusCO2Factor = 0; // kg CO2e of CO2 per unit
            double localBusCH4Factor = 0; // kg CO2e of CH4 per unit
            double localBusN2OFactor = 0; // kg CO2e of N2O per unit

            string connectionString = $"Data Source={dbPath};Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                //string query = "SELECT * FROM conversion_factor WHERE Unit = @Unit";
                string query = "SELECT* FROM conversion_factor WHERE Activity = @Activity AND Type = @Type AND Year = @Year AND Unit = @Unit";
                //string query = input;
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Activity", "Bus");
                    command.Parameters.AddWithValue("@Type", "Local bus (not London)");
                    command.Parameters.AddWithValue("@Unit", "passenger.km");
                    command.Parameters.AddWithValue("@Year", selectedYear);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Emission factors for national rail per passenger.km
                            localBusTotalFactor = reader.GetDouble(reader.GetOrdinal("kg CO2e")); // total kg CO2e per mile
                            localBusCO2Factor = reader.GetDouble(reader.GetOrdinal("kg CO2e of CO2 per unit")); // kg CO2e of CO2 per mile
                            localBusCH4Factor = reader.GetDouble(reader.GetOrdinal("kg CO2e of CH4 per unit")); // kg CO2e of CH4 per mile
                            localBusN2OFactor = reader.GetDouble(reader.GetOrdinal("kg CO2e of N2O per unit")); // kg CO2e of N2O per mile
                        }
                    }
                }
            }
            string emission_factors = $"Emission Factors: {localBusTotalFactor:F6} kg CO2e (CO2: {localBusCO2Factor:F6}, CH4: {localBusCH4Factor:F6}, N2O: {localBusN2OFactor:F6})";
            return emission_factors; // Small car, petrol, miles
        }

        //Organic food and drink waste
        private void OrganicFoodWaste_CalculateCarbon(object sender, EventArgs e)
        {
            double wasteConsumptionInKgsPerPerson = 0;
            double numPersons = 0;

            // Validate inputs
            bool isValid = true;

            // Validate Waste Consumption per Person
            if (string.IsNullOrWhiteSpace(OrganicFoodWaste_InKgs_textbox.Text))
            {
                OrganicFoodWaste_Emission_label.Text = "Emission"; // Assign default value
                Feedback_OrganicFoodWaste_label.Text = "Feedback";
                OrganicFoodWaste_TotalWaste_label.Text = "TotalWaste";

                // Clear the picturebox and label
                Award_OrganicFoodWaste_picturebox.Image = null;
                Award_OrganicFoodWaste_picturebox.Visible = false; // Hide the picturebox
                Award_OrganicFoodWaste_label.Text = string.Empty;
                Award_OrganicFoodWaste_label.Visible = false; // Hide the label

                totalOrganicFoodWasteEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                if (isWasteConsumptionErrorSet)
                {
                    organicFoodWaste_errorProvider.SetError(OrganicFoodWaste_InKgs_textbox, string.Empty);
                    isWasteConsumptionErrorSet = false;
                }
            }
            else if (!double.TryParse(OrganicFoodWaste_InKgs_textbox.Text, out wasteConsumptionInKgsPerPerson) || wasteConsumptionInKgsPerPerson <= 0 || wasteConsumptionInKgsPerPerson > 200)
            {
                isValid = false;
                if (!isWasteConsumptionErrorSet)
                {
                    organicFoodWaste_errorProvider.SetError(OrganicFoodWaste_InKgs_textbox, "Enter the amount of organic food waste generated per person annually. Valid range: 1 kg to 200 kg. Example: 95 kg. Click for Help.");
                    isWasteConsumptionErrorSet = true;
                }
                OrganicFoodWaste_Emission_label.Text = "Emission"; // Assign default value
                Feedback_OrganicFoodWaste_label.Text = "Feedback";
                OrganicFoodWaste_TotalWaste_label.Text = "TotalWaste";

                // Clear the picturebox and label
                Award_OrganicFoodWaste_picturebox.Image = null;
                Award_OrganicFoodWaste_picturebox.Visible = false; // Hide the picturebox
                Award_OrganicFoodWaste_label.Text = string.Empty;
                Award_OrganicFoodWaste_label.Visible = false; // Hide the label

                totalOrganicFoodWasteEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);
            }
            else
            {
                if (isWasteConsumptionErrorSet)
                {
                    organicFoodWaste_errorProvider.SetError(OrganicFoodWaste_InKgs_textbox, string.Empty);
                    isWasteConsumptionErrorSet = false;
                }
            }

            // Validate Number of Persons
            if (string.IsNullOrWhiteSpace(OrganicFoodWaste_NumberOfPerson_textbox.Text))
            {
                OrganicFoodWaste_Emission_label.Text = "Emission"; // Assign default value
                Feedback_OrganicFoodWaste_label.Text = "Feedback";
                OrganicFoodWaste_TotalWaste_label.Text = "TotalWaste";

                // Clear the picturebox and label
                Award_OrganicFoodWaste_picturebox.Image = null;
                Award_OrganicFoodWaste_picturebox.Visible = false; // Hide the picturebox
                Award_OrganicFoodWaste_label.Text = string.Empty;
                Award_OrganicFoodWaste_label.Visible = false; // Hide the label

                totalOrganicFoodWasteEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                if (isNumberPersonWasteErrorSet)
                {
                    organicFoodWaste_errorProvider.SetError(OrganicFoodWaste_NumberOfPerson_textbox, string.Empty);
                    isNumberPersonWasteErrorSet = false;
                }
            }
            else if (!double.TryParse(OrganicFoodWaste_NumberOfPerson_textbox.Text, out numPersons) || numPersons <= 0 || numPersons > 6)
            {
                isValid = false;
                if (!isNumberPersonWasteErrorSet)
                {
                    organicFoodWaste_errorProvider.SetError(OrganicFoodWaste_NumberOfPerson_textbox, "Enter the number of people in your household. Valid range: 1 to 6 persons. Example: 4 persons. Click for Help.");
                    isNumberPersonWasteErrorSet = true;
                }
                OrganicFoodWaste_Emission_label.Text = "Emission"; // Assign default value
                Feedback_OrganicFoodWaste_label.Text = "Feedback";
                OrganicFoodWaste_TotalWaste_label.Text = "TotalWaste";

                // Clear the picturebox and label
                Award_OrganicFoodWaste_picturebox.Image = null;
                Award_OrganicFoodWaste_picturebox.Visible = false; // Hide the picturebox
                Award_OrganicFoodWaste_label.Text = string.Empty;
                Award_OrganicFoodWaste_label.Visible = false; // Hide the label

                totalOrganicFoodWasteEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);
            }
            else
            {
                if (isNumberPersonWasteErrorSet)
                {
                    organicFoodWaste_errorProvider.SetError(OrganicFoodWaste_NumberOfPerson_textbox, string.Empty);
                    isNumberPersonWasteErrorSet = false;
                }
            }

            // If validation fails, return
            if (!isValid)
            {
                OrganicFoodWaste_Emission_label.Text = "Emission"; // Assign default value
                Feedback_OrganicFoodWaste_label.Text = "Feedback";
                OrganicFoodWaste_TotalWaste_label.Text = "TotalWaste";

                // Clear the picturebox and label
                Award_OrganicFoodWaste_picturebox.Image = null;
                Award_OrganicFoodWaste_picturebox.Visible = false; // Hide the picturebox
                Award_OrganicFoodWaste_label.Text = string.Empty;
                Award_OrganicFoodWaste_label.Visible = false; // Hide the label

                totalOrganicFoodWasteEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                return;
            }

            // Perform the calculation only if all textboxes are non-empty
            if (!string.IsNullOrWhiteSpace(OrganicFoodWaste_InKgs_textbox.Text) &&
               !string.IsNullOrWhiteSpace(OrganicFoodWaste_NumberOfPerson_textbox.Text))
            {
                // Convert kg to tonne
                double wasteInTonnePerPerson = wasteConsumptionInKgsPerPerson / 1000;
                double totalWasteInTonne = wasteInTonnePerPerson * numPersons;

                // Calculate total carbon emission from organic food waste
                totalOrganicFoodWasteEmission = CalculateOrganicFoodWasteCarbonEmission(totalWasteInTonne);

                // Update labels
                OrganicFoodWaste_Emission_label.Text = $"Emission: {ExtractEmissionValue(totalOrganicFoodWasteEmission)} kg CO2e";
                updateGlobalLabel(this, EventArgs.Empty);

                // Provide feedback based on average waste usage
                double averageWastePerPerson = 95; // Example average waste in kg per person per year
                double totalWasteKg = wasteConsumptionInKgsPerPerson * numPersons; // User's input for total waste
                //Update totalWaste label
                OrganicFoodWaste_TotalWaste_label.Text = $"TotalWaste: {totalWasteKg} kg";

                // Calculate the average annual waste
                double averageAnnualWaste = averageWastePerPerson * numPersons;
                string improvementTips = "";
                string youTubeLink = "";

                if (totalWasteKg > averageAnnualWaste)
                {
                    Feedback_OrganicFoodWaste_label.Text = $"Your annual waste of {totalWasteKg:F2} kg for {numPersons} person(s) is higher than the expected average of {averageAnnualWaste:F2} kg for {numPersons} person(s).";
                    improvementTips = "Consider reducing waste by composting, better meal planning, and understanding portion sizes.";
                    youTubeLink = "https://www.youtube.com/watch?v=xyQ5ukvSRnA";
                }
                else
                {
                    Feedback_OrganicFoodWaste_label.Text = $"Your annual waste of {totalWasteKg:F2} kg for {numPersons} person(s) is within the expected average of {averageAnnualWaste:F2} kg for {numPersons} person(s).";
                    improvementTips = "Great job! Keep maintaining your low waste levels and consider sharing your practices with others.";
                    youTubeLink = "No suggestions";
                }

                // Update the picture box and label based on the user's performance
                UpdateOrganicFoodWasteBadge(totalWasteKg, averageAnnualWaste);
                // Append the report to the HomeEnergy category
                // Conditionally append the report data
                if (shouldAppend)
                {
                    AppendReport("Waste", "Food and Organic Waste", totalWasteKg, averageAnnualWaste, Feedback_OrganicFoodWaste_label.Text, improvementTips, youTubeLink, "Kg");
                }
            }
        }
        private string CalculateOrganicFoodWasteCarbonEmission(double totalWasteInTonne)
        {
            double scalingFactorOrganicFoodWaste = 0; // kg CO2e/tonne

            string connectionString = $"Data Source={dbPath};Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                //string query = "SELECT * FROM conversion_factor WHERE Unit = @Unit";
                string query = "SELECT* FROM conversion_factor WHERE Activity = @Activity AND Type = @Type AND Year = @Year AND Unit = @Unit";
                //string query = input;
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Activity", "Refuse");
                    command.Parameters.AddWithValue("@Type", "Organic: food and drink waste");
                    command.Parameters.AddWithValue("@Unit", "tonnes");
                    command.Parameters.AddWithValue("@Year", selectedYear);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Carbon emission factors per kWh for electricity generation in the UK
                            scalingFactorOrganicFoodWaste = reader.GetDouble(reader.GetOrdinal("Landfill"));
                        }
                    }
                }
            }
            // Assuming the emission factor for organic food waste is 700.20961 kg CO2e per tonne
            //double scalingFactorOrganicFoodWaste = 700.20961; // kg CO2e/tonne
            double totalEmission = totalWasteInTonne * scalingFactorOrganicFoodWaste;
            string result = $"Total Emission: {totalEmission:F6} kg CO2e";

            // Output for debugging purposes
            Debug.WriteLine(result);

            return result; // Format the emission value to 6 decimal places
        }
        private void UpdateOrganicFoodWasteBadge(double userWasteKg, double averageWasteKg)
        {
            // Define arrays for the images
            Bitmap[] goodPerformanceImages = {
                Properties.Resources.crown1,
                Properties.Resources.crown2,
                Properties.Resources.trophy_star,
                Properties.Resources.award,
                Properties.Resources.trophy,
                Properties.Resources.ribbon
            };

                    Bitmap[] improvementImages = {
                Properties.Resources.target,
                Properties.Resources.person,
                Properties.Resources.business,
                Properties.Resources.fail
            };

            // Define arrays for the phrases (shortened to two words)
            string[] goodPerformancePhrases = {
                "Eco Star",
                "Great Job",
                "Top Performer",
                "Keep Going",
                "Well Done"
            };

                    string[] improvementPhrases = {
                "Try Harder",
                "Improve More",
                "Keep Going",
                "Almost There",
                "Step Up"
            };

            // Generate random indexes for each array separately
            int goodImageIndex = random.Next(goodPerformanceImages.Length);
            int improvementImageIndex = random.Next(improvementImages.Length);

            int goodPhraseIndex = random.Next(goodPerformancePhrases.Length);
            int improvementPhraseIndex = random.Next(improvementPhrases.Length);

            if (userWasteKg <= averageWasteKg)
            {
                // Show the "Eco Warrior" badge
                Award_OrganicFoodWaste_picturebox.Image = goodPerformanceImages[goodImageIndex];
                Award_OrganicFoodWaste_label.Text = goodPerformancePhrases[goodPhraseIndex];
            }
            else
            {
                // Show the "You Can Do Better" feedback
                Award_OrganicFoodWaste_picturebox.Image = improvementImages[improvementImageIndex];
                Award_OrganicFoodWaste_label.Text = improvementPhrases[improvementPhraseIndex];
            }

            // Set the PictureBox's SizeMode to StretchImage to ensure the image covers the entire PictureBox
            Award_OrganicFoodWaste_picturebox.SizeMode = PictureBoxSizeMode.StretchImage;

            // Make sure the PictureBox and Label are visible
            Award_OrganicFoodWaste_picturebox.Visible = true;
            Award_OrganicFoodWaste_label.Visible = true;
        }
        private void HelpClickMe_OrganicFoodWaste_button_Click(object sender, EventArgs e)
        {
            // Show detailed help message for Organic Food Waste
            MessageBox.Show(
                "**Annual Organic Food Waste Data**\n\n" +
                "1. **Waste Consumption per Person (kg):**\n" +
                "   - Enter the amount of organic food waste generated **per person annually**.\n" +
                "   - **Example:** 95 kg per year (average).\n" +
                "   - **Valid Range:** 1 kg to 200 kg.\n\n" +
                "2. **Number of Persons in Household:**\n" +
                "   - Enter the total number of people in your household.\n" +
                "   - **Example:** 4 persons.\n" +
                "   - **Valid Range:** 1 to 6 persons.\n\n" +
                " **Note:**\n" +
                "The average annual food waste per person in the UK is approximately **95 kg**, according to WRAP.\n" +
                "For more details, refer to the source: [WRAP Report](https://www.wrap.ngo/sites/default/files/2024-05/WRAP-Household-Food-and-Drink-Waste-in-the-United-Kingdom-2021-22-v6.1.pdf).\n\n" +
                "Accurate data entry will help calculate your household's annual carbon emissions related to organic food waste.",
                "Help Information - Organic Food Waste",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        //Organic GardenWaste
        private void OrganicGardenWaste_CalculateCarbon(object sender, EventArgs e)
        {
            double wasteConsumptionInKgsPerPerson = 0;
            double numPersons = 0;

            // Validate inputs
            bool isValid = true;

            // Validate Waste Consumption per Person
            if (string.IsNullOrWhiteSpace(OrganicGardenWaste_InKgs_textbox.Text))
            {
                GardenWaste_Emission_label.Text = "Emission"; // Assign default value
                Feedback_Garden_Waste_label.Text = "Feedback";
                OrganicGardenWaste_TotalWaste_label.Text = "TotalWaste";

                // Clear the picturebox and label
                Award_OrganicGardenWaste_picturebox.Image = null;
                Award_OrganicGardenWaste_picturebox.Visible = false; // Hide the picturebox
                Award_OrganicGardenWaste_label.Text = string.Empty;
                Award_OrganicGardenWaste_label.Visible = false; // Hide the label

                totalOrganicGardenWasteEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                if (isWasteConsumptionErrorSet)
                {
                    organicGardenWaste_errorProvider.SetError(OrganicGardenWaste_InKgs_textbox, string.Empty);
                    isWasteConsumptionErrorSet = false;
                }
            }
            else if (!double.TryParse(OrganicGardenWaste_InKgs_textbox.Text, out wasteConsumptionInKgsPerPerson) || wasteConsumptionInKgsPerPerson <= 0 || wasteConsumptionInKgsPerPerson > 200)
            {
                isValid = false;
                if (!isWasteConsumptionErrorSet)
                {
                    organicGardenWaste_errorProvider.SetError(OrganicGardenWaste_InKgs_textbox, "Enter a valid value between 1 kg and 200 kg per person per year. Average: 120 kg. Click for Help.");
                    isWasteConsumptionErrorSet = true;
                }
                GardenWaste_Emission_label.Text = "Emission"; // Assign default value
                Feedback_Garden_Waste_label.Text = "Feedback";
                OrganicGardenWaste_TotalWaste_label.Text = "TotalWaste";

                // Clear the picturebox and label
                Award_OrganicGardenWaste_picturebox.Image = null;
                Award_OrganicGardenWaste_picturebox.Visible = false; // Hide the picturebox
                Award_OrganicGardenWaste_label.Text = string.Empty;
                Award_OrganicGardenWaste_label.Visible = false; // Hide the label

                totalOrganicGardenWasteEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);
            }
            else
            {
                if (isWasteConsumptionErrorSet)
                {
                    organicGardenWaste_errorProvider.SetError(OrganicGardenWaste_InKgs_textbox, string.Empty);
                    isWasteConsumptionErrorSet = false;
                }
            }

            // Validate Number of Persons
            if (string.IsNullOrWhiteSpace(OrganicGardenWaste_NumberOfPerson_textbox.Text))
            {
                GardenWaste_Emission_label.Text = "Emission"; // Assign default value
                Feedback_Garden_Waste_label.Text = "Feedback";
                OrganicGardenWaste_TotalWaste_label.Text = "TotalWaste";

                // Clear the picturebox and label
                Award_OrganicGardenWaste_picturebox.Image = null;
                Award_OrganicGardenWaste_picturebox.Visible = false; // Hide the picturebox
                Award_OrganicGardenWaste_label.Text = string.Empty;
                Award_OrganicGardenWaste_label.Visible = false; // Hide the label

                totalOrganicGardenWasteEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                if (isNumberPersonWasteErrorSet)
                {
                    organicGardenWaste_errorProvider.SetError(OrganicGardenWaste_NumberOfPerson_textbox, string.Empty);
                    isNumberPersonWasteErrorSet = false;
                }
            }
            else if (!double.TryParse(OrganicGardenWaste_NumberOfPerson_textbox.Text, out numPersons) || numPersons <= 0 || numPersons > 6)
            {
                isValid = false;
                if (!isNumberPersonWasteErrorSet)
                {
                    organicGardenWaste_errorProvider.SetError(OrganicGardenWaste_NumberOfPerson_textbox, "Enter the number of persons in the family. Valid range: 1 to 6 persons. Click for Help.");
                    isNumberPersonWasteErrorSet = true;
                }
                GardenWaste_Emission_label.Text = "Emission"; // Assign default value
                Feedback_Garden_Waste_label.Text = "Feedback";
                OrganicGardenWaste_TotalWaste_label.Text = "TotalWaste";

                // Clear the picturebox and label
                Award_OrganicGardenWaste_picturebox.Image = null;
                Award_OrganicGardenWaste_picturebox.Visible = false; // Hide the picturebox
                Award_OrganicGardenWaste_label.Text = string.Empty;
                Award_OrganicGardenWaste_label.Visible = false; // Hide the label

                totalOrganicGardenWasteEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);
            }
            else
            {
                if (isNumberPersonWasteErrorSet)
                {
                    organicGardenWaste_errorProvider.SetError(OrganicGardenWaste_NumberOfPerson_textbox, string.Empty);
                    isNumberPersonWasteErrorSet = false;
                }
            }

            // If validation fails, return
            if (!isValid)
            {
                GardenWaste_Emission_label.Text = "Emission"; // Assign default value
                Feedback_Garden_Waste_label.Text = "Feedback";
                OrganicGardenWaste_TotalWaste_label.Text = "TotalWaste";

                // Clear the picturebox and label
                Award_OrganicGardenWaste_picturebox.Image = null;
                Award_OrganicGardenWaste_picturebox.Visible = false; // Hide the picturebox
                Award_OrganicGardenWaste_label.Text = string.Empty;
                Award_OrganicGardenWaste_label.Visible = false; // Hide the label

                totalOrganicGardenWasteEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                return;
            }

            // Perform the calculation only if all textboxes are non-empty
            if (!string.IsNullOrWhiteSpace(OrganicGardenWaste_InKgs_textbox.Text) &&
               !string.IsNullOrWhiteSpace(OrganicGardenWaste_NumberOfPerson_textbox.Text))
            {
                // Convert kg to tonne
                double wasteInTonnePerPerson = wasteConsumptionInKgsPerPerson / 1000;
                double totalWasteInTonne = wasteInTonnePerPerson * numPersons;

                // Calculate total carbon emission from organic garden waste
                totalOrganicGardenWasteEmission = CalculateOrganicGardenWasteCarbonEmission(totalWasteInTonne);

                // Update labels
                GardenWaste_Emission_label.Text = $"Emission: {ExtractEmissionValue(totalOrganicGardenWasteEmission)} kg CO2e";
                updateGlobalLabel(this, EventArgs.Empty);

                // Provide feedback based on average waste usage
                double averageWastePerPerson = 120; // Example average waste in kg per person per year
                double totalWasteKg = wasteConsumptionInKgsPerPerson * numPersons; // User's input for total waste
                                                                                   //Update totalWaste label
                OrganicGardenWaste_TotalWaste_label.Text = $"TotalWaste: {totalWasteKg} kg";

                // Calculate the average annual waste
                double averageAnnualWaste = averageWastePerPerson * numPersons;

                string improvementTips = "";
                string youTubeLink = "";

                if (totalWasteKg > averageAnnualWaste)
                {
                    Feedback_Garden_Waste_label.Text = $"Feedback: Your annual waste of {totalWasteKg} kg for {numPersons} persons is higher than the expected average of {averageAnnualWaste} kg for {numPersons} persons.";
                    improvementTips = "Consider composting organic waste or reducing garden waste through better planning.";
                    youTubeLink = "https://www.youtube.com/watch?v=mcVQBtJyNIA";
                    Feedback_Garden_Waste_label.Visible = true;
                }
                else
                {
                    Feedback_Garden_Waste_label.Text = $"Feedback: Your annual waste of {totalWasteKg} kg for {numPersons} persons is within the expected average of {averageAnnualWaste} kg for {numPersons} persons.";
                    improvementTips = "Great job! Keep up the good work by continuing to manage your garden waste efficiently.";
                    youTubeLink = "No suggestions";
                    Feedback_Garden_Waste_label.Visible = true;
                }
                // Update the picture box and label based on the user's performance
                UpdateOrganicGardenWasteBadge(totalWasteKg, averageAnnualWaste);
                // Append the report to the HomeEnergy category
                // Conditionally append the report data
                if (shouldAppend)
                {
                    AppendReport("Waste", "Garden Waste", totalWasteKg, averageAnnualWaste, Feedback_Garden_Waste_label.Text, improvementTips, youTubeLink, "Kg");
                }
            }
        }
        private string CalculateOrganicGardenWasteCarbonEmission(double organicGardenwasteConinKgsPerPerson)
        {
            double scalingFactorOrganicGardenWasteLandfill = 0;
            string connectionString = $"Data Source={dbPath};Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                //string query = "SELECT * FROM conversion_factor WHERE Unit = @Unit";
                string query = "SELECT* FROM conversion_factor WHERE Activity = @Activity AND Type = @Type AND Year = @Year AND Unit = @Unit";
                //string query = input;
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Activity", "Refuse");
                    command.Parameters.AddWithValue("@Type", "Organic: garden waste");
                    command.Parameters.AddWithValue("@Unit", "tonnes");
                    command.Parameters.AddWithValue("@Year", selectedYear);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Carbon emission factors per kWh for electricity generation in the UK
                            scalingFactorOrganicGardenWasteLandfill = reader.GetDouble(reader.GetOrdinal("Landfill"));
                        }
                    }
                }
            }

            // Assuming the emission factor for water supply is 0.177 kg CO2e per cubic meter
            //double scalingFactorOrganicGardenWasteLandfill = 646.60632; // kg CO2e/tonne
            double totalEmission = organicGardenwasteConinKgsPerPerson * scalingFactorOrganicGardenWasteLandfill;
            string result = $"Total Emission: {totalEmission:F6} kg CO2e)";

            // Output for debugging purposes
            Debug.WriteLine(result);

            return result; // Format the emission value to 6 decimal places

        }
        private void UpdateOrganicGardenWasteBadge(double userWasteKg, double averageWasteKg)
        {
            // Define arrays for the images
            Bitmap[] goodPerformanceImages = {
        Properties.Resources.crown1,
        Properties.Resources.crown2,
        Properties.Resources.trophy_star,
        Properties.Resources.award,
        Properties.Resources.trophy,
        Properties.Resources.ribbon
    };

            Bitmap[] improvementImages = {
        Properties.Resources.target,
        Properties.Resources.person,
        Properties.Resources.business,
        Properties.Resources.fail
    };

            // Define arrays for the phrases (shortened to two words)
            string[] goodPerformancePhrases = {
        "Eco Star",
        "Great Job",
        "Top Performer",
        "Keep Going",
        "Well Done"
    };

            string[] improvementPhrases = {
        "Try Harder",
        "Improve More",
        "Keep Going",
        "Almost There",
        "Step Up"
    };

            // Generate random indexes for each array separately
            int goodImageIndex = random.Next(goodPerformanceImages.Length);
            int improvementImageIndex = random.Next(improvementImages.Length);

            int goodPhraseIndex = random.Next(goodPerformancePhrases.Length);
            int improvementPhraseIndex = random.Next(improvementPhrases.Length);

            if (userWasteKg <= averageWasteKg)
            {
                // Show the "Eco Star" badge
                Award_OrganicGardenWaste_picturebox.Image = goodPerformanceImages[goodImageIndex];
                Award_OrganicGardenWaste_label.Text = goodPerformancePhrases[goodPhraseIndex];
            }
            else
            {
                // Show the "You Can Do Better" feedback
                Award_OrganicGardenWaste_picturebox.Image = improvementImages[improvementImageIndex];
                Award_OrganicGardenWaste_label.Text = improvementPhrases[improvementPhraseIndex];
            }

            // Set the PictureBox's SizeMode to StretchImage to ensure the image covers the entire PictureBox
            Award_OrganicGardenWaste_picturebox.SizeMode = PictureBoxSizeMode.StretchImage;

            // Make sure the PictureBox and Label are visible
            Award_OrganicGardenWaste_picturebox.Visible = true;
            Award_OrganicGardenWaste_label.Visible = true;
        }
        private void HelpClickMe_OrganicGardenWaste_button_Click(object sender, EventArgs e)
        {
            // Show detailed help message for organic garden waste
            MessageBox.Show(
                "Annual Organic Garden Waste Data:\n\n" +
                "1. **Garden Waste Consumption (kg/person/year):**\n" +
                "   - Enter a valid garden waste consumption value in kilograms per person.\n" +
                "   - Example: 120 kg per year (average).\n" +
                "   - Valid range: 1 kg to 200 kg.\n\n" +
                "2. **Number of Persons:**\n" +
                "   - Enter the number of persons in the family.\n" +
                "   - Example: 4 persons.\n" +
                "   - Valid range: 1 to 6 persons.\n\n" +
                "Note: The average annual garden waste per person is approximately 120 kg, according to the study published in MDPI. For more details, refer to the source: https://www.mdpi.com/2079-9276/9/1/8#:~:text=Considering%20the%20average%20household%20size,person%E2%88%921%20year%E2%88%921.\n\n" +
                "Accurate data entry will help calculate your annual carbon emissions related to organic garden waste.",
                "Help Information - Organic Garden Waste",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }


        //Residual Waste
        private void HouseholdResidualWaste_CalculateCarbon(object sender, EventArgs e)
        {
            double wasteConsumptionInKgsPerPerson = 0;
            double numPersons = 0;

            // Validate inputs
            bool isValid = true;

            // Validate Waste Consumption per Person
            if (string.IsNullOrWhiteSpace(HouseResidualWaste_InKgs_textbox.Text))
            {
                HouseholdResidualWaste_Emission_label.Text = "Emission"; // Assign default value
                Feedback_Residul_Waste_label.Text = "Feedback";
                ResidualWaste_TotalWaste_label.Text = "TotalWaste";

                // Clear the picturebox and label
                Award_Residual_Waste_pictureBox.Image = null;
                Award_Residual_Waste_pictureBox.Visible = false; // Hide the picturebox
                Award_Residual_Waste_label.Text = string.Empty;
                Award_Residual_Waste_label.Visible = false; // Hide the label

                totalHouseholdResidualWasteEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                if (isWasteConsumptionErrorSet)
                {
                    ResidualWaste_errorProvider.SetError(HouseResidualWaste_InKgs_textbox, string.Empty);
                    isWasteConsumptionErrorSet = false;
                }
            }
            else if (!double.TryParse(HouseResidualWaste_InKgs_textbox.Text, out wasteConsumptionInKgsPerPerson) || wasteConsumptionInKgsPerPerson <= 0 || wasteConsumptionInKgsPerPerson > 1000)
            {
                isValid = false;
                if (!isWasteConsumptionErrorSet)
                {
                    ResidualWaste_errorProvider.SetError(HouseResidualWaste_InKgs_textbox,
                        "Enter a value between 1 kg and 1000 kg per person per year. Average value is 465 kg. Click for Help.");
                    isWasteConsumptionErrorSet = true;
                }
                HouseholdResidualWaste_Emission_label.Text = "Emission"; // Assign default value
                Feedback_Residul_Waste_label.Text = "Feedback";
                ResidualWaste_TotalWaste_label.Text = "TotalWaste";

                // Clear the picturebox and label
                Award_Residual_Waste_pictureBox.Image = null;
                Award_Residual_Waste_pictureBox.Visible = false; // Hide the picturebox
                Award_Residual_Waste_label.Text = string.Empty;
                Award_Residual_Waste_label.Visible = false; // Hide the label

                totalHouseholdResidualWasteEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);
            }
            else
            {
                if (isWasteConsumptionErrorSet)
                {
                    ResidualWaste_errorProvider.SetError(HouseResidualWaste_InKgs_textbox, string.Empty);
                    isWasteConsumptionErrorSet = false;
                }
            }

            // Validate Number of Persons
            if (string.IsNullOrWhiteSpace(HouseholdResidualWaste_NumberOfPerson_textbox.Text))
            {
                HouseholdResidualWaste_Emission_label.Text = "Emission"; // Assign default value
                Feedback_Residul_Waste_label.Text = "Feedback";
                ResidualWaste_TotalWaste_label.Text = "TotalWaste";

                // Clear the picturebox and label
                Award_Residual_Waste_pictureBox.Image = null;
                Award_Residual_Waste_pictureBox.Visible = false; // Hide the picturebox
                Award_Residual_Waste_label.Text = string.Empty;
                Award_Residual_Waste_label.Visible = false; // Hide the label

                totalHouseholdResidualWasteEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                if (isNumberPersonWasteErrorSet)
                {
                    ResidualWaste_errorProvider.SetError(HouseholdResidualWaste_NumberOfPerson_textbox, string.Empty);
                    isNumberPersonWasteErrorSet = false;
                }
            }
            else if (!double.TryParse(HouseholdResidualWaste_NumberOfPerson_textbox.Text, out numPersons) || numPersons <= 0 || numPersons > 6)
            {
                isValid = false;
                if (!isNumberPersonWasteErrorSet)
                {
                    ResidualWaste_errorProvider.SetError(HouseholdResidualWaste_NumberOfPerson_textbox, "Enter a number between 1 and 6. Click for Help.");
                    isNumberPersonWasteErrorSet = true;
                }
                HouseholdResidualWaste_Emission_label.Text = "Emission"; // Assign default value
                Feedback_Residul_Waste_label.Text = "Feedback";
                ResidualWaste_TotalWaste_label.Text = "TotalWaste";

                // Clear the picturebox and label
                Award_Residual_Waste_pictureBox.Image = null;
                Award_Residual_Waste_pictureBox.Visible = false; // Hide the picturebox
                Award_Residual_Waste_label.Text = string.Empty;
                Award_Residual_Waste_label.Visible = false; // Hide the label

                totalHouseholdResidualWasteEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);
            }
            else
            {
                if (isNumberPersonWasteErrorSet)
                {
                    ResidualWaste_errorProvider.SetError(HouseholdResidualWaste_NumberOfPerson_textbox, string.Empty);
                    isNumberPersonWasteErrorSet = false;
                }
            }

            // If validation fails, return
            if (!isValid)
            {
                HouseholdResidualWaste_Emission_label.Text = "Emission"; // Assign default value
                Feedback_Residul_Waste_label.Text = "Feedback";
                ResidualWaste_TotalWaste_label.Text = "TotalWaste";

                // Clear the picturebox and label
                Award_Residual_Waste_pictureBox.Image = null;
                Award_Residual_Waste_pictureBox.Visible = false; // Hide the picturebox
                Award_Residual_Waste_label.Text = string.Empty;
                Award_Residual_Waste_label.Visible = false; // Hide the label

                totalHouseholdResidualWasteEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                return;
            }

            // Perform the calculation only if all textboxes are non-empty
            if (!string.IsNullOrWhiteSpace(HouseResidualWaste_InKgs_textbox.Text) &&
               !string.IsNullOrWhiteSpace(HouseholdResidualWaste_NumberOfPerson_textbox.Text))
            {
                // Convert kg to tonne
                double wasteInTonnePerPerson = wasteConsumptionInKgsPerPerson / 1000;
                double totalWasteInTonne = wasteInTonnePerPerson * numPersons;

                // Calculate total carbon emission from household residual waste
                totalHouseholdResidualWasteEmission = CalculateHouseholdResidualWasteCarbonEmission(totalWasteInTonne);

                // Update labels
                HouseholdResidualWaste_Emission_label.Text = $"Emission: {ExtractEmissionValue(totalHouseholdResidualWasteEmission)} kg CO2e";
                updateGlobalLabel(this, EventArgs.Empty);

                // Provide feedback based on average waste usage
                double averageWastePerPerson = 465; // Example average waste in kg per person per year
                double totalWasteKg = wasteConsumptionInKgsPerPerson * numPersons; // User's input for total waste
                                                                                   //Update totalWaste label
                ResidualWaste_TotalWaste_label.Text = $"TotalWaste: {totalWasteKg} kg";

                // Calculate the average annual waste
                double averageAnnualWaste = averageWastePerPerson * numPersons;

                string improvementTips = "";
                string youTubeLink = "";
                if (totalWasteKg > averageAnnualWaste)
                {
                    Feedback_Residul_Waste_label.Text = $"Your annual residual waste of {totalWasteKg} kg for {numPersons} person(s) is higher than the expected average of {averageAnnualWaste} kg for {numPersons} person(s).";
                    improvementTips = "Consider reducing waste by recycling more.";
                    youTubeLink = "https://www.youtube.com/watch?v=Qyu-fZ8BOnI"; // Example link
                    Feedback_Residul_Waste_label.Visible = true;
                }
                else
                {
                    Feedback_Residul_Waste_label.Text = $"Your annual residual waste of {totalWasteKg} kg for {numPersons} person(s) is within the expected average of {averageAnnualWaste} kg for {numPersons} person(s).";
                    improvementTips = "Great job! Continue your waste management practices and consider sharing them with others.";
                    youTubeLink = "No suggestions";
                    Feedback_Residul_Waste_label.Visible = true;
                }
                // Update the picture box and label based on the user's performance
                UpdateHouseholdResidualWasteBadge(totalWasteKg, averageAnnualWaste);
                // Append the report to the HomeEnergy category
                // Conditionally append the report data
                if (shouldAppend)
                {
                    AppendReport("Waste", "Residual Waste", totalWasteKg, averageAnnualWaste, Feedback_Residul_Waste_label.Text, improvementTips, youTubeLink, "Kg");
                }
            }
        }
        private void HelpClickMe_HouseholdResidualWaste_button_Click(object sender, EventArgs e)
        {
            // Show detailed help message for household residual waste
            MessageBox.Show(
                "Annual Household Residual Waste Data:\n\n" +
                "1. **Residual Waste Consumption (kg):**\n" +
                "   - Enter a valid residual waste consumption value in kilograms per person.\n" +
                "   - Example: 465 kg per year (average).\n" +
                "   - Valid range: 1 kg to 1000 kg.\n\n" +
                "2. **Number of Persons:**\n" +
                "   - Enter the number of persons in the family.\n" +
                "   - Example: 4 persons.\n" +
                "   - Valid range: 1 to 6 persons.\n\n" +
                "Note: The average annual residual waste per person is around 465 kg, according to UK government statistics. For more details, refer to the source: [UK Government Residual Waste Statistics](https://www.gov.uk/government/statistics/estimates-of-residual-waste-excluding-major-mineral-wastes-and-municipal-residual-waste-in-england/estimates-of-residual-waste-excluding-major-mineral-wastes-and-municipal-residual-waste-in-england).\n\n" +
                "Accurate data entry will help calculate your annual carbon emissions related to household residual waste.",
                "Help Information - Household Residual Waste",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private string CalculateHouseholdResidualWasteCarbonEmission(double totalWasteInTonne)
        {
            double scalingFactorHouseholdResidualWasteLandfill = 0;
            string connectionString = $"Data Source={dbPath};Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT* FROM conversion_factor WHERE Activity = @Activity AND Type = @Type AND Year = @Year AND Unit = @Unit";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Activity", "Refuse");
                    command.Parameters.AddWithValue("@Type", "Household residual waste");
                    command.Parameters.AddWithValue("@Unit", "tonnes");
                    command.Parameters.AddWithValue("@Year", selectedYear);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            scalingFactorHouseholdResidualWasteLandfill = reader.GetDouble(reader.GetOrdinal("Landfill"));
                        }
                    }
                }
            }

            double totalEmission = totalWasteInTonne * scalingFactorHouseholdResidualWasteLandfill;
            string result = $"Total Emission: {totalEmission:F6} kg CO2e";

            // Output for debugging purposes
            Debug.WriteLine(result);

            return result; // Format the emission value to 6 decimal places
        }
        private void UpdateHouseholdResidualWasteBadge(double userWasteKg, double averageWasteKg)
        {
            // Define arrays for the images
            Bitmap[] goodPerformanceImages = {
                Properties.Resources.crown1,
                Properties.Resources.crown2,
                Properties.Resources.trophy_star,
                Properties.Resources.award,
                Properties.Resources.trophy,
                Properties.Resources.ribbon
            };

            Bitmap[] improvementImages = {
                Properties.Resources.target,
                Properties.Resources.person,
                Properties.Resources.business,
                Properties.Resources.fail
            };

            // Define arrays for the phrases (shortened to two words)
            string[] goodPerformancePhrases = {
                "Eco Star",
                "Great Job",
                "Top Performer",
                "Keep Going",
                "Well Done"
            };

            string[] improvementPhrases = {
                "Try Harder",
                "Improve More",
                "Keep Going",
                "Almost There",
                "Step Up"
            };

            // Generate random indexes for each array separately
            int goodImageIndex = random.Next(goodPerformanceImages.Length);
            int improvementImageIndex = random.Next(improvementImages.Length);

            int goodPhraseIndex = random.Next(goodPerformancePhrases.Length);
            int improvementPhraseIndex = random.Next(improvementPhrases.Length);

            if (userWasteKg <= averageWasteKg)
            {
                // Show the "Eco Warrior" badge
                Award_Residual_Waste_pictureBox.Image = goodPerformanceImages[goodImageIndex];
                Award_Residual_Waste_label.Text = goodPerformancePhrases[goodPhraseIndex];
            }
            else
            {
                // Show the "You Can Do Better" feedback
                Award_Residual_Waste_pictureBox.Image = improvementImages[improvementImageIndex];
                Award_Residual_Waste_label.Text = improvementPhrases[improvementPhraseIndex];
            }

            // Set the PictureBox's SizeMode to StretchImage to ensure the image covers the entire PictureBox
            Award_Residual_Waste_pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

            // Make sure the PictureBox and Label are visible
            Award_Residual_Waste_pictureBox.Visible = true;
            Award_Residual_Waste_label.Visible = true;
        }

        //Water supply carbon emission calculations
        private void HomeEnergy_CalculateWaterCarbon(object sender, EventArgs e)
        {
            double waterConsumptionLitersPerPerson = 0;
            double numPersons = 0;

            // Validate inputs
            bool isValid = true;

            // Validate Water Consumption per Person
            if (string.IsNullOrWhiteSpace(AvgLitersDaily_WaterSupply_HomeEnergy_textbox.Text))
            {
                EnergyUsage_WaterSupply_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_WaterSupply_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_WaterSupply_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_WaterSupply_HomeEnergy_picturebox.Image = null;
                Award_WaterSupply_HomeEnergy_picturebox.Visible = false; // Hide the picturebox
                Award_WaterSupply_HomeEnergy_label.Text = string.Empty;
                Award_WaterSupply_HomeEnergy_label.Visible = false; // Hide the label

                totalWaterEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                if (isWattWaterErrorSet)
                {
                    water_LeisureTravel_errorProvider.SetError(AvgLitersDaily_WaterSupply_HomeEnergy_textbox, string.Empty);
                    isWattWaterErrorSet = false;
                }
            }
            else if (!double.TryParse(AvgLitersDaily_WaterSupply_HomeEnergy_textbox.Text, out waterConsumptionLitersPerPerson) || waterConsumptionLitersPerPerson < 10 || waterConsumptionLitersPerPerson > 300)
            {
                isValid = false;
                if (!isWattWaterErrorSet)
                {
                    water_LeisureTravel_errorProvider.SetError(AvgLitersDaily_WaterSupply_HomeEnergy_textbox, "Please enter a valid water consumption value between 10 and 300 liters per person.");
                    isWattWaterErrorSet = true;
                }
                EnergyUsage_WaterSupply_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_WaterSupply_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_WaterSupply_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_WaterSupply_HomeEnergy_picturebox.Image = null;
                Award_WaterSupply_HomeEnergy_picturebox.Visible = false; // Hide the picturebox
                Award_WaterSupply_HomeEnergy_label.Text = string.Empty;
                Award_WaterSupply_HomeEnergy_label.Visible = false; // Hide the label

                totalWaterEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);
            }
            else
            {
                if (isWattWaterErrorSet)
                {
                    water_LeisureTravel_errorProvider.SetError(AvgLitersDaily_WaterSupply_HomeEnergy_textbox, string.Empty);
                    isWattWaterErrorSet = false;
                }
            }

            // Validate Number of Persons
            if (string.IsNullOrWhiteSpace(NumberOfPersons_WaterSupply_HomeEnergy_textBox.Text))
            {
                EnergyUsage_WaterSupply_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_WaterSupply_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_WaterSupply_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_WaterSupply_HomeEnergy_picturebox.Image = null;
                Award_WaterSupply_HomeEnergy_picturebox.Visible = false; // Hide the picturebox
                Award_WaterSupply_HomeEnergy_label.Text = string.Empty;
                Award_WaterSupply_HomeEnergy_label.Visible = false; // Hide the label
                totalWaterEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                if (isNumnerPersonWaterErrorSet)
                {
                    water_LeisureTravel_errorProvider.SetError(NumberOfPersons_WaterSupply_HomeEnergy_textBox, string.Empty);
                    isNumnerPersonWaterErrorSet = false;
                }
            }
            else if (!double.TryParse(NumberOfPersons_WaterSupply_HomeEnergy_textBox.Text, out numPersons) || numPersons < 1 || numPersons > 5)
            {
                isValid = false;
                if (!isNumnerPersonWaterErrorSet)
                {
                    water_LeisureTravel_errorProvider.SetError(NumberOfPersons_WaterSupply_HomeEnergy_textBox, "Please enter a valid number of persons between 1 and 5.");
                    isNumnerPersonWaterErrorSet = true;
                }
                EnergyUsage_WaterSupply_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_WaterSupply_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_WaterSupply_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_WaterSupply_HomeEnergy_picturebox.Image = null;
                Award_WaterSupply_HomeEnergy_picturebox.Visible = false; // Hide the picturebox
                Award_WaterSupply_HomeEnergy_label.Text = string.Empty;
                Award_WaterSupply_HomeEnergy_label.Visible = false; // Hide the label

                totalWaterEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);
            }
            else
            {
                if (isNumnerPersonWaterErrorSet)
                {
                    water_LeisureTravel_errorProvider.SetError(NumberOfPersons_WaterSupply_HomeEnergy_textBox, string.Empty);
                    isNumnerPersonWaterErrorSet = false;
                }
            }

            // If validation fails, return
            if (!isValid)
            {
                EnergyUsage_WaterSupply_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_WaterSupply_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_WaterSupply_HomeEnergy_label.Text = "Feedback"; //Assogn default value
                                                                         // Clear the picturebox and label
                Award_WaterSupply_HomeEnergy_picturebox.Image = null;
                Award_WaterSupply_HomeEnergy_picturebox.Visible = false; // Hide the picturebox
                Award_WaterSupply_HomeEnergy_label.Text = string.Empty;
                Award_WaterSupply_HomeEnergy_label.Visible = false; // Hide the label
                totalWaterEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                return;
            }

            // Perform the calculation in watts only if all textboxes are non-empty
            if (!string.IsNullOrWhiteSpace(AvgLitersDaily_WaterSupply_HomeEnergy_textbox.Text) &&
               !string.IsNullOrWhiteSpace(NumberOfPersons_WaterSupply_HomeEnergy_textBox.Text))
            {
                // Perform the calculation in cubic meters
                double waterConsumptionCubicMetersPerPerson = waterConsumptionLitersPerPerson / 1000;
                double totalWaterConsumptionCubicMeters = waterConsumptionCubicMetersPerPerson * numPersons;

                // Calculate total carbon emission from water consumption
                totalWaterEmission = CalculateWaterSupplyCarbonEmission(totalWaterConsumptionCubicMeters);

                // Update labels
                EnergyUsage_WaterSupply_HomeEnergy_label.Text = $"{waterConsumptionLitersPerPerson * numPersons} liters/day";
                Emission_WaterSupply_HomeEnergy_label.Text = $"Emission: {ExtractEmissionValue(totalWaterEmission)} kg CO2e";
                updateGlobalLabel(this, EventArgs.Empty);

                // Provide feedback based on average water usage
                double averageWaterConsumptionPerPerson = 150; ; // Average water consumption in liters per person per day
                double dailyWaterConsumption = waterConsumptionLitersPerPerson * numPersons; // User's input for daily water consumption

                // Calculate the average daily water consumption
                double averageDailyWaterConsumption = averageWaterConsumptionPerPerson * numPersons;
                string improvementTips = "";
                string youTubeLink = "";
                if (dailyWaterConsumption > averageDailyWaterConsumption)
                {
                    Feedback_WaterSupply_HomeEnergy_label.Text = $"Your water consumption of {dailyWaterConsumption} liters/day for {numPersons} person(s) is higher than the average of {averageDailyWaterConsumption} liters/day for {numPersons} person(s).";
                    improvementTips = "Consider reducing water usage by fixing leaks, installing water-saving devices, or taking shorter showers.";
                    youTubeLink = "https://www.youtube.com/watch?v=8tA3GnlaX18";
                }
                else
                {
                    Feedback_WaterSupply_HomeEnergy_label.Text = $"Your water consumption of {dailyWaterConsumption} liters/day for {numPersons} person(s) is within the average range of {averageDailyWaterConsumption} liters/day for {numPersons} person(s).";
                    improvementTips = "Great job on maintaining efficient water usage! Keep it up.";
                    youTubeLink = "No suggestions";
                }

                // Update the picture box and label based on the user's performance
                UpdateWaterSupplyUsageBadge(dailyWaterConsumption, averageDailyWaterConsumption);
                // Append the report to the HomeEnergy category
                // Conditionally append the report data
                if (shouldAppend)
                {
                    AppendReport("HomeEnergy", "Water", dailyWaterConsumption, averageDailyWaterConsumption, Feedback_WaterSupply_HomeEnergy_label.Text, improvementTips, youTubeLink, "Liters/day");
                }
            }
        }
        private void UpdateWaterSupplyUsageBadge(double userUsage, double averageUsage)
        {
            // Define arrays for the images
            Bitmap[] goodPerformanceImages = {
                Properties.Resources.crown1,
                Properties.Resources.crown2,
                Properties.Resources.trophy_star,
                Properties.Resources.award,
                Properties.Resources.trophy,
                Properties.Resources.ribbon
            };

                    Bitmap[] improvementImages = {
                Properties.Resources.target,
                Properties.Resources.person,
                Properties.Resources.business,
                Properties.Resources.fail
            };

                    // Define arrays for the phrases (shortened to two words)
                    string[] goodPerformancePhrases = {
                "Eco Star",
                "Great Job",
                "Top Performer",
                "Keep Going",
                "Well Done"
            };

                    string[] improvementPhrases = {
                "Try Harder",
                "Improve More",
                "Keep Going",
                "Almost There",
                "Step Up"
            };

            // Generate random indexes for each array separately
            int goodImageIndex = random.Next(goodPerformanceImages.Length);
            int improvementImageIndex = random.Next(improvementImages.Length);

            int goodPhraseIndex = random.Next(goodPerformancePhrases.Length);
            int improvementPhraseIndex = random.Next(improvementPhrases.Length);

            if (userUsage < averageUsage)
            {
                Award_WaterSupply_HomeEnergy_picturebox.Image = goodPerformanceImages[goodImageIndex];
                Award_WaterSupply_HomeEnergy_label.Text = goodPerformancePhrases[goodPhraseIndex];
            }
            else
            {
                Award_WaterSupply_HomeEnergy_picturebox.Image = improvementImages[improvementImageIndex];
                Award_WaterSupply_HomeEnergy_label.Text = improvementPhrases[improvementPhraseIndex];
            }

            Award_WaterSupply_HomeEnergy_picturebox.SizeMode = PictureBoxSizeMode.StretchImage;
            Award_WaterSupply_HomeEnergy_picturebox.Visible = true;
            Award_WaterSupply_HomeEnergy_label.Visible = true;
        }
        private string CalculateWaterSupplyCarbonEmission(double waterConsumptionCubicMeters)
        {
            double emissionFactor = 0;

            string connectionString = $"Data Source={dbPath};Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                //string query = "SELECT * FROM conversion_factor WHERE Unit = @Unit";
                string query = "SELECT* FROM conversion_factor WHERE Activity = @Activity AND Year = @Year AND Unit = @Unit";
                //string query = input;
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Activity", "Water supply");
                    command.Parameters.AddWithValue("@Unit", "cubic metres");
                    command.Parameters.AddWithValue("@Year", selectedYear);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Carbon emission factors per kWh for electricity generation in the UK
                            emissionFactor = reader.GetDouble(reader.GetOrdinal("kg CO2e"));
                        }
                    }
                }
            }
            // Assuming the emission factor for water supply is 0.177 kg CO2e per cubic meter
            double totalEmission = waterConsumptionCubicMeters * emissionFactor;
            string result = $"Total Emission: {totalEmission:F6} kg CO2e)";

            // Output for debugging purposes
            Debug.WriteLine(result);

            return result; // Format the emission value to 6 decimal places

        }
        private void HelpClickMe_WaterSupply_HomeEnergy_button_Click(object sender, EventArgs e)
        {
            // Show detailed help message for Water usage
            MessageBox.Show(
                "Daily Water Usage Data:\n\n" +
                "1. **Water Consumption (Liters):**\n" +
                "   - Enter the water consumption per person in liters.\n" +
                "   - Example: 142 liters per day is a typical value.\n" +
                "   - Valid range: Please ensure the value is realistic based on your household's usage.\n\n" +
                "2. **Number of Persons:**\n" +
                "   - Enter the number of persons in your household.\n" +
                "   - Example: 4 persons.\n" +
                "   - Valid range: At least 1 person.\n\n" +
                "Note: The average water usage per person is approximately 142 liters per day, according to [UK Household Water Usage](https://www.cladcodecking.co.uk/blog/post/uk-household-water-usage#:~:text=Average%20Water%20Use-,WHAT%20IS%20THE%20AVERAGE%20WATER%20USE%20PER%20PERSON%20IN%20THE,appliances%2C%20plumbing%20and%20bad%20habits).\n\n" +
                "Accurate data entry will help calculate your daily water consumption and related carbon emissions.",
                "Help Information - Water Usage",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        int maxQtyLimit = 15; // or 20
        //LED carbon emission calculation
        private void LED_HomeEnergy_Carbon_Calculation(object sender, EventArgs e)
        {
            double wattHoursResult = 0;
            double wattResult = 0;
            double wattQty = 0;

            // Validate inputs
            bool isValid = true;

            // Validate Wattage
            if (string.IsNullOrWhiteSpace(Watt_LED_HomeEnergy_textBox.Text))
            {
                EnergyUsage_LED_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_LED_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_LED_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_LED_HomeEnergy_picturebox.Image = null;
                Award_LED_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_LED_HomeEnergy_label.Text = string.Empty;
                Award_LED_HomeEnergy_label.Visible = false; // Hide the label

                totalLedEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                if (isWattLEDErrorSet)
                {
                    LED_homeEnergy_errorProvider.SetError(Watt_LED_HomeEnergy_textBox, string.Empty);
                    isWattLEDErrorSet = false;
                }
                //return;
            }
            else if (!double.TryParse(Watt_LED_HomeEnergy_textBox.Text, out double wattNumber) || wattNumber < 5 || wattNumber > 50)
            {
                isValid = false;
                if (!isWattLEDErrorSet)
                {
                    LED_homeEnergy_errorProvider.SetError(Watt_LED_HomeEnergy_textBox, "Enter a value between 5 W and 50 W. Click help for more details.");
                    isWattLEDErrorSet = true;
                }
                EnergyUsage_LED_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_LED_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_LED_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_LED_HomeEnergy_picturebox.Image = null;
                Award_LED_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_LED_HomeEnergy_label.Text = string.Empty;
                Award_LED_HomeEnergy_label.Visible = false; // Hide the label


                totalLedEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);
            }
            else
            {
                if (isWattLEDErrorSet)
                {
                    LED_homeEnergy_errorProvider.SetError(Watt_LED_HomeEnergy_textBox, string.Empty);
                    isWattLEDErrorSet = false;
                }
                wattResult = wattNumber;
            }

            // Validate HoursDay Hours
            if (string.IsNullOrWhiteSpace(HoursDay_LED_HomeEnergy_textBox.Text))
            {
                EnergyUsage_LED_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_LED_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_LED_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_LED_HomeEnergy_picturebox.Image = null;
                Award_LED_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_LED_HomeEnergy_label.Text = string.Empty;
                Award_LED_HomeEnergy_label.Visible = false; // Hide the label

                if (isHoursLEDErrorSet)
                {
                    LED_homeEnergy_errorProvider.SetError(HoursDay_LED_HomeEnergy_textBox, string.Empty);
                    isHoursLEDErrorSet = false;
                }
                totalLedEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                //return;
            }
            else if (!double.TryParse(HoursDay_LED_HomeEnergy_textBox.Text, out double wattHoursNumber) || wattHoursNumber < 1 || wattHoursNumber > 24)
            {
                isValid = false;
                if (!isHoursLEDErrorSet)
                {
                    LED_homeEnergy_errorProvider.SetError(HoursDay_LED_HomeEnergy_textBox, "Enter a value between 1 and 24 hours. Click help for more details");

                    isHoursLEDErrorSet = true;
                }

                EnergyUsage_LED_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_LED_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_LED_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_LED_HomeEnergy_picturebox.Image = null;
                Award_LED_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_LED_HomeEnergy_label.Text = string.Empty;
                Award_LED_HomeEnergy_label.Visible = false; // Hide the label

                totalLedEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

            }
            else
            {
                if (isHoursLEDErrorSet)
                {
                    LED_homeEnergy_errorProvider.SetError(HoursDay_LED_HomeEnergy_textBox, string.Empty);
                    isHoursLEDErrorSet = false;
                }
                wattHoursResult = wattHoursNumber;
            }

            // Validate Quantity
            if (string.IsNullOrWhiteSpace(Qty_LED_HomeEnergy_textBox.Text))
            {
                EnergyUsage_LED_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_LED_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_LED_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_LED_HomeEnergy_picturebox.Image = null;
                Award_LED_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_LED_HomeEnergy_label.Text = string.Empty;
                Award_LED_HomeEnergy_label.Visible = false; // Hide the label

                if (isQtyLEDErrorSet)
                {
                    LED_homeEnergy_errorProvider.SetError(Qty_LED_HomeEnergy_textBox, string.Empty);
                    isQtyLEDErrorSet = false;
                }
                totalLedEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                //return;
            }
            else if (!double.TryParse(Qty_LED_HomeEnergy_textBox.Text, out double wattqty) || wattqty < 1 || wattqty > maxQtyLimit)
            {
                isValid = false;
                if (!isQtyLEDErrorSet)
                {
                    LED_homeEnergy_errorProvider.SetError(Qty_LED_HomeEnergy_textBox, $"Please enter a valid quantity between 1 and {maxQtyLimit}.Click help for more details");
                    isQtyLEDErrorSet = true;
                }
                EnergyUsage_LED_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_LED_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_LED_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_LED_HomeEnergy_picturebox.Image = null;
                Award_LED_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_LED_HomeEnergy_label.Text = string.Empty;
                Award_LED_HomeEnergy_label.Visible = false; // Hide the label

                totalLedEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

            }
            else
            {
                if (isQtyLEDErrorSet)
                {
                    LED_homeEnergy_errorProvider.SetError(Qty_LED_HomeEnergy_textBox, string.Empty);
                    isQtyLEDErrorSet = false;
                }
                wattQty = wattqty;
            }

            // If validation fails, return
            if (!isValid)
            {
                EnergyUsage_LED_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_LED_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_LED_HomeEnergy_label.Text = "Feedback"; //Assogn default value
                                                                 // Clear the picturebox and label
                Award_LED_HomeEnergy_picturebox.Image = null;
                Award_LED_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_LED_HomeEnergy_label.Text = string.Empty;
                Award_LED_HomeEnergy_label.Visible = false; // Hide the label

                return;
            }

            // Perform the calculation in watts only if all textboxes are non-empty
            if (!string.IsNullOrWhiteSpace(HoursDay_LED_HomeEnergy_textBox.Text) &&
               !string.IsNullOrWhiteSpace(Qty_LED_HomeEnergy_textBox.Text) &&
               !string.IsNullOrWhiteSpace(Watt_LED_HomeEnergy_textBox.Text))
            {
                // Perform the calculation in watts
                double totalWatts = wattResult * wattHoursResult * wattQty;
                // Convert to kilowatts (kW)
                double totalKilowatts = totalWatts / 1000;

                EnergyUsage_LED_HomeEnergy_label.Text = $"Energy: {totalWatts} W / {totalKilowatts} kWh";
                totalLedEmission = CalculateTotalCarbonEmission(totalKilowatts);
                Emission_LED_HomeEnergy_label.Text = $"Emission: {ExtractEmissionValue(totalLedEmission):F6} kg CO2e";
                updateGlobalLabel(this, EventArgs.Empty);

                // Provide feedback based on average usage
                double averageUsageHours = 8; // Average usage in hours per day
                double averageWattage = 12; // Average wattage in watts
                double dailyUsageHours = wattHoursResult; // User's input for usage hours

                // Calculate the average daily energy consumption in watts
                double averageDailyUsage = averageUsageHours * averageWattage * wattQty;
                double userDailyUsage = wattHoursResult * wattResult * wattQty; // User's input for daily usage
                string improvementTips = "";
                string youTubeLink = "";
                if (userDailyUsage > averageDailyUsage)
                {
                    Feedback_LED_HomeEnergy_label.Text = $"Your usage of {dailyUsageHours} hours/day with {wattResult} watts for {wattQty} LED(s) is higher than the average of {averageUsageHours} hours/day with {averageWattage} watts for {wattQty} LED(s).";
                    improvementTips = "Consider switching to more energy-efficient LEDs or reducing usage duration.";
                    youTubeLink = "https://www.youtube.com/watch?v=Ei5vS-g4DTo";

                }
                else
                {
                    Feedback_LED_HomeEnergy_label.Text = $"Your usage of {dailyUsageHours} hours/day with {wattResult} watts for {wattQty} LED(s) is within the average range of {averageUsageHours} hours/day with {averageWattage} watts for {wattQty} LED(s).";
                    improvementTips = "Keep up the good work! Consider sharing your efficient practices with others.";
                    youTubeLink = "No suggestions";

                }

                UpdateLEDUsageBadge(userDailyUsage, averageDailyUsage);
                // Append the report to the HomeEnergy category
                // Conditionally append the report data
                if (shouldAppend)
                {
                    AppendReport("HomeEnergy", "LED", userDailyUsage, averageDailyUsage, Feedback_LED_HomeEnergy_label.Text, improvementTips, youTubeLink, "Watt");
                }
            }
        }
        private void DisplayAllReportsInMessageBox()
        {
            StringBuilder sb = new StringBuilder();

            var groupedReports = energyReports.GroupBy(r => r.Category);

            foreach (var categoryGroup in groupedReports)
            {
                sb.AppendLine($"Category: {categoryGroup.Key}");
                sb.AppendLine(new string('-', 20));

                foreach (var report in categoryGroup)
                {
                    sb.AppendLine($"Item: {report.Item}");
                    sb.AppendLine($"Usage: {report.Usage:F2} {report.Unit}");  // Use the Unit property
                    sb.AppendLine($"Average Usage: {report.AverageUsage:F2} {report.Unit}");  // Use the Unit property
                    sb.AppendLine($"Feedback: {report.Feedback}");
                    sb.AppendLine($"Improvement Tips: {report.ImprovementTips}");
                    sb.AppendLine($"YouTube Link: {report.YouTubeLink}");
                    sb.AppendLine();
                }
                sb.AppendLine(new string('=', 40)); // Separator between categories
                sb.AppendLine();
            }

            MessageBox.Show(sb.ToString(), "Energy Reports", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // Clear the data after generating the report
            energyReports.Clear();
        }
        private void UpdateLEDUsageBadge(double userUsage, double averageUsage)
        {
            // Define arrays for the images
            Bitmap[] goodPerformanceImages = {
                Properties.Resources.crown1,
                Properties.Resources.crown2,
                Properties.Resources.trophy_star,
                Properties.Resources.award,
                Properties.Resources.trophy,
                Properties.Resources.ribbon
            };

            Bitmap[] improvementImages = {
                Properties.Resources.target,
                Properties.Resources.person,
                Properties.Resources.business,
                Properties.Resources.fail
            };

            // Define arrays for the phrases (shortened to two words)
            string[] goodPerformancePhrases = {
                "Eco Star",
                "Great Job",
                "Top Performer",
                "Keep Going",
                "Well Done"
            };

            string[] improvementPhrases = {
                "Try Harder",
                "Improve More",
                "Keep Going",
                "Almost There",
                "Step Up"
            };
            // Generate random indexes for each array separately
            int goodImageIndex = random.Next(goodPerformanceImages.Length);
            int improvementImageIndex = random.Next(improvementImages.Length);

            // Generate random indexes for each phrase array separately
            int goodPhraseIndex = random.Next(goodPerformancePhrases.Length);
            int improvementPhraseIndex = random.Next(improvementPhrases.Length);

            if (userUsage < averageUsage)
            {
                // Show the "Eco Warrior" badge
                Award_LED_HomeEnergy_picturebox.Image = goodPerformanceImages[goodImageIndex];
                Award_LED_HomeEnergy_label.Text = goodPerformancePhrases[goodPhraseIndex];
            }
            else
            {
                // Show the "You Can Do Better" feedback
                Award_LED_HomeEnergy_picturebox.Image = improvementImages[improvementImageIndex];
                Award_LED_HomeEnergy_label.Text = improvementPhrases[improvementPhraseIndex];
            }
            // Set the PictureBox's SizeMode to StretchImage to ensure the image covers the entire PictureBox
            Award_LED_HomeEnergy_picturebox.SizeMode = PictureBoxSizeMode.StretchImage;

            // Make sure the PictureBox and Label are visible
            Award_LED_HomeEnergy_picturebox.Visible = true;
            Award_LED_HomeEnergy_label.Visible = true;
        }
        private void HelpClickMe_LED_HomeEnergy_button_Click(object sender, EventArgs e)
        {
            // Show detailed help message for LED usage
            MessageBox.Show(
                "Daily LED Usage Data:\n\n" +
                "1. **Power Consumption (W):**\n" +
                "   - Enter the power consumption of the LED in watts.\n" +
                "   - Example: 12 W is a typical value.\n" +
                "   - Valid range: 5 W to 50 W.\n\n" +
                "2. **Number of LED Units:**\n" +
                "   - Enter the number of LED units used.\n" +
                "   - Example: 5 units.\n" +
                "   - Valid range: 1 to 15 units.\n\n" +
                "3. **Daily Usage Hours:**\n" +
                "   - Enter the number of hours the LED is used per day.\n" +
                "   - Example: 10 hours per day.\n" +
                "   - Valid range: 1 to 24 hours.\n" +
                "   - The average daily usage is approximately 8 hours, according to [LED Lighting Usage](https://www.linkedin.com/pulse/how-many-watts-led-lights-good-home-use-winny-wen/).\n\n" +
                "Note: The typical power consumption of an LED bulb is around 12 W, as noted by [Crompton LED Light Power Consumption](https://www.crompton.co.in/blogs/lights/led-light-power-consumption).\n" +
                "Accurate data entry will help calculate your daily energy consumption and carbon emissions related to LED usage.",
                "Help Information - LED Usage",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }


        //Fan carbon emission calculation
        private void Fan_HomeEnergy_Carbon_Calculation(object sender, EventArgs e)
        {
            double wattHoursResult = 0;
            double wattResult = 0;
            double wattQty = 0;

            // Validate inputs
            bool isValid = true;

            // Validate Wattage
            if (string.IsNullOrWhiteSpace(Watt_Fan_HomeEnergy_textBox.Text))
            {
                EnergyUsage_Fan_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Fan_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Fan_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_Fan_HomeEnergy_picturebox.Image = null;
                Award_Fan_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_Fan_HomeEnergy_label.Text = string.Empty;
                Award_Fan_HomeEnergy_label.Visible = false; // Hide the label

                if (isWattFanErrorSet)
                {
                    Fan_homeEnergy_errorProvider.SetError(Watt_Fan_HomeEnergy_textBox, string.Empty);
                    isWattFanErrorSet = false;
                }
                totalFanEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                //return;
            }
            else if (!double.TryParse(Watt_Fan_HomeEnergy_textBox.Text, out double wattNumber) || wattNumber < 5 || wattNumber > 50)
            {
                isValid = false;
                if (!isWattFanErrorSet)
                {
                    Fan_homeEnergy_errorProvider.SetError(Watt_Fan_HomeEnergy_textBox, "Enter a value between 5 W and 50 W. Click help for more details.");
                    isWattFanErrorSet = true;
                }
                EnergyUsage_Fan_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Fan_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Fan_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_Fan_HomeEnergy_picturebox.Image = null;
                Award_Fan_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_Fan_HomeEnergy_label.Text = string.Empty;
                Award_Fan_HomeEnergy_label.Visible = false; // Hide the label


                totalFanEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);
            }
            else
            {
                if (isWattFanErrorSet)
                {
                    Fan_homeEnergy_errorProvider.SetError(Watt_Fan_HomeEnergy_textBox, string.Empty);
                    isWattFanErrorSet = false;
                }
                wattResult = wattNumber;
            }

            // Validate HoursDay Hours
            if (string.IsNullOrWhiteSpace(HoursDay_Fan_HomeEnergy_textBox.Text))
            {
                EnergyUsage_Fan_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Fan_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Fan_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_Fan_HomeEnergy_picturebox.Image = null;
                Award_Fan_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_Fan_HomeEnergy_label.Text = string.Empty;
                Award_Fan_HomeEnergy_label.Visible = false; // Hide the label

                if (isHoursFanErrorSet)
                {
                    Fan_homeEnergy_errorProvider.SetError(HoursDay_Fan_HomeEnergy_textBox, string.Empty);
                    isHoursFanErrorSet = false;
                }
                totalFanEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                //return;
            }
            else if (!double.TryParse(HoursDay_Fan_HomeEnergy_textBox.Text, out double wattHoursNumber) || wattHoursNumber < 1 || wattHoursNumber > 24)
            {
                isValid = false;
                if (!isHoursFanErrorSet)
                {
                    Fan_homeEnergy_errorProvider.SetError(HoursDay_Fan_HomeEnergy_textBox, "Enter a value between 1 and 24 hours. Click help for more details.");

                    isHoursFanErrorSet = true;
                }

                EnergyUsage_Fan_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Fan_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Fan_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_Fan_HomeEnergy_picturebox.Image = null;
                Award_Fan_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_Fan_HomeEnergy_label.Text = string.Empty;
                Award_Fan_HomeEnergy_label.Visible = false; // Hide the label

                totalFanEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

            }
            else
            {
                if (isHoursFanErrorSet)
                {
                    Fan_homeEnergy_errorProvider.SetError(HoursDay_Fan_HomeEnergy_textBox, string.Empty);
                    isHoursFanErrorSet = false;
                }
                wattHoursResult = wattHoursNumber;
            }

            // Validate Quantity
            if (string.IsNullOrWhiteSpace(Qty_Fan_HomeEnergy_textBox.Text))
            {
                EnergyUsage_Fan_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Fan_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Fan_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_Fan_HomeEnergy_picturebox.Image = null;
                Award_Fan_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_Fan_HomeEnergy_label.Text = string.Empty;
                Award_Fan_HomeEnergy_label.Visible = false; // Hide the label

                if (isQtyFanErrorSet)
                {
                    Fan_homeEnergy_errorProvider.SetError(Qty_Fan_HomeEnergy_textBox, string.Empty);
                    isQtyFanErrorSet = false;
                }
                //return;
                totalFanEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

            }
            else if (!double.TryParse(Qty_Fan_HomeEnergy_textBox.Text, out double wattqty) || wattqty < 1 || wattqty > 10)
            {
                isValid = false;
                if (!isQtyFanErrorSet)
                {
                    Fan_homeEnergy_errorProvider.SetError(Qty_Fan_HomeEnergy_textBox, "Enter a quantity between 1 and 10. Click help for more details..");
                    isQtyFanErrorSet = true;
                }
                EnergyUsage_Fan_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Fan_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Fan_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_Fan_HomeEnergy_picturebox.Image = null;
                Award_Fan_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_Fan_HomeEnergy_label.Text = string.Empty;
                Award_Fan_HomeEnergy_label.Visible = false; // Hide the label

                totalFanEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

            }
            else
            {
                if (isQtyFanErrorSet)
                {
                    Fan_homeEnergy_errorProvider.SetError(Qty_Fan_HomeEnergy_textBox, string.Empty);
                    isQtyFanErrorSet = false;
                }
                wattQty = wattqty;
            }

            // If validation fails, return
            if (!isValid)
            {
                EnergyUsage_Fan_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Fan_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Fan_HomeEnergy_label.Text = "Feedback"; //Assogn default value
                                                                 // Clear the picturebox and label
                Award_Fan_HomeEnergy_picturebox.Image = null;
                Award_Fan_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_Fan_HomeEnergy_label.Text = string.Empty;
                Award_Fan_HomeEnergy_label.Visible = false; // Hide the label

                return;
            }

            // Perform the calculation in watts only if all textboxes are non-empty
            if (!string.IsNullOrWhiteSpace(HoursDay_Fan_HomeEnergy_textBox.Text) &&
               !string.IsNullOrWhiteSpace(Qty_Fan_HomeEnergy_textBox.Text) &&
               !string.IsNullOrWhiteSpace(Watt_Fan_HomeEnergy_textBox.Text))
            {
                // Perform the calculation in watts
                double totalWatts = wattResult * wattHoursResult * wattQty;
                // Convert to kilowatts (kW)
                double totalKilowatts = totalWatts / 1000;

                EnergyUsage_Fan_HomeEnergy_label.Text = $"Energy: {totalWatts} W / {totalKilowatts} kWh";
                totalFanEmission = CalculateTotalCarbonEmission(totalKilowatts);
                Emission_Fan_HomeEnergy_label.Text = $"Emission: {ExtractEmissionValue(totalFanEmission):F6} kg CO2e";
                updateGlobalLabel(this, EventArgs.Empty);

                // Provide feedback based on average usage
                double averageUsageHours = 6; // Average usage in hours per day
                double averageWattage = 50; // Average wattage in watts
                double dailyUsageHours = wattHoursResult; // User's input for usage hours

                // Calculate the average daily energy consumption in watts
                double averageDailyUsage = averageUsageHours * averageWattage * wattQty;
                double userDailyUsage = wattHoursResult * wattResult * wattQty; // User's input for daily usage

                string improvementTips = "";
                string youTubeLink = "";

                if (userDailyUsage > averageDailyUsage)
                {
                    Feedback_Fan_HomeEnergy_label.Text = $"Your usage of {dailyUsageHours} hours/day with {wattResult} watts for {wattQty} Fan(s) is higher than the average of {averageUsageHours} hours/day with {averageWattage} watts for {wattQty} Fan(s).";
                    improvementTips = "Consider switching to more energy-efficient Fans or reducing usage duration.";
                    youTubeLink = "https://www.youtube.com/watch?v=LwX0FK1Z5QE";

                }
                else
                {
                    Feedback_Fan_HomeEnergy_label.Text = $"Your usage of {dailyUsageHours} hours/day with {wattResult} watts for {wattQty} Fan(s) is within the average range of {averageUsageHours} hours/day with {averageWattage} watts for {wattQty} Fan(s).";
                    improvementTips = "Keep up the good work! Consider sharing your efficient practices with others.";
                    youTubeLink = "No suggestions";

                }
                UpdateFanUsageBadge(userDailyUsage, averageDailyUsage);
                if (shouldAppend)
                {
                    AppendReport("HomeEnergy", "FAN", userDailyUsage, averageDailyUsage, Feedback_Fan_HomeEnergy_label.Text, improvementTips, youTubeLink, "Watt");
                }
            }
        }
        private void UpdateFanUsageBadge(double userUsage, double averageUsage)
        {
            // Define arrays for the images
            Bitmap[] goodPerformanceImages = {
                    Properties.Resources.crown1,
                    Properties.Resources.crown2,
                    Properties.Resources.trophy_star,
                    Properties.Resources.award,
                    Properties.Resources.trophy,
                    Properties.Resources.ribbon
            };

            Bitmap[] improvementImages = {
                    Properties.Resources.target,
                    Properties.Resources.person,
                    Properties.Resources.business,
                    Properties.Resources.fail
            };

            // Define arrays for the phrases (shortened to two words)
            string[] goodPerformancePhrases = {
                "Eco Star",
                "Great Job",
                "Top Performer",
                "Keep Going",
                "Well Done"
            };

            string[] improvementPhrases = {
                "Try Harder",
                "Improve More",
                "Improve",
                "Almost There",
                "Step Up"
            };

            // Generate random indexes for each array separately
            int goodImageIndex = random.Next(goodPerformanceImages.Length);
            int improvementImageIndex = random.Next(improvementImages.Length);

            int goodPhraseIndex = random.Next(goodPerformancePhrases.Length);
            int improvementPhraseIndex = random.Next(improvementPhrases.Length);

            if (userUsage < averageUsage)
            {
                Award_Fan_HomeEnergy_picturebox.Image = goodPerformanceImages[goodImageIndex];
                Award_Fan_HomeEnergy_label.Text = goodPerformancePhrases[goodPhraseIndex];
            }
            else
            {
                Award_Fan_HomeEnergy_picturebox.Image = improvementImages[improvementImageIndex];
                Award_Fan_HomeEnergy_label.Text = improvementPhrases[improvementPhraseIndex];
            }

            Award_Fan_HomeEnergy_picturebox.SizeMode = PictureBoxSizeMode.StretchImage;
            Award_Fan_HomeEnergy_picturebox.Visible = true;
            Award_Fan_HomeEnergy_label.Visible = true;
        }
        private void HelpClickMe_Fan_HomeEnergy_button_Click(object sender, EventArgs e)
        {
            // Show detailed help message for Fan usage
            MessageBox.Show(
                "Daily Fan Usage Data:\n\n" +
                "1. **Power Consumption (W):**\n" +
                "   - Enter the power consumption of the Fan in watts.\n" +
                "   - Example: 50 W is a typical value.\n" +
                "   - Valid range: 5 W to 50 W.\n\n" +
                "2. **Number of Fan Units:**\n" +
                "   - Enter the number of Fan units used.\n" +
                "   - Example: 3 units.\n" +
                "   - Valid range: 1 to 10 units.\n\n" +
                "3. **Daily Usage Hours:**\n" +
                "   - Enter the number of hours the Fan is used per day.\n" +
                "   - Example: 6 hours per day.\n" +
                "   - Valid range: 1 to 24 hours.\n" +
                "   - The average daily usage is approximately 6 hours.\n\n" +
                "Note: Accurate data entry will help calculate your daily energy consumption and carbon emissions related to Fan usage.",
                "Help Information - Fan Usage",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }


        //Kettle carbon emission calculation
        private void Kettle_HomeEnergy_Carbon_Calculation(object sender, EventArgs e)
        {
            double wattHoursResult = 0;
            double wattResult = 0;
            double wattQty = 0;

            // Validate inputs
            bool isValid = true;

            // Validate Wattage
            if (string.IsNullOrWhiteSpace(Watt_Kettle_HomeEnergy_textBox.Text))
            {
                EnergyUsage_Kettle_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Kettle_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Kettle_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_Kettle_HomeEnergy_picturebox.Image = null;
                Award_Kettle_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_Kettle_HomeEnergy_label.Text = string.Empty;
                Award_Kettle_HomeEnergy_label.Visible = false; // Hide the label

                if (isWattKettleErrorSet)
                {
                    Kettl_homeEnergy_errorProvider.SetError(Watt_Kettle_HomeEnergy_textBox, string.Empty);
                    isWattKettleErrorSet = false;
                }
                totalKettleEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                //return;
            }
            else if (!double.TryParse(Watt_Kettle_HomeEnergy_textBox.Text, out double wattNumber) || wattNumber < 1300 || wattNumber > 3000)
            {
                isValid = false;
                if (!isWattKettleErrorSet)
                {
                    Kettl_homeEnergy_errorProvider.SetError(Watt_Kettle_HomeEnergy_textBox, "Enter a value between 1300 W and 3000 W. Click help for more details..");
                    isWattKettleErrorSet = true;
                }
                EnergyUsage_Kettle_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Kettle_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Kettle_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_Kettle_HomeEnergy_picturebox.Image = null;
                Award_Kettle_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_Kettle_HomeEnergy_label.Text = string.Empty;
                Award_Kettle_HomeEnergy_label.Visible = false; // Hide the label


                totalKettleEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);
            }
            else
            {
                if (isWattKettleErrorSet)
                {
                    Kettl_homeEnergy_errorProvider.SetError(Watt_Kettle_HomeEnergy_textBox, string.Empty);
                    isWattKettleErrorSet = false;
                }
                wattResult = wattNumber;
            }

            // Validate HoursDay Hours
            if (string.IsNullOrWhiteSpace(HoursDay_Kettle_HomeEnergy_textBox.Text))
            {
                EnergyUsage_Kettle_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Kettle_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Kettle_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_Kettle_HomeEnergy_picturebox.Image = null;
                Award_Kettle_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_Kettle_HomeEnergy_label.Text = string.Empty;
                Award_Kettle_HomeEnergy_label.Visible = false; // Hide the label

                if (isHoursKettleErrorSet)
                {
                    Kettl_homeEnergy_errorProvider.SetError(HoursDay_Kettle_HomeEnergy_textBox, string.Empty);
                    isHoursKettleErrorSet = false;
                }
                totalKettleEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                //return;
            }
            else if (!double.TryParse(HoursDay_Kettle_HomeEnergy_textBox.Text, out double wattHoursNumber) || wattHoursNumber < 1 || wattHoursNumber > 3)
            {
                isValid = false;
                if (!isHoursKettleErrorSet)
                {
                    Kettl_homeEnergy_errorProvider.SetError(HoursDay_Kettle_HomeEnergy_textBox, "Enter a value between 1 and 3 hours. Click help for more details.");
                    isHoursKettleErrorSet = true;
                }

                EnergyUsage_Kettle_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Kettle_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Kettle_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_Kettle_HomeEnergy_picturebox.Image = null;
                Award_Kettle_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_Kettle_HomeEnergy_label.Text = string.Empty;
                Award_Kettle_HomeEnergy_label.Visible = false; // Hide the label

                totalKettleEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);
            }
            else
            {
                if (isHoursKettleErrorSet)
                {
                    Kettl_homeEnergy_errorProvider.SetError(HoursDay_Kettle_HomeEnergy_textBox, string.Empty);
                    isHoursKettleErrorSet = false;
                }
                wattHoursResult = wattHoursNumber;
            }

            // Validate Quantity
            if (string.IsNullOrWhiteSpace(Qty_Kettle_HomeEnergy_textBox.Text))
            {
                EnergyUsage_Kettle_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Kettle_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Kettle_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_Kettle_HomeEnergy_picturebox.Image = null;
                Award_Kettle_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_Kettle_HomeEnergy_label.Text = string.Empty;
                Award_Kettle_HomeEnergy_label.Visible = false; // Hide the label

                if (isQtyKettleErrorSet)
                {
                    Kettl_homeEnergy_errorProvider.SetError(Qty_Kettle_HomeEnergy_textBox, string.Empty);
                    isQtyKettleErrorSet = false;
                }
                totalKettleEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                //return;
            }
            else if (!double.TryParse(Qty_Kettle_HomeEnergy_textBox.Text, out double wattqty) || wattqty < 1 || wattqty > 3)
            {
                isValid = false;
                if (!isQtyKettleErrorSet)
                {
                    Kettl_homeEnergy_errorProvider.SetError(Qty_Kettle_HomeEnergy_textBox, "Enter a quantity between 1 and 3. Click help for more details.");
                    isQtyKettleErrorSet = true;
                }
                EnergyUsage_Kettle_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Kettle_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Kettle_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_Kettle_HomeEnergy_picturebox.Image = null;
                Award_Kettle_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_Kettle_HomeEnergy_label.Text = string.Empty;
                Award_Kettle_HomeEnergy_label.Visible = false; // Hide the label

                totalKettleEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

            }
            else
            {
                if (isQtyKettleErrorSet)
                {
                    Kettl_homeEnergy_errorProvider.SetError(Qty_Kettle_HomeEnergy_textBox, string.Empty);
                    isQtyKettleErrorSet = false;
                }
                wattQty = wattqty;
            }

            // If validation fails, return
            if (!isValid)
            {
                EnergyUsage_Kettle_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Kettle_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Kettle_HomeEnergy_label.Text = "Feedback"; //Assogn default value
                                                                    // Clear the picturebox and label
                Award_Kettle_HomeEnergy_picturebox.Image = null;
                Award_Kettle_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_Kettle_HomeEnergy_label.Text = string.Empty;
                Award_Kettle_HomeEnergy_label.Visible = false; // Hide the label

                return;
            }

            // Perform the calculation in watts only if all textboxes are non-empty
            if (!string.IsNullOrWhiteSpace(HoursDay_Kettle_HomeEnergy_textBox.Text) &&
               !string.IsNullOrWhiteSpace(Qty_Kettle_HomeEnergy_textBox.Text) &&
               !string.IsNullOrWhiteSpace(Watt_Kettle_HomeEnergy_textBox.Text))
            {
                // Perform the calculation in watts
                double totalWatts = wattResult * wattHoursResult * wattQty;
                // Convert to kilowatts (kW)
                double totalKilowatts = totalWatts / 1000;

                EnergyUsage_Kettle_HomeEnergy_label.Text = $"Energy: {totalWatts} W / {totalKilowatts} kWh";
                totalKettleEmission = CalculateTotalCarbonEmission(totalKilowatts);
                Emission_Kettle_HomeEnergy_label.Text = $"Emission: {ExtractEmissionValue(totalKettleEmission):F6} kg CO2e";
                updateGlobalLabel(this, EventArgs.Empty);

                // Provide feedback based on average usage
                double averageUsageHours = 1; // Average usage in hours per day
                double averageWattage = 1300; // Average wattage in watts
                double dailyUsageHours = wattHoursResult; // User's input for usage hours

                // Calculate the average daily energy consumption in watts
                double averageDailyUsage = averageUsageHours * averageWattage * wattQty;
                double userDailyUsage = wattHoursResult * wattResult * wattQty; // User's input for daily usage
                string improvementTips = "";
                string youTubeLink = "";
                if (userDailyUsage > averageDailyUsage)
                {
                    Feedback_Kettle_HomeEnergy_label.Text = $"Your usage of {dailyUsageHours} hours/day with {wattResult} watts for {wattQty} Kettle(s) is higher than the average of {averageUsageHours} hours/day with {averageWattage} watts for {wattQty} Kettle(s).";
                    improvementTips = "Consider switching to more energy-efficient Kettles or reducing usage duration.";
                    youTubeLink = "https://www.youtube.com/watch?v=yioVEC6oi74";

                }
                else
                {
                    Feedback_Kettle_HomeEnergy_label.Text = $"Your usage of {dailyUsageHours} hours/day with {wattResult} watts for {wattQty} Kettle(s) is within the average range of {averageUsageHours} hours/day with {averageWattage} watts for {wattQty} Kettle(s).";
                    improvementTips = "Keep up the good work! Consider sharing your efficient practices with others.";
                    youTubeLink = "No suggestions";

                }

                UpdateKettleUsageBadge(userDailyUsage, averageDailyUsage);
                // Append the report to the HomeEnergy category
                // Conditionally append the report data
                if (shouldAppend)
                {
                    AppendReport("HomeEnergy", "Kettle", userDailyUsage, averageDailyUsage, Feedback_Kettle_HomeEnergy_label.Text, improvementTips, youTubeLink, "Watt");
                }
            }
        }
        private void UpdateKettleUsageBadge(double userUsage, double averageUsage)
        {
            // Define arrays for the images
            Bitmap[] goodPerformanceImages = {
                    Properties.Resources.crown1,
                    Properties.Resources.crown2,
                    Properties.Resources.trophy_star,
                    Properties.Resources.award,
                    Properties.Resources.trophy,
                    Properties.Resources.ribbon
            };

            Bitmap[] improvementImages = {
                    Properties.Resources.target,
                    Properties.Resources.person,
                    Properties.Resources.business,
                    Properties.Resources.fail
            };

            // Define arrays for the phrases (shortened to two words)
            string[] goodPerformancePhrases = {
                "Eco Star",
                "Great Job",
                "Top Performer",
                "Keep Going",
                "Well Done"
            };

            string[] improvementPhrases = {
                "Try Harder",
                "Improve More",
                "Improve",
                "Almost There",
                "Step Up"
            };

            // Generate random indexes for each array separately
            int goodImageIndex = random.Next(goodPerformanceImages.Length);
            int improvementImageIndex = random.Next(improvementImages.Length);

            int goodPhraseIndex = random.Next(goodPerformancePhrases.Length);
            int improvementPhraseIndex = random.Next(improvementPhrases.Length);

            if (userUsage < averageUsage)
            {
                Award_Kettle_HomeEnergy_picturebox.Image = goodPerformanceImages[goodImageIndex];
                Award_Kettle_HomeEnergy_label.Text = goodPerformancePhrases[goodPhraseIndex];
            }
            else
            {
                Award_Kettle_HomeEnergy_picturebox.Image = improvementImages[improvementImageIndex];
                Award_Kettle_HomeEnergy_label.Text = improvementPhrases[improvementPhraseIndex];
            }

            Award_Kettle_HomeEnergy_picturebox.SizeMode = PictureBoxSizeMode.StretchImage;
            Award_Kettle_HomeEnergy_picturebox.Visible = true;
            Award_Kettle_HomeEnergy_label.Visible = true;
        }
        private void HelpClickMe_Kettle_HomeEnergy_button_Click(object sender, EventArgs e)
        {
            // Show detailed help message for Kettle usage
            MessageBox.Show(
                "Daily Kettle Usage Data:\n\n" +
                "1. **Power Consumption (W):**\n" +
                "   - Enter the power consumption of the Kettle in watts.\n" +
                "   - Example: 1300 W is a typical value.\n" +
                "   - Valid range: 1300 W to 3000 W.\n\n" +
                "2. **Number of Kettle Units:**\n" +
                "   - Enter the number of Kettle units used.\n" +
                "   - Example: 1 unit.\n" +
                "   - Valid range: 1 to 3 units.\n\n" +
                "3. **Daily Usage Hours:**\n" +
                "   - Enter the number of hours the Kettle is used per day.\n" +
                "   - Example: 1 hour per day.\n" +
                "   - Valid range: 1 to 3 hours.\n" +
                "   - The average daily usage is approximately 1 hour, according to [Kettle Power Consumption](https://www.daftlogic.com/information-appliance-power-consumption.htm).\n\n" +
                "Note: Accurate data entry will help calculate your daily energy consumption and carbon emissions related to Kettle usage.",
                "Help Information - Kettle Usage",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }


        //Heater carbon emission calculation 
        private void Heater_HomeEnergy_Carbon_Calculation(object sender, EventArgs e)
        {
            double wattHoursResult = 0;
            double wattResult = 0;
            double wattQty = 0;

            // Validate inputs
            bool isValid = true;

            // Validate Wattage
            if (string.IsNullOrWhiteSpace(Watt_Heater_HomeEnergy_textBox.Text))
            {
                EnergyUsage_Heater_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Heater_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Heater_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_Heater_HomeEnergy_picturebox.Image = null;
                Award_Heater_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_Heater_HomeEnergy_label.Text = string.Empty;
                Award_Heater_HomeEnergy_label.Visible = false; // Hide the label

                if (isWattHeaterErrorSet)
                {
                    heater_LeisureTravel_errorProvider.SetError(Watt_Heater_HomeEnergy_textBox, string.Empty);
                    isWattHeaterErrorSet = false;
                }
                totalElectricHeaterEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                //return;
            }
            else if (!double.TryParse(Watt_Heater_HomeEnergy_textBox.Text, out double wattNumber) || wattNumber < 1500 || wattNumber > 3000)
            {
                isValid = false;
                if (!isWattHeaterErrorSet)
                {
                    heater_LeisureTravel_errorProvider.SetError(Watt_Heater_HomeEnergy_textBox, "Enter a value between 1500 W and 3000 W. Average value: 2500 W. Click help for more details..");
                    isWattHeaterErrorSet = true;
                }
                EnergyUsage_Heater_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Heater_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Heater_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_Heater_HomeEnergy_picturebox.Image = null;
                Award_Heater_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_Heater_HomeEnergy_label.Text = string.Empty;
                Award_Heater_HomeEnergy_label.Visible = false; // Hide the label


                totalElectricHeaterEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);
            }
            else
            {
                if (isWattHeaterErrorSet)
                {
                    heater_LeisureTravel_errorProvider.SetError(Watt_Heater_HomeEnergy_textBox, string.Empty);
                    isWattHeaterErrorSet = false;
                }
                wattResult = wattNumber;
            }

            // Validate HoursDay Hours
            if (string.IsNullOrWhiteSpace(HoursDay_Heater_HomeEnergy_textBox.Text))
            {
                EnergyUsage_Heater_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Heater_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Heater_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_Heater_HomeEnergy_picturebox.Image = null;
                Award_Heater_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_Heater_HomeEnergy_label.Text = string.Empty;
                Award_Heater_HomeEnergy_label.Visible = false; // Hide the label

                if (isHoursHeaterErrorSet)
                {
                    heater_LeisureTravel_errorProvider.SetError(HoursDay_Heater_HomeEnergy_textBox, string.Empty);
                    isHoursHeaterErrorSet = false;
                }
                totalElectricHeaterEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                //return;
            }
            else if (!double.TryParse(HoursDay_Heater_HomeEnergy_textBox.Text, out double wattHoursNumber) || wattHoursNumber < 1 || wattHoursNumber > 12)
            {
                isValid = false;
                if (!isHoursHeaterErrorSet)
                {
                    heater_LeisureTravel_errorProvider.SetError(HoursDay_Heater_HomeEnergy_textBox, "Enter a value between 1 and 12 hours. Average value: 6 hours. Click help for more details.");
                    isHoursHeaterErrorSet = true;
                }

                EnergyUsage_Heater_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Heater_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Heater_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_Heater_HomeEnergy_picturebox.Image = null;
                Award_Heater_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_Heater_HomeEnergy_label.Text = string.Empty;
                Award_Heater_HomeEnergy_label.Visible = false; // Hide the label

                totalElectricHeaterEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);
            }
            else
            {
                if (isHoursHeaterErrorSet)
                {
                    heater_LeisureTravel_errorProvider.SetError(HoursDay_Heater_HomeEnergy_textBox, string.Empty);
                    isHoursHeaterErrorSet = false;
                }
                wattHoursResult = wattHoursNumber;
            }

            // Validate Quantity
            if (string.IsNullOrWhiteSpace(Qty_Heater_HomeEnergy_textBox.Text))
            {
                EnergyUsage_Heater_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Heater_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Heater_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_Heater_HomeEnergy_picturebox.Image = null;
                Award_Heater_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_Heater_HomeEnergy_label.Text = string.Empty;
                Award_Heater_HomeEnergy_label.Visible = false; // Hide the label

                if (isQtyHeaterErrorSet)
                {
                    heater_LeisureTravel_errorProvider.SetError(Qty_Heater_HomeEnergy_textBox, string.Empty);
                    isQtyHeaterErrorSet = false;
                }
                totalElectricHeaterEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                //return;
            }
            else if (!double.TryParse(Qty_Heater_HomeEnergy_textBox.Text, out double wattqty) || wattqty < 1 || wattqty > 3)
            {
                isValid = false;
                if (!isQtyHeaterErrorSet)
                {
                    heater_LeisureTravel_errorProvider.SetError(Qty_Heater_HomeEnergy_textBox, "Enter a quantity between 1 and 3. Example: 1 unit. Click help for more details.");
                    isQtyHeaterErrorSet = true;
                }
                EnergyUsage_Heater_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Heater_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Heater_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                // Clear the picturebox and label
                Award_Heater_HomeEnergy_picturebox.Image = null;
                Award_Heater_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_Heater_HomeEnergy_label.Text = string.Empty;
                Award_Heater_HomeEnergy_label.Visible = false; // Hide the label

                totalElectricHeaterEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

            }
            else
            {
                if (isQtyHeaterErrorSet)
                {
                    heater_LeisureTravel_errorProvider.SetError(Qty_Heater_HomeEnergy_textBox, string.Empty);
                    isQtyKettleErrorSet = false;
                }
                wattQty = wattqty;
            }

            // If validation fails, return
            if (!isValid)
            {
                EnergyUsage_Heater_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Heater_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Heater_HomeEnergy_label.Text = "Feedback"; //Assogn default value
                                                                    // Clear the picturebox and label
                Award_Heater_HomeEnergy_picturebox.Image = null;
                Award_Heater_HomeEnergy_picturebox.Visible = false; // Hide the picturebox

                Award_Heater_HomeEnergy_label.Text = string.Empty;
                Award_Heater_HomeEnergy_label.Visible = false; // Hide the label

                return;
            }

            // Perform the calculation in watts only if all textboxes are non-empty
            if (!string.IsNullOrWhiteSpace(HoursDay_Heater_HomeEnergy_textBox.Text) &&
               !string.IsNullOrWhiteSpace(Qty_Heater_HomeEnergy_textBox.Text) &&
               !string.IsNullOrWhiteSpace(Watt_Heater_HomeEnergy_textBox.Text))
            {
                // Perform the calculation in watts
                double totalWatts = wattResult * wattHoursResult * wattQty;
                // Convert to kilowatts (kW)
                double totalKilowatts = totalWatts / 1000;

                EnergyUsage_Heater_HomeEnergy_label.Text = $"Energy: {totalWatts} W / {totalKilowatts} kWh";
                totalElectricHeaterEmission = CalculateTotalCarbonEmission(totalKilowatts);
                Emission_Heater_HomeEnergy_label.Text = $"Emission: {ExtractEmissionValue(totalElectricHeaterEmission):F6} kg CO2e";
                updateGlobalLabel(this, EventArgs.Empty);

                // Provide feedback based on average usage
                double averageUsageHours = 6; // Average usage in hours per day
                double averageWattage = 2500; // Average wattage in watts
                double dailyUsageHours = wattHoursResult; // User's input for usage hours


                // Calculate the average daily energy consumption in watts
                double averageDailyUsage = averageUsageHours * averageWattage * wattQty;
                double userDailyUsage = wattHoursResult * wattResult * wattQty; // User's input for daily usage

                string improvementTips = "";
                string youTubeLink = "";
                if (userDailyUsage > averageDailyUsage)
                {
                    Feedback_Heater_HomeEnergy_label.Text = $"Your usage of {dailyUsageHours} hours/day with {wattResult} watts for {wattQty} Heater(s) is higher than the average of {averageUsageHours} hours/day with {averageWattage} watts for {wattQty} Heater(s).";
                    improvementTips = "Consider reducing heater usage or insulating your home better to retain heat, reducing the need for prolonged heater use.";
                    youTubeLink = "https://www.youtube.com/watch?v=l6XMuf2b0ag"; //
                }
                else
                {
                    Feedback_Heater_HomeEnergy_label.Text = $"Your usage of {dailyUsageHours} hours/day with {wattResult} watts for {wattQty} Heater(s) is within the average range of {averageUsageHours} hours/day with {averageWattage} watts for {wattQty} Heater(s).";
                    improvementTips = "Great job! You’re efficiently using your heater. Consider sharing your energy-saving tips with others.";
                    youTubeLink = "No suggestions";
                }


                UpdateHeaterUsageBadge(userDailyUsage, averageDailyUsage);
                // Append the report to the HomeEnergy category
                // Conditionally append the report data
                if (shouldAppend)
                {
                    AppendReport("HomeEnergy", "Heater", userDailyUsage, averageDailyUsage, Feedback_Heater_HomeEnergy_label.Text, improvementTips, youTubeLink, "Watt");
                }
            }
        }
        private void UpdateHeaterUsageBadge(double userUsage, double averageUsage)
        {
            // Define arrays for the images
            Bitmap[] goodPerformanceImages = {
                    Properties.Resources.crown1,
                    Properties.Resources.crown2,
                    Properties.Resources.trophy_star,
                    Properties.Resources.award,
                    Properties.Resources.trophy,
                    Properties.Resources.ribbon
            };

            Bitmap[] improvementImages = {
                    Properties.Resources.target,
                    Properties.Resources.person,
                    Properties.Resources.business,
                    Properties.Resources.fail
            };

            // Define arrays for the phrases (shortened to two words)
            string[] goodPerformancePhrases = {
                "Eco Star",
                "Great Job",
                "Top Performer",
                "Keep Going",
                "Well Done"
            };

            string[] improvementPhrases = {
                "Try Harder",
                "Improve More",
                "Improve",
                "Almost There",
                "Step Up"
            };

            // Generate random indexes for each array separately
            int goodImageIndex = random.Next(goodPerformanceImages.Length);
            int improvementImageIndex = random.Next(improvementImages.Length);

            int goodPhraseIndex = random.Next(goodPerformancePhrases.Length);
            int improvementPhraseIndex = random.Next(improvementPhrases.Length);

            if (userUsage < averageUsage)
            {
                Award_Heater_HomeEnergy_picturebox.Image = goodPerformanceImages[goodImageIndex];
                Award_Heater_HomeEnergy_label.Text = goodPerformancePhrases[goodPhraseIndex];
            }
            else
            {
                Award_Heater_HomeEnergy_picturebox.Image = improvementImages[improvementImageIndex];
                Award_Heater_HomeEnergy_label.Text = improvementPhrases[improvementPhraseIndex];
            }

            Award_Heater_HomeEnergy_picturebox.SizeMode = PictureBoxSizeMode.StretchImage;
            Award_Heater_HomeEnergy_picturebox.Visible = true;
            Award_Heater_HomeEnergy_label.Visible = true;
        }
        private void HelpClickMe_Heater_HomeEnergy_button_Click(object sender, EventArgs e)
        {
            // Show detailed help message for Heater usage
            MessageBox.Show(
                "Daily Heater Usage Data:\n\n" +
                "1. **Power Consumption (W):**\n" +
                "   - Enter the power consumption of the electric heater in watts.\n" +
                "   - Example: 2500 W is a typical value.\n" +
                "   - Valid range: 1500 W to 3000 W.\n\n" +
                "2. **Number of Heater Units:**\n" +
                "   - Enter the number of heater units used.\n" +
                "   - Example: 1 unit.\n" + 
                "   - Valid range: 1 to 3 units.\n\n" +
                "3. **Daily Usage Hours:**\n" +
                "   - Enter the number of hours the heater is used per day.\n" +
                "   - Example: 6 hours per day.\n" +
                "   - Valid range: 1 to 12 hours.\n\n" +
                "Note: The typical power consumption for electric heaters is around 2500 W, as detailed in the [Electricity Usage of Heaters](https://www.cse.org.uk/advice/how-much-electricity-am-i-using/?gad_source=1&gclid=Cj0KCQjwt4a2BhD6ARIsALgH7DquYjZpqM0AwEtNpfPKcXwH1W7THSpOhI5S5upg6dXIYd1R1bxwZcwaAn2ZEALw_wcB).\n\n" +
                "Accurate data entry will help calculate your daily energy consumption and carbon emissions related to heater usage.",
                "Help Information - Heater Usage",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }


        //CustomEntry carbon emission calculation
        private void CustomEntry_HomeEnergy_Carbon_Calculation(object sender, EventArgs e)
        {
            double wattHoursResult = 0;
            double wattResult = 0;
            double wattQty = 0;

            // Validate inputs
            bool isValid = true;

            // Validate Wattage
            if (string.IsNullOrWhiteSpace(Watt_CustomEntry_HomeEnergy_textBox.Text))
            {
                EnergyUsage_CustomEntry_HomeEnergy_label.Text = "kWh"; // Assign default value
                Emission_CustomEntry_HomeEnergy_label.Text = "Emission"; // Assign default value
                //Feedback_CustomEntry_HomeEnergy_label.Text = "Feedback"; // Assign default value

                // Clear the picturebox and label
                Award_CustomEntry_HomeEnergy_picturebox.Image = null;
                Award_CustomEntry_HomeEnergy_picturebox.Visible = false; // Hide the picturebox
                Award_CustomEntry_HomeEnergy_label.Text = string.Empty;
                Award_CustomEntry_HomeEnergy_label.Visible = false; // Hide the label

                totalCustomEntryEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                if (isWattCustomErrorSet)
                {
                    customEntry_LeisureTravel_errorProvider.SetError(Watt_CustomEntry_HomeEnergy_textBox, string.Empty);
                    isWattCustomErrorSet = false;
                }
            }
            else if (!double.TryParse(Watt_CustomEntry_HomeEnergy_textBox.Text, out double wattNumber) || wattNumber < 1 || wattNumber > 100)
            {
                isValid = false;
                if (!isWattCustomErrorSet)
                {
                    customEntry_LeisureTravel_errorProvider.SetError(Watt_CustomEntry_HomeEnergy_textBox, "Please enter a valid wattage between 1 and 100.");
                    isWattCustomErrorSet = true;
                }
                EnergyUsage_CustomEntry_HomeEnergy_label.Text = "kWh"; // Assign default value
                Emission_CustomEntry_HomeEnergy_label.Text = "Emission"; // Assign default value
                //Feedback_CustomEntry_HomeEnergy_label.Text = "Feedback"; // Assign default value

                // Clear the picturebox and label
                Award_CustomEntry_HomeEnergy_picturebox.Image = null;
                Award_CustomEntry_HomeEnergy_picturebox.Visible = false; // Hide the picturebox
                Award_CustomEntry_HomeEnergy_label.Text = string.Empty;
                Award_CustomEntry_HomeEnergy_label.Visible = false; // Hide the label

                totalCustomEntryEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);
            }
            else
            {
                if (isWattCustomErrorSet)
                {
                    customEntry_LeisureTravel_errorProvider.SetError(Watt_CustomEntry_HomeEnergy_textBox, string.Empty);
                    isWattCustomErrorSet = false;
                }
                wattResult = wattNumber;
            }

            // Validate HoursDay Hours
            if (string.IsNullOrWhiteSpace(HoursDay_CustomEntry_HomeEnergy_textBox.Text))
            {
                EnergyUsage_CustomEntry_HomeEnergy_label.Text = "kWh"; // Assign default value
                Emission_CustomEntry_HomeEnergy_label.Text = "Emission"; // Assign default value
                //Feedback_CustomEntry_HomeEnergy_label.Text = "Feedback"; // Assign default value

                // Clear the picturebox and label
                Award_CustomEntry_HomeEnergy_picturebox.Image = null;
                Award_CustomEntry_HomeEnergy_picturebox.Visible = false; // Hide the picturebox
                Award_CustomEntry_HomeEnergy_label.Text = string.Empty;
                Award_CustomEntry_HomeEnergy_label.Visible = false; // Hide the label

                totalCustomEntryEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                if (isHoursCustomErrorSet)
                {
                    customEntry_LeisureTravel_errorProvider.SetError(HoursDay_CustomEntry_HomeEnergy_textBox, string.Empty);
                    isHoursCustomErrorSet = false;
                }
            }
            else if (!double.TryParse(HoursDay_CustomEntry_HomeEnergy_textBox.Text, out double wattHoursNumber) || wattHoursNumber < 1 || wattHoursNumber > 24)
            {
                isValid = false;
                if (!isHoursCustomErrorSet)
                {
                    customEntry_LeisureTravel_errorProvider.SetError(HoursDay_CustomEntry_HomeEnergy_textBox, "Please enter a valid number of hours between 1 and 24.");
                    isHoursCustomErrorSet = true;
                }

                EnergyUsage_CustomEntry_HomeEnergy_label.Text = "kWh"; // Assign default value
                Emission_CustomEntry_HomeEnergy_label.Text = "Emission"; // Assign default value
                //Feedback_CustomEntry_HomeEnergy_label.Text = "Feedback"; // Assign default value

                // Clear the picturebox and label
                Award_CustomEntry_HomeEnergy_picturebox.Image = null;
                Award_CustomEntry_HomeEnergy_picturebox.Visible = false; // Hide the picturebox
                Award_CustomEntry_HomeEnergy_label.Text = string.Empty;
                Award_CustomEntry_HomeEnergy_label.Visible = false; // Hide the label

                totalCustomEntryEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);
            }
            else
            {
                if (isHoursCustomErrorSet)
                {
                    customEntry_LeisureTravel_errorProvider.SetError(HoursDay_CustomEntry_HomeEnergy_textBox, string.Empty);
                    isHoursCustomErrorSet = false;
                }
                wattHoursResult = wattHoursNumber;
            }

            // Validate Quantity
            if (string.IsNullOrWhiteSpace(Qty_CustomEntry_HomeEnergy_textBox.Text))
            {
                EnergyUsage_CustomEntry_HomeEnergy_label.Text = "kWh"; // Assign default value
                Emission_CustomEntry_HomeEnergy_label.Text = "Emission"; // Assign default value
                //Feedback_CustomEntry_HomeEnergy_label.Text = "Feedback"; // Assign default value

                // Clear the picturebox and label
                Award_CustomEntry_HomeEnergy_picturebox.Image = null;
                Award_CustomEntry_HomeEnergy_picturebox.Visible = false; // Hide the picturebox
                Award_CustomEntry_HomeEnergy_label.Text = string.Empty;
                Award_CustomEntry_HomeEnergy_label.Visible = false; // Hide the label

                totalCustomEntryEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                if (isQtyCustomErrorSet)
                {
                    customEntry_LeisureTravel_errorProvider.SetError(Qty_CustomEntry_HomeEnergy_textBox, string.Empty);
                    isQtyCustomErrorSet = false;
                }
            }
            else if (!double.TryParse(Qty_CustomEntry_HomeEnergy_textBox.Text, out double wattqty) || wattqty < 1)
            {
                isValid = false;
                if (!isQtyCustomErrorSet)
                {
                    customEntry_LeisureTravel_errorProvider.SetError(Qty_CustomEntry_HomeEnergy_textBox, "Please enter a valid quantity (at least 1).");
                    isQtyCustomErrorSet = true;
                }
                EnergyUsage_CustomEntry_HomeEnergy_label.Text = "kWh"; // Assign default value
                Emission_CustomEntry_HomeEnergy_label.Text = "Emission"; // Assign default value
                //Feedback_CustomEntry_HomeEnergy_label.Text = "Feedback"; // Assign default value

                // Clear the picturebox and label
                Award_CustomEntry_HomeEnergy_picturebox.Image = null;
                Award_CustomEntry_HomeEnergy_picturebox.Visible = false; // Hide the picturebox
                Award_CustomEntry_HomeEnergy_label.Text = string.Empty;
                Award_CustomEntry_HomeEnergy_label.Visible = false; // Hide the label

                totalCustomEntryEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);
            }
            else
            {
                if (isQtyCustomErrorSet)
                {
                    customEntry_LeisureTravel_errorProvider.SetError(Qty_CustomEntry_HomeEnergy_textBox, string.Empty);
                    isQtyCustomErrorSet = false;
                }
                wattQty = wattqty;
            }

            // If validation fails, return
            if (!isValid)
            {
                EnergyUsage_CustomEntry_HomeEnergy_label.Text = "kWh"; // Assign default value
                Emission_CustomEntry_HomeEnergy_label.Text = "Emission"; // Assign default value
                //Feedback_CustomEntry_HomeEnergy_label.Text = "Feedback"; // Assign default value

                // Clear the picturebox and label
                Award_CustomEntry_HomeEnergy_picturebox.Image = null;
                Award_CustomEntry_HomeEnergy_picturebox.Visible = false; // Hide the picturebox
                Award_CustomEntry_HomeEnergy_label.Text = string.Empty;
                Award_CustomEntry_HomeEnergy_label.Visible = false; // Hide the label

                return;
            }

            // Perform the calculation in watts only if all textboxes are non-empty
            if (!string.IsNullOrWhiteSpace(HoursDay_CustomEntry_HomeEnergy_textBox.Text) &&
               !string.IsNullOrWhiteSpace(Qty_CustomEntry_HomeEnergy_textBox.Text) &&
               !string.IsNullOrWhiteSpace(Watt_CustomEntry_HomeEnergy_textBox.Text))
            {
                // Perform the calculation in watts
                double totalWatts = wattResult * wattHoursResult * wattQty;
                // Convert to kilowatts (kW)
                double totalKilowatts = totalWatts / 1000;

                EnergyUsage_CustomEntry_HomeEnergy_label.Text = $"Energy: {totalWatts} W / {totalKilowatts} kWh";
                totalCustomEntryEmission = CalculateTotalCarbonEmission(totalKilowatts);
                Emission_CustomEntry_HomeEnergy_label.Text = $"Emission: {ExtractEmissionValue(totalCustomEntryEmission):F6} kg CO2e";
                updateGlobalLabel(this, EventArgs.Empty);
            }
        }
        private void HelpClickMe_CustomEntry_HomeEnergy_button_Click(object sender, EventArgs e)
        {
            // Show detailed help message
            MessageBox.Show(
                "Daily Usage Data:\n\n" +
                "1. Enter the power consumption for a Specific Custom Entry in watts (W). E.g., 100\n" +
                "2. Enter the number of unit used. E.g., 1\n" +
                "3. Enter the number of hours used per day. E.g., 5",
                "Help Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        //Common functions.
        private string CalculateTotalCarbonEmission(double electricityConsumptionKWh)
        {
            // Carbon emission factors per kWh for electricity generation in the UK
            double totalGenerationEmissionFactor = 0; // kg CO2e per kWh
            double co2GenerationFactor = 0; // kg CO2e per kWh
            double ch4GenerationFactor = 0; // kg CO2e per kWh
            double n2oGenerationFactor = 0; // kg CO2e per kWh

            double totalTDemissionFactor = 0; // kg CO2e per kWh
            double co2TDemissionFactor = 0; // kg CO2e per kWh
            double ch4TDemissionFactor = 0; // kg CO2e per kWh
            double n2oTDemissionFactor = 0; // kg CO2e per kWh

            string connectionString = $"Data Source={dbPath};Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                //string query = "SELECT * FROM conversion_factor WHERE Unit = @Unit";
                string query = "SELECT* FROM conversion_factor WHERE Activity = @Activity AND Year = @Year AND Unit = @Unit";
                //string query = input;
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Activity", "Electricity generated");
                    command.Parameters.AddWithValue("@Unit", "kWh");
                    command.Parameters.AddWithValue("@Year", selectedYear);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Carbon emission factors per kWh for electricity generation in the UK
                            totalGenerationEmissionFactor = reader.GetDouble(reader.GetOrdinal("kg CO2e"));
                            co2GenerationFactor = reader.GetDouble(reader.GetOrdinal("kg CO2e of CO2 per unit"));
                            ch4GenerationFactor = reader.GetDouble(reader.GetOrdinal("kg CO2e of CH4 per unit"));
                            n2oGenerationFactor = reader.GetDouble(reader.GetOrdinal("kg CO2e of N2O per unit"));
                        }
                    }
                }
                query = "SELECT* FROM conversion_factor WHERE Activity = @Activity AND Year = @Year AND Unit = @Unit";
                //string query = input;
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Activity", "T&D- UK electricity");
                    command.Parameters.AddWithValue("@Unit", "kWh");
                    command.Parameters.AddWithValue("@Year", selectedYear);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Carbon emission factors per kWh for electricity generation in the UK
                            totalTDemissionFactor = reader.GetDouble(reader.GetOrdinal("kg CO2e"));
                            co2TDemissionFactor = reader.GetDouble(reader.GetOrdinal("kg CO2e of CO2 per unit"));
                            ch4TDemissionFactor = reader.GetDouble(reader.GetOrdinal("kg CO2e of CH4 per unit"));
                            n2oTDemissionFactor = reader.GetDouble(reader.GetOrdinal("kg CO2e of N2O per unit"));
                        }
                    }
                }

            }

            // Calculate total carbon emissions from generation
            double totalGenerationEmission = electricityConsumptionKWh * totalGenerationEmissionFactor;
            double co2GenerationEmission = electricityConsumptionKWh * co2GenerationFactor;
            double ch4GenerationEmission = electricityConsumptionKWh * ch4GenerationFactor;
            double n2oGenerationEmission = electricityConsumptionKWh * n2oGenerationFactor;

            // Calculate total carbon emissions from T&D
            double totalTDemission = electricityConsumptionKWh * totalTDemissionFactor;
            double co2TDemission = electricityConsumptionKWh * co2TDemissionFactor;
            double ch4TDemission = electricityConsumptionKWh * ch4TDemissionFactor;
            double n2oTDemission = electricityConsumptionKWh * n2oTDemissionFactor;

            // Calculate overall total carbon emissions
            double overallTotalEmission = totalGenerationEmission + totalTDemission;
            double overallCO2Emission = co2GenerationEmission + co2TDemission;
            double overallCH4Emission = ch4GenerationEmission + ch4TDemission;
            double overallN2OEmission = n2oGenerationEmission + n2oTDemission;

            // Output or use these values as needed
            Debug.WriteLine($"Total Carbon Emission: {overallTotalEmission} kg CO2e");
            Debug.WriteLine($"CO2 Emission: {overallCO2Emission} kg CO2e");
            Debug.WriteLine($"CH4 Emission: {overallCH4Emission} kg CO2e");
            Debug.WriteLine($"N2O Emission: {overallN2OEmission} kg CO2e");

            string result = $"Total Emission: {overallTotalEmission:F6} kg CO2e (CO2: {overallCO2Emission:F6}, CH4: {overallCH4Emission:F6}, N2O: {overallN2OEmission:F6})";

            // Output for debugging purposes
            Debug.WriteLine(result);

            // Return the result string
            return result;
        }
        private string ExtractEmissionFactorsValue(string emissionString)
        {
            if (emissionString.Contains("Emission Factors:"))
            {
                //string emission_factors = $"Emission Factors: {largeDieselTotalFactor:F6} kg CO2e (CO2: {largeDieselCO2Factor:F6}, CH4: {largeDieselCH4Factor:F6}, N2O: {largeDieselN2OFactor:F6})";

                string emissionPart = emissionString.Substring(emissionString.IndexOf("Emission Factors:") + "Emission Factors:".Length).Trim();
                emissionPart = emissionPart.Substring(0, emissionPart.IndexOf("kg CO2e")).Trim();
                return emissionPart;
            }
            return "0"; // Return "0" if the label is not found
        }

        //pie chart
        private void UpdatePieChartplot(double homeEmission, double leisureTravelEmission, double commuteTravelEmission, double personalWasteEmission)
        {
            // Create a new PlotModel
            var model = new PlotModel { Title = "Carbon Emission" };

            // Create a new PieSeries
            //var pieSeries = new PieSeries
            /*{
                StrokeThickness = 2.0,
                InsideLabelPosition = 0,
                InsideLabelFormat = string.Empty, // No inside labels
                AngleSpan = 360,
                StartAngle = 0,
            };*/
            // Create a new PieSeries
            var pieSeries = new PieSeries
            {
                StrokeThickness = 2.0,
                InsideLabelPosition = 0.4,
                AngleSpan = 360,
                StartAngle = 0,
                Diameter = 0.7, // Adjust this to make the pie chart smaller
                //InnerDiameter = 0.4, // Adjust this to change the inner diameter
            };
            // Add data points to the PieSeries
            pieSeries.Slices.Add(new PieSlice("Energy", homeEmission) { IsExploded = false, Fill = OxyColors.Blue });
            pieSeries.Slices.Add(new PieSlice("Leisure", leisureTravelEmission) { IsExploded = false, Fill = OxyColors.Green });
            pieSeries.Slices.Add(new PieSlice("Office", commuteTravelEmission) { IsExploded = false, Fill = OxyColors.Red });
            pieSeries.Slices.Add(new PieSlice("Waste", personalWasteEmission) { IsExploded = false, Fill = OxyColors.Purple });

            // Add the series to the model
            model.Series.Add(pieSeries);

            // Assign the PlotModel to the PlotView
            plotView1.Model = model;
            // Adjust the size of the PlotView control if needed

            // Refresh the plot view
            plotView1.InvalidatePlot(true);
        }
        private void updateGlobalLabel(object sender, EventArgs e)
        {
            // Get the text from each label
            double Carbon = 0;
            string kettle = totalKettleEmission;// kettle_op_Total_KWh_label.Text;
            string fan = totalFanEmission;// fan_op_Total_KWh_label.Text;
            string led = totalLedEmission;// led_emission_label.Text;
            string water = totalWaterEmission;//LitersEmissionPerDayLabel.Text;
            string electricheater = totalElectricHeaterEmission;
            string CustomEntryEmission = totalCustomEntryEmission;

            string LeisureTravelCarEmission = totalLeisureTravelCarEmission;
            string lLeisureTravelBikeEmission = totalLeisureTravelBikeEmission;
            string LeisureTravelHotelStayEmission = totalHotelStayEmission;
            string CommuteTravelCarEmission = totalCommuteTravelCarEmission;
            string CommuteTravelTrainEmission = totalCommuteTravelTrainEmission;
            string CommuteTravelBusEmission = totalCommuteTravelBusEmission;
            string WorkHoursEmission = totalWorkHoursEmission;
            string HouseholdResidualWasteEmission = totalHouseholdResidualWasteEmission;
            string OrganicFoodWasteEmission = totalOrganicFoodWasteEmission;
            string OrganicGardenWasteEmission = totalOrganicGardenWasteEmission;

            // Extract the total emission part
            string ledEmissionPart = ExtractEmissionValue(led);
            Debug.WriteLine($"ledEmissionPart: {ledEmissionPart}");

            string fanEmissionPart = ExtractEmissionValue(fan);
            Debug.WriteLine($"fanEmissionPart: {fanEmissionPart}");

            string kettleEmissionPart = ExtractEmissionValue(kettle);
            Debug.WriteLine($"kettleEmissionPart: {kettleEmissionPart}");

            string electricheaterPart = ExtractEmissionValue(electricheater);
            Debug.WriteLine($"electricheaterPart: {electricheaterPart}");

            string customEntryEmissionPart = ExtractEmissionValue(CustomEntryEmission);
            Debug.WriteLine($"electricheaterPart: {customEntryEmissionPart}");

            string waterEmissionPart = ExtractEmissionValue(water);
            Debug.WriteLine($"waterEmissionPart: {waterEmissionPart}");

            string LeisureTravelCarEmissionPart = ExtractEmissionValue(LeisureTravelCarEmission);
            Debug.WriteLine($"LeisureTravelCarEmissionPart: {LeisureTravelCarEmissionPart}");

            string LeisureTravelCarEmissionBikePart = ExtractEmissionValue(lLeisureTravelBikeEmission);
            Debug.WriteLine($"LeisureTravelCarEmissionBikePart: {LeisureTravelCarEmissionBikePart}");

            string LeisureTravelHotelStayEmissionPart = ExtractEmissionValue(LeisureTravelHotelStayEmission);
            Debug.WriteLine($"LeisureTravelHotelStayEmissionPart: {LeisureTravelHotelStayEmissionPart}");

            string CommuteTravelCarEmissionPart = ExtractEmissionValue(CommuteTravelCarEmission);
            Debug.WriteLine($"CommuteTravelCarEmissionPart: {CommuteTravelCarEmissionPart}");

            string CommuteTravelTrainEmissionPart = ExtractEmissionValue(CommuteTravelTrainEmission);
            Debug.WriteLine($"CommuteTravelTrainEmissionPart: {CommuteTravelTrainEmissionPart}");

            string CommuteTravelBusEmissionPart = ExtractEmissionValue(CommuteTravelBusEmission);
            Debug.WriteLine($"CommuteTravelBusEmissionPart: {CommuteTravelBusEmissionPart}");

            string WorkHoursEmissionPart = ExtractEmissionValue(WorkHoursEmission);
            Debug.WriteLine($"WorkHoursEmissionPart: {WorkHoursEmissionPart}");

            string HouseholdResidualWasteEmissionPart = ExtractEmissionValue(HouseholdResidualWasteEmission);
            Debug.WriteLine($"HouseholdResidualWasteEmissionPart: {HouseholdResidualWasteEmissionPart}");

            string OrganicFoodWasteEmissionPart = ExtractEmissionValue(OrganicFoodWasteEmission);
            Debug.WriteLine($"OrganicFoodWasteEmissionPart: {OrganicFoodWasteEmissionPart}");

            string OrganicGardenWasteEmissionPart = ExtractEmissionValue(OrganicGardenWasteEmission);
            Debug.WriteLine($"OrganicFoodWasteEmissionPart: {OrganicGardenWasteEmissionPart}");

            // Convert the extracted parts to doubles
            // Extract and parse the total emission part for daily inputs
            double ledEmission = TryParseEmission(ledEmissionPart);
            double fanEmission = TryParseEmission(fanEmissionPart);
            double kettleEmission = TryParseEmission(kettleEmissionPart);
            double electricHeaterEmission = TryParseEmission(electricheaterPart);
            double waterEmission = TryParseEmission(waterEmissionPart);
            double customEntryEmission = TryParseEmission(customEntryEmissionPart);
            double LeiTravelCarEmission = TryParseEmission(LeisureTravelCarEmissionPart);
            double LeiTravelBikeEmission = TryParseEmission(LeisureTravelCarEmissionBikePart);
            double LeiTravelHotelStayEmission = TryParseEmission(LeisureTravelHotelStayEmissionPart);
            double WorkHrsEmission = TryParseEmission(WorkHoursEmissionPart);

            // Extract and parse the total emission part for Annual inputs
            double CommuTravelCarEmission = TryParseEmission(CommuteTravelCarEmissionPart);
            double CommuTravelTrainEmission = TryParseEmission(CommuteTravelTrainEmissionPart);
            double CommuTravelBusEmission = TryParseEmission(CommuteTravelBusEmissionPart);
            double HousehldResidualWasteEmission = TryParseEmission(HouseholdResidualWasteEmissionPart);
            double OrganicFodWasteEmissionPart = TryParseEmission(OrganicFoodWasteEmissionPart);
            double OrganicGrdnWasteEmissionPart = TryParseEmission(OrganicGardenWasteEmissionPart);

            // Define number of days in a year and working days for commute
            bool isAnnualMode = mode_annual_radioButton.Checked;

            if (isAnnualMode)
            {
                // Convert daily emissions to annual if in annual mode
                ledEmission *= daysInYear;
                fanEmission *= daysInYear;
                kettleEmission *= daysInYear;
                electricHeaterEmission *= daysInYear;
                waterEmission *= daysInYear;
                customEntryEmission *= daysInYear;

                // Use working days for commute emissions
                CommuTravelCarEmission = (CommuTravelCarEmission * 2) *workingDaysInYear;
                CommuTravelTrainEmission = (CommuTravelTrainEmission * 2) * workingDaysInYear;
                CommuTravelBusEmission = (CommuTravelBusEmission * 2) * workingDaysInYear;
                WorkHrsEmission *= workingDaysInYear;
            }
            else
            {
                // Convert annual emissions to daily if in daily mode
                LeiTravelCarEmission /= daysInYear;
                LeiTravelBikeEmission /= daysInYear;
                LeiTravelHotelStayEmission /= daysInYear;

                HousehldResidualWasteEmission /= daysInYear;
                OrganicFodWasteEmissionPart /= daysInYear;
                OrganicGrdnWasteEmissionPart /= daysInYear;
            }

            // Sum the emission values
            double totalEmission = ledEmission + fanEmission + kettleEmission + electricHeaterEmission + waterEmission + customEntryEmission;
            double totalEmissionLeisureTravel = LeiTravelCarEmission + LeiTravelBikeEmission + LeiTravelHotelStayEmission;
            double totalEmissionCommuteTravel = WorkHrsEmission + CommuTravelCarEmission + CommuTravelTrainEmission + CommuTravelBusEmission;
            double totalEmissionPersonalWaste = OrganicGrdnWasteEmissionPart + HousehldResidualWasteEmission + OrganicFodWasteEmissionPart;

            if (isAnnualMode)
            {
                // Convert emissions from kg to tonnes
                double totalEmissionTonnes = totalEmission / 1000;
                double totalEmissionLeisureTravelTonnes = totalEmissionLeisureTravel / 1000;
                double totalEmissionCommuteTravelTonnes = totalEmissionCommuteTravel / 1000;
                double totalEmissionPersonalWasteTonnes = totalEmissionPersonalWaste / 1000;

                // Assign the result to the global label with appropriate formatting
                HomeEnergyGlobalLabel.Text = $"HomeEnergy Emission: {totalEmissionTonnes:F6} tonnes CO2e";
                LeisureEnergyGlobalLabel.Text = $"Leisure Emission: {totalEmissionLeisureTravelTonnes:F6} tonnes CO2e";
                HomeOfficeCommuteEnergyGlobalLabel.Text = $"Homeoffice/Commute Emission: {totalEmissionCommuteTravelTonnes:F6} tonnes CO2e";
                PersonalHouseholdWasteEnergyGlobalLabel.Text = $"Oragnic waste Emission: {totalEmissionPersonalWasteTonnes:F6} tonnes CO2e";

                // Calculate the total carbon emission in tonnes
                Carbon = totalEmissionTonnes + totalEmissionLeisureTravelTonnes + totalEmissionCommuteTravelTonnes + totalEmissionPersonalWasteTonnes;
                CarbonLabel.Text = $"Total Emission: {Carbon:F6} tonnes CO2e";
            }
            else
            {
                // Assign the result to the global label with appropriate formatting for daily mode
                HomeEnergyGlobalLabel.Text = $"HomeEnergy Emission: {totalEmission:F6} Kg CO2e";
                LeisureEnergyGlobalLabel.Text = $"Leisure Emission: {totalEmissionLeisureTravel:F6} Kg CO2e";
                HomeOfficeCommuteEnergyGlobalLabel.Text = $"Homeoffice/Commute Emission: {totalEmissionCommuteTravel:F6} Kg CO2e";
                PersonalHouseholdWasteEnergyGlobalLabel.Text = $"Oragnic waste Emission: {totalEmissionPersonalWaste:F6} Kg CO2e";

                Carbon = totalEmission + totalEmissionLeisureTravel + totalEmissionCommuteTravel + totalEmissionPersonalWaste;
                CarbonLabel.Text = $"Total Emission: {Carbon:F6} kg CO2e";
            }


            //UpdatePieChartplot();
            UpdatePieChartplot(totalEmission, totalEmissionLeisureTravel, totalEmissionCommuteTravel, totalEmissionPersonalWaste);
        }
        private string ExtractEmissionValue(string emissionString)
        {
            if (emissionString.Contains("Total Emission:"))
            {
                string emissionPart = emissionString.Substring(emissionString.IndexOf("Total Emission:") + "Total Emission:".Length).Trim();
                emissionPart = emissionPart.Substring(0, emissionPart.IndexOf("kg CO2e")).Trim();
                return emissionPart;
            }
            return "0"; // Return "0" if the label is not found
        }
        private double TryParseEmission(string emissionPart)
        {
            return double.TryParse(emissionPart, out double emission) ? emission : 0;
        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox18_Click(object sender, EventArgs e)
        {

        }

        private void fuelType_groupBox_Enter(object sender, EventArgs e)
        {

        }

        bool shouldAppend = false;
        private void RecalculateGenerateReport(object sender, EventArgs e)
        {
            shouldAppend = true;

            //Home Energy
            LED_HomeEnergy_Carbon_Calculation(sender, e);
            Fan_HomeEnergy_Carbon_Calculation(sender, e);
            Kettle_HomeEnergy_Carbon_Calculation(sender, e);
            HomeEnergy_CalculateWaterCarbon(sender, e);
            Heater_HomeEnergy_Carbon_Calculation(sender, e);

            //Waste/Organic/Residual/Garden wasts
            OrganicFoodWaste_CalculateCarbon(sender, e);
            OrganicGardenWaste_CalculateCarbon(sender, e);
            HouseholdResidualWaste_CalculateCarbon(sender, e);

            //Leisure Car/Bike
            CarLeisureTravel_CalculateCarCarbon(sender, e);
            BikeLeisureTravel_CalculateBikeCarbon(sender, e);
            LeisureTravel_CalculateHotelRoomCarbon(sender, e);

            //HomeOffice/Commute
            OfficeCommute_CalculateCarbon(sender, e);
            CalculateHomeOfficeCarbon(sender, e);
            // Display all reports in the message box
            //DisplayAllReportsInMessageBox();
            //DisplayAllReportsInPDF();
            //DisplayAllReportsInPDFSharp();
            GeneratePdfFromReports();
            shouldAppend = false;

        }
        private bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true; // The file is locked
            }
            return false; // The file is not locked
        }
        private void DisplayAllReportsInPDFSharp()
        {
            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Created with PDFsharp";

            // Create an empty page
            PdfPage page = document.AddPage();

            // Get an XGraphics object for drawing
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Create a font using a supported typeface and style
            //XFont font = new XFont("Arial", 20, XFontStyle.Normal);
            var myFont = new XFont("Arial", 10, XFontStyleEx.Regular);

            // Draw the text "Hello, World!" in the center of the page
            gfx.DrawString("Hello, World!", myFont, XBrushes.Black,
                new XRect(0, 0, page.Width, page.Height),
                XStringFormats.Center);

            // Save the document...
            const string filename = "HelloWorld.pdf";
            document.Save(filename);

            // ...and start a viewer to display the file
            Process.Start(new ProcessStartInfo(filename) { UseShellExecute = true });
        }
        private const double PageMargin = 40;
        private const double LineHeight = 15;
        private const double PageHeight = 842;  // A4 page height in points (approx.)

        private void GeneratePdfFromReports()
        {
            // Get the application directory path
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Generate a filename with a timestamp
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = Path.Combine(appDirectory, $"EnergyReport_{timestamp}.pdf");

            if (energyReports.Count == 0)
            {
                MessageBox.Show("No data available to generate PDF.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Energy Report";

            // Create a new page
            PdfPage page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            // Get an XGraphics object for drawing
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XTextFormatter tf = new XTextFormatter(gfx);  // Create XTextFormatter for text formatting

            // Set up the font
            var myFont = new XFont("Arial", 10, XFontStyleEx.Regular);
            var boldFont = new XFont("Arial", 10, XFontStyleEx.Bold);

            // Set up initial Y position for text drawing
            double yPosition = 40;
            double maxWidth = page.Width - 2 * PageMargin;
            var groupedReports = energyReports.GroupBy(r => r.Category);

            foreach (var categoryGroup in groupedReports)
            {
                yPosition = CheckForPageBreak(document, ref page, ref gfx, ref tf, yPosition);

                gfx.DrawString($"Category: {categoryGroup.Key}", boldFont, XBrushes.Black, new XRect(40, yPosition, page.Width - 80, page.Height), XStringFormats.TopLeft);
                yPosition += 20;

                gfx.DrawString(new string('-', 20), myFont, XBrushes.Black, new XRect(40, yPosition, page.Width - 80, page.Height), XStringFormats.TopLeft);
                yPosition += 20;

                foreach (var report in categoryGroup)
                {
                    yPosition = CheckForPageBreak(document, ref page, ref gfx, ref tf, yPosition);

                    gfx.DrawString($"Item: {report.Item}", myFont, XBrushes.Black, new XRect(40, yPosition, page.Width - 80, page.Height), XStringFormats.TopLeft);
                    yPosition += 15;

                    gfx.DrawString($"Usage: {report.Usage:F2} {report.Unit}", myFont, XBrushes.Black, new XRect(40, yPosition, page.Width - 80, page.Height), XStringFormats.TopLeft);
                    yPosition += 15;

                    gfx.DrawString($"Average Usage: {report.AverageUsage:F2} {report.Unit}", myFont, XBrushes.Black, new XRect(40, yPosition, page.Width - 80, page.Height), XStringFormats.TopLeft);
                    yPosition += 15;

                    XRect feedbackRect = new XRect(40, yPosition, maxWidth, page.Height);
                    tf.DrawString($"Feedback: {report.Feedback}", myFont, XBrushes.Black, feedbackRect);
                    yPosition += CalculateTextHeight(gfx, report.Feedback, myFont, feedbackRect.Width);
                    yPosition += 10;

                    yPosition = CheckForPageBreak(document, ref page, ref gfx, ref tf, yPosition);

                    XRect improvementTipsRect = new XRect(40, yPosition, maxWidth, page.Height);
                    tf.DrawString($"Improvement Tips: {report.ImprovementTips}", myFont, XBrushes.Black, improvementTipsRect);
                    yPosition += CalculateTextHeight(gfx, report.ImprovementTips, myFont, improvementTipsRect.Width);
                    yPosition += 15; // Add some space after Improvement Tips

                    gfx.DrawString($"YouTube Link: {report.YouTubeLink}", myFont, XBrushes.Black, new XRect(40, yPosition, page.Width - 80, page.Height), XStringFormats.TopLeft);
                    yPosition += 20;
                }

                gfx.DrawString(new string('=', 40), myFont, XBrushes.Black, new XRect(40, yPosition, page.Width - 80, page.Height), XStringFormats.TopLeft);
                yPosition += 30; // Add extra space between categories
            }

            // Save the document to the specified file
            document.Save(filename);

            // Show success message with file path
            MessageBox.Show($"PDF generated successfully: {filename}", "File Created", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Clear the energyReports list to avoid appending old data in the next report
            energyReports.Clear();
        }

        private double CheckForPageBreak(PdfDocument document, ref PdfPage page, ref XGraphics gfx, ref XTextFormatter tf, double yPosition)
        {
            if (yPosition > page.Height - 100) // Adjust 100 as needed for bottom margin
            {
                // Create a new page
                page = document.AddPage();
                page.Size = PdfSharp.PageSize.A4;

                // Get new XGraphics and XTextFormatter objects
                gfx = XGraphics.FromPdfPage(page);
                tf = new XTextFormatter(gfx);

                // Reset Y position for the new page
                yPosition = 40;
            }
            return yPosition;
        }

        private double CalculateTextHeight(XGraphics gfx, string text, XFont font, double maxWidth)
        {
            // Measure the height of the text when wrapped within the maxWidth
            var size = gfx.MeasureString(text, font);
            int lines = (int)Math.Ceiling(size.Width / maxWidth);
            return lines * size.Height;
        }

        private void plotView1_Click(object sender, EventArgs e)
        {

        }

        private void pdf_button_Click(object sender, EventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        /*
        private void GeneratePdfFromReports()
        {
            // Get the application directory path
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Generate a filename with a timestamp
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = Path.Combine(appDirectory, $"EnergyReport_{timestamp}.pdf");

            if (energyReports.Count == 0)
            {
                Console.WriteLine("No reports available to generate PDF.");
                return;
            }

            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Energy Report";

            // Create a new page
            PdfPage page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            // Get an XGraphics object for drawing
            //XGraphics gfx = XGraphics.FromPdfPage(page);
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XTextFormatter tf = new XTextFormatter(gfx);  // Create XTextFormatter for text formatting

            // Set up the font
            var myFont = new XFont("Arial", 10, XFontStyleEx.Regular);
            var boldFont = new XFont("Arial", 10, XFontStyleEx.Bold);

            // Set up initial Y position for text drawing
            //double yPosition = 40;
            //double maxWidth = page.Width - 80;  // Define maxWidth within the context
            double yPosition = PageMargin;
            double maxWidth = page.Width - 2 * PageMargin;
            var groupedReports = energyReports.GroupBy(r => r.Category);

            foreach (var categoryGroup in groupedReports)
            {
                gfx.DrawString($"Category: {categoryGroup.Key}", boldFont, XBrushes.Black, new XRect(40, yPosition, page.Width - 80, page.Height), XStringFormats.TopLeft);
                yPosition += 20;

                gfx.DrawString(new string('-', 20), myFont, XBrushes.Black, new XRect(40, yPosition, page.Width - 80, page.Height), XStringFormats.TopLeft);
                yPosition += 20;

                foreach (var report in categoryGroup)
                {
                    gfx.DrawString($"Item: {report.Item}", myFont, XBrushes.Black, new XRect(40, yPosition, page.Width - 80, page.Height), XStringFormats.TopLeft);
                    yPosition += 15;

                    gfx.DrawString($"Usage: {report.Usage:F2} {report.Unit}", myFont, XBrushes.Black, new XRect(40, yPosition, page.Width - 80, page.Height), XStringFormats.TopLeft);
                    yPosition += 15;

                    gfx.DrawString($"Average Usage: {report.AverageUsage:F2} {report.Unit}", myFont, XBrushes.Black, new XRect(40, yPosition, page.Width - 80, page.Height), XStringFormats.TopLeft);
                    yPosition += 15;

                    // Calculate feedback height and check for overflow
                    XRect feedbackRect = new XRect(40, yPosition, maxWidth, page.Height);
                    tf.DrawString($"Feedback: {report.Feedback}", myFont, XBrushes.Black, feedbackRect);
                    yPosition += CalculateTextHeight(gfx, report.Feedback, myFont, feedbackRect.Width);
                    yPosition += 10;

                    // Handle Improvement Tips
                    XRect improvementTipsRect = new XRect(40, yPosition, maxWidth, page.Height);
                    tf.DrawString($"Improvement Tips: {report.ImprovementTips}", myFont, XBrushes.Black, improvementTipsRect);
                    yPosition += CalculateTextHeight(gfx, report.ImprovementTips, myFont, improvementTipsRect.Width);
                    yPosition += 15; // Add some space after Improvement Tips

                    //gfx.DrawString($"Improvement Tips: {report.ImprovementTips}", myFont, XBrushes.Black, new XRect(40, yPosition, page.Width - 80, page.Height), XStringFormats.TopLeft);
                    //yPosition += 15;

                    gfx.DrawString($"YouTube Link: {report.YouTubeLink}", myFont, XBrushes.Black, new XRect(40, yPosition, page.Width - 80, page.Height), XStringFormats.TopLeft);
                    yPosition += 20;

                }

                gfx.DrawString(new string('=', 40), myFont, XBrushes.Black, new XRect(40, yPosition, page.Width - 80, page.Height), XStringFormats.TopLeft);
                yPosition += 30; // Add extra space between categories
            }

            // Save the document to the specified file
            document.Save(filename);

            // Show success message with file path
            MessageBox.Show($"PDF generated successfully: {filename}", "File Created", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Clear the energyReports list to avoid appending old data in the next report
            energyReports.Clear();
        }
        private double CalculateTextHeight(XGraphics gfx, string text, XFont font, double maxWidth)
        {
            // Measure the height of the text when wrapped within the maxWidth
            var size = gfx.MeasureString(text, font);
            int lines = (int)Math.Ceiling(size.Width / maxWidth);
            return lines * size.Height;
        }
        */
    }
}
