using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;

namespace MouseHook_noUIAutomatic
{
    public partial class Form1 : Form
    {
        private GlobalMouseHook mouseHook;
        private bool isDragging = false;
        private string selectedText = string.Empty;

        public Form1()
        {
            InitializeComponent();
            InitializeHook();

            System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox
            {
                Multiline = true,
                Location = new System.Drawing.Point(10, 10),
                Size = new Size(300, 100),
                ReadOnly = true
            };
            this.Controls.Add(textBox);
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
            }
        }

        private void MouseHook_OnMouseUp(object sender, MouseEventArgs e)
        {
            if (isDragging && e.Button == MouseButtons.Left)
            {
                selectedText = GetSelectedText();
                if (!string.IsNullOrEmpty(selectedText))
                {

                    this.Invoke((MethodInvoker)delegate
                    {
                        ((System.Windows.Forms.TextBox)this.Controls[0]).Text = selectedText;
                    });
                }
                isDragging = false;
            }
        }

        private string GetSelectedText()
        {
            string text = string.Empty;



            if (string.IsNullOrEmpty(text))
            {
                // Simulate Ctrl+C to copy selected text
                SendKeys.Send("^c");
                Thread.Sleep(100);

                // Get text from clipboard
                if (Clipboard.ContainsText())
                {
                    text = Clipboard.GetText().Trim();
                }
            }
            return text;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            mouseHook?.Stop();
            base.OnFormClosing(e);
        }
    }
}
