using System;
using System.Globalization;
using System.Windows.Forms;

namespace Maglinte_BSIT201NS_Activity3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // enable automatic scrollbars when controls exceed the client area
            this.AutoScroll = true;

            // ensure buttons are wired even if designer wiring is missing
            button1.Click -= button1_Click; // safe-remove then add to avoid duplicate handlers
            button1.Click += button1_Click;

            button2.Click -= button2_Click;
            button2.Click += button2_Click;

            button3.Click -= button3_Click;
            button3.Click += button3_Click;

            button4.Click -= button4_Click;
            button4.Click += button4_Click;
        }

        // Form load: populate simple dropdowns for immediate use
        private void Form1_Load(object sender, EventArgs e)
        {
                cmbProgram.Items.Clear();
            cmbProgram.Items.AddRange(new object[] { "BSIT", "BSCS", "BSBA" });

            cmbYearLevel.Items.Clear();
            cmbYearLevel.Items.AddRange(new object[] { "1", "2", "3", "4" });

            cmbScholar.Items.Clear();
            cmbScholar.Items.AddRange(new object[] { "None", "Partial", "Full" });

            cmbMode.Items.Clear();
            cmbMode.Items.AddRange(new object[] { "R", "O" }); // R=Regular, O=Online

            dtpDateEnrolled.Value = DateTime.Today;
        }

        // Compute fees: calculate credit units per row, total credits, tuition and totals
        private void button1_Click(object sender, EventArgs e)
        {
            ComputeFees();
        }

        private void ComputeFees()
        {
            // Maintain existing per-row credit calculation (lecture + lab -> credit)
            TextBox[] lecBoxes = { textBox19, textBox20, textBox21, textBox22, textBox23, textBox24 };
            TextBox[] labBoxes = { textBox25, textBox26, textBox27, textBox28, textBox29, textBox30 };
            TextBox[] credBoxes = { textBox31, textBox32, textBox33, textBox34, textBox35, textBox36 };

            decimal totalCred = 0m;
            decimal totalLectureUnits = 0m;
            decimal totalLabUnits = 0m;

            for (int i = 0; i < 6; i++)
            {
                decimal lec = ParseDecimalSafe(lecBoxes[i].Text);
                decimal lab = ParseDecimalSafe(labBoxes[i].Text);

                decimal cred = lec + lab;
                credBoxes[i].Text = cred > 0 ? cred.ToString("0.##", CultureInfo.InvariantCulture) : string.Empty;

                totalCred += cred;
                totalLectureUnits += lec;
                totalLabUnits += lab;
            }

            // Show total credit units (keeps previous behavior)
            textBox55.Text = totalCred.ToString("0.##", CultureInfo.InvariantCulture);

            //
            // Business rules from supplied reference:
            // - Total tuition fee = total lecture units * lecture fee (per unit)
            // - Computer laboratory fee = total laboratory units * laboratory fee (per unit)
            // - Total other fees = computer laboratory fee + CISCO lab fee + SAP fee + exam booklet fee
            // - Miscellaneous fees = total other fees
            // - Total tuition and fees = total tuition fee + miscellaneous fees
            //

            // Lecture fee per unit: try to read from textBox56; fallback to default 1000.00
            decimal lectureFeePerUnit;
            if (!TryParseDecimal(textBox56.Text, out lectureFeePerUnit) || lectureFeePerUnit <= 0m)
            {
                lectureFeePerUnit = 1000.00m;
            }

            // Laboratory fee per unit: try to read from textBox57 if user provided; otherwise default 0 (we'll permit computing from com lab box if present)
            decimal labFeePerUnit;
            bool labFeeProvided = TryParseDecimal(textBox57.Text, out labFeePerUnit) && labFeePerUnit > 0m;

            // Compute total tuition fee (lecture-based)
            decimal totalTuitionFee = totalLectureUnits * lectureFeePerUnit;
            textBox63.Text = totalTuitionFee.ToString("0.00", CultureInfo.InvariantCulture); // Total Tuition Fees output

            // Compute computer laboratory fee:
            // If user already entered a computer lab fee into textBox58, respect that value.
            // Otherwise compute as totalLabUnits * labFeePerUnit if labFeePerUnit provided,
            // otherwise use default lab per-unit 500.00 and compute.
            decimal comLabFee = ParseDecimalSafe(textBox58.Text);
            if (comLabFee <= 0m)
            {
                if (!labFeeProvided)
                {
                    labFeePerUnit = 500.00m; // sensible default
                }

                comLabFee = totalLabUnits * labFeePerUnit;
                // reflect computed computer lab fee in UI
                textBox58.Text = comLabFee > 0m ? comLabFee.ToString("0.00", CultureInfo.InvariantCulture) : string.Empty;
            }

            // Read the other components (SAP, CISCO, Exam)
            decimal sapFee = ParseDecimalSafe(textBox59.Text);
            decimal ciscoFee = ParseDecimalSafe(textBox60.Text);
            decimal examBookletFee = ParseDecimalSafe(textBox61.Text);

            // Total other fees (and miscellaneous fees per spec)
            decimal totalOtherFees = comLabFee + sapFee + ciscoFee + examBookletFee;
            textBox62.Text = totalOtherFees.ToString("0.00", CultureInfo.InvariantCulture); // Total Oth. Fee (misc)

            // According to the spec miscellaneous fees = total other fees
            decimal miscellaneousFees = totalOtherFees;

            // Total tuition and fees
            decimal totalTuitionAndFees = totalTuitionFee + miscellaneousFees;

            // Place totals into the UI (Amount Due / Grand Total boxes)
            textBox70.Text = totalTuitionAndFees.ToString("0.00", CultureInfo.InvariantCulture); // Amount Due
            textBox71.Text = totalTuitionAndFees.ToString("0.00", CultureInfo.InvariantCulture); // Grand Total

            // Keep previous behavior of informing the user
            MessageBox.Show("Fees computed.", "Compute", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Exit button
        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        // Clear student information
        private void button3_Click(object sender, EventArgs e)
        {
            // Clear student fields
            txtStudentName.Text = string.Empty;
            txtStudentNo.Text = string.Empty;
            cmbProgram.SelectedIndex = -1;
            cmbYearLevel.SelectedIndex = -1;
            cmbScholar.SelectedIndex = -1;
            cmbMode.SelectedIndex = -1;
            dtpDateEnrolled.Value = DateTime.Today;

            // If you want to also clear computed totals related to the student, uncomment:
            // textBox70.Text = string.Empty;
            // textBox71.Text = string.Empty;
        }

        // Clear schedule of courses (clears schedule rows and related totals)
        private void button4_Click(object sender, EventArgs e)
        {
            // Course-related boxes by columns/rows
            TextBox[] courseCodeBoxes = { textBox2, textBox1, textBox3, textBox4, textBox5, textBox6 };
            TextBox[] sectionBoxes = { textBox7, textBox8, textBox9, textBox10, textBox11, textBox12 };
            TextBox[] descBoxes = { textBox13, textBox14, textBox15, textBox16, textBox17, textBox18 };
            TextBox[] lecBoxes = { textBox19, textBox20, textBox21, textBox22, textBox23, textBox24 };
            TextBox[] labBoxes = { textBox25, textBox26, textBox27, textBox28, textBox29, textBox30 };
            TextBox[] credBoxes = { textBox31, textBox32, textBox33, textBox34, textBox35, textBox36 };
            TextBox[] timeBoxes = { textBox37, textBox38, textBox39, textBox40, textBox41, textBox42 };
            TextBox[] dayBoxes = { textBox43, textBox44, textBox45, textBox46, textBox47, textBox48 };
            TextBox[] roomBoxes = { textBox49, textBox50, textBox51, textBox52, textBox53, textBox54 };

            ClearTextBoxes(courseCodeBoxes);
            ClearTextBoxes(sectionBoxes);
            ClearTextBoxes(descBoxes);
            ClearTextBoxes(lecBoxes);
            ClearTextBoxes(labBoxes);
            ClearTextBoxes(credBoxes);
            ClearTextBoxes(timeBoxes);
            ClearTextBoxes(dayBoxes);
            ClearTextBoxes(roomBoxes);

            // Reset totals that are derived from the schedule
            textBox55.Text = "0";      // Total Cred. Units
            textBox63.Text = "0.00";   // Total Tuition Fees
            textBox70.Text = "0.00";   // Amount Due
            textBox71.Text = "0.00";   // Grand Total

            // Note: miscellaneous and other fees (textBox56/textBox57/textBox62) are left intact.
            // If you want those cleared too, uncomment the following:
            // textBox56.Text = string.Empty;
            // textBox57.Text = string.Empty;
            // textBox62.Text = string.Empty;
            // textBox72.Text = string.Empty; // discount
        }

        #region Helpers

        private static decimal ParseDecimalSafe(string s)
        {
            decimal value;
            if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
                return value;
            if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.CurrentCulture, out value))
                return value;
            return 0m;
        }

        private static bool TryParseDecimal(string s, out decimal value)
        {
            if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
                return true;
            if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.CurrentCulture, out value))
                return true;
            value = 0m;
            return false;
        }

        private static void ClearTextBoxes(TextBox[] boxes)
        {
            foreach (var tb in boxes)
            {
                if (tb == null) continue;
                tb.Text = string.Empty;
            }
        }

        #endregion

        //
        // Keep empty placeholder handlers so designer event wiring remains valid.
        //
        private void label7_Click(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void textBox2_TextChanged(object sender, EventArgs e) { }
        private void textBox4_TextChanged(object sender, EventArgs e) { }
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e) { }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e) { }
        private void label6_Click(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void groupBox1_Enter(object sender, EventArgs e) { }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void textBox1_TextChanged_1(object sender, EventArgs e) { }
        private void label8_Click(object sender, EventArgs e) { }
        private void label8_Click_1(object sender, EventArgs e) { }
        private void textBox1_TextChanged_2(object sender, EventArgs e) { }
        private void label10_Click(object sender, EventArgs e) { }
        private void textBox2_TextChanged_1(object sender, EventArgs e) { }
        private void textBox1_TextChanged_3(object sender, EventArgs e) { }
        private void textBox3_TextChanged(object sender, EventArgs e) { }
        private void textBox5_TextChanged(object sender, EventArgs e) { }
        private void label12_Click(object sender, EventArgs e) { }
        private void label9_Click(object sender, EventArgs e) { }
        private void label11_Click(object sender, EventArgs e) { }
        private void label14_Click(object sender, EventArgs e) { }
        private void label15_Click(object sender, EventArgs e) { }
        private void label17_Click(object sender, EventArgs e) { }
        private void textBox13_TextChanged(object sender, EventArgs e) { }
        private void textBox26_TextChanged(object sender, EventArgs e) { }
        private void textBox31_TextChanged(object sender, EventArgs e) { }
        private void label18_Click(object sender, EventArgs e) { }
        private void label23_Click(object sender, EventArgs e) { }
        private void groupBox6_Enter(object sender, EventArgs e) { }
        private void textBox56_TextChanged(object sender, EventArgs e) { }
        private void label27_Click(object sender, EventArgs e) { }
        private void groupBox3_Enter(object sender, EventArgs e) { }
        private void label30_Click(object sender, EventArgs e) { }
        private void groupBox5_Enter(object sender, EventArgs e) { }
        private void label32_Click(object sender, EventArgs e) { }
        private void label35_Click(object sender, EventArgs e) { }
        private void label36_Click(object sender, EventArgs e) { }
        private void label37_Click(object sender, EventArgs e) { }
        private void label38_Click(object sender, EventArgs e) { }
        private void textBox66_TextChanged(object sender, EventArgs e) { }
        private void button1_Click_1(object sender, EventArgs e) { }
        private void label41_Click(object sender, EventArgs e) { }
        private void textBox70_TextChanged(object sender, EventArgs e) { }
        private void groupBox4_Enter(object sender, EventArgs e) { }
    }
}