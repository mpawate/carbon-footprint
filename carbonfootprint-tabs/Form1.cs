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
            //OrganicWaste_CalcuateCarbon(sender, e);
            //HouseholdResidualWaste_CalculateCarbon(sender, e);
            //OrganicFoodWaste_CalculateCarbon(sender, e);
            //HomeOffice_CalculateCarbon(sender, e);
            //LeisureTravel_CalculateMotorBikeCarbon(sender, e);
            //LeisureTravel_CalculateCarCarbon(sender, e);
            //LeisureTravel_CalculateMotorHotelCarbon(sender, e);
            //LeisureTravel_CalculateHomeOfficeCarbon(sender, e);
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
            //using (SQLiteConnection connection = new SQLiteConnection(connectionString))


            try
            {
                // Replace with your actual database connection check logic
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
                database_status_button.BackColor = Color.Green;
                database_status_button.ForeColor = Color.White; // Optional: To make the text readable
            }
            else
            {
                database_status_button.Text = "DB Disconnected";
                database_status_button.BackColor = Color.Red;
                database_status_button.ForeColor = Color.White; // Optional: To make the text readable
            }
        }
        private void ExitApp_button_Click(object sender, EventArgs e)
        {
            Application.Exit();
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
                    errorProvider1.SetError(AvgLitersDaily_WaterSupply_HomeEnergy_textbox, string.Empty);
                    isWattWaterErrorSet = false;
                }
            }
            else if (!double.TryParse(AvgLitersDaily_WaterSupply_HomeEnergy_textbox.Text, out waterConsumptionLitersPerPerson) || waterConsumptionLitersPerPerson < 0)
            {
                isValid = false;
                if (!isWattWaterErrorSet)
                {
                    errorProvider1.SetError(AvgLitersDaily_WaterSupply_HomeEnergy_textbox, "Please enter a valid water consumption value in liters per person. Ex: 142 liters per day");
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
                    errorProvider1.SetError(AvgLitersDaily_WaterSupply_HomeEnergy_textbox, string.Empty);
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
                    errorProvider1.SetError(NumberOfPersons_WaterSupply_HomeEnergy_textBox, string.Empty);
                    isNumnerPersonWaterErrorSet = false;
                }
            }
            else if (!double.TryParse(NumberOfPersons_WaterSupply_HomeEnergy_textBox.Text, out numPersons) || numPersons <= 0)
            {
                isValid = false;
                if (!isNumnerPersonWaterErrorSet)
                {
                    errorProvider1.SetError(NumberOfPersons_WaterSupply_HomeEnergy_textBox, "Please enter a valid number of persons (at least 1).");
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
                    errorProvider1.SetError(NumberOfPersons_WaterSupply_HomeEnergy_textBox, string.Empty);
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
                double averageWaterConsumptionPerPerson = 150; // Average water consumption in liters per person per day
                double dailyWaterConsumption = waterConsumptionLitersPerPerson * numPersons; // User's input for daily water consumption

                // Calculate the average daily water consumption
                double averageDailyWaterConsumption = averageWaterConsumptionPerPerson * numPersons;

                if (dailyWaterConsumption > averageDailyWaterConsumption)
                {
                    Feedback_WaterSupply_HomeEnergy_label.Text = $"Feedback: Your daily water usage of {dailyWaterConsumption} liters for {numPersons} persons is higher than the average of {averageDailyWaterConsumption} liters.";
                }
                else
                {
                    Feedback_WaterSupply_HomeEnergy_label.Text = $"Feedback: Your daily water usage of {dailyWaterConsumption} liters for {numPersons} persons is within the average range of {averageDailyWaterConsumption} liters.";
                }

                // Update the picture box and label based on the user's performance
                UpdateWaterSupplyUsageBadge(dailyWaterConsumption, averageDailyWaterConsumption);

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
            // Show detailed help message
            MessageBox.Show(
                "Daily Usage Data:\n\n" +
                "1. Please enter a valid water consumption value in liters per person. Ex: 142 liters per day\n" +
                "2. Please enter a valid number of persons (at least 1).",
                "Help Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

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
                    errorProvider1.SetError(Watt_LED_HomeEnergy_textBox, string.Empty);
                    isWattLEDErrorSet = false;
                }
                //return;
            }
            else if (!double.TryParse(Watt_LED_HomeEnergy_textBox.Text, out double wattNumber) || wattNumber < 5 || wattNumber > 100)
            {
                isValid = false;
                if (!isWattLEDErrorSet)
                {
                    errorProvider1.SetError(Watt_LED_HomeEnergy_textBox, "Please enter a valid wattage between 5 and 100.");
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
                    errorProvider1.SetError(Watt_LED_HomeEnergy_textBox, string.Empty);
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
                    errorProvider1.SetError(HoursDay_LED_HomeEnergy_textBox, string.Empty);
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
                    errorProvider1.SetError(HoursDay_LED_HomeEnergy_textBox, "Please enter a valid number of hours between 1 and 24.");

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
                    errorProvider1.SetError(HoursDay_LED_HomeEnergy_textBox, string.Empty);
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
                    errorProvider1.SetError(Qty_LED_HomeEnergy_textBox, string.Empty);
                    isQtyLEDErrorSet = false;
                }
                totalLedEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                //return;
            }
            else if (!double.TryParse(Qty_LED_HomeEnergy_textBox.Text, out double wattqty) || wattqty < 1)
            {
                isValid = false;
                if (!isQtyLEDErrorSet)
                {
                    errorProvider1.SetError(Qty_LED_HomeEnergy_textBox, "Please enter a valid quantity (at least 1).");
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
                    errorProvider1.SetError(Qty_LED_HomeEnergy_textBox, string.Empty);
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
                double averageDailyUsage = averageUsageHours * averageWattage;
                double userDailyUsage = wattHoursResult * wattResult; // User's input for daily usage

                if (userDailyUsage > averageDailyUsage)
                {
                    Feedback_LED_HomeEnergy_label.Text = $"Feedback: Your usage of {dailyUsageHours} hours/day with {wattResult} watts is higher than the average of {averageUsageHours} hours/day with {averageWattage} watts.";
                }
                else
                {
                    Feedback_LED_HomeEnergy_label.Text = $"Feedback: Your usage of {dailyUsageHours} hours/day with {wattResult} watts is within the average range of {averageUsageHours} hours/day with {averageWattage} watts.";
                }

                UpdateLEDUsageBadge(userDailyUsage, averageDailyUsage);
            }
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
            // Show detailed help message
            MessageBox.Show(
                "Daily Usage Data:\n\n" +
                "1. Enter the power consumption of the LED in watts (W). E.g., 40\n" +
                "2. Enter the number of LED units used. E.g., 5\n" +
                "3. Enter the number of hours the LED is used per day. E.g., 10",
                "Help Information",
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
                    errorProvider1.SetError(Watt_Fan_HomeEnergy_textBox, string.Empty);
                    isWattFanErrorSet = false;
                }
                totalFanEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                //return;
            }
            else if (!double.TryParse(Watt_Fan_HomeEnergy_textBox.Text, out double wattNumber) || wattNumber < 5 || wattNumber > 100)
            {
                isValid = false;
                if (!isWattFanErrorSet)
                {
                    errorProvider1.SetError(Watt_Fan_HomeEnergy_textBox, "Please enter a valid wattage between 5 and 100.");
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
                    errorProvider1.SetError(Watt_Fan_HomeEnergy_textBox, string.Empty);
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
                    errorProvider1.SetError(HoursDay_Fan_HomeEnergy_textBox, string.Empty);
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
                    errorProvider1.SetError(HoursDay_Fan_HomeEnergy_textBox, "Please enter a valid number of hours between 1 and 24.");

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
                    errorProvider1.SetError(HoursDay_Fan_HomeEnergy_textBox, string.Empty);
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
                    errorProvider1.SetError(Qty_Fan_HomeEnergy_textBox, string.Empty);
                    isQtyFanErrorSet = false;
                }
                //return;
                totalFanEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

            }
            else if (!double.TryParse(Qty_Fan_HomeEnergy_textBox.Text, out double wattqty) || wattqty < 1)
            {
                isValid = false;
                if (!isQtyFanErrorSet)
                {
                    errorProvider1.SetError(Qty_Fan_HomeEnergy_textBox, "Please enter a valid quantity (at least 1).");
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
                    errorProvider1.SetError(Qty_Fan_HomeEnergy_textBox, string.Empty);
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
                double averageUsageHours = 8; // Average usage in hours per day
                double averageWattage = 12; // Average wattage in watts
                double dailyUsageHours = wattHoursResult; // User's input for usage hours

                // Calculate the average daily energy consumption in watts
                double averageDailyUsage = averageUsageHours * averageWattage;
                double userDailyUsage = wattHoursResult * wattResult; // User's input for daily usage

                if (userDailyUsage > averageDailyUsage)
                {
                    Feedback_Fan_HomeEnergy_label.Text = $"Feedback: Your usage of {dailyUsageHours} hours/day with {wattResult} watts is higher than the average of {averageUsageHours} hours/day with {averageWattage} watts.";
                }
                else
                {
                    Feedback_Fan_HomeEnergy_label.Text = $"Feedback: Your usage of {dailyUsageHours} hours/day with {wattResult} watts is within the average range of {averageUsageHours} hours/day with {averageWattage} watts.";
                }

                UpdateFanUsageBadge(userDailyUsage, averageDailyUsage);
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
            // Show detailed help message
            MessageBox.Show(
                "Daily Usage Data:\n\n" +
                "1. Enter the power consumption of the Fan in watts (W). E.g., 40\n" +
                "2. Enter the number of Fans unit used. E.g., 1\n" +
                "3. Enter the number of hours the Fan is used per day. E.g., 10",
                "Help Information",
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
                    errorProvider1.SetError(Watt_Kettle_HomeEnergy_textBox, string.Empty);
                    isWattKettleErrorSet = false;
                }
                totalKettleEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                //return;
            }
            else if (!double.TryParse(Watt_Kettle_HomeEnergy_textBox.Text, out double wattNumber) || wattNumber < 1300 || wattNumber > 1500)
            {
                isValid = false;
                if (!isWattKettleErrorSet)
                {
                    errorProvider1.SetError(Watt_Kettle_HomeEnergy_textBox, "Please enter a valid wattage between 1300 and 1500.");
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
                    errorProvider1.SetError(Watt_Kettle_HomeEnergy_textBox, string.Empty);
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
                    errorProvider1.SetError(HoursDay_Kettle_HomeEnergy_textBox, string.Empty);
                    isHoursKettleErrorSet = false;
                }
                totalKettleEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                //return;
            }
            else if (!double.TryParse(HoursDay_Kettle_HomeEnergy_textBox.Text, out double wattHoursNumber) || wattHoursNumber < 1 || wattHoursNumber > 2)
            {
                isValid = false;
                if (!isHoursKettleErrorSet)
                {
                    errorProvider1.SetError(HoursDay_Kettle_HomeEnergy_textBox, "Please enter a valid number of hours between 1 and 2.");
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
                    errorProvider1.SetError(HoursDay_Kettle_HomeEnergy_textBox, string.Empty);
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
                    errorProvider1.SetError(Qty_Kettle_HomeEnergy_textBox, string.Empty);
                    isQtyKettleErrorSet = false;
                }
                totalKettleEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                //return;
            }
            else if (!double.TryParse(Qty_Kettle_HomeEnergy_textBox.Text, out double wattqty) || wattqty < 1)
            {
                isValid = false;
                if (!isQtyKettleErrorSet)
                {
                    errorProvider1.SetError(Qty_Kettle_HomeEnergy_textBox, "Please enter a valid quantity (at least 1).");
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
                    errorProvider1.SetError(Qty_Kettle_HomeEnergy_textBox, string.Empty);
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
                double averageUsageHours = 8; // Average usage in hours per day
                double averageWattage = 12; // Average wattage in watts
                double dailyUsageHours = wattHoursResult; // User's input for usage hours

                // Calculate the average daily energy consumption in watts
                double averageDailyUsage = averageUsageHours * averageWattage;
                double userDailyUsage = wattHoursResult * wattResult; // User's input for daily usage

                if (userDailyUsage > averageDailyUsage)
                {
                    Feedback_Kettle_HomeEnergy_label.Text = $"Feedback: Your usage of {dailyUsageHours} hours/day with {wattResult} watts is higher than the average of {averageUsageHours} hours/day with {averageWattage} watts.";
                }
                else
                {
                    Feedback_Kettle_HomeEnergy_label.Text = $"Feedback: Your usage of {dailyUsageHours} hours/day with {wattResult} watts is within the average range of {averageUsageHours} hours/day with {averageWattage} watts.";
                }

                UpdateKettleUsageBadge(userDailyUsage, averageDailyUsage);
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
            // Show detailed help message
            MessageBox.Show(
                "Daily Usage Data:\n\n" +
                "1. Enter the power consumption of the Kettle in watts (W). E.g., 1300\n" +
                "2. Enter the number of Kettle unit used. E.g., 1\n" +
                "3. Enter the number of hours the Kettle is used per day. E.g., 1",
                "Help Information",
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
                    errorProvider1.SetError(Watt_Heater_HomeEnergy_textBox, string.Empty);
                    isWattHeaterErrorSet = false;
                }
                totalElectricHeaterEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                //return;
            }
            else if (!double.TryParse(Watt_Heater_HomeEnergy_textBox.Text, out double wattNumber) || wattNumber < 1300 || wattNumber > 1500)
            {
                isValid = false;
                if (!isWattHeaterErrorSet)
                {
                    errorProvider1.SetError(Watt_Heater_HomeEnergy_textBox, "Please enter a valid wattage between 1300 and 1500.");
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
                    errorProvider1.SetError(Watt_Heater_HomeEnergy_textBox, string.Empty);
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
                    errorProvider1.SetError(HoursDay_Heater_HomeEnergy_textBox, string.Empty);
                    isHoursHeaterErrorSet = false;
                }
                totalElectricHeaterEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                //return;
            }
            else if (!double.TryParse(HoursDay_Heater_HomeEnergy_textBox.Text, out double wattHoursNumber) || wattHoursNumber < 1 || wattHoursNumber > 8)
            {
                isValid = false;
                if (!isHoursHeaterErrorSet)
                {
                    errorProvider1.SetError(HoursDay_Heater_HomeEnergy_textBox, "Please enter a valid number of hours between 1 and 8.");
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
                    errorProvider1.SetError(HoursDay_Heater_HomeEnergy_textBox, string.Empty);
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
                    errorProvider1.SetError(Qty_Heater_HomeEnergy_textBox, string.Empty);
                    isQtyHeaterErrorSet = false;
                }
                totalElectricHeaterEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

                //return;
            }
            else if (!double.TryParse(Qty_Heater_HomeEnergy_textBox.Text, out double wattqty) || wattqty < 1)
            {
                isValid = false;
                if (!isQtyHeaterErrorSet)
                {
                    errorProvider1.SetError(Qty_Heater_HomeEnergy_textBox, "Please enter a valid quantity (at least 1).");
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
                    errorProvider1.SetError(Qty_Heater_HomeEnergy_textBox, string.Empty);
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
                double averageUsageHours = 8; // Average usage in hours per day
                double averageWattage = 12; // Average wattage in watts
                double dailyUsageHours = wattHoursResult; // User's input for usage hours

                // Calculate the average daily energy consumption in watts
                double averageDailyUsage = averageUsageHours * averageWattage;
                double userDailyUsage = wattHoursResult * wattResult; // User's input for daily usage

                if (userDailyUsage > averageDailyUsage)
                {
                    Feedback_Heater_HomeEnergy_label.Text = $"Feedback: Your usage of {dailyUsageHours} hours/day with {wattResult} watts is higher than the average of {averageUsageHours} hours/day with {averageWattage} watts.";
                }
                else
                {
                    Feedback_Heater_HomeEnergy_label.Text = $"Feedback: Your usage of {dailyUsageHours} hours/day with {wattResult} watts is within the average range of {averageUsageHours} hours/day with {averageWattage} watts.";
                }

                UpdateHeaterUsageBadge(userDailyUsage, averageDailyUsage);
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
            // Show detailed help message
            MessageBox.Show(
                "Daily Usage Data:\n\n" +
                "1. Enter the power consumption of the Space Heater in watts (W). E.g., 1500\n" +
                "2. Enter the number of Space Heater unit used. E.g., 1\n" +
                "3. Enter the number of hours the Space Heater is used per day. E.g., 5",
                "Help Information",
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
                    errorProvider1.SetError(Watt_CustomEntry_HomeEnergy_textBox, string.Empty);
                    isWattCustomErrorSet = false;
                }
            }
            else if (!double.TryParse(Watt_CustomEntry_HomeEnergy_textBox.Text, out double wattNumber) || wattNumber < 1 || wattNumber > 100)
            {
                isValid = false;
                if (!isWattCustomErrorSet)
                {
                    errorProvider1.SetError(Watt_CustomEntry_HomeEnergy_textBox, "Please enter a valid wattage between 1 and 100.");
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
                    errorProvider1.SetError(Watt_CustomEntry_HomeEnergy_textBox, string.Empty);
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
                    errorProvider1.SetError(HoursDay_CustomEntry_HomeEnergy_textBox, string.Empty);
                    isHoursCustomErrorSet = false;
                }
            }
            else if (!double.TryParse(HoursDay_CustomEntry_HomeEnergy_textBox.Text, out double wattHoursNumber) || wattHoursNumber < 1 || wattHoursNumber > 24)
            {
                isValid = false;
                if (!isHoursCustomErrorSet)
                {
                    errorProvider1.SetError(HoursDay_CustomEntry_HomeEnergy_textBox, "Please enter a valid number of hours between 1 and 24.");
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
                    errorProvider1.SetError(HoursDay_CustomEntry_HomeEnergy_textBox, string.Empty);
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
                    errorProvider1.SetError(Qty_CustomEntry_HomeEnergy_textBox, string.Empty);
                    isQtyCustomErrorSet = false;
                }
            }
            else if (!double.TryParse(Qty_CustomEntry_HomeEnergy_textBox.Text, out double wattqty) || wattqty < 1)
            {
                isValid = false;
                if (!isQtyCustomErrorSet)
                {
                    errorProvider1.SetError(Qty_CustomEntry_HomeEnergy_textBox, "Please enter a valid quantity (at least 1).");
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
                    errorProvider1.SetError(Qty_CustomEntry_HomeEnergy_textBox, string.Empty);
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

            // Define number of days in a year and working days for commute
            int daysInYear = 365;
            int workingDaysInYear = 260;

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

                //LeiTravelCarEmission *= daysInYear;
                //LeiTravelBikeEmission *= daysInYear;
                //LeiTravelHotelStayEmission *= daysInYear;
                WorkHrsEmission *= workingDaysInYear;

                // Use working days for commute emissions
                CommuTravelCarEmission *= workingDaysInYear;
                CommuTravelTrainEmission *= workingDaysInYear;
                CommuTravelBusEmission *= workingDaysInYear;
            }
            else
            {
                // Convert annual emissions to daily if in daily mode
                //CommuTravelCarEmission /= workingDaysInYear;
                //CommuTravelTrainEmission /= workingDaysInYear;
                //CommuTravelBusEmission /= workingDaysInYear;
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
                HomeEnergyGlobalLabel.Text = $"Total Emission: {totalEmissionTonnes:F6} tonnes CO2e";
                LeisureEnergyGlobalLabel.Text = $"Total Emission: {totalEmissionLeisureTravelTonnes:F6} tonnes CO2e";
                HomeOfficeCommuteEnergyGlobalLabel.Text = $"Total Emission: {totalEmissionCommuteTravelTonnes:F6} tonnes CO2e";
                PersonalHouseholdWasteEnergyGlobalLabel.Text = $"Total Emission: {totalEmissionPersonalWasteTonnes:F6} tonnes CO2e";

                // Calculate the total carbon emission in tonnes
                Carbon = totalEmissionTonnes + totalEmissionLeisureTravelTonnes + totalEmissionCommuteTravelTonnes + totalEmissionPersonalWasteTonnes;
                CarbonLabel.Text = $"Total Emission: {Carbon:F6} tonnes CO2e";
            }
            else
            {
                // Assign the result to the global label with appropriate formatting for daily mode
                HomeEnergyGlobalLabel.Text = $"Total Emission: {totalEmission:F6} kg CO2e";
                LeisureEnergyGlobalLabel.Text = $"Total Emission: {totalEmissionLeisureTravel:F6} kg CO2e";
                HomeOfficeCommuteEnergyGlobalLabel.Text = $"Total Emission: {totalEmissionCommuteTravel:F6} kg CO2e";
                PersonalHouseholdWasteEnergyGlobalLabel.Text = $"Total Emission: {totalEmissionPersonalWaste:F6} kg CO2e";

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
    }
}
