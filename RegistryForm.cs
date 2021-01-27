using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ContextMenuEditor
{
    /// <summary>
    /// A custom Windows Form with controls to display and manage context menu items
    /// </summary>
    class RegistryForm : Form
    {
        private const int footerSize = 1;
        private const int headerSize = 1;
        private TableLayoutPanel table;

        /// <summary>
        /// Invoked when the RegistryForm's "Save" button is clicked
        /// </summary>
        public event EventHandler OnSave;

        /// <summary>
        /// Constructs an empty RegistryForm
        /// </summary>
        public RegistryForm()
        {
            // Assign the form title and size constraints
            this.Text = "Context Menu Editor";
            this.Size = new Size(400, 400);
            this.MinimumSize = new Size(250, 100);

            // Construct a table to organize controls for the form header, footer, and data elements
            table = new TableLayoutPanel();
            table.Dock = DockStyle.Fill;
            table.Padding = Padding.Empty;
            table.Margin = Padding.Empty;
            table.AutoScroll = true;
            table.ColumnCount = 4;
            table.RowCount = 1 + footerSize + headerSize;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            this.Controls.Add(table);

            // Construct a name label, located in the table header row, to identify the name input controls
            Label nameHeader = new Label();
            nameHeader.Text = "NAME";
            nameHeader.Dock = DockStyle.Fill;
            nameHeader.TextAlign = ContentAlignment.MiddleLeft;
            nameHeader.Padding = Padding.Empty;
            table.Controls.Add(nameHeader, 0, 0);

            // Construct a target label, located in the table header row,  to identify the target input controls
            Label targetHeader = new Label();
            targetHeader.Text = "TARGET";
            targetHeader.Dock = DockStyle.Fill;
            targetHeader.TextAlign = ContentAlignment.MiddleLeft;
            targetHeader.Padding = Padding.Empty;
            table.Controls.Add(targetHeader, 1, 0);

            // Construct a save button, located in the table header row, to invoke the OnSave event
            Button saveButton = new Button();
            saveButton.Dock = DockStyle.Fill;
            saveButton.Margin = Padding.Empty;
            saveButton.TextAlign = ContentAlignment.MiddleCenter;
            saveButton.Text = "Save";
            saveButton.Click += (o, a) => OnSave(this, EventArgs.Empty);
            table.Controls.Add(saveButton, 2, 0);
            table.SetColumnSpan(saveButton, 2);

            // Construct an add button, located in the table footer row, to append new data input rows
            Button addButton = new Button();
            addButton.Dock = DockStyle.Fill;
            addButton.Margin = Padding.Empty;
            addButton.TextAlign = ContentAlignment.MiddleCenter;
            addButton.Text = "+";
            addButton.Click += appendRow;
            table.Controls.Add(addButton, 3, 1);
        }

        /// <summary>
        /// Reads the contents of the RegistryForm
        /// </summary>
        /// <returns>A Dictionary with Name inputs corresponding to Keys, and Target inputs to Values.</returns>
        public Dictionary<string, string> Get()
        {
            // Create an empty dictionary to contain and return form contents
            Dictionary<string, string> content = new Dictionary<string, string>();

            // For each row excluding the header and footer,
            for (int row = headerSize; row < table.RowCount - 1 - footerSize; row++)
            {
                // Find the name and target input controls
                var nameBox = table.GetControlFromPosition(0, row) as TextBox;
                var targetBox = table.GetControlFromPosition(1, row) as TextBox;

                // Add their contents to the dictionary
                content.Add(nameBox.Text, targetBox.Text);
            }

            // Return the now-populated dictionary
            return content;
        }

        /// <summary>
        /// Overwrites the contents of the RegistryForm
        /// </summary>
        /// <param name="content">An IDictionary with Name inputs corresponding to Keys, and Target inputs to Values.</param>
        public void Set(IDictionary<string, string> content)
        {
            // While data rows exist, delete the last data row
            while (table.RowCount > 1 + headerSize + footerSize)
                deleteRow(table.RowCount - 1 - footerSize);

            // Create new rows containing the provided content
            foreach (KeyValuePair<string, string> row in content)
                appendRow(row.Key, row.Value);
        }

        /// <summary>
        /// Presents a file selection dialogue. Sets the Target input of the invoking control's table row to the selected filepath.
        /// </summary>
        private void selectFile(object obj, EventArgs args)
        {
            // Get the row number of the invoking control
            int rowNumber = table.GetRow(obj as Control);

            // Find the row's target input
            Control targetBox = table.GetControlFromPosition(1, rowNumber);

            // Create and display an OpenFileDialog
            using (OpenFileDialog fileDialog = new OpenFileDialog())
            {
                fileDialog.Title = "Select target";
                fileDialog.AddExtension = true;
                fileDialog.Multiselect = false;

                DialogResult result = fileDialog.ShowDialog();

                // If a file was selected, set the target input to the filepath
                if (result == DialogResult.OK)
                    targetBox.Text = "\"" + fileDialog.FileName + "\"";
            }
        }

        /// <summary>
        /// Appends a new, empty input row to the end of the form table.
        /// </summary>
        private void appendRow(object obj, EventArgs args)
        {
            appendRow(String.Empty, String.Empty);
        }

        /// <summary>
        /// Appends a new input row to the end of the form table. The contents of the Name and Target inputs are set by the arguments.
        /// </summary>
        private void appendRow(string name, string target)
        {
            // Calculate the next data row number
            int rowNumber = table.RowCount - 1 - footerSize;

            // Shift the footer down to move it out of the way
            shift(rowNumber, 1);

            // Create a row name input control
            TextBox nameBox = new TextBox();
            nameBox.Dock = DockStyle.Fill;
            nameBox.Margin = Padding.Empty;
            nameBox.Text = name;

            // Create a row target input control
            TextBox targetBox = new TextBox();
            targetBox.Dock = DockStyle.Fill;
            targetBox.Margin = Padding.Empty;
            targetBox.Text = target;

            // Create a row button to spawn a file select dialog by invoking selectFile
            Button selectButton = new Button();
            selectButton.Size = new Size(20, 20);
            selectButton.Margin = Padding.Empty;
            selectButton.TextAlign = ContentAlignment.MiddleCenter;
            selectButton.Text = "..";
            selectButton.Click += selectFile;

            // Create a row deletion button, invoking deleteRow
            Button removeButton = new Button();
            removeButton.Size = new Size(20, 20);
            removeButton.Location = new Point(20, 0);
            removeButton.Margin = Padding.Empty;
            removeButton.TextAlign = ContentAlignment.MiddleCenter;
            removeButton.Text = "-";
            removeButton.Click += deleteRow;

            // Add the new controls to the table
            table.Controls.Add(nameBox, 0, rowNumber);
            table.Controls.Add(targetBox, 1, rowNumber);
            table.Controls.Add(selectButton, 2, rowNumber);
            table.Controls.Add(removeButton, 3, rowNumber);
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
        }

        /// <summary>
        /// Deletes the invoking object's table row.
        /// </summary>
        private void deleteRow(object obj, EventArgs args)
        {
            // Get the row number of the invoking control
            int rowNumber = table.GetRow(obj as Control);

            // Delete the row at that number
            deleteRow(rowNumber);
        }

        /// <summary>
        /// Deletes the table row at the provided zero-indexed row number
        /// </summary>
        private void deleteRow(int rowNumber)
        {
            // Remove all controls from the target row
            for (int i = 0; i < table.ColumnCount; i++)
            {
                Control control = table.GetControlFromPosition(i, rowNumber);

                table.Controls.Remove(control);
            }

            // Shift all following rows upwards, to occupy the vacated space
            shift(rowNumber + 1, -1);
        }

        /// <summary>
        /// Takes each row from the origin row number to the end of the form. Shifts these row up or down by an amount. Positive amounts shift down.
        /// </summary>
        private void shift(int origin, int amount)
        {
            //
            int stop = table.RowCount - (int)Math.Max(1, amount);

            //
            if (amount > 0)
                table.RowCount += amount;

            //
            for (int j = origin; j < stop; j++)
            {
                for (int i = 0; i < table.ColumnCount; i++)
                {
                    Control control = table.GetControlFromPosition(i, j);

                    if (control == null)
                        continue;

                    table.Controls.Remove(control);

                    table.Controls.Add(control, i, j + amount);
                }
            }

            //
            if (amount < 0)
                table.RowCount += amount;
        }
    }
}
