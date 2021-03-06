﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Data.SqlClient;

namespace FaceDetectionAndRecognition
{
    public partial class Form1 : Form
    {
       string connectionString = @"Data Source= PRABU95;Initial Catalog=usersRegister;Integrated Security=True;";


        MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_TRIPLEX, 0.6d,0.6d);
        HaarCascade faceDetected;
        Image<Bgr, Byte> Frame;
        Capture camera;
        Image<Gray, Byte> result;
        Image<Gray, Byte> trainedFace = null;
        Image<Gray, Byte> grayFace = null;
        List<Image<Gray, Byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        List<string> users = new List<string>();
        int count, numLabels, t;
        string name, names = null;

        private void saveButton_Click(object sender, EventArgs e)
        {
            count = count + 1;
            grayFace = camera.QueryGrayFrame().Resize(640, 480, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            MCvAvgComp[][] detectedFace = grayFace.DetectHaarCascade(faceDetected, 1.2, 10,
                Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
            foreach (MCvAvgComp f in detectedFace[0])
            {
                trainedFace = Frame.Copy(f.rect).Convert<Gray, Byte>();
                break;
            }

            trainedFace = result.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            trainingImages.Add(trainedFace);
            labels.Add(nameTextBox.Text);
            File.WriteAllText(Application.StartupPath+"/Faces/Faces.txt",trainingImages.ToArray().Length.ToString()+",");
            for (int i=1; i<trainingImages.ToArray().Length; i++)
            {
                trainingImages.ToArray()[i-1].Save(Application.StartupPath+"/Faces/face"+i+".bmp");
                File.AppendAllText(Application.StartupPath+"/Faces/Faces.txt",labels.ToArray()[i-1]+",");
            }

            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {

                sqlCon.Open();
                SqlCommand sqlCmd = new SqlCommand("Addpersons", sqlCon);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                //sqlCmd.Parameters.AddWithValue("@UserId", txtbox_id.Text);

                sqlCmd.Parameters.AddWithValue("@personid", nameTextBox.Text.Trim());
                sqlCmd.Parameters.AddWithValue("@nic", txtbox_nic.Text.Trim());
                sqlCmd.Parameters.AddWithValue("@surname", txtbox_surname.Text.Trim());
                sqlCmd.Parameters.AddWithValue("@fullname", txtbox_fulname.Text.Trim());
                sqlCmd.Parameters.AddWithValue("@address", txtbox_address.Text.Trim());
                sqlCmd.Parameters.AddWithValue("@phoneno1", txtbox_phone1.Text.Trim());
                sqlCmd.Parameters.AddWithValue("@phoneno2", txtbox_phone2.Text.Trim());
                sqlCmd.Parameters.AddWithValue("@email", txtbox_email.Text.Trim());
                sqlCmd.Parameters.AddWithValue("@vehicleno", txtbox_vehocleno.Text.Trim());
                sqlCmd.Parameters.AddWithValue("@birthday", txtbox_bday.Text.Trim());
                sqlCmd.Parameters.AddWithValue("@gender", txtbox_gender.Text.Trim());
                

                sqlCmd.ExecuteNonQuery();
                MessageBox.Show("Registration is successfull");

                //Clear();

               


            }

            MessageBox.Show(nameTextBox.Text + "saved successfully");

            registerForm obj = new registerForm();
            obj.Show();
        }

       void Clear()
        {
            nameTextBox.Text = txtbox_nic.Text = txtbox_nic.Text = txtbox_surname.Text = txtbox_email.Text = txtbox_fulname.Text = txtbox_address.Text = txtbox_phone1.Text = txtbox_phone2.Text= txtbox_email.Text= txtbox_vehocleno.Text= txtbox_bday.Text= txtbox_gender.Text="";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            //registerForm obj = new registerForm();
            addPersonsForm obj = new addPersonsForm();
            obj.Show();
        }

        private void btn_find1_Click(object sender, EventArgs e)
        {
            findpersonPage obj2 = new findpersonPage();
            obj2.Show();
        }

        public Form1()
        {
            InitializeComponent();
            faceDetected = new HaarCascade("haarcascade_frontalface_default.xml");
            try
            {
                string labelsInfo = File.ReadAllText(Application.StartupPath + "Faces/Faces.txt");
                string[] Labels = labelsInfo.Split(',');
                numLabels = Convert.ToInt16(Labels[0].Length);
                count = numLabels;
                string facesLoad;
                for (int i = 1; i < numLabels + 1; i++)
                {
                    facesLoad = "face" + i + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath+"/Faces/Faces.txt"));
                    labels.Add(Labels[i]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Nothing found.");
            }
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            camera = new Capture();
            camera.QueryFrame();
            Application.Idle += new EventHandler(FrameProcedure);
        }

        private void FrameProcedure(object sender, EventArgs e)
        {
            users.Add("");
            Frame = camera.QueryFrame().Resize(640, 480, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            grayFace = Frame.Convert<Gray, Byte>();
            MCvAvgComp[][] facesDetectedNow = grayFace.DetectHaarCascade(faceDetected,1.2,10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
            foreach (MCvAvgComp f in facesDetectedNow[0])
            {
                result = Frame.Copy(f.rect).Convert<Gray, Byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                Frame.Draw(f.rect, new Bgr(Color.Beige),3);
                if (trainingImages.ToArray().Length != 0)
                {
                    MCvTermCriteria termCriterias = new MCvTermCriteria(count, 0.001);
                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), 1500, ref termCriterias);
                    name = recognizer.Recognize(result);
                    Frame.Draw(name, ref font,new Point(f.rect.X-2,f.rect.Y-2), new Bgr(Color.Lime));
                }
                users.Add("");
            }
            cameraBox.Image = Frame;
            names = "";
            users.Clear();
        }
    }
}
