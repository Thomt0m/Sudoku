﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


using System.Diagnostics;




namespace Sudoku
{
    /// <summary>
    /// Play the game Sudoku
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Get info on the state of a sudoku, or its solution
        /// </summary>
        private SudokuSolver SudokuSolver;

        /// <summary>
        /// Source of Sudokus
        /// </summary>
        private SudokuSource SudokuSource;

        /// <summary>
        /// Sudoku Storage, containing StorageGroup(s), which Category(s), which contain Item(s)
        /// </summary>
        private List<StorageGroup> SudokuStorage;

        /// <summary>
        /// The starting grid of the current sudoku, starting state
        /// </summary>
        private int[,] SudokuStart = new int[9, 9];

        /// <summary>
        /// The current state of the sudoku, cell values, must be 9x9 to ensure conformity with the UI
        /// </summary>
        private int[,] Sudoku = new int[9, 9];

        /// <summary>
        /// The index values of currently selected sudoku from storage
        /// </summary>
        private int[] SelectedSudokuStorageIndex = new int[3];

        /// <summary>
        /// Should only valid input be allowed on the sudoku grid, or should mistakes be allowed
        /// </summary>
        private bool ValidInputOnly = false;

        /// <summary>
        /// Random number generator
        /// </summary>
        private Random rand = new Random();




        /// <summary>
        /// Path to provided xml file
        /// </summary>
        private string StandardXmlFilePath = "SudokuStorage.xml";

        
        // Font values for numbers on sudoku grid
        private int SudokuGridFontSize = 32;
        private Windows.UI.Text.FontWeight StartingValueFontWeight = Windows.UI.Text.FontWeights.Bold;


        private SolidColorBrush MessageTextBlockBrush;










        public MainPage()
        {
            this.InitializeComponent();

            MessageTextBlockBrush = new SolidColorBrush() { Color = Windows.UI.Colors.Black };

            InstantiateSudokuSource();
            ImportSudokusFromXmlFile();
        }








        // -----------------------------------------------------------------
        // --------------      Instantiating methods      ------------------
        // -----------------------------------------------------------------

        /// <summary>
        /// Create a new SudokuSource
        /// </summary>
        private void InstantiateSudokuSource()
        {
            SudokuSource = new SudokuSource();
        }

        /// <summary>
        /// Instatiate a new empty storage collection for Storages
        /// Can be used to clear the current content Storages
        /// </summary>
        private void InstatiateSudokuStorage()
        {
            SudokuStorage = new List<StorageGroup>();
        }

        /// <summary>
        /// Create the UI elements to hold the values of the sudoku grid
        /// </summary>
        private void InstantiateSudokuGridCells()
        {
            SudokuGrid.Children.Clear();

            for (int m = 0; m < Sudoku.GetLength(0); m++)
            {
                for (int n = 0; n < Sudoku.GetLength(1); n++)
                {
                    SudokuGrid.Children.Add(new TextBlock() { Text = string.Empty, FontSize = SudokuGridFontSize, TextAlignment = 0, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
                    SudokuGrid.Children.Last().SetValue(Grid.RowProperty, m);
                    SudokuGrid.Children.Last().SetValue(Grid.ColumnProperty, n);
                }
            }
        }

        /// <summary>
        /// Create the UI elements to hold the values of the sudoku grid in storage
        /// </summary>
        private void InstantiateSudokuStorageGridCells()
        {            
            SudokuStorageGrid.Children.Clear();

            for (int m = 0; m < Sudoku.GetLength(0); m++)
            {
                for (int n = 0; n < Sudoku.GetLength(1); n++)
                {
                    SudokuStorageGrid.Children.Add(new TextBlock() { Text = string.Empty, FontSize = SudokuGridFontSize, FontWeight = StartingValueFontWeight, TextAlignment = 0, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
                    SudokuStorageGrid.Children.Last().SetValue(Grid.RowProperty, m);
                    SudokuStorageGrid.Children.Last().SetValue(Grid.ColumnProperty, n);
                }
            }
        }









        // -----------------------------------------------------------------
        // ------------------      Sudoku Logic      -----------------------
        // -----------------------------------------------------------------

        /// <summary>
        /// Process the input on the sudoku grid from the player
        /// </summary>
        /// <param name="value">Value selected</param>
        /// <param name="cellCoor">Cell selected</param>
        private void ProcessSudokuInput(int value, int cellCoor = -1)
        {
            // If cellCoor was not specified, use the cell-index of the currently selected cell on SudokuCellButtonsGridView
            if (cellCoor == -1) { cellCoor = SudokuCellButtonsGridView.SelectedIndex; }

            // If any given value is smaller than 0, do nothing
            if (value < 0 || cellCoor < 0) { return; }

            int m = cellCoor / Sudoku.GetLength(0);
            int n = cellCoor % Sudoku.GetLength(1);

            // If the given cell contains a starting value, display an error message
            if (SudokuStart[m, n] != 0) { ShowMessageBelowPlayGrid("Cell is staring value", 2); }
            // If only valid input is allowed, and the given value is invalid for the given cell, display an error message
            else if (ValidInputOnly && !IsCellValid(m, n, value)) { ShowMessageBelowPlayGrid("Invalid value", 2); }
            // Else enter the value onto the grid and clear any message
            else
            {
                LoadSingleValueToGrid(m, n, value);
                ShowMessageBelowPlayGrid("");
            }
        }

        /// <summary>
        /// Validate the current sudoku.
        /// Check for any errors and win-state
        /// </summary>
        private void ValidateSudoku()
        {
            // Check if there are any empty fields, ie has the sudoku been completed
            bool isFull = true;
            for (int iM = 0; iM < Sudoku.GetLength(0); iM++)
            {
                for (int iN = 0; iN < Sudoku.GetLength(1); iN++)
                {
                    if (Sudoku[iM, iN] == 0) { isFull = false; }
                }
            }

            // Go over every cell and row

        }

        /// <summary>
        /// Check whether the number v in valid in the cell at coors m, n
        /// </summary>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <param name="v"></param>
        /// <returns>True is no conflicting value(s) is found. False is there is</returns>
        private bool IsCellValid(int m, int n, int v = -1)
        {
            if (v < 0) { v = Sudoku[m, n]; }

            return IsCellValidRow(m, n, v) && IsCellValidColumn(m, n, v) && IsCellValidRegion(m, n, v);
        }

        /// <summary>
        /// Check whether the number v is present anywhere else the row m
        /// </summary>
        /// <param name="m">Row to search through</param
        /// <param name="n">Column of the original cell with value v, needed to not hit on the original cell and its value. Provide a negative number to ignore and match with any cell</param>
        /// <param name="v">Value to match</param>
        /// <returns>True is no matching value is found. False is there is</returns>
        private bool IsCellValidRow(int m, int n, int v = -1)
        {
            // If no v was specified, get the current value of the Sudoku cell at [m, n]
            if (v < 0) { v = Sudoku[m, n]; }

            // If v is not 0, ie the cell is not empty
            if (v != 0)
            {
                // Go over the cells in row m
                for (int i = 0; i < Sudoku.GetLength(1); i++)
                {
                    // If the value v matches with the value in the cell, and the cell is not the original at column n, cell is not valid so return false
                    if (v == Sudoku[m, i] && i != n) { return false; }
                }
            }

            // Value is 0 or no match was found, return true
            return true;
        }

        /// <summary>
        /// Check whether the number v is present anywhere else in the column n
        /// </summary>
        /// <param name="m">Row of the original cell with value v, needed to not hit on the original cell and its value. Provide a negative number to ignore and match with any cell</param>
        /// <param name="n"></param>
        /// <param name="v"></param>
        /// <returns>True is no matching value is found. False is there is</returns>
        private bool IsCellValidColumn(int m, int n, int v = -1)
        {
            // If no v was specified, get the current value of the Sudoku cell at [m, n]
            if (v < 0) { v = Sudoku[m, n]; }

            // If v is not 0, ie the cell is not empty
            if (v != 0)
            {
                // Go over the cells in row m
                for (int i = 0; i < Sudoku.GetLength(0); i++)
                {
                    // If the value v matches with the value in the cell, and the cell is not the original at row m, cell is not valid so return false
                    if (v == Sudoku[i, n] && i != m) { return false; }
                }
            }

            // Value is 0 or no match was found, return true
            return true;
        }

        /// <summary>
        /// Check whether the number v is present anywhere else in the local region
        /// The dimensions of the region are determined by dividing the Sudoku into segments/squares/rectangles, with their dimensions equal to the square root of the length of the corresponding dimension of the sudoku itself
        /// To ignore the coors m and n, and hit on any cell in the region with specify negative values
        /// </summary>
        /// <param name="m">Row coor of the original cell</param>
        /// <param name="n">Column coor of the original cell</param>
        /// <param name="v">Value to match</param>
        /// <returns>True is no matching value is found. False is there is</returns>
        private bool IsCellValidRegion(int m, int n, int v = -1)
        {
            // If no v was specified, get the current value of the Sudoku cell at [m, n]
            if (v < 0) { v = Sudoku[m, n]; }

            if (v != 0)
            {
                // Calculate the lengths of a single region, log debug messages if sqrt does not result in a whole integer
                if (!IsSqrtInt(Sudoku.GetLength(0))) { Debug.WriteLine($"MainPage: IsCellValidRegion() the square root of the grid dimension M is not a whole number, this might cause problems while determining the length for regions of the sudoku, due to required rounding."); }
                if (!IsSqrtInt(Sudoku.GetLength(1))) { Debug.WriteLine($"MainPage: IsCellValidRegion() the square root of the grid dimension N is not a whole number, this might cause problems while determining the length for regions of the sudoku, due to required rounding."); }
                int regLengthM = (int)Math.Sqrt(Sudoku.GetLength(0));
                int regLengthN = (int)Math.Sqrt(Sudoku.GetLength(1));

                // Test, TODO remove
                Debug.WriteLine($"MainPage(): IsCellValidRegion(m={m}, n={n}, v={v}) region dimensions are {regLengthM}, {regLengthN}");

                // Go over each cell in the region
                for (int m1 = m / regLengthM * regLengthM; m1 < m / regLengthM * regLengthM + regLengthM; m1++)
                {
                    for (int n1 = n / regLengthN * regLengthN; n1 < n / regLengthN * regLengthN + regLengthN; n1++)
                    {
                        // Test, TODO remove
                        Debug.WriteLine($"MainPage(): IsCellValidRegion(m={m}, n={n}, v={v}) checking cell {m1}, {n1}");

                        if (v == Sudoku[m1, n1] && m1 != m && n1 != n) { return false; }
                    }
                }
            }

            // Value is 0 or no match was found, return true
            return true;
        }

        /// <summary>
        /// Check whether the values in row m are valid or if there are any conflicts/duplicates
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private bool IsRowValid(int m)
        {
            if (m < 0 || m >= Sudoku.GetLength(0)) { return false; }

            bool?[] numbers = new bool?[Sudoku.GetLength(0)];

            for (int n = 0; n < Sudoku.GetLength(1); n++)
            {
                // If the number is encounter for the first time, set numbers[n] to true
                // If it is encounter again, numbers[n] being not null but true this time, set numbers[n] to false
                if (Sudoku[m, n] != 0 && numbers[n] != false) { numbers[n] = (numbers[n] == null) ? true : false; }
            }

            bool result = true;

            foreach (bool? num in numbers)
            {
                if (result != false ) { result = num != false; }
            }
            return result;
        }















        // -----------------------------------------------------------------
        // ---------------- SudokuSource, File, handeling   ----------------
        // -----------------------------------------------------------------

        /// <summary>
        /// Import Sudokus from the specified xml file
        /// </summary>
        /// <param name="path">file path</param>
        private void ImportSudokusFromXmlFile(string path = "")
        {
            // Use the default xml file if no file was specified
            if (path == "") { path = StandardXmlFilePath; }

            // Abort if file could not be found or is not .xml
            if (!File.Exists(path) || path.Substring(path.Length - 4) != ".xml")
            {
                Debug.WriteLine($"MainPage: ImportSudokusFromXmlFile() recieved invalid file path, {path}");
                return;
            }

            // Make sure SudokuStorage has been instantiated
            if (SudokuStorage == null) { InstatiateSudokuStorage(); }

            // Get the content from the xml file, and add it to SudokuStorage
            AddToSudokuStorage(SudokuSource.GetContentFromXmlFile(path));
        }


        /// <summary>
        /// Add a collection to the current SudokuStorage collection
        /// </summary>
        /// <param name="addition"></param>
        private void AddToSudokuStorage(IEnumerable<StorageGroup> addition)
        {
            SudokuStorage.AddRange(addition);
            ReloadStorageTreeView();
        }










        // -----------------------------------------------------------------
        // --------------------     Opening Sudoku      --------------------
        // -----------------------------------------------------------------

        /// <summary>
        /// Start a sudoku from SudokuStorage, with the specified storage, category, item index
        /// </summary>
        /// <param name="storageIndex"></param>
        /// <param name="categoryIndex"></param>
        /// <param name="itemIndex"></param>
        private void StartNewSudoku(int storageIndex, int categoryIndex, int itemIndex)
        {
            if (SudokuStorage == null || storageIndex < 0 || categoryIndex < 0 || itemIndex < 0 || SudokuStorage.Count == 0)
            {
                int[,] grid = new int[Sudoku.GetLength(0), Sudoku.GetLength(1)];
                for (int i = 0; i < grid.Length; i++) { grid[i / Sudoku.GetLength(0), i % Sudoku.GetLength(1)] = i; }
                LoadNewSudokuToGrid(grid);
            }
            else
            {
                LoadNewSudokuToGrid(SudokuStorage[storageIndex].Categories[categoryIndex].Items[itemIndex].Grid);
                SetCurrentSudokuDetails(storageIndex, categoryIndex, itemIndex);
            }
        }

        /// <summary>
        /// Start a random sudoku from SudokuStorage
        /// </summary>
        private void StartRandomSudoku()
        {
            if (SudokuStorage == null || SudokuStorage.Count == 0)
            {
                // Load the grid with cell-count values, signaling a sudoku could not be loaded
                int[,] grid = new int[Sudoku.GetLength(0), Sudoku.GetLength(1)];
                for (int i = 0; i < grid.Length; i++) { grid[i / Sudoku.GetLength(0), i % Sudoku.GetLength(1)] = i; }
                LoadNewSudokuToGrid(grid);
            }
            else
            {
                int i0 = rand.Next(SudokuStorage.Count);
                int i1 = rand.Next(SudokuStorage[i0].Categories.Count);
                int i2 = rand.Next(SudokuStorage[i0].Categories[i1].Items.Count);
                LoadNewSudokuToGrid(SudokuStorage[i0].Categories[i1].Items[i2].Grid);
                SetCurrentSudokuDetails(i0, i1, i2);
            }
        }

        /// <summary>
        /// Restart the current sudoku. Loads SudokuStart to the play grid
        /// </summary>
        private void RestartSudoku()
        {
            LoadNewSudokuToGrid(SudokuStart);
        }











        // -----------------------------------------------------------------
        // ----------     Sudoku Grid (UI grid) manipulation    ----------
        // -----------------------------------------------------------------

        /// <summary>
        /// Load a new sudoku to play
        /// Use Start...Sudoku() methods instead
        /// </summary>
        /// <param name="gridValues">int[] (1x81) containing the values to load</param>
        private void LoadNewSudokuToGrid(int[] gridValues)
        {
            // Make sure the provided array gridValues is of the correct length, if not then return
            if (gridValues.Length != Sudoku.Length)
            {
                Debug.WriteLine($"LoadSudokuToGrid: gridValues length must be equal 81. This value is required because of the UI. length = {gridValues.Length}");
                return;
            }

            // Make sure the grid has been populated with the required elements/cells
            if (SudokuGrid.Children.Count != Sudoku.Length)
            {
                InstantiateSudokuGridCells();
            }

            // Load the gridValues as the current sudoku, and to the grid on screen
            int x;
            for (int m = 0; m < Sudoku.GetLength(0); m++)
            {
                for (int n = 0; n < Sudoku.GetLength(1); n++)
                {                    
                    x = m * Sudoku.GetLength(1) + n;
                    SudokuStart[m, n] = gridValues[x];
                    Sudoku[m, n] = gridValues[x];
                    LoadSingleValueToGrid(m, n, gridValues[x], true);
                }
            }

            // Set whether to allow only valid input for this game
            ValidInputOnly = ValidNumberEntryCheckBox.IsChecked ?? false;
            // Clear any message from the previous game
            ShowMessageBelowPlayGrid("");
        }

        /// <summary>
        /// Load a new sudoku play
        /// Use Start...Sudoku() methods instead
        /// </summary>
        /// <param name="gridValues">int[9, 9] containing the values to load</param>
        private void LoadNewSudokuToGrid(int[,] gridValues)
        {
            // Make sure the provided array gridValues is of the correct length, if not then return
            if (gridValues.Rank != 2 || gridValues.GetLength(0) != Sudoku.GetLength(0) || gridValues.GetLength(1) != Sudoku.GetLength(1))
            {
                Debug.WriteLine($"LoadSudokuToGrid: gridValues must be 9x9. This value is required because of the UI. length0 = {gridValues.GetLength(0)}, length1 = {gridValues.GetLength(1)}");
                return;
            }

            // Populated with clean elements/cells
            InstantiateSudokuGridCells();

            // Load the gridValues as the current sudoku, and to the grid on screen
            SudokuStart = gridValues;
            Sudoku = gridValues;
            for (int m = 0; m < Sudoku.GetLength(0); m++)
            {
                for (int n = 0; n < Sudoku.GetLength(1); n++)
                {
                    LoadSingleValueToGrid(m, n, gridValues[m, n], true);
                }
            }

            // Set whether to allow only valid input for this game
            ValidInputOnly = ValidNumberEntryCheckBox.IsChecked ?? false;
            // Clear any message from the previous game
            ShowMessageBelowPlayGrid("");
        }

        /// <summary>
        /// Load a value to the specified element on SudokuGrid
        /// </summary>
        /// <param name="m">M-coordinate. must be (Sudoku.GetLength(0) > m >= 0)</param>
        /// <param name="n">N-coordinate. must be (Sudoku.GetLength(1) > n >= 0)</param>
        /// <param name="value">value to load to grid element/cell</param>
        /// <param name="isStart">Whether this element is a starting element of the sudoku, and should be noted in bold</param>
        private void LoadSingleValueToGrid(int m, int n, int value, bool isStart = false)
        {
            if (m < 0 || n < 0 || m >= Sudoku.GetLength(0) || n >= Sudoku.GetLength(1)) { return; }

            SudokuGrid.Children.ElementAt(m * Sudoku.GetLength(1) + n).SetValue(TextBlock.TextProperty, value == 0 ? string.Empty : value.ToString());
            if (isStart)
            {
                SudokuGrid.Children.ElementAt(m * Sudoku.GetLength(1) + n).SetValue(TextBlock.FontWeightProperty, StartingValueFontWeight);
            }
        }

        /// <summary>
        /// Load a value to the specified element on SudokuGrid
        /// </summary>
        /// <param name="m">M-coordinate. must be (Sudoku.GetLength(0) > m >= 0)</param>
        /// <param name="n">N-coordinate. must be (Sudoku.GetLength(1) > n >= 0)</param>
        /// <param name="value">value to load to grid element/cell</param>
        /// <param name="isStart">Whether this element is a starting element of the sudoku, and should be noted in bold</param>
        private void LoadSingleValueToGrid(int m, int n, string value, bool isStart = false)
        {
            SudokuGrid.Children.ElementAt(m * Sudoku.GetLength(1) + n).SetValue(TextBlock.TextProperty, value == "0" ? string.Empty : value.ToString());
            if (isStart)
            {
                SudokuGrid.Children.ElementAt(m * Sudoku.GetLength(1) + n).SetValue(TextBlock.FontWeightProperty, StartingValueFontWeight);
            }
        }










        // -----------------------------------------------------------------
        // -----------     Sudoku Storage Grid manipulation      -----------
        // -----------------------------------------------------------------

        /// <summary>
        /// Load a sudoku, from storage, to the StorageGrid
        /// </summary>
        /// <param name="storageIndex"></param>
        /// <param name="categoryIndex"></param>
        /// <param name="itemIndex"></param>
        private void LoadSudokuToStorageGrid(int storageIndex, int categoryIndex, int itemIndex)
        {
            // grid to load
            int[,] grid;

            // Ensure the given values are valid. Should be redundant since the values are to be gotten from StorageTreeView, which is based on SudokuStorage, but just in case
            if (SudokuStorage == null || storageIndex < 0 || categoryIndex < 0 || itemIndex < 0 ||
                storageIndex >= SudokuStorage.Count || categoryIndex >= SudokuStorage[storageIndex].Categories.Count || itemIndex >= SudokuStorage[storageIndex].Categories[categoryIndex].Items.Count)
            {
                // Load the grid with cell-count values, signaling a sudoku could not be loaded
                grid = new int[Sudoku.GetLength(0), Sudoku.GetLength(1)];
                for (int i = 0; i < grid.Length; i++) { grid[i / Sudoku.GetLength(0), i % Sudoku.GetLength(1)] = i; }
            }
            else
            {
                // Else load the desired sudoku from SudokuStorage
                grid = SudokuStorage[storageIndex].Categories[categoryIndex].Items[itemIndex].Grid;
            }

            // Make sure the grid has been populated with the required elements/cells
            if (SudokuStorageGrid.Children.Count != Sudoku.Length)
            {
                InstantiateSudokuStorageGridCells();
            }

            for (int m = 0; m < Sudoku.GetLength(0); m++)
            {
                for (int n = 0; n < Sudoku.GetLength(1); n++)
                {
                    SudokuStorageGrid.Children.ElementAt(m * Sudoku.GetLength(1) + n).SetValue(TextBlock.TextProperty, grid[m, n] == 0 ? string.Empty : grid[m, n].ToString());
                }
            }
        }









        // -----------------------------------------------------------------
        // ---------------      UI element manipulation      ---------------
        // -----------------------------------------------------------------

        /// <summary>
        /// Read entire SudokuStorage collection to StorageTreeView
        /// </summary>
        private void ReloadStorageTreeView()
        {
            // Make sure SudokuStorage has content, else return
            if (SudokuStorage == null || SudokuStorage.Count == 0) { return; }

            // Clear any old content from the TreeView
            StorageTreeView.RootNodes.Clear();

            // Object/Content to add to the TreeView
            TreeViewNode storageNode;
            TreeViewNode categoryNode;
            TreeViewNode itemNode;

            // For each storage in Storages
            for (int i0 = 0; i0 < SudokuStorage.Count; i0++)
            {
                // Create a node
                storageNode = new TreeViewNode() { Content = SudokuStorage[i0].Name };
                // Assign negative values to the tag. This prevents a "System.Exception: Catastrophic failure" from occuring at StorageTreeView_ItemInvoked(). The node tag of all nodes can/might be read, but only the itemNode contains needed values
                storageNode.SetValue(TagProperty, new int[3] { -2, -2, -2 });

                // For each Category in Storage
                for (int i1 = 0; i1 < SudokuStorage[i0].Categories.Count; i1++)
                {
                    // Create a node
                    categoryNode = new TreeViewNode() { Content = SudokuStorage[i0].Categories[i1].Name };
                    // Assign negative values to the tag. This prevents a "System.Exception: Catastrophic failure" from occuring at StorageTreeView_ItemInvoked(). The node tag of all nodes can/might be read, but only the itemNode contains needed values
                    categoryNode.SetValue(TagProperty, new int[3] { -2, -2, -2 });

                    // For each Item in Category
                    for (int i2 = 0; i2 < SudokuStorage[i0].Categories[i1].Items.Count; i2++)
                    {
                        // Create a node
                        itemNode = new TreeViewNode() { Content = string.IsNullOrWhiteSpace(SudokuStorage[i0].Categories[i1].Items[i2].Name) ? $"Sudoku {i2}" : SudokuStorage[i0].Categories[i1].Items[i2].Name };
                        // Add tag to node with index info, to retrieve item from SudokuStorage. Tag consists of int[3] { StorageGroup-Index, Category-Index, Item-Index}
                        itemNode.SetValue(TagProperty, new int[3] { i0, i1, i2 });
                        // Add the Item to the current Category node
                        categoryNode.Children.Add(itemNode);
                    }
                    // Add the Category to the current Storage node
                    storageNode.Children.Add(categoryNode);
                }
                // Add the storage node to the TreeView as a RootNode
                StorageTreeView.RootNodes.Add(storageNode);
            }
        }

        /// <summary>
        /// Show the details of a sudoku next to the SudokuGrid.
        /// </summary>
        /// <param name="storageIndex"></param>
        /// <param name="categoryIndex"></param>
        /// <param name="itemIndex"></param>
        private void SetCurrentSudokuDetails(int storageIndex, int categoryIndex, int itemIndex)
        {
            CurrentSudokuStorageNameTextBlock.Text = SudokuStorage[storageIndex].Name;
            CurrentSudokuCategoryNameTextBlock.Text = SudokuStorage[storageIndex].Categories[categoryIndex].Name;
            CurrentSudokuItemNameTextBlock.Text = string.IsNullOrWhiteSpace(SudokuStorage[storageIndex].Categories[categoryIndex].Items[itemIndex].Name) ? itemIndex.ToString() : SudokuStorage[storageIndex].Categories[categoryIndex].Items[itemIndex].Name;
        }

        /// <summary>
        /// Set IsEnabled for the specified NumSelButton
        /// </summary>
        /// <param name="number"></param>
        /// <param name="isEnabled"></param>
        private void SetIsEnabledForInputNumber(int number, bool isEnabled)
        {
            switch (number)
            {
                case 1: { NumSelButton1.IsEnabled = isEnabled; break; }
                case 2: { NumSelButton2.IsEnabled = isEnabled; break; }
                case 3: { NumSelButton3.IsEnabled = isEnabled; break; }
                case 4: { NumSelButton4.IsEnabled = isEnabled; break; }
                case 5: { NumSelButton5.IsEnabled = isEnabled; break; }
                case 6: { NumSelButton6.IsEnabled = isEnabled; break; }
                case 7: { NumSelButton7.IsEnabled = isEnabled; break; }
                case 8: { NumSelButton8.IsEnabled = isEnabled; break; }
                case 9: { NumSelButton9.IsEnabled = isEnabled; break; }
            }
        }

        /// <summary>
        /// Display a message just below the Sudoku Grid
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="severity">Severity of message, determines the colour of the text. 0 = black, 1 = orange, 2 = red</param>
        /// <param name="time">Time after which the message will be deleted from screen</param>
        private void ShowMessageBelowPlayGrid(string message, int severity = 0)
        {
            // Change the colour of the text according to the severity
            switch (severity)
            {
                case 0: { MessageTextBlockBrush.Color = Windows.UI.Colors.Black; break; }
                case 1: { MessageTextBlockBrush.Color = Windows.UI.Colors.Orange; break; }
                case 2: { MessageTextBlockBrush.Color = Windows.UI.Colors.Red; break; }
            }
            MessageTextBlock.Text = message;
        }















        // -----------------------------------------------------------------
        // --------------------      Helper Method      --------------------
        // -----------------------------------------------------------------

        /// <summary>
        /// Get the current sudoku as a single string
        /// </summary>
        /// <returns></returns>
        private string SudokuAsString()
        {
            string s = "";
            foreach (int i in Sudoku) { s += i; }
            return s;
        }

        /// <summary>
        /// Get the starting state of the current sudoku as a single string
        /// </summary>
        /// <returns></returns>
        private string SudokuStartAsString()
        {
            string s = "";
            foreach (int i in SudokuStart) { s += i; }
            return s;
        }

        /// <summary>
        /// Write all the content of Storages to the output, debug
        /// </summary>
        private void PrintSudokuStorageContentDebug()
        {
            for (int curStor = 0; curStor < SudokuStorage.Count; curStor++)
            {
                Debug.WriteLine($"SudokuSource: DebugPrintStoragesContentToOutput(), storage {curStor}, name = {SudokuStorage[curStor].Name}, source = {SudokuStorage[curStor].Source}, content# = {SudokuStorage[curStor].Categories.Count}");

                for (int curCat = 0; curCat < SudokuStorage[curStor].Categories.Count; curCat++)
                {
                    Debug.WriteLine($"SudokuSource: DebugPrintStoragesContentToOutput(), category {curCat}, name = {SudokuStorage[curStor].Categories[curCat].Name}, content# = {SudokuStorage[curStor].Categories[curCat].Items.Count}");

                    for (int curItem = 0; curItem < SudokuStorage[curStor].Categories[curCat].Items.Count; curItem++)
                    {
                        Debug.WriteLine($"SudokuSource: DebugPrintStoragesContentToOutput(), item {curItem}, name = {SudokuStorage[curStor].Categories[curCat].Items[curItem].Name}, content = {SudokuStorage[curStor].Categories[curCat].Items[curItem].GridAsString()}");
                    }
                }
            }
        }

        /// <summary>
        /// Is the square root of x a whole number
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private bool IsSqrtInt(int x)
        {
            if (x < 0) { return false; }
            return Math.Sqrt(x) % 1 == 0;
        }













        // -----------------------------------------------------------------
        // ----------------------      Events      -------------------------
        // -----------------------------------------------------------------

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"MainPage: TestButton_Click() SudokuCellButtonsGridView.SelectedIndex = {SudokuCellButtonsGridView.SelectedIndex}");
        }

        private void Test2Button_Click(object sender, RoutedEventArgs e)
        {
            RestartSudoku();
        }

        // Use SudokuCellButtonsGridView_SelectionChanged() instead, click happens before GridView.SelectedIndex has updated (event is not bound)
        private void SudokuCellButtonsGridView_Click(object sender, ItemClickEventArgs e)
        {
            // Click event is too quick for SelectedIndex to be updated
            // Debug.WriteLine($"MainPage: SudokuCellButtonsGridView_Click() {SudokuCellButtonsGridView.SelectedIndex}");
        }

        private void SudokuCellButtonsGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine($"MainPage: SudokuCellButtonsGridView_SelectionChanged() {SudokuCellButtonsGridView.SelectedIndex}");
        }

        private void NumSel_Click(object sender, RoutedEventArgs e)
        {
            // Debug.WriteLine($"SudokuNumSel_Click() clicked for {((Button)sender).Tag}");

            int i = -1;
            try
            {
                int.TryParse(((Button)sender).Tag.ToString(), out i);
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"SudokuNumSel_Click() caught Exception {exception}");
                throw;
            }
            ProcessSudokuInput(i, SudokuCellButtonsGridView.SelectedIndex);
        }

        private void GetRandomSudokuButton_Click(object sender, RoutedEventArgs e)
        {
            StartRandomSudoku();
        }

        private void StoragesDropDownButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SavedGridsTestButton_Click(object sender, RoutedEventArgs e)
        {
            ReloadStorageTreeView();
        }

        private void StorageTreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            int[] tag = new int[3] { -1, -1, -1 };
            try
            {
                tag = ((TreeViewNode)args.InvokedItem).GetValue(TagProperty) as int[];
                // Only the TreeViewNode of a sudoku Item contains positive values, Storage and Category contain negatives
                if (tag[0] >= 0)
                {
                    SelectedSudokuStorageIndex = tag;
                    LoadSudokuToStorageGrid(tag[0], tag[1], tag[2]);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"MainPage: StorageTreeView_ItemInvoked() TreeViewNode.GetValue(TagProperty) threw exception {e}");
            }            
        }

        private void LoadSudokutoPlayGridButton_Click(object sender, RoutedEventArgs e)
        {
            StartNewSudoku(SelectedSudokuStorageIndex[0], SelectedSudokuStorageIndex[1], SelectedSudokuStorageIndex[2]);
            MainPagePivot.SelectedIndex = 0;
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            RestartSudoku();
        }

        private void CheckCurrentStateForErrorsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SolveSudokuButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SudokuPivotItem_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Number0:
                case Windows.System.VirtualKey.Number1:
                case Windows.System.VirtualKey.Number2:
                case Windows.System.VirtualKey.Number3:
                case Windows.System.VirtualKey.Number4:
                case Windows.System.VirtualKey.Number5:
                case Windows.System.VirtualKey.Number6:
                case Windows.System.VirtualKey.Number7:
                case Windows.System.VirtualKey.Number8:
                case Windows.System.VirtualKey.Number9:
                    {
                        ProcessSudokuInput((int)e.Key - (int)Windows.System.VirtualKey.Number0, SudokuCellButtonsGridView.SelectedIndex);
                        break;
                    }
                case Windows.System.VirtualKey.NumberPad0:
                case Windows.System.VirtualKey.NumberPad1:
                case Windows.System.VirtualKey.NumberPad2:
                case Windows.System.VirtualKey.NumberPad3:
                case Windows.System.VirtualKey.NumberPad4:
                case Windows.System.VirtualKey.NumberPad5:
                case Windows.System.VirtualKey.NumberPad6:
                case Windows.System.VirtualKey.NumberPad7:
                case Windows.System.VirtualKey.NumberPad8:
                case Windows.System.VirtualKey.NumberPad9:
                    {
                        ProcessSudokuInput((int)e.Key - (int)Windows.System.VirtualKey.NumberPad0, SudokuCellButtonsGridView.SelectedIndex);
                        break;
                    }

                default:
                    {
                        Debug.WriteLine($"MainPage: SudokuPivotItem() KeyUp {e.Key.ToString()}");
                        break;
                    }
            }
        }

    }
}
