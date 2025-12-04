//  Just Another Calendar Widget
//  By: Carl Sorensen
//  v1.0


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace JustAnotherCalendarWidget
{
    public partial class MainWindow : Window
    {
        private DateTime currentDate;
        private readonly List<DateTime> holidays = new List<DateTime>
        {
            // 2025
            new DateTime(2025, 1, 1),
            new DateTime(2025, 1, 20),
            new DateTime(2025, 2, 17),
            new DateTime(2025, 5, 26),
            new DateTime(2025, 6, 19),
            new DateTime(2025, 7, 4),
            new DateTime(2025, 9, 1),
            new DateTime(2025, 10, 13),
            new DateTime(2025, 11, 11),
            new DateTime(2025, 11, 27),
            new DateTime(2025, 12, 25),
            // 2026
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 19),
            new DateTime(2026, 2, 16),
            new DateTime(2026, 5, 25),
            new DateTime(2026, 6, 19),
            new DateTime(2026, 7, 3),
            new DateTime(2026, 9, 7),
            new DateTime(2026, 10, 12),
            new DateTime(2026, 11, 11),
            new DateTime(2026, 11, 26),
            new DateTime(2026, 12, 25),
            // 2027
            new DateTime(2027, 1, 1),
            new DateTime(2027, 1, 18),
            new DateTime(2027, 2, 15),
            new DateTime(2027, 5, 31),
            new DateTime(2027, 6, 18),
            new DateTime(2027, 7, 5),
            new DateTime(2027, 9, 6),
            new DateTime(2027, 10, 11),
            new DateTime(2027, 11, 11),
            new DateTime(2027, 11, 25),
            new DateTime(2027, 12, 24),
            // 2028
            new DateTime(2028, 1, 1),
            new DateTime(2028, 1, 17),
            new DateTime(2028, 2, 21),
            new DateTime(2028, 5, 29),
            new DateTime(2028, 6, 19),
            new DateTime(2028, 7, 4),
            new DateTime(2028, 9, 4),
            new DateTime(2028, 10, 9),
            new DateTime(2028, 11, 11),
            new DateTime(2028, 11, 23),
            new DateTime(2028, 12, 25)
        };

        // P/Invoke for desktop parenting
        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public MainWindow()
        {
            InitializeComponent();
            currentDate = DateTime.Today; // For testing: new DateTime(2025, 8, 26);
            BuildCalendar();

            // Position and size
            this.Left = 550;
            this.Top = 150;
            this.Width = 1000;
            this.Height = 700;

            // Set as desktop widget after load
            this.Loaded += (s, e) =>
            {
                var helper = new WindowInteropHelper(this);
                IntPtr hwnd = helper.Handle;

                // Find Progman
                IntPtr progman = FindWindow("Progman", null);

                // Send message to create WorkerW if needed
                IntPtr result = IntPtr.Zero;
                SendMessageTimeout(progman, 0x052C, IntPtr.Zero, IntPtr.Zero, 0x0, 1000, out result);

                // Find the correct WorkerW
                IntPtr workerw = IntPtr.Zero;
                EnumWindows((hWndEnum, lParam) =>
                {
                    IntPtr defView = FindWindowEx(hWndEnum, IntPtr.Zero, "SHELLDLL_DefView", null);
                    if (defView != IntPtr.Zero)
                    {
                        workerw = FindWindowEx(IntPtr.Zero, hWndEnum, "WorkerW", null);
                        return false; // Stop enumeration
                    }
                    return true;
                }, IntPtr.Zero);

                // If WorkerW found, set parent; else fall back to Progman
                if (workerw != IntPtr.Zero)
                {
                    SetParent(hwnd, workerw);
                }
                else
                {
                    SetParent(hwnd, progman); // Fallback
                }
            };

            // Timer to check for date change every minute
            DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
            timer.Tick += (s, e) =>
            {
                if (DateTime.Today != currentDate)
                {
                    currentDate = DateTime.Today;
                    BuildCalendar();
                }
            };
            timer.Start();
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void BuildCalendar()
        {
            MonthsGrid.Children.Clear();
            YearText.Text = currentDate.Year.ToString();

            for (int i = 0; i < 4; i++)
            {
                DateTime monthDate = currentDate.AddMonths(i);
                int month = monthDate.Month;
                int year = monthDate.Year;

                // Month panel: sidebar + calendar grid
                StackPanel monthPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(30, 15, 30, 15) };

                // Blue sidebar
                Border sidebar = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(0, 102, 204)),
                    Width = 50,
                    MinHeight = 0 // Removed fixed min height to fit content
                };
                TextBlock monthText = new TextBlock
                {
                    Text = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(month),
                    Foreground = Brushes.White,
                    FontSize = 32,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                sidebar.Child = monthText;
                monthPanel.Children.Add(sidebar);

                // Calendar grid
                Grid calGrid = new Grid { Background = Brushes.Black, Margin = new Thickness(5, 0, 0, 0) };
                for (int c = 0; c < 7; c++)
                {
                    calGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(54) });
                }

                // Headers
                calGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                string[] headers = { "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT" };
                for (int c = 0; c < 7; c++)
                {
                    TextBlock header = new TextBlock
                    {
                        Text = headers[c],
                        Foreground = Brushes.White,
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(2)
                    };
                    Grid.SetRow(header, 0);
                    Grid.SetColumn(header, c);
                    calGrid.Children.Add(header);
                }

                // Calendar days with blanks before the 1st
                DateTime firstDay = new DateTime(year, month, 1);
                int startCol = (int)firstDay.DayOfWeek; // 0=Sun, 6=Sat
                int row = 1;
                int col = 0;

                // Add blank cells before the 1st
                for (int b = 0; b < startCol; b++)
                {
                    if (col == 0)
                    {
                        calGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    }

                    Border blankBorder = new Border
                    {
                        Background = Brushes.Black,
                        Width = 54,
                        Height = 39,
                        Margin = new Thickness(1) // Reduced margin for better centering
                    };
                    Grid.SetRow(blankBorder, row);
                    Grid.SetColumn(blankBorder, col);
                    calGrid.Children.Add(blankBorder);

                    col++;
                    if (col == 7)
                    {
                        col = 0;
                        row++;
                    }
                }

                // Add days 1 to lastDay
                int lastDay = DateTime.DaysInMonth(year, month);
                for (int d = 1; d <= lastDay; d++)
                {
                    if (col == 0)
                    {
                        calGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    }

                    DateTime dt = new DateTime(year, month, d);
                    string fgColor = (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday) ? "Gray" : "White";
                    SolidColorBrush bg = Brushes.Black;

                    if (holidays.Contains(dt.Date))
                    {
                        fgColor = "Yellow"; // Holiday: yellow font on black bg
                    }

                    TextBlock dayText = new TextBlock
                    {
                        Text = d.ToString(),
                        Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(fgColor),
                        Background = bg,
                        FontSize = 18,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center, // Added for text centering within block
                        Width = 50,
                        Height = 35,
                        Margin = new Thickness(2)
                    };

                    Border dayBorder = new Border
                    {
                        Background = bg, // Keep black bg
                        Child = dayText,
                        Width = 53,
                        Height = 39,
                        Margin = new Thickness(1)
                    };

                    if (dt.Date == currentDate.Date)
                    {
                        dayBorder.BorderBrush = Brushes.Red;
                        dayBorder.BorderThickness = new Thickness(3);
                    }

                    Grid.SetRow(dayBorder, row);
                    Grid.SetColumn(dayBorder, col);
                    calGrid.Children.Add(dayBorder);

                    col++;
                    if (col == 7)
                    {
                        col = 0;
                        row++;
                    }
                }

                monthPanel.Children.Add(calGrid);

                // Add to main grid
                Grid.SetRow(monthPanel, i / 2);
                Grid.SetColumn(monthPanel, i % 2);
                MonthsGrid.Children.Add(monthPanel);
            }
        }
    }
}