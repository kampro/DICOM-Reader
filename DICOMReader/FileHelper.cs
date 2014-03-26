using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO;
using Ionic.Zip;

namespace DICOMReader
{
    class FileHelper
    {
        public const string tempDirPath = @".\TEMP\";

        public RunWorkerCompletedEventHandler zipCompleted;
        public ProgressChangedEventHandler zipProgressChanged;

        private TreeView treeView;
        private BackgroundWorker backgroundWorker;

        public FileHelper(TreeView tv)
        {
            this.treeView = tv;
            this.backgroundWorker = new BackgroundWorker();
            this.backgroundWorker.WorkerReportsProgress = true;
        }

        public bool IsDCM(FileInfo fileInfo)
        {
            if (/*((fileInfo.Attributes & FileAttributes.Normal) == FileAttributes.Normal) &&*/
                (fileInfo.Extension == ".dcm" || fileInfo.Extension == ".DCM"))
                return true;
            else
                return false;
        }

        public bool IsZip(FileInfo fileInfo)
        {
            if (((fileInfo.Attributes & FileAttributes.Archive) == FileAttributes.Archive) &&
                (fileInfo.Extension == ".zip" || fileInfo.Extension == ".ZIP"))
                return true;
            else
                return false;
        }

        public void AddDCM(FileInfo fileInfo)
        {
            TreeNode node = new TreeNode(fileInfo.Name);

            node.Tag = fileInfo.FullName;
            this.treeView.Nodes.Add(node);
        }

        public void AddDir(FileInfo fileInfo)
        {
        }

        public void AddZip(FileInfo fileInfo)
        {
            this.backgroundWorker.DoWork += (s, e) =>
            {
                using (ZipFile zip = ZipFile.Read(fileInfo.FullName))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(FileHelper.tempDirPath);

                    if (!directoryInfo.Exists)
                        directoryInfo.Create();

                    string fileExtension;
                    FileInfo extractedFileInfo;

                    int i = 0;
                    int total = zip.Count;

                    foreach (ZipEntry ze in zip)
                    {
                        fileExtension = ze.FileName.Substring(ze.FileName.Length - 4);

                        if (fileExtension == ".dcm" || fileExtension == ".DCM")
                        {
                            ze.Extract(FileHelper.tempDirPath, ExtractExistingFileAction.OverwriteSilently);

                            extractedFileInfo = new FileInfo(FileHelper.tempDirPath + ze.FileName.Replace('/', '\\'));
                            // run in GUI thread
                            this.treeView.Invoke(
                                new EventHandler(
                                    (s2, e2) =>
                                    {
                                        this.AddDCM(extractedFileInfo);
                                    }
                                )
                            );
                        }

                        i++;

                        this.backgroundWorker.ReportProgress((int)(i / (double)total * 100));

                        // DEBUG
                        System.Diagnostics.Debug.WriteLine("file: {0} | un.size: {1} | cp.size: {2} | cp.ratio: {3} | encrypt: {4}",
                            ze.FileName,
                            ze.UncompressedSize,
                            ze.CompressedSize,
                            ze.CompressionRatio,
                            (ze.UsesEncryption) ? "Y" : "N");
                    }
                }
            };
            this.backgroundWorker.RunWorkerCompleted += this.zipCompleted;
            this.backgroundWorker.ProgressChanged += this.zipProgressChanged;
            this.backgroundWorker.RunWorkerAsync();
        }
    }
}
