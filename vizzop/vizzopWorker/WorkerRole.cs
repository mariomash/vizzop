using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using vizzopWeb;
using vizzopWeb.Models;
using System.Collections.Generic;

namespace vizzopWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        private vizzopContext db = new vizzopContext();
        private Utils utils = new Utils();
        private string tempPath;

        public override void Run()
        {
            var localResource = RoleEnvironment.GetLocalResource("LocalImageProcessingTemp");
            tempPath = localResource.RootPath;

            // This is a sample worker implementation. Replace with your logic.
            //Trace.TraceInformation("vizzopWorker entry point called", "Information");
            utils.GrabaLog(Utils.NivelLog.info, "vizzopWorker entry point called");
            LanzaYControlaProcesoFileScreenShots();
            LanzaYControlaProcesoPhantom();
            LanzaYControlaProcesoCreaVideos();
#if DEBUG
#else
#endif
            while (true)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }


        private class ScreenGroup
        {
            public int Key { get; set; }
            public int count { get; set; }
        }

        private void CreaVideos()
        {
            try
            {
                //utils.GrabaLog(Utils.NivelLog.info, "Iniciando CreaVideos " + tempPath);

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                if (Directory.Exists(tempPath + @"videos\"))
                {
                    foreach (var file in Directory.GetFiles(tempPath + @"videos\"))
                    {
                        File.Delete(file);
                    }
                }
                else
                {
                    Directory.CreateDirectory(tempPath + @"videos\");
                }

                if (Directory.Exists(tempPath + @"captures\"))
                {
                    foreach (var file in Directory.GetFiles(tempPath + @"captures\"))
                    {
                        File.Delete(file);
                    }
                }
                else
                {
                    Directory.CreateDirectory(tempPath + @"captures\");
                }


                if (Directory.Exists(tempPath + @"img\"))
                {
                    foreach (var file in Directory.GetFiles(tempPath + @"img\"))
                    {
                        File.Delete(file);
                    }
                }
                else
                {
                    Directory.CreateDirectory(tempPath + @"img\");
                }

                DateTime dtFrom = DateTime.Now.ToUniversalTime().Subtract(TimeSpan.FromHours(1));

                //IEnumerable<ScreenGroup> groups = new List<ScreenGroup>();
                var groups = (from m in db.ScreenCaptures
                              where m.CreatedOn < dtFrom
                              group m by m.converser.ID into g
                              select new ScreenGroup()
                              {
                                  Key = g.Key,
                                  count = g.Count()
                              }).ToList<ScreenGroup>();

                foreach (var g in groups)
                {
                    try
                    {
                        var videoname = g.Key.ToString() + @".mp4";

                        ScreenMovie sm = null;

                        Int32 intkey = Convert.ToInt32(g.Key);
                        sm = (from m in db.ScreenMovies
                              where m.converser.ID == intkey
                              select m).FirstOrDefault();

                        if (sm != null)
                        {

                            // Retrieve storage account from connection string.
                            CloudBlobContainer container = blobClient.GetContainerReference("videos");
                            CloudBlockBlob blockBlob = container.GetBlockBlobReference(videoname);
                            if (blockBlob.Exists())
                            {
                                blockBlob.DownloadToFile(tempPath + @"videos\" + videoname, FileMode.Create);
                            }
                        }

                        var captures = (from m in db.ScreenCaptures
                                        where m.converser.ID == intkey && m.Blob != null
                                        orderby m.ID ascending
                                        select m).Take(10).ToList<ScreenCapture>();

                        if (captures.Count() == 0)
                        {
                            continue;
                        }

                        ScreenCapture captureToCreate = null;
                        if (sm != null)
                        {
                            captureToCreate = new ScreenCapture();
                            captureToCreate.CreatedOn = sm.LastFrameCreatedOn;
                            captureToCreate.Data = sm.LastFrameData;
                            captureToCreate.Height = sm.LastFrameHeight;
                            captureToCreate.Width = sm.LastFrameWidth;
                            captureToCreate.ScrollTop = sm.LastFrameScrollTop;
                            captureToCreate.ScrollLeft = sm.LastFrameScrollLeft;
                            captureToCreate.Url = sm.LastFrameUrl;
                            //Metemos el elemento que ya teníamos..
                            captures.Insert(0, captureToCreate);
                        }

                        DateTime NextImgDate = captures.FirstOrDefault().CreatedOn;

                        int counter = 0;
                        for (int i = 0; i < captures.Count(); i++)
                        {
                            try
                            {
                                //Lo primerito sacamos la imagen que tendremos que agregar...
                                captureToCreate = captures[i];
                                if ((captureToCreate.Data == null) || (captureToCreate.Data == ""))
                                {
                                    captureToCreate.Data = null;
                                    //No tiene imagen... la renderizamos!!!!
                                    captureToCreate.Data = RenderCaptureData(captureToCreate);
                                    if (captureToCreate.Data == null)
                                    {
                                        utils.GrabaLog(Utils.NivelLog.error, "Ojo!! captura ha fallado ID: " + captureToCreate.ID);
                                        continue;
                                    }
                                }

                                //Ahora creamos la imagen que ira al video..
                                CreateCaptureImage(captureToCreate, intkey.ToString() + "_" + counter);
                                counter++;

                                //Y si hay mas imagenes detras... miramos si hay que rellenar segundos
                                if ((i + 1) < captures.Count())
                                {
                                    NextImgDate = captures[i + 1].CreatedOn;
                                    DateTime TempImgDate = NextImgDate;
                                    if (NextImgDate.Subtract(captureToCreate.CreatedOn).TotalSeconds < 10)
                                    {
                                        while (captureToCreate.CreatedOn < NextImgDate)
                                        {
                                            CreateCaptureImage(captureToCreate, intkey.ToString() + "_" + counter);
                                            captureToCreate.CreatedOn = captureToCreate.CreatedOn.AddMilliseconds(250);//250 es 4fps //33.3 milliseconds — the amount of time one frame lasts in 30fps video
                                            counter++;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex__)
                            {
                                db.ScreenCaptures.Remove((from m in db.ScreenCaptures
                                                          where m.ID == captures[i].ID
                                                          select m).FirstOrDefault());
                                utils.GrabaLogExcepcion(ex__);
                            }
                        }

                        if (File.Exists(tempPath + @"img\" + g.Key + "_0.png"))
                        {
                            if (ConvertAndUpload(g.Key.ToString()) == true)
                            {
                                if (sm == null)
                                {
                                    sm = new ScreenMovie();
                                    sm.CreatedOn = DateTime.Now;
                                    db.ScreenMovies.Add(sm);

                                }
                                int _convid = captureToCreate.converser.ID;
                                sm.ModifiedOn = DateTime.Now;
                                sm.converser = (from m in db.Conversers
                                                where m.ID == _convid
                                                select m).FirstOrDefault();
                                sm.LastFrameCreatedOn = captureToCreate.CreatedOn;
                                sm.LastFrameData = captureToCreate.Data;
                                sm.LastFrameHeight = captureToCreate.Height;
                                sm.LastFrameScrollLeft = captureToCreate.ScrollLeft;
                                sm.LastFrameScrollTop = captureToCreate.ScrollTop;
                                sm.LastFrameWidth = captureToCreate.Width;
                                sm.LastFrameUrl = captureToCreate.Url;
                                sm.ModifiedOn = sm.CreatedOn;

                                db.SaveChanges();

                            }
                        }

                        //Borramos incluso si falló...
                        foreach (var sc in captures)
                        {
                            var _id = sc.ID;
                            var toRemove = (from m in db.ScreenCaptures
                                            where m.ID == _id
                                            select m).FirstOrDefault();
                            if (toRemove != null)
                            {
                                db.ScreenCaptures.Remove(toRemove);
                            }
                        }
                        db.SaveChanges();

                        foreach (var file in Directory.GetFiles(tempPath + @"img\"))
                        {
                            File.Delete(file);
                        }

                    }
                    catch (Exception ex_)
                    {
                        utils.GrabaLogExcepcion(ex_);
                    }
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
            }
        }

        private string RenderCaptureData(ScreenCapture captureToCreate)
        {
            string strBase64 = null;
            try
            {
#if DEBUG
                utils.GrabaLog(Utils.NivelLog.info, "Iniciando Render. Capture.ID:  " + captureToCreate.ID);
#endif

                string pathjs = "phantom_videos.js";

                Process proc = utils.DoLaunchCaptureProcess(pathjs, captureToCreate.converser.UserName, captureToCreate.converser.Business.Domain, captureToCreate.converser.Password, captureToCreate.GUID);

                while (ProcessExtensions.IsRunning(proc))
                {
                    //utils.GrabaLog(Utils.NivelLog.info, "Esperando a que el proceso termine GUID: " + captureToCreate.GUID);
                }

                var pathtosearch = AppDomain.CurrentDomain.BaseDirectory + @"\img\captures_video";
                foreach (var file in Directory.GetFiles(pathtosearch))
                {
                    if (file.EndsWith("png") == false)
                    {
                        File.Delete(file);
                        continue;
                    }
                    using (var stream = File.OpenRead(file))
                    using (var image = Image.FromStream(stream))
                    {
                        strBase64 = utils.ImageToJpegBase64(image, 100L);
                    }


                    if (strBase64 != null)
                    {

                        string FileName = file.Split('\\')[file.Split('\\').Length - 1];
                        string Guid = FileName.Split('_')[0];

                        if (captureToCreate.GUID == Guid)
                        {
                            if (File.Exists(file))
                            {
#if DEBUG
#else
                                File.Delete(file);
#endif
                            }
                        }
                    }
                }

                return strBase64;
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return strBase64;
            }
        }

        internal Boolean CreateCaptureImage(ScreenCapture captureToCreate, string name)
        {
            try
            {

                Bitmap bitmap = utils.PrepareScreenToReturn(captureToCreate, null, "800", true);
                //Bitmap bm3 = new Bitmap(bitmap);

                MemoryStream MemStream = new MemoryStream();
                bitmap.Save(MemStream, System.Drawing.Imaging.ImageFormat.Png);
                using (var fs = File.Create(tempPath + "img/" + name + ".png"))
                {
                    //MemStream.copyTo(fs);
                    MemStream.WriteTo(fs);
                }

                bitmap.Dispose();
                MemStream.Dispose();
                /*
                bitmap.Save("img/" + name + ".png", System.Drawing.Imaging.ImageFormat.Bmp);
                Thread.Sleep(TimeSpan.FromMilliseconds(500));
                 */
                return true;
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return false;
            }
        }

        internal Boolean ConvertAndUpload(string inputFileName)
        {
            try
            {
                string ffmpeg_filename = "ffmpeg.exe"; // Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.LastIndexOf("\\")) + "\\ffmpeg.exe";
                Process process = new Process();
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = ffmpeg_filename;
                //estaba en -r 25 (25 frames por segundo...) y lo paso a 1
                //estandar
                //ffmpeg -i input_file.avi -codec:v libx264 -profile: high -preset slow -b:v 500k -maxrate 500k -bufsize 1000k -vf scale=-1:480 -threads 0 -codec:a libfdk_aac -b:a 128k output_file.mp4

                //-start_number 0 ffmpeg -i input_file.avi -codec:v libx264 -profile: high -preset slow -b:v 500k -maxrate 500k -bufsize 1000k -vf scale=-1:480 -threads 0 -codec:a libfdk_aac -b:a 128k output_file.mp4
                //psi.Arguments = string.Format(@"-start_number 0 -r 25 -i {0} -codec:v libx264 -profile: high -preset slow -b:v 500k -maxrate 500k -bufsize 1000k -vf scale=-1:480 -threads 0 -codec:a libfdk_aac -b:a 128k -r 25 {1}", tempPath + @"img\" + inputFileName + "_%d.png", tempPath + @"videos\" + inputFileName + "_temp.mp4");//-s 640x360
                psi.Arguments = string.Format(@" -framerate 4 -i " + tempPath + @"img\" + inputFileName + @"_%d.png -s:v 1280x720 -c:v libx264 -profile:v high -crf 23 -pix_fmt yuv420p -r 4 " + tempPath + @"videos\" + inputFileName + @"_temp.mp4");
                //psi.Arguments = string.Format(@"-start_number 0 -r 25 -i {0} -y -b 1500k -vcodec libx264 -r 25 {1}", tempPath + @"img\" + inputFileName + "_%d.png", tempPath + @"videos\" + inputFileName + "_temp.mp4");//-s 640x360
                psi.CreateNoWindow = true;
                psi.ErrorDialog = false;
                psi.UseShellExecute = false;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                //psi.RedirectStandardOutput = true;
                //psi.RedirectStandardInput = false;
                //psi.RedirectStandardError = true;
                process.StartInfo = psi;
                process.Start();
                process.WaitForExit();

                if (File.Exists(tempPath + @"videos\" + inputFileName + ".mp4"))
                {
                    process = new Process();

                    process.StartInfo.FileName = ffmpeg_filename;
                    process.StartInfo.Arguments = string.Format(@"-i {0} -y -c copy -bsf:v h264_mp4toannexb -f mpegts " + tempPath + @"videos\intermediate1.ts", tempPath + @"videos\" + inputFileName + ".mp4");
                    psi.CreateNoWindow = true;
                    psi.ErrorDialog = false;
                    psi.UseShellExecute = false;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.Start();
                    process.WaitForExit();// Waits here for the process to exit.
                    if (File.Exists(tempPath + @"videos\" + inputFileName + ".mp4"))
                    {
                        File.Delete(tempPath + @"videos\" + inputFileName + ".mp4");
                    }

                    process.StartInfo.FileName = ffmpeg_filename;
                    process.StartInfo.Arguments = string.Format(@"-i {0} -y -c copy -bsf:v h264_mp4toannexb -f mpegts " + tempPath + @"videos\intermediate2.ts", tempPath + @"videos\" + inputFileName + "_temp.mp4");
                    psi.CreateNoWindow = true;
                    psi.ErrorDialog = false;
                    psi.UseShellExecute = false;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.Start();
                    process.WaitForExit();// Waits here for the process to exit.

                    process.StartInfo.FileName = ffmpeg_filename;
                    process.StartInfo.Arguments = string.Format(@"-i ""concat:" + tempPath + @"videos\intermediate1.ts|" + tempPath + @"videos\intermediate2.ts"" -y -c copy -bsf:a aac_adtstoasc {0}", tempPath + @"videos\" + inputFileName + ".mp4");
                    psi.CreateNoWindow = true;
                    psi.ErrorDialog = false;
                    psi.UseShellExecute = false;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.Start();
                    process.WaitForExit();// Waits here for the process to exit.

                }
                else
                {
                    File.Move(tempPath + @"videos\" + inputFileName + "_temp.mp4", tempPath + @"videos\" + inputFileName + ".mp4");
                }

                using (var fileStream = System.IO.File.OpenRead(tempPath + @"videos\" + inputFileName + ".mp4"))
                {
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer container = blobClient.GetContainerReference("videos");
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(inputFileName + ".mp4");
                    blockBlob.DeleteIfExists();
                    blockBlob.UploadFromStream(fileStream);

                    //utils.GrabaLog(Utils.NivelLog.info, "Subido " + inputFileName + ".mp4");
                }

                return true;
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return false;
            }
        }


        private void LanzaYControlaProcesoCreaVideos()
        {

            BackgroundWorker bw = new BackgroundWorker();

            // this allows our worker to report progress during work
            bw.WorkerReportsProgress = true;

            // what to do in the background thread
            bw.DoWork += new DoWorkEventHandler(
            delegate(object o, DoWorkEventArgs args)
            {
                BackgroundWorker b = o as BackgroundWorker;
                CreaVideos();
            });

            // what to do when progress changed (update the progress bar for example)
            bw.ProgressChanged += new ProgressChangedEventHandler(
            delegate(object o, ProgressChangedEventArgs args)
            {
                //label1.Text = string.Format("{0}% Completed", args.ProgressPercentage);
            });

            // what to do when worker completes its task (notify the user)
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
            delegate(object o, RunWorkerCompletedEventArgs args)
            {
                LanzaYControlaProcesoCreaVideos();
            });

            bw.RunWorkerAsync();
        }

        /*
         * Tema capturas...
         */

        private void LanzaYControlaProcesoFileScreenShots()
        {

            BackgroundWorker bw = new BackgroundWorker();

            // this allows our worker to report progress during work
            bw.WorkerReportsProgress = true;

            // what to do in the background thread
            bw.DoWork += new DoWorkEventHandler(
            delegate(object o, DoWorkEventArgs args)
            {
                BackgroundWorker b = o as BackgroundWorker;
                utils.LaunchScreenShotsFileControl();
            });

            // what to do when progress changed (update the progress bar for example)
            bw.ProgressChanged += new ProgressChangedEventHandler(
            delegate(object o, ProgressChangedEventArgs args)
            {
                //label1.Text = string.Format("{0}% Completed", args.ProgressPercentage);
            });

            // what to do when worker completes its task (notify the user)
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
            delegate(object o, RunWorkerCompletedEventArgs args)
            {
                LanzaYControlaProcesoFileScreenShots();
            });

            bw.RunWorkerAsync();
        }

        private void LanzaYControlaProcesoPhantom()
        {

            BackgroundWorker bw = new BackgroundWorker();

            // this allows our worker to report progress during work
            bw.WorkerReportsProgress = true;

            // what to do in the background thread
            bw.DoWork += new DoWorkEventHandler(
            delegate(object o, DoWorkEventArgs args)
            {
                BackgroundWorker b = o as BackgroundWorker;
                utils.LaunchCaptureProcesses();
            });

            // what to do when progress changed (update the progress bar for example)
            bw.ProgressChanged += new ProgressChangedEventHandler(
            delegate(object o, ProgressChangedEventArgs args)
            {
                //label1.Text = string.Format("{0}% Completed", args.ProgressPercentage);
            });

            // what to do when worker completes its task (notify the user)
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
            delegate(object o, RunWorkerCompletedEventArgs args)
            {
                LanzaYControlaProcesoPhantom();
            });

            bw.RunWorkerAsync();
        }

    }
}
