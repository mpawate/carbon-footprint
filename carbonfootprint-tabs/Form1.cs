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
        private string selectedYear = "2024";

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

        string dbPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\conversion_factors.db";

        //Unique functions
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            CheckDatabaseConnection();
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
                if (isHoursLEDErrorSet)
                {
                    errorProvider1.SetError(HoursDay_LED_HomeEnergy_textBox, string.Empty);
                    isHoursLEDErrorSet = false;
                }

                //return;
            }
            else if (!double.TryParse(HoursDay_LED_HomeEnergy_textBox.Text, out double wattHoursNumber) || wattHoursNumber < 0 || wattHoursNumber > 24)
            {
                isValid = false;
                if (!isHoursLEDErrorSet)
                {
                    errorProvider1.SetError(HoursDay_LED_HomeEnergy_textBox, "Please enter a valid number of hours between 0 and 24.");

                    isHoursLEDErrorSet = true;
                }

                EnergyUsage_LED_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_LED_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_LED_HomeEnergy_label.Text = "Feedback"; //Assogn default value

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
                if (isQtyLEDErrorSet)
                {
                    errorProvider1.SetError(Qty_LED_HomeEnergy_textBox, string.Empty);
                    isQtyLEDErrorSet = false;
                }
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
                return;
            }


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
                if (isWattFanErrorSet)
                {
                    errorProvider1.SetError(Watt_Fan_HomeEnergy_textBox, string.Empty);
                    isWattFanErrorSet = false;
                }
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
                if (isHoursFanErrorSet)
                {
                    errorProvider1.SetError(HoursDay_Fan_HomeEnergy_textBox, string.Empty);
                    isHoursFanErrorSet = false;
                }

                //return;
            }
            else if (!double.TryParse(HoursDay_Fan_HomeEnergy_textBox.Text, out double wattHoursNumber) || wattHoursNumber < 0 || wattHoursNumber > 24)
            {
                isValid = false;
                if (!isHoursFanErrorSet)
                {
                    errorProvider1.SetError(HoursDay_Fan_HomeEnergy_textBox, "Please enter a valid number of hours between 0 and 24.");

                    isHoursFanErrorSet = true;
                }

                EnergyUsage_Fan_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Fan_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Fan_HomeEnergy_label.Text = "Feedback"; //Assogn default value

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
                if (isQtyFanErrorSet)
                {
                    errorProvider1.SetError(Qty_Fan_HomeEnergy_textBox, string.Empty);
                    isQtyFanErrorSet = false;
                }
                //return;
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
                return;
            }


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
                if (isWattKettleErrorSet)
                {
                    errorProvider1.SetError(Watt_Kettle_HomeEnergy_textBox, string.Empty);
                    isWattKettleErrorSet = false;
                }
                //return;
            }
            else if (!double.TryParse(Watt_Kettle_HomeEnergy_textBox.Text, out double wattNumber) || wattNumber < 1200 || wattNumber > 1500)
            {
                isValid = false;
                if(!isWattKettleErrorSet)
                {
                    errorProvider1.SetError(Watt_Kettle_HomeEnergy_textBox, "Please enter a valid wattage between 1200 and 1500.");
                    isWattKettleErrorSet = true;
                }
                EnergyUsage_Kettle_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Kettle_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Kettle_HomeEnergy_label.Text = "Feedback"; //Assogn default value

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
                if(isHoursKettleErrorSet)
                {
                    errorProvider1.SetError(HoursDay_Kettle_HomeEnergy_textBox, string.Empty);
                    isHoursKettleErrorSet = false;
                }

                //return;
            }
            else if (!double.TryParse(HoursDay_Kettle_HomeEnergy_textBox.Text, out double wattHoursNumber) || wattHoursNumber < 0 || wattHoursNumber > 24)
            {
                isValid = false;
                if(!isHoursKettleErrorSet)
                {
                    errorProvider1.SetError(HoursDay_Kettle_HomeEnergy_textBox, "Please enter a valid number of hours between .5 and 1.");
                    isHoursKettleErrorSet = true;
                }

                EnergyUsage_Kettle_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Kettle_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Kettle_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                totalKettleEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

            }
            else
            {
                if(isHoursKettleErrorSet)
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
                if(isQtyKettleErrorSet)
                {
                    errorProvider1.SetError(Qty_Kettle_HomeEnergy_textBox, string.Empty);
                    isQtyKettleErrorSet = false;
                }
                //return;
            }
            else if (!double.TryParse(Qty_Kettle_HomeEnergy_textBox.Text, out double wattqty) || wattqty < 1)
            {
                isValid = false;
                if(!isQtyKettleErrorSet)
                {
                    errorProvider1.SetError(Qty_Kettle_HomeEnergy_textBox, "Please enter a valid quantity (at least 1).");
                    isQtyKettleErrorSet = true;
                }
                EnergyUsage_Kettle_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Kettle_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Kettle_HomeEnergy_label.Text = "Feedback"; //Assogn default value

                totalKettleEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

            }
            else
            {
                if(isQtyKettleErrorSet)
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
                return;
            }


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
                if (isWattHeaterErrorSet)
                {
                    errorProvider1.SetError(Watt_Heater_HomeEnergy_textBox, string.Empty);
                    isWattHeaterErrorSet = false;
                }
                //return;
            }
            else if (!double.TryParse(Watt_Heater_HomeEnergy_textBox.Text, out double wattNumber) || wattNumber < 1500 || wattNumber > 1600)
            {
                isValid = false;
                if (!isWattHeaterErrorSet)
                {
                    errorProvider1.SetError(Watt_Heater_HomeEnergy_textBox, "Please enter a valid wattage between 1500 and 1600.");
                    isWattHeaterErrorSet = true;
                }
                EnergyUsage_Heater_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Heater_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Heater_HomeEnergy_label.Text = "Feedback"; //Assogn default value

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
                if (isHoursHeaterErrorSet)
                {
                    errorProvider1.SetError(HoursDay_Heater_HomeEnergy_textBox, string.Empty);
                    isHoursHeaterErrorSet = false;
                }

                //return;
            }
            else if (!double.TryParse(HoursDay_Heater_HomeEnergy_textBox.Text, out double wattHoursNumber) || wattHoursNumber < 1 || wattHoursNumber > 12)
            {
                isValid = false;
                if (!isHoursHeaterErrorSet)
                {
                    errorProvider1.SetError(HoursDay_Heater_HomeEnergy_textBox, "Please enter a valid number of hours between 1 and 12.");

                    isHoursHeaterErrorSet = true;
                }

                EnergyUsage_Heater_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Heater_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Heater_HomeEnergy_label.Text = "Feedback"; //Assogn default value

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
                if (isQtyHeaterErrorSet)
                {
                    errorProvider1.SetError(Qty_Heater_HomeEnergy_textBox, string.Empty);
                    isQtyHeaterErrorSet = false;
                }
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

                totalElectricHeaterEmission = "";
                updateGlobalLabel(this, EventArgs.Empty);

            }
            else
            {
                if (isQtyHeaterErrorSet)
                {
                    errorProvider1.SetError(Qty_Heater_HomeEnergy_textBox, string.Empty);
                    isQtyHeaterErrorSet = false;
                }
                wattQty = wattqty;
            }

            // If validation fails, return
            if (!isValid)
            {
                EnergyUsage_Heater_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_Heater_HomeEnergy_label.Text = "Emission"; // Assogn default value
                Feedback_Heater_HomeEnergy_label.Text = "Feedback"; //Assogn default value
                return;
            }


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
                EnergyUsage_CustomEntry_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_CustomEntry_HomeEnergy_label.Text = "Emission"; // Assogn default value
                if (isWattCustomErrorSet)
                {
                    errorProvider1.SetError(Watt_CustomEntry_HomeEnergy_textBox, string.Empty);
                    isWattCustomErrorSet = false;
                }
                //return;
            }
            else if (!double.TryParse(Watt_CustomEntry_HomeEnergy_textBox.Text, out double wattNumber) || wattNumber < 1500 || wattNumber > 1600)
            {
                isValid = false;
                if (!isWattCustomErrorSet)
                {
                    errorProvider1.SetError(Watt_CustomEntry_HomeEnergy_textBox, "Please enter a valid wattage between 1500 and 1600.");
                    isWattCustomErrorSet = true;
                }
                EnergyUsage_CustomEntry_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_CustomEntry_HomeEnergy_label.Text = "Emission"; // Assogn default value

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
                EnergyUsage_CustomEntry_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_CustomEntry_HomeEnergy_label.Text = "Emission"; // Assogn default value
                if (isHoursCustomErrorSet)
                {
                    errorProvider1.SetError(HoursDay_CustomEntry_HomeEnergy_textBox, string.Empty);
                    isHoursCustomErrorSet = false;
                }

                //return;
            }
            else if (!double.TryParse(HoursDay_CustomEntry_HomeEnergy_textBox.Text, out double wattHoursNumber) || wattHoursNumber < 1 || wattHoursNumber > 12)
            {
                isValid = false;
                if (!isHoursCustomErrorSet)
                {
                    errorProvider1.SetError(HoursDay_CustomEntry_HomeEnergy_textBox, "Please enter a valid number of hours between 1 and 12.");

                    isHoursCustomErrorSet = true;
                }

                EnergyUsage_CustomEntry_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_CustomEntry_HomeEnergy_label.Text = "Emission"; // Assogn default value

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
                EnergyUsage_CustomEntry_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_CustomEntry_HomeEnergy_label.Text = "Emission"; // Assogn default value
                if (isQtyCustomErrorSet)
                {
                    errorProvider1.SetError(Qty_CustomEntry_HomeEnergy_textBox, string.Empty);
                    isQtyCustomErrorSet = false;
                }
                //return;
            }
            else if (!double.TryParse(Qty_CustomEntry_HomeEnergy_textBox.Text, out double wattqty) || wattqty < 1)
            {
                isValid = false;
                if (!isQtyCustomErrorSet)
                {
                    errorProvider1.SetError(Qty_CustomEntry_HomeEnergy_textBox, "Please enter a valid quantity (at least 1).");
                    isQtyCustomErrorSet = true;
                }
                EnergyUsage_CustomEntry_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_CustomEntry_HomeEnergy_label.Text = "Emission"; // Assogn default value

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
                EnergyUsage_CustomEntry_HomeEnergy_label.Text = "kWh"; // Assogn default value
                Emission_CustomEntry_HomeEnergy_label.Text = "Emission"; // Assogn default value
                return;
            }


            // Perform the calculation in watts
            double totalWatts = wattResult * wattHoursResult * wattQty;
            // Convert to kilowatts (kW)
            double totalKilowatts = totalWatts / 1000;

            EnergyUsage_CustomEntry_HomeEnergy_label.Text = $"Energy: {totalWatts} W / {totalKilowatts} kWh";
            totalCustomEntryEmission = CalculateTotalCarbonEmission(totalKilowatts);
            Emission_CustomEntry_HomeEnergy_label.Text = $"Emission: {ExtractEmissionValue(totalCustomEntryEmission):F6} kg CO2e";
            updateGlobalLabel(this, EventArgs.Empty);

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
                HomeEnergyGlobalLabel.Text = $"Total Emission: {totalEmissionTonnes:F2} tonnes CO2e";
                LeisureEnergyGlobalLabel.Text = $"Total Emission: {totalEmissionLeisureTravelTonnes:F2} tonnes CO2e";
                HomeOfficeCommuteEnergyGlobalLabel.Text = $"Total Emission: {totalEmissionCommuteTravelTonnes:F2} tonnes CO2e";
                PersonalHouseholdWasteEnergyGlobalLabel.Text = $"Total Emission: {totalEmissionPersonalWasteTonnes:F2} tonnes CO2e";

                // Calculate the total carbon emission in tonnes
                Carbon = totalEmissionTonnes + totalEmissionLeisureTravelTonnes + totalEmissionCommuteTravelTonnes + totalEmissionPersonalWasteTonnes;
                CarbonLabel.Text = $"Total Emission: {Carbon:F2} tonnes CO2e";
            }
            else
            {
                // Assign the result to the global label with appropriate formatting for daily mode
                HomeEnergyGlobalLabel.Text = $"Total Emission: {totalEmission:F2} kg CO2e";
                LeisureEnergyGlobalLabel.Text = $"Total Emission: {totalEmissionLeisureTravel:F2} kg CO2e";
                HomeOfficeCommuteEnergyGlobalLabel.Text = $"Total Emission: {totalEmissionCommuteTravel:F2} kg CO2e";
                PersonalHouseholdWasteEnergyGlobalLabel.Text = $"Total Emission: {totalEmissionPersonalWaste:F2} kg CO2e";

                Carbon = totalEmission + totalEmissionLeisureTravel + totalEmissionCommuteTravel + totalEmissionPersonalWaste;
                CarbonLabel.Text = $"Total Emission: {Carbon:F2} kg CO2e";
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

    }
}
