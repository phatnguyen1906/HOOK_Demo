using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
//using Microsoft.Office.Interop.Word;
//using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using Interop.UIAutomationClient;
using System.Diagnostics;


namespace MouseHook
{
    public partial class Form1 : Form
    {
        private GlobalMouseHook mouseHook;
        private CUIAutomation automation;
        private TextBox displayTextBox;
        private bool isDragging = false;  
        private Point startPoint;        


        public Form1()
        {
            InitializeComponent();
            InitializeComponents();
            InitializeAutomation();
            InitializeHook();
        }

        private void InitializeComponents()
        {
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(100, 200);
            this.BackColor = Color.White;
            this.TransparencyKey = Color.White;
            displayTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(240, 240, 240)
            };
            this.Controls.Add(displayTextBox);
        }

        private void InitializeAutomation()
        {
            automation = new CUIAutomation();
        }

        private void InitializeHook()
        {
            mouseHook = new GlobalMouseHook();
            mouseHook.OnMouseDown += MouseHook_OnMouseDown;
            mouseHook.OnMouseUp += MouseHook_OnMouseUp;
            mouseHook.Start();
        }

        private void MouseHook_OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                startPoint = new Point(e.X, e.Y);
            }
        }

        private void MouseHook_OnMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (isDragging)
                {
                    string text;
                    if (Math.Abs(e.X - startPoint.X) > 5 || Math.Abs(e.Y - startPoint.Y) > 5)
                    {
                        // Có kéo thả
                        text = GetSelectedTextFromDrag(startPoint, new Point(e.X, e.Y));
                    }
                    else
                    {
                        // Chỉ click
                        text = GetTextFromPoint(e.X, e.Y);
                    }

                    if (!string.IsNullOrEmpty(text))
                    {
                        ShowTextAtPosition(text, e.X, e.Y);
                    }
                }
                isDragging = false;
            }
        }

        private string GetSelectedTextFromDrag(Point start, Point end)
        {
                // Lấy element tại điểm bắt đầu
             IUIAutomationElement element = automation.ElementFromPoint(
                    new tagPOINT { x = start.X, y = start.Y });

                if (element != null)
                {
                    var textPattern = element.GetCurrentPattern(UIA_PatternIds.UIA_TextPatternId)
                        as IUIAutomationTextPattern;

                    if (textPattern != null)
                    {
                        // Lấy range tại điểm bắt đầu
                        IUIAutomationTextRange startRange = textPattern.RangeFromPoint(
                            new tagPOINT { x = start.X, y = start.Y });

                        // Lấy range tại điểm kết thúc
                        IUIAutomationTextRange endRange = textPattern.RangeFromPoint(
                            new tagPOINT { x = end.X, y = end.Y });

                        if (startRange != null && endRange != null)
                        {
                            // Tạo range từ điểm bắt đầu đến điểm kết thúc
                            IUIAutomationTextRange selectedRange = startRange.Clone();
                            selectedRange.MoveEndpointByRange(
                                (Interop.UIAutomationClient.TextPatternRangeEndpoint)TextPatternRangeEndpoint.TextPatternRangeEndpoint_End,
                                endRange,
                                (Interop.UIAutomationClient.TextPatternRangeEndpoint)TextPatternRangeEndpoint.TextPatternRangeEndpoint_End);

                            // Lấy text được chọn
                            string selectedText = selectedRange.GetText(-1)?.Trim();
                            if (!string.IsNullOrWhiteSpace(selectedText))
                            {
                                return selectedText;
                            }
                        }
                    }

                //không lấy được bằng TextPattern, dùng clipboard
                try
                {
                    // Giả lập Ctrl+C
                    SendKeys.Send("^c");
                    Thread.Sleep(100);

                    if (Clipboard.ContainsText())
                    {
                        string clipboardText = Clipboard.GetText().Trim();
                        if (!string.IsNullOrWhiteSpace(clipboardText))
                        {
                            return clipboardText;
                        }
                    }
                }
                catch { }
            }

            return string.Empty;
        }
        private string GetTextFromPoint(int x, int y)
        {
            try
            {
                IUIAutomationElement element = automation.ElementFromPoint(new tagPOINT { x = x, y = y });

                if (element != null)
                {
                    var textPattern = element.GetCurrentPattern(UIA_PatternIds.UIA_TextPatternId)
                        as IUIAutomationTextPattern;

                    if (textPattern != null)
                    {
                        // Lấy range tại vị trí click
                        IUIAutomationTextRange rangeAtPoint = textPattern.RangeFromPoint(
                            new tagPOINT { x = x, y = y }
                        );

                        if (rangeAtPoint != null)
                        {
                            // Mở rộng selection để lấy cả từ
                            rangeAtPoint.ExpandToEnclosingUnit(Interop.UIAutomationClient.TextUnit.TextUnit_Word);

                            // Lấy text của từ
                            string word = rangeAtPoint.GetText(-1)?.Trim();

                            // Kiểm tra xem có phải là từ hợp lệ không
                            if (!string.IsNullOrWhiteSpace(word))
                            {
                                return word;
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting text: {ex.Message}");
            }

            return string.Empty;
        }

        public enum TextPatternRangeEndpoint
        {
            TextPatternRangeEndpoint_Start = 0,
            TextPatternRangeEndpoint_End = 1
        }
        private void ShowTextAtPosition(string text, int x, int y)
        {
            this.Invoke((MethodInvoker)delegate
            {
                int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
                int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;

                int formX = Math.Min(x + 10, screenWidth - this.Width);
                int formY = Math.Min(y + 10, screenHeight - this.Height);

                this.Location = new Point(formX, formY);
               
                displayTextBox.Text = text;

                if (!string.IsNullOrEmpty(text))
                {
                    Clipboard.SetText(text);
                }

                this.Show();
                this.BringToFront();
            });
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            this.Hide();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Hide();
            }
            base.OnKeyDown(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (mouseHook != null)
            {
                mouseHook.Stop();
                mouseHook = null;
            }

            if (automation != null)
            {
                Marshal.ReleaseComObject(automation);
                automation = null;
            }

            base.OnFormClosing(e);
        }
    }

    // một số ID pattern và property cần thiết
    public static class UIA_PatternIds
    {
        public const int UIA_TextPatternId = 10014;
        public const int UIA_ValuePatternId = 10002;
    }
    public static class UIA_PropertyIds
    {
        public const int UIA_ValueValuePropertyId = 30045;
    }
}

