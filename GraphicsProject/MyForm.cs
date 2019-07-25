using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GraphicsProject
{
    public class MyForm : Form
    {
        // No properties.
        PictureBox CaptachaBox;
        TextBox AnswerBox;
        Button Genrate_btn;
        Button Submit_btn;

        string[] words = { "swung", "clothing", "bottle", "whom", "people", "swimming", "mud", "mail", "layers", "open", "perhaps", "track", "lucky", "globe", "rich", "distant", "enough", "shaking", "chose", "consider" };
        string[] fonts = { "Arial", "Comicsans"};
        private int CurrentWordIndex;

        public MyForm()
        {
            Text = "Graphical App";
            Size = new Size(700, 250);
            CenterToScreen();
            Render();
        }

        private void Render()
        {
            CaptachaBox = new PictureBox
            {
                Location = new Point(10, 10),
                Width = 500,
                Height = 100
            };

            Genrate_btn = new Button
            {
                Text = "Create",
                Location = new Point(600, 20)
            };

            AnswerBox = new TextBox
            {
                Location = new Point(10, 120),
                Width = 200
            };

            Submit_btn = new Button
            {
                Location = new Point(250, 120),
                Text = "Submit"
            };

            Genrate_btn.Click += Genrate_btn_Click;
            Submit_btn.Click += Submit_btn_Click;

            // Attach these objects to the graphics window.
            Controls.Add(CaptachaBox);
            Controls.Add(Genrate_btn);

            Bitmap img = new Bitmap(CaptachaBox.Width, CaptachaBox.Height);
            Graphics.FromImage(img).FillRectangle(Brushes.White, 0, 0, CaptachaBox.Width, CaptachaBox.Height);
            CaptachaBox.Image = img;
        }

        private void Submit_btn_Click(object sender, EventArgs e)
        {
            string answer = AnswerBox.Text;
            string actual = words[CurrentWordIndex];

            if (answer == actual)
            {
                MessageBox.Show("You got it Right!!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("You a crappy captcha Solver!!", "", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
            }
        }

        void Genrate_btn_Click(object sender, EventArgs e)
        {
            Bitmap img = new Bitmap(CaptachaBox.Width, CaptachaBox.Height);
            Graphics graphics = Graphics.FromImage(img);

            // creates a white background
            graphics.FillRectangle(Brushes.White, 0, 0, img.Width, img.Height);

            Random random = new Random(DateTime.Now.Millisecond);

            int x1 = random.Next(10, 20);
            int y1 = random.Next(15, 25);

            int x2 = random.Next(50, 100);
            int y2 = (random.Next(0, 2) == 0 ? (-1) : 1) * random.Next(30, 50);

            int x3 = random.Next(100, 130);
            int y3 = (random.Next(0, 2) == 0 ? (-1) : 1) * random.Next(30, 50);

            int x4 = random.Next(240, 250);
            int y4 = random.Next(15,25);
            // (random.Next(0,2) == 0 ? (-1) : 1) * 

            GraphicsPath path = new GraphicsPath();

            path.AddBezier
            (
                new Point(x1, y1), 
                new Point(x2, y2), 
                new Point(x3, y3), 
                new Point(x4, y4)
            );

            CurrentWordIndex = random.Next(0, words.Length);

            DrawTextOnPath
            (
                graphics, 
                new SolidBrush(Color.Black), 
                new Font
                (
                    fonts[random.Next(0,fonts.Length)], 
                    random.Next(25, 40)
                ), 
                words[CurrentWordIndex], 
                path, 
                false
            );

            graphics.DrawBezier
            (
                new Pen( Color.Black,random.Next(1,4)),
                new Point(x1, y1 + 30), 
                new Point(x2, y2 + 30), 
                new Point(x3, y3 + 30), 
                new Point(x4, y4 + 30)
            );

            CaptachaBox.Image = img;

            Controls.Add(AnswerBox);
            Controls.Add(Submit_btn);
        }

        // Draw some text along a GraphicsPath.
        private void DrawTextOnPath(Graphics gr, Brush brush, Font font,
            string txt, GraphicsPath path, bool text_above_path)
        {
            // Make a copy so we don't mess up the original.
            path = (GraphicsPath)path.Clone();

            // Flatten the path into segments.
            path.Flatten();

            // Draw characters.
            int start_ch = 0;
            PointF start_point = path.PathPoints[0];
            for (int i = 1; i < path.PointCount; i++)
            {
                PointF end_point = path.PathPoints[i];
                DrawTextOnSegment(gr, brush, font, txt, ref start_ch,
                    ref start_point, end_point, text_above_path);
                if (start_ch >= txt.Length) break;
            }
        }

        // Draw some text along a line segment.
        // Leave char_num pointing to the next character to be drawn.
        // Leave start_point holding the last point used.
        private void DrawTextOnSegment(Graphics gr, Brush brush,
            Font font, string txt, ref int first_ch,
            ref PointF start_point, PointF end_point,
            bool text_above_segment)
        {
            float dx = end_point.X - start_point.X;
            float dy = end_point.Y - start_point.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            dx /= dist;
            dy /= dist;

            // See how many characters will fit.
            int last_ch = first_ch;
            while (last_ch < txt.Length)
            {
                string test_string =
                    txt.Substring(first_ch, last_ch - first_ch + 1);
                if (gr.MeasureString(test_string, font).Width > dist)
                {
                    // This is one too many characters.
                    last_ch--;
                    break;
                }
                last_ch++;
            }
            if (last_ch < first_ch) return;
            if (last_ch >= txt.Length) last_ch = txt.Length - 1;
            string chars_that_fit =
                txt.Substring(first_ch, last_ch - first_ch + 1);

            // Rotate and translate to position the characters.
            GraphicsState state = gr.Save();
            if (text_above_segment)
            {
                gr.TranslateTransform(0,
                    -gr.MeasureString(chars_that_fit, font).Height,
                    MatrixOrder.Append);
            }
            float angle = (float)(180 * Math.Atan2(dy, dx) / Math.PI);
            gr.RotateTransform(angle, MatrixOrder.Append);
            gr.TranslateTransform(start_point.X, start_point.Y,
                MatrixOrder.Append);

            // Draw the characters that fit.
            gr.DrawString(chars_that_fit, font, brush, 0, 0);

            // Restore the saved state.
            gr.Restore(state);

            // Update first_ch and start_point.
            first_ch = last_ch + 1;
            float text_width =
                gr.MeasureString(chars_that_fit, font).Width;
            start_point = new PointF(
                start_point.X + dx * text_width,
                start_point.Y + dy * text_width);
        }
    }
}