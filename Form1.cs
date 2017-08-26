using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;

namespace ComputerVisionAPI_Cs
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();

            if (d.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string filePath = d.FileName;
            //OCR OcrResults
            OcrResults OcrResults = default(OcrResults);
            //建立VisionServiceClient
            var visionClient = new Microsoft.ProjectOxford.Vision.VisionServiceClient(
               "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!換成你的key!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ",
                "https://southeastasia.api.cognitive.microsoft.com/vision/v1.0");
            using (var fs  = new FileStream(filePath, FileMode.Open))
            {
                //  this.textBox.Text = "辨識中...";
                //以繁體中文辨識
                //  this.textBox.Text = "";
                OcrResults = visionClient.RecognizeTextAsync(fs, LanguageCodes.AutoDetect).Result;
            }

            string result = "";
            //抓取每一區塊的辨識結果
            foreach (var Region in OcrResults.Regions)
            {
                //抓取每一行
                foreach (var line_loopVariable in Region.Lines)
                {
                   var line = line_loopVariable;
                    dynamic aline = "";
                    //抓取每一個字
                    foreach (var Word_loopVariable in line.Words)
                    {
                      var  Word = Word_loopVariable;
                        //顯示辨識結果
                        aline += Word.Text;
                    }

                    //加換行
                    result += aline + "\n";
                }
            }

            this.TextBox1.Text = result;

            //load picture
            this.PictureBox1.Image = Image.FromFile(filePath);
        }

        private void Button2_Click(object sender, EventArgs e)
        {

            OpenFileDialog d = new OpenFileDialog();

            if (d.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string filePath = d.FileName;

            //取得原始檔案讀入BPM
            var fs2 = new FileStream(filePath, FileMode.Open);
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(fs2);
            Graphics g = Graphics.FromImage(bmp);
            fs2.Close();

            //OCR OcrResults
            AnalysisResult AnalysisResult = default(AnalysisResult);

            //建立VisionServiceClient
            var visionClient = new Microsoft.ProjectOxford.Vision.VisionServiceClient(
                "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!換成你的key!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ",
                "https://southeastasia.api.cognitive.microsoft.com/vision/v1.0");
            using (var fs  = new FileStream(filePath, FileMode.Open))
            {
                //  this.textBox.Text = "辨識中...";
                //以繁體中文辨識
                //  this.textBox.Text = "";
                var t = new List<VisualFeature>();
                t.Add(VisualFeature.Faces);
                t.Add(VisualFeature.Description);
                AnalysisResult = visionClient.AnalyzeImageAsync(fs, t).Result;
            }

            int isM = 0;
            int isF = 0;
            int resizeFactor = 1;

            string result = "";
            //抓取每一區塊的辨識結果
            foreach (var item_loopVariable in AnalysisResult.Faces)
            {
             var   item = item_loopVariable;
                var faceRect = item.FaceRectangle;

                //畫框
                g.DrawRectangle(
                    new Pen(Brushes.Red, 10), 
                    new System.Drawing.Rectangle((int)faceRect.Left * resizeFactor,
                    (int)faceRect.Top * resizeFactor, (int)faceRect.Width * resizeFactor, (int)faceRect.Height * resizeFactor));
                //顯示年紀
                var age = 0;
                if (item.Gender.StartsWith("F"))
                {
                    age = item.Age - 2;
                }
                else
                {
                    age = item.Age;
                }
                g.DrawString(age.ToString(), new Font("Arial", 16), new SolidBrush(System.Drawing.Color.Black), faceRect.Left * resizeFactor + 3, faceRect.Top * resizeFactor + 3);
                //紀錄性別
                if (item.Gender.StartsWith("M"))
                {
                    isM += 1;
                }
                else
                {
                    isF += 1;
                }
            }

            var PictureDescription = AnalysisResult.Description.Captions[0].Text;
            //如果update了照片，則顯示新圖
            if (AnalysisResult.Faces.Count() > 0)
            {
                PictureDescription += String.Format("找到{0}張臉, {1}男 {2}女", AnalysisResult.Faces.Count(), isM, isF);
                string filename = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(filePath);
                bmp.Save(filename);
                //load picture
                this.PictureBox1.Image = Image.FromFile(filename);
            }

            this.TextBox1.Text = PictureDescription;
        }
    }
}
